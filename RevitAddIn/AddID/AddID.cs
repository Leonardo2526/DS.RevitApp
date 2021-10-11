using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AddIns
{

    public class AddID : Autodesk.Revit.UI.IExternalApplication
    {
        public Result OnStartup(Autodesk.Revit.UI.UIControlledApplication application)
        {
            Form1 form1 = new Form1();
            form1.Show();

            // Register wall updater with Revit
            WallUpdater updater = new WallUpdater(application.ActiveAddInId, form1);
            UpdaterRegistry.RegisterUpdater(updater);


            List<BuiltInCategory> builtInCats = new List<BuiltInCategory>();
            builtInCats.Add(BuiltInCategory.OST_Walls);
            builtInCats.Add(BuiltInCategory.OST_StructuralColumns);
            builtInCats.Add(BuiltInCategory.OST_Floors);
            builtInCats.Add(BuiltInCategory.OST_StructuralFoundation);
            builtInCats.Add(BuiltInCategory.OST_StructuralFraming);

            ElementMulticategoryFilter filter1 = new ElementMulticategoryFilter(builtInCats);

            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter1, Element.GetChangeTypeElementAddition());

            return Result.Succeeded;
        }

        public Result OnShutdown(Autodesk.Revit.UI.UIControlledApplication application)
        {
            Form1 form1 = null;
            WallUpdater updater = new WallUpdater(application.ActiveAddInId, form1);
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
            return Result.Succeeded;
        }

    }




    public class WallUpdater : IUpdater
    {
        static AddInId m_appId;
        static UpdaterId m_updaterId;
        Form1 form1;

        // constructor takes the AddInId for the add-in associated with this updater
        public WallUpdater(AddInId id, Form1 form)
        {
            m_appId = id;
            m_updaterId = new UpdaterId(m_appId, new Guid("890b1c75-0be7-4782-b8ad-2125032e7feb"));
            form1 = form;
        }

        public void Execute(UpdaterData data)
        {


            if (form1.checkBox1.Checked)
            {
                Document doc = data.GetDocument();


                // Change the wall to the cached wall type.
                foreach (ElementId addedElemId in data.GetAddedElementIds())
                {
                    Element el = doc.GetElement(addedElemId);
                    Type tp = doc.GetElement(addedElemId).GetType();

                    GetElementParameterInformation(doc, el);

                }
            }

        }

        public string GetAdditionalInformation()
        {
            return "Wall type updater example: updates all newly created walls to a special wall";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FloorsRoofsStructuralWalls;
        }

        public UpdaterId GetUpdaterId()
        {
            return m_updaterId;
        }

        public string GetUpdaterName()
        {
            return "Wall Type Updater";
        }


        void GetElementParameterInformation(Document document, Element element)
        {
            // iterate element's parameters
            foreach (Parameter param in element.Parameters)
            {
                if (param.Definition.Name == "Комментарии")
                {
                    param.Set(element.Id.ToString());
                }

            }

        }

        public static List<Element> FindSpecificTypes(Document doc)
        {

            List<Element> list = new List<Element>();

            FilteredElementCollector finalCollector = new FilteredElementCollector(doc);

            ElementCategoryFilter filter1 = new ElementCategoryFilter(BuiltInCategory.OST_Walls, false);
            finalCollector.WherePasses(filter1);
            ElementCategoryFilter filter2 = new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns, false);
            finalCollector.UnionWith((new FilteredElementCollector(doc)).WherePasses(filter2));
            ElementCategoryFilter filter3 = new ElementCategoryFilter(BuiltInCategory.OST_Floors, false);
            finalCollector.UnionWith((new FilteredElementCollector(doc)).WherePasses(filter3));
            ElementCategoryFilter filter4 = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFoundation, false);
            finalCollector.UnionWith((new FilteredElementCollector(doc)).WherePasses(filter4));
            ElementCategoryFilter filter5 = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming, false);
            finalCollector.UnionWith((new FilteredElementCollector(doc)).WherePasses(filter5));
            finalCollector.IntersectWith((new FilteredElementCollector(doc)).WherePasses(filter5));

            list = finalCollector.ToList<Element>();

            return list;
        }
    }

}
