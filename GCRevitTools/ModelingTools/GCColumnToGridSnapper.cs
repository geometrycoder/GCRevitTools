using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GCRevit;
using GCRevit.Collectors;
using GCRevit.ElementDatas;
using GCRevit.Elements;
using GCRevit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GCRevitTools.ModelingTools {

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GCColumnToGridSnapper : IExternalCommand {

        GCRevitDocument doc;
        const string toolName = "Column To Grid Snapper";

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements) {
            try {
                this.doc = GCRevitDocument.DocumentInstance(commandData);
                List<GCColumnVertical> cols = GCColumnCollector.CollectSelectedVerticalColumns(this.doc).ToList<GCColumnVertical>();
                List<AGCGridBase> grids = GCGridCollector.CollectSelectedGrids(this.doc).ToList<AGCGridBase>();
                List<GCGridIntersection> ints = GCGridIntersection.GetGridIntersections(grids);
                if (0 < cols.Count && 0 < ints.Count) {
                    GCLogger.AppendLine("Ints Ct - Cols ct: {0} - {1}", ints.Count, cols.Count);
                    GCLogger.AppendLine("Intersections with less than 0 intersections:");
                    foreach (GCGridIntersection inter in ints) {
                        int intCt = inter.IntersectionPoints.Count;
                        if (0 == intCt) {
                            GCLogger.AppendLine("{0}-{1}", inter.Grid1.RevitElementIdInt, inter.Grid2.RevitElementIdInt);
                        }
                    }
                    GCLogger.AppendLine();
                    using (Transaction trans = new Transaction(this.doc.Document)) {
                        if (TransactionStatus.Started == trans.Start(toolName)) {
                            foreach (GCColumnVertical col in cols) {
                                GCGridIntersection gridInt;
                                XYZ closIntPt = GCGridIntersection.GetClosestGridIntersection(col.GCCurve.StartPoint, ints, out gridInt);
                                GCLogger.AppendLine("Column Id: {0}", col.RevitElementId.IntegerValue);
                                GCLogger.AppendLine("Grids: {0} - {1}", gridInt.Grid1.RevitElement.Name, gridInt.Grid2.RevitElement.Name);
                                col.MoveTo(closIntPt);
                            }
                            trans.Commit();
                            GCLogger.DisplayLog(true);
                            return Result.Succeeded;
                        } else {
                            GCLogger.AppendLine("Failed to start transaction");
                            trans.RollBack();
                        }
                    }
                } else {
                    GCLogger.AppendLine("No grid intersections were found.");
                }
                GCLogger.DisplayLog(true);
                return Result.Cancelled;
            } catch (Exception x) {
                GCLogger.AppendLine("{0} - {1}", x.Message, x.StackTrace);
                GCLogger.DisplayLog(true);
                return Result.Failed;
            }
        }
    }
}
