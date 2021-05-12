using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DS_SystemTools;
using System.IO;

namespace DisallowjoinStructure
{

    [Transaction(TransactionMode.Manual)]
    public class EntryCommand : IExternalCommand
    {
        //Get current date and time    
        readonly string CurDate = DateTime.Now.ToString("yyMMdd");
        readonly string CurDateTime = DateTime.Now.ToString("yyMMdd_HHmmss");

        public Result Execute(ExternalCommandData revit,
         ref string message, ElementSet elements)
        {
            UIApplication uiapp = revit.Application;

            try
            {
                TransactionCommit(uiapp);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private void TransactionCommit(UIApplication uiapp)
        {
            Document doc = uiapp.ActiveUIDocument.Document;


            using (Transaction transNew = new Transaction(doc, "Create project parameter"))
            {
                try
                {
                    transNew.Start();
                    DisconnectElements(doc);

                }


                catch (Exception e)
                {
                    transNew.RollBack();
                    MessageBox.Show(e.ToString());
                }

                transNew.Commit();
            }
        }

        private void DisconnectElements(Document doc)
        {

            FilteredElementCollector docCollector = GetStructuralElements1(doc);

            string DirName = @"%USERPROFILE%\Desktop\Logs\";
            string ExpDirName = Environment.ExpandEnvironmentVariables(DirName);

            DS_Tools dS_Tools = new DS_Tools
            {
                DS_LogName = CurDateTime + "_Log.txt",
                DS_LogOutputPath = ExpDirName
            };
            foreach (Element el in docCollector)
            {
                dS_Tools.DS_StreamWriter(el.Name + "_" + el.Category + "_" + el.DesignOption + "_" + el.DesignOption + "_" + el.Parameters + "_" + el.Id);
            }

            
            foreach (Element el in docCollector)
            { 
                Autodesk.Revit.DB.Structure.StructuralFramingUtils.DisallowJoinAtEnd((FamilyInstance)el, 0);
                Autodesk.Revit.DB.Structure.StructuralFramingUtils.DisallowJoinAtEnd((FamilyInstance)el, 1);
            }

            MessageBox.Show("Done!");
        }

        FilteredElementCollector GetStructuralElements1(Document doc)
        {
            // what categories of family instances
            // are we interested in?

            BuiltInCategory[] bics = new BuiltInCategory[] {
    BuiltInCategory.OST_StructuralColumns,
    BuiltInCategory.OST_StructuralFraming,
    BuiltInCategory.OST_StructuralFoundation
  };

            IList<ElementFilter> a
              = new List<ElementFilter>(bics.Count());

            foreach (BuiltInCategory bic in bics)
            {
                a.Add(new ElementCategoryFilter(bic));
            }

            LogicalOrFilter categoryFilter
              = new LogicalOrFilter(a);

            LogicalAndFilter familyInstanceFilter
              = new LogicalAndFilter(categoryFilter,
                new ElementClassFilter(
                  typeof(FamilyInstance)));

            IList<ElementFilter> b
              = new List<ElementFilter>(6);

            b.Add(familyInstanceFilter);

            LogicalOrFilter classFilter
              = new LogicalOrFilter(b);

            FilteredElementCollector collector
              = new FilteredElementCollector(doc);

            collector.WherePasses(classFilter);

            return collector;
        }

        FilteredElementCollector GetStructuralElements2(Document doc)
        {
            BuiltInCategory[] bics = new BuiltInCategory[] {
    BuiltInCategory.OST_StructuralColumns,
    BuiltInCategory.OST_StructuralFraming,
    BuiltInCategory.OST_StructuralFoundation
  };

            IList<ElementFilter> a
              = new List<ElementFilter>(bics.Count());

            LogicalOrFilter categoryFilter
    = new LogicalOrFilter(a);

            LogicalAndFilter familyInstanceFilter
    = new LogicalAndFilter(categoryFilter,
      new ElementClassFilter(
        typeof(FamilyInstance)));

            IList<ElementFilter> b
              = new List<ElementFilter>(6);

            b.Add(new ElementClassFilter(
              typeof(BeamSystem)));
            LogicalOrFilter classFilter
    = new LogicalOrFilter(b);

            FilteredElementCollector collector
              = new FilteredElementCollector(doc);

            collector.WherePasses(classFilter);

            return collector;
        }


    }

}
