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

            using (Transaction transNew = new Transaction(doc, "Disallow join"))
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

            FilteredElementCollector docCollector = GetStructuralElements(doc); 
            
            foreach (Element el in docCollector)
            { 
                Autodesk.Revit.DB.Structure.StructuralFramingUtils.DisallowJoinAtEnd((FamilyInstance)el, 0);
                Autodesk.Revit.DB.Structure.StructuralFramingUtils.DisallowJoinAtEnd((FamilyInstance)el, 1);
            }

            MessageBox.Show("Балочные конструкции отсоединены!");
        }

        FilteredElementCollector GetStructuralElements(Document doc)
        {
            // what categories of family instances
            // are we interested in?
            BuiltInCategory[] bics = new BuiltInCategory[] {BuiltInCategory.OST_StructuralFraming};

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
              = new List<ElementFilter>(6)
              {
                  familyInstanceFilter
              };

            LogicalOrFilter classFilter
              = new LogicalOrFilter(b);

            FilteredElementCollector collector
              = new FilteredElementCollector(doc);

            collector.WherePasses(classFilter);

            return collector;
        }

    }

}
