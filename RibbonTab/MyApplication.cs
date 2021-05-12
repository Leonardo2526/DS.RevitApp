using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace DS.RevitApp.RibbonTab
{
    public class MyApplication : IExternalApplication
    {
        // class instance
        internal static MyApplication thisApp = null;         

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            // Create a custom ribbon tab
            string tabName = "ГЦМ_ДС";
            application.CreateRibbonTab(tabName);

            var button1Path = Environment.ExpandEnvironmentVariables(@"%AppData%\Autodesk\Revit\Addins\2020\DS.RVTtoNWC\DS.RVTtoNWC.dll");
            var button2Path = Environment.ExpandEnvironmentVariables(@"%AppData%\Autodesk\Revit\Addins\2020\DS.FamiliesUpdate\DS.FamiliesUpdate.dll");
            var button3Path = Environment.ExpandEnvironmentVariables(@"%AppData%\Autodesk\Revit\Addins\2020\DS.AddSharedParameters\DS.AddSharedParameters.dll");
            var button4Path = Environment.ExpandEnvironmentVariables(@"%AppData%\Autodesk\Revit\Addins\2020\DS.AddProjectParameters\DS.AddProjectParameters.dll");
            var button5Path = Environment.ExpandEnvironmentVariables(@"%AppData%\Autodesk\Revit\Addins\2020\DS.DisallowjoinStructure\DS.DisallowjoinStructure.dll");


            // Create push buttons
            PushButtonData button1 = new PushButtonData("Button1", "RVTtoNWC", button1Path, "DS.RevitApp.RVTtoNWC.DS_MainClass");
            PushButtonData button2 = new PushButtonData("Button2", "FamiliesUpdate", button2Path, "DS.RevitApp.FamiliesUpdate.EntryCommand");
            PushButtonData button3 = new PushButtonData("Button3", "AddSharedParameters", button3Path, "AddSharedParameters.EntryCommand");
            PushButtonData button4 = new PushButtonData("Button4", "AddProjectParameters", button4Path, "AddProjectParameters.EntryCommand");
            PushButtonData button5 = new PushButtonData("Button5", "DisallowjoinStructure", button5Path, "DisallowjoinStructure.EntryCommand");


            // Create tool tips
            button1.ToolTip = "Export files from *.rvt to *.nwc.\nv1.2";
            button2.ToolTip = "Update families in *.rvt files.\nv1.2";
            button3.ToolTip = "Add new parameters to shared parameters file.\nv1.0";
            button4.ToolTip = "Add new parameters from shared parameters file to *.rvt projects.\nv1.0";
            button5.ToolTip = "Disallow beams joining in current document.\nv1.0";


            // Create a ribbon panel
            RibbonPanel m_projectPanel_1 = application.CreateRibbonPanel(tabName, "Tools");
            RibbonPanel m_projectPanel_2 = application.CreateRibbonPanel(tabName, "Parameters");


            // Add the buttons to the panels
            List<RibbonItem> projectButtons = new List<RibbonItem>();
            projectButtons.AddRange(m_projectPanel_1.AddStackedItems(button1, button2));
            projectButtons.AddRange(m_projectPanel_2.AddStackedItems(button3, button4, button5));


            return Result.Succeeded;
        }
    }
}
