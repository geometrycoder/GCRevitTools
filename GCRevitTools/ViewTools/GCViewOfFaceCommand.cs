using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GCRevit;
using GCRevit.Creators;
using GCRevit.Elements;
using GCRevit.Utils;
using System;

namespace GCRevitTools.ViewTools {

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GCViewOfFaceCommand : IExternalCommand {

        GCRevitDocument doc;
        const string toolName = "View of Face";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            try {
                this.doc = GCRevitDocument.DocumentInstance(commandData);
                Face face = this.doc.SelectFaceFromDocument();
                BoundingBoxUV bb = face.GetBoundingBox();
                UV mid = (bb.Max + bb.Min) / 2.0;
                using (Transaction trans = new Transaction(this.doc.Document)) {
                    if (TransactionStatus.Started == trans.Start(toolName)) {
                        GCViewLive view = GCViewCreator.CreateSectionAtFaceParameter(this.doc, face, mid, 50, 10);
                        view.RevitView.Name = "Face View";
                        trans.Commit();
                        return Result.Succeeded;
                    } else {
                        GCLogger.AppendLine("Failed to start transaction");
                        GCLogger.DisplayLog(true);
                        trans.RollBack();
                        return Result.Cancelled;
                    }
                }
            } catch (Exception x) {
                GCLogger.AppendLine("{0} - {1}", x.Message, x.StackTrace);
                GCLogger.DisplayLog(true);
                return Result.Failed;
            }
        }
    }
}
