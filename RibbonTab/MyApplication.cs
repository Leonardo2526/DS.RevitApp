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
            string tabName = "DS.Tab";
            application.CreateRibbonTab(tabName);

            var button1Path = Environment.ExpandEnvironmentVariables(@"%AppData%\Autodesk\Revit\Addins\2020\DS_RevitApp_RVTtoNWC_FW4.8\DS_RevitApp_RVTtoNWC_FW4.8.dll");
            var button2Path = Environment.ExpandEnvironmentVariables(@"%AppData%\Autodesk\Revit\Addins\2020\DS_RevitApp_FamiliesUpdate_4.8\DS_RevitApp_FamiliesUpdate_4.8.dll");
            //var button3Path = Environment.ExpandEnvironmentVariables(@"%AppData%\Autodesk\Revit\Addins\2020\DS_RevitApp_FamiliesUpdate_4.8\DS_RevitApp_FamiliesUpdate_4.8.dll");


            // Create two push buttons
            //PushButtonData button1 = new PushButtonData("Button1", "Hello World", button1Path, "DS_RevitSpace.HelloWorld");
            PushButtonData button1 = new PushButtonData("Button1", "DS.RVTtoNWC.v1.2", button1Path, "DS.RevitApp.RVTtoNWC.DS_MainClass");
            PushButtonData button2 = new PushButtonData("Button2", "DS.FamiliesUpdate.v2", button2Path, "DS.RevitApp.FamiliesUpdate.EntryCommand");


            // Create a ribbon panel
            RibbonPanel m_projectPanel_1 = application.CreateRibbonPanel(tabName, "Tools");
            //RibbonPanel m_projectPanel_2 = application.CreateRibbonPanel(tabName, "DS_Panel_2");

            // Add the buttons to the panel
            List<RibbonItem> projectButtons = new List<RibbonItem>();
            projectButtons.AddRange(m_projectPanel_1.AddStackedItems(button1, button2));
            //projectButtons.AddRange(m_projectPanel_2.AddStackedItems(button1, button2));


            return Result.Succeeded;
        }


    }


}
