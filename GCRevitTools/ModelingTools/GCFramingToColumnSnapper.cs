using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GCRevit;
using GCRevit.Collectors;
using GCRevit.Elements;
using GCRevit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCRevitTools.ModelingTools {

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GCFramingToColumnSnapper : IExternalCommand {

        GCRevitDocument doc;
        const string toolName = "Column To Grid Snapper";

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements) {
            try {
                this.doc = GCRevitDocument.DocumentInstance(commandData);
                List<AGCFrameCurveDriven> frams = GCFrameCollector.CollectSelectedCurveDrivenFraming(this.doc).ToList<AGCFrameCurveDriven>();
                List<GCColumnVertical> cols = GCColumnCollector.CollectSelectedVerticalColumns(this.doc).ToList<GCColumnVertical>();
                if (0 < cols.Count && 0 < frams.Count) {
                    using (Transaction trans = new Transaction(this.doc.Document)) {
                        if (TransactionStatus.Started == trans.Start(toolName)) {
                            foreach (AGCFrameCurveDriven fram in frams) {
                                GCColumnVertical closCol = GetClosestColumn(fram, cols);
                                XYZ colPt = closCol.GCCurve.StartPoint;
                                if (IsStartPointCloserToColumnThanEndPoint(fram, colPt)) {
                                    fram.MoveStartPoint(colPt);
                                } else {
                                    fram.MoveEndPoint(colPt);
                                }
                            }
                            trans.Commit();
                            return Result.Succeeded;
                        } else {
                            GCLogger.AppendLine("Failed to start transaction");
                            trans.RollBack();
                        }
                    }
                } else {
                    GCLogger.AppendLine("Frames and Columns need to be selected");
                }
                GCLogger.DisplayLog(true);
                return Result.Cancelled;
            } catch (Exception x) {
                GCLogger.AppendLine("{0} - {1}", x.Message, x.StackTrace);
                GCLogger.DisplayLog(true);
                return Result.Failed;
            }
        }

        GCColumnVertical GetClosestColumn(AGCFrameCurveDriven fram, List<GCColumnVertical> cols) {
            double closDist = Double.MaxValue;
            GCColumnVertical closCol = null;
            foreach (GCColumnVertical col in cols) {
                double strDist = GeometryUtil.XYDistance(fram.GCCurve.StartPoint, col.GCCurve.StartPoint);
                double endDist = GeometryUtil.XYDistance(fram.GCCurve.EndPoint, col.GCCurve.StartPoint);
                if (closDist > strDist) {
                    closDist = strDist;
                    closCol = col;
                }
                if (closDist > endDist) {
                    closDist = endDist;
                    closCol = col;
                }
            }
            return closCol;
        }

        bool IsStartPointCloserToColumnThanEndPoint(AGCFrameCurveDriven fram, XYZ colPt) {
            return GeometryUtil.XYDistance(fram.GCCurve.StartPoint, colPt) < GeometryUtil.XYDistance(fram.GCCurve.EndPoint, colPt);
        }
    }
}