using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.Revit.MEPAutoCoordination.Offset
{

    class ElementCollision
    {
        readonly Document Doc;

        public ElementCollision(Document doc)
        {
            Doc = doc;
        }

        ElementUtils ElemUtils = new ElementUtils();

        /// <summary>
        /// Check collisions between source elementID and all others. Return true if no collisions have been found.
        /// </summary>
        public bool CheckCollisionsBetweenAllElements(ElementId sourceElementId, out IList<Element> collisionElements)
        {
            collisionElements = new List<Element>();
            if (sourceElementId == null)
                return true;

            FilteredElementCollector newCollector = new FilteredElementCollector(Doc, Data.AllModelElementsIds);

            Element sourseElement = Doc.GetElement(sourceElementId);
            ElementIntersectsElementFilter elementIntersectsElementFilter = new ElementIntersectsElementFilter(sourseElement);

            ICollection<ElementId> excludedElementsIds = new List<ElementId>
            {
                sourceElementId
            };
            ExclusionFilter exclusionFilter = new ExclusionFilter(excludedElementsIds);

            collisionElements = newCollector.WherePasses(elementIntersectsElementFilter).ToElements();
            collisionElements = newCollector.WherePasses(exclusionFilter).ToElements();

            if (collisionElements.Count != 0)
                return false;

            return true;
        }


        /// <summary>
        /// Check collisions between elements and one another element. Return true if no collisions have been found.
        /// </summary>
        public bool CheckCollisionsBetweenElements(ICollection<ElementId> elementIds, ElementId elementId,
            out IList<Element> collisionElements)
        {
            FilteredElementCollector newCollector = new FilteredElementCollector(Doc, elementIds);

            Element element = Doc.GetElement(elementId);
            ElementIntersectsElementFilter elementIntersectsElementFilter = new ElementIntersectsElementFilter(element);

            ICollection<ElementId> excludedElementsIds = new List<ElementId>
            {
                elementId
            };

            ExclusionFilter exclusionFilter = new ExclusionFilter(excludedElementsIds);

            collisionElements = newCollector.WherePasses(elementIntersectsElementFilter).ToElements();
            collisionElements = newCollector.WherePasses(exclusionFilter).ToElements();

            if (collisionElements.Count != 0)
                return false;

            return true;
        }

        private BoundingBoxXYZ GetElementBoundingBox(Element element)
        {
            // Get the BoundingBox instance for current view.
            BoundingBoxXYZ box = element.get_BoundingBox(null);
            if (null == box)
            {
                throw new Exception("Selected element doesn't contain a bounding box.");
            }
           
            //string info = "Bounding box is enabled: " + box.Enabled.ToString();

            return box;
        }
    }
}
