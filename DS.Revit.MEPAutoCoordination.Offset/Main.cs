using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.Revit.MEPAutoCoordination.Offset
{
    public class Main
    {

        public Main(Document doc, List<Element> allModelElements, 
            List<RevitLinkInstance> allLinks, List<Element> allLinkedElements, 
            MEPCurve elem1Curve, MEPCurve elem2Curve, double MaxZCoordinate, double MinZCoordinate)
        {
            Data.Doc = doc;
            Data.AllModelElements = allModelElements;
            Data.AllLinks = allLinks;
            Data.AllLinkedElements = allLinkedElements;
            Data.Elem1Curve = elem1Curve;
            Data.Elem2Curve = elem2Curve;
            Data.MinZCoordinate = MinZCoordinate;
            Data.MaxZCoordinate = MaxZCoordinate;

            Data.GetAllData();
        }

        public bool IsCollisionResolved { get; set; } = false;

        public void Run()
        {
            CollisionResolver collisionDestroyer = new CollisionResolver();
            collisionDestroyer.Resolve();
            if (collisionDestroyer.IsResolved)
            {
                IsCollisionResolved = true;
            }
           
        }
    }
}
