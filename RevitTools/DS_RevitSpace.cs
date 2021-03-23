using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS_Forms;
using DS_SystemTools;
using System;



namespace DS_RevitSpace

{
    [Transaction(TransactionMode.Manual)]
    public class HelloWorld : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit,
         ref string message, ElementSet elements)
        {
            TaskDialog.Show("Revit", "Hello World");
            return Result.Succeeded;
        }
    }


    [Transaction(TransactionMode.Manual)]
    public class DS_GetAllModelElements : IExternalCommand
    //Output list with active document physical elements to txt file

    {
        //Get current date and time 
        readonly string CurDate = DateTime.Now.ToString("yyMMdd");
        readonly string CurDateTime = DateTime.Now.ToString("yyMMdd_HHmmss");
        public string SourceFilePath { get; set; }
        public string DestinationPath { get; set; }

        public Result Execute(ExternalCommandData commandData,
        ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

            Program(uiapp);

            return Result.Succeeded;
        }
        public void Program(UIApplication uiapp)
        {
            DS_Tools dS_Tools = new DS_Tools
            {
                DS_LogName = CurDateTime + "_Log.txt"
            };

            GetAllModelElements(uiapp, dS_Tools);

            dS_Tools.DS_FileExistMessage();
        }

        public void GetAllModelElements(UIApplication uiapp, DS_Tools dS_Tools)
        //Get all physical elements in active document

        {
            Document doc = uiapp.ActiveUIDocument.Document;

            // Use shortcut WhereElementIsNotElementType() to find wall instances only
            FilteredElementCollector collector = new FilteredElementCollector(doc).WhereElementIsNotElementType();

            dS_Tools.DS_StreamWriter("The physical elements in the '" + doc.PathName + "' are:\n");

            foreach (Element e in collector)
            {
                if (IsPhysicalElement(e))
                {
                    string output = string.Format("Category: {0}  / Name: {1} + \n", e.Category.Name, e.Name);
                    dS_Tools.DS_StreamWriter(output);
                }
            }
        }

        public bool IsPhysicalElement(Element e)
        {
            if (e.Category == null) return false;
            if (e.ViewSpecific) return false;
            if (e.LevelId == null) return false;

            // exclude specific unwanted categories
            if (((BuiltInCategory)e.Category.Id.IntegerValue) == BuiltInCategory.OST_HVAC_Zones) return false;

            return e.Category.CategoryType == CategoryType.Model && e.Category.CanAddSubcategory;
        }
    }


    [Transaction(TransactionMode.Manual)]
    public class DS_IsEmptyModel : IExternalCommand
        //Check if active document has physical elements
    {
        public Result Execute(ExternalCommandData commandData,
        ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

            if (IsEmptyModel(uiapp))
                TaskDialog.Show("Revit", "No physical elements have been found in this document!");
            else
                TaskDialog.Show("Revit", "Document has physical elements!");
            return Result.Succeeded;
        }

        public bool IsEmptyModel(UIApplication uiapp)
        {
            Document doc = uiapp.ActiveUIDocument.Document;

            // Use shortcut WhereElementIsNotElementType() to find wall instances only
            FilteredElementCollector collector = new FilteredElementCollector(doc).WhereElementIsNotElementType();

            foreach (Element e in collector)
            {
                if (IsPhysicalElement(e))
                    return false;
            }
            return true;
        }

        public bool IsPhysicalElement(Element e)
        {
            if (e.Category == null) return false;
            if (e.ViewSpecific) return false;
            if (e.LevelId == null) return false;

            // exclude specific unwanted categories
            if (((BuiltInCategory)e.Category.Id.IntegerValue) == BuiltInCategory.OST_HVAC_Zones) return false;

            return e.Category.CategoryType == CategoryType.Model && e.Category.CanAddSubcategory;
        }
    }
}