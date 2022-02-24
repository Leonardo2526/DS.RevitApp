using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace DS.CollisionsElliminator
{
    class ConnectorUtils
    {
        public static List<Connector> GetConnectors(Element element)
        {
            //1. Get connector set
            ConnectorSet connectorSet = null;

            Type type = element.GetType();

            if (type.ToString().Contains("FamilyInstance"))
            {
                FamilyInstance familyInstance = element as FamilyInstance;
                connectorSet = familyInstance.MEPModel.ConnectorManager.Connectors;
            }
            else
            {
                try
                {
                    MEPCurve mepCurve = element as MEPCurve;
                    connectorSet = mepCurve.ConnectorManager.Connectors;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                }

            }            
        
            //2. Initialise empty list of connectors
            List<Connector> connectorList = new List<Connector>();

            //3. Loop through connector set and add to list
            foreach (Connector connector in connectorSet)
            {
                connectorList.Add(connector);
            }
            return connectorList;
        }

        public static List<Element> GetConnectedElements(Element element)
        {
            List<Connector> connectors = GetConnectors(element);

            List<Element> connectedElements = new List<Element>();
            foreach (Connector connector in connectors)
            {
                ConnectorSet connectorSet = connector.AllRefs;

                foreach (Connector con in connectorSet)
                {
                    ElementId elementId = con.Owner.Id;
                    if (elementId != element.Id)
                        connectedElements.Add(con.Owner);
                }
            }
            return connectedElements;
        }

        public static List<Element> GetConnectedFamilyInstances(Element element)
        {
            List<Connector> connectors = GetConnectors(element);

            List<Element> connectedElements = new List<Element>();
            foreach (Connector connector in connectors)
            {
                ConnectorSet connectorSet = connector.AllRefs;

                foreach (Connector con in connectorSet)
                {
                    ElementId elementId = con.Owner.Id;
                    Type type = con.Owner.GetType();
                    if (elementId != element.Id && type.ToString().Contains("FamilyInstance"))
                        connectedElements.Add(con.Owner);
                }
            }
            return connectedElements;
        }

    }
}
