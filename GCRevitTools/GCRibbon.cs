using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCRevitTools {

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GCRibbon : IExternalApplication {

        #region members
        const string ribbonName = "GC Revit Tools";
        const string viewTools = "GC View Tools";
        const string modelTools = "GC Modeling Tools";

        static string executingAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        #endregion

        public Result OnShutdown(UIControlledApplication application) {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application) {
            try {
                CreateRibbon(application);
                return Result.Succeeded;
            } catch {
                return Result.Failed;
            }
        }

        void CreateRibbon(UIControlledApplication application) {
            application.CreateRibbonTab(ribbonName);
            CreateModelingToolPanel(application);
            CreateViewToolPanel(application);
        }

        void CreateModelingToolPanel(UIControlledApplication application) {
            RibbonPanel modPanel = application.CreateRibbonPanel(ribbonName, modelTools);
            CreatePushButton("Column to Grid Snapper", "Moves Columns to nearest grid intersection", typeof(GCRevitTools.GCColumnToGridSnapper));
        }

        void CreateViewToolPanel(UIControlledApplication application) {
            RibbonPanel viewPanel = application.CreateRibbonPanel(ribbonName, viewTools);
            CreatePushButton("View of Face", "Creates a view oriented to the selected face", typeof(GCRevitTools.GCViewOfFaceCommand));
        }

        PushButtonData CreatePushButton(string name, string toolTip, Type toolClass) {
            PushButtonData button = new PushButtonData(name, toolTip, executingAssemblyPath, toolClass.FullName);
            //TODO: Set up help location on geometrycoder.com
            //ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, "");
            //button.SetContextualHelp(contextHelp);
            return button;
        }
    }
}
