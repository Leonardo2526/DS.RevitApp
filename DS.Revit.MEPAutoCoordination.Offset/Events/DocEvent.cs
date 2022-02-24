using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.Revit.MEPAutoCoordination.Offset
{

    class DocEvent
    {

        readonly Application App;

        public ICollection<ElementId> modifiedElementsIds = new List<ElementId>();
        public ICollection<Element> modifiedElements = new List<Element>();

        public DocEvent(Application app)
        {
            App = app;
        }

        public void RegisterEvent()
        {
            try
            {
                // Register event. 
                App.DocumentChanged += new EventHandler<
    Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(application_DocumentChanged);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", ex.ToString());
            }
        }

        public void application_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            Document Doc = e.GetDocument();

            ICollection<ElementId> colElID = e.GetModifiedElementIds().ToList();

            //Fill lists of modifiedElements
            string IDS = "";
            foreach (ElementId elID in colElID)
            {
                Element element = Doc.GetElement(elID);

                if (element.IsPhysicalElement())
                {
                    modifiedElementsIds.Add(elID);
                    modifiedElements.Add(element);
                    IDS += "\n" + elID.ToString();
                }

            }

            //TaskDialog.Show("Revit", "Modified elements IDs: \n" + IDS);

        }

    }

    
}
