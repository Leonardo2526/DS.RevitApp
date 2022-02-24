using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.CollisionsElliminator
{

    interface IMovableElemCollision
    {
        public bool IsModelsElementsCountValid { get; set; }

        List<int> GetCollisions();

        List<Element> GetModelElementsByBB();
        List<Element> GetLinkElementsByBB();

    }

    class MovableElementCollision : IMovableElemCollision
    {
        List<Element> _MovableElements;
        BoundingBoxIntersectsFilter BBIntersectsFilter;
        List<Solid> Solids;
        Element FamInstToMove;

        public MovableElementCollision(List<Element> elems, BoundingBoxIntersectsFilter bbIntersectsFilter, List<Solid> solids, Element famInstToMove)
        {
            _MovableElements = elems;
            BBIntersectsFilter = bbIntersectsFilter;
            Solids = solids;
            FamInstToMove = famInstToMove;
        }

        public bool IsModelsElementsCountValid { get; set; } = true;

        Document Doc = Data.Elem1.Document;


        public List<int> GetCollisions()
        {
            List<Element> includedModelElements = GetModelElementsByBB();

            if (!CheckElementsCount(includedModelElements))
                return new List<int>();

            ICollection<ElementId> modelElementsIds = includedModelElements.Select(el => el.Id).ToList();           

            List<Element> includedLinkElements = GetLinkElementsByBB();

            if (!CheckElementsCount(includedLinkElements))
                return new List<int>();

            ICollection<ElementId> linkElementsIds = includedLinkElements.Select(el => el.Id).ToList();

            List<int> collisions = new List<int>();

            foreach (Solid solid in Solids)
            {
                int SolidCollision = 0;

                ElementIntersectsSolidFilter elementIntersectsSolidFilter = new ElementIntersectsSolidFilter(solid);

                int modelCollisions = GetModelCollisions(elementIntersectsSolidFilter, modelElementsIds);
                int linkCollisions = GetLinkedCollisions(elementIntersectsSolidFilter, linkElementsIds);

                SolidCollision = modelCollisions + linkCollisions;

                if (SolidCollision == 0)
                    continue;

                collisions.Add(SolidCollision);
            }

            return collisions;
        }

        public int GetModelCollisions(ElementIntersectsSolidFilter elementIntersectsSolidFilter, ICollection<ElementId> modelElementsIds)
        {
            if (modelElementsIds.Count == 0)
                return 0;

            // Get the handle to the element in the link
            FilteredElementCollector modelCollector = new FilteredElementCollector(Doc, modelElementsIds);
            List<Element> modelCollision = modelCollector.WherePasses(elementIntersectsSolidFilter).ToElements().ToList();

            return modelCollision.Count;
        }

        public int GetLinkedCollisions(ElementIntersectsSolidFilter elementIntersectsSolidFilter, ICollection<ElementId> linkElementsIds)
        {
            if (linkElementsIds.Count == 0)
                return 0;

            int collisions = 0;

            foreach (RevitLinkInstance m_currentInstance in Data.AllLinks)
            {
                if (m_currentInstance != null)
                {
                    // Get the handle to the element in the link
                    Document linkedDoc = m_currentInstance.GetLinkDocument();

                    FilteredElementCollector linkCollector = new FilteredElementCollector(linkedDoc, linkElementsIds);
                    List<Element> linkCollision = linkCollector.WherePasses(elementIntersectsSolidFilter).ToElements().ToList();

                    collisions = collisions + linkCollision.Count;
                }
            }

            return collisions;
        }




        public List<Element> GetModelElementsByBB()
        {
            FilteredElementCollector modelCollector = new FilteredElementCollector(Doc, Data.AllModelElementsIds);
         
            ICollection<ElementId> elementsIds = _MovableElements.Select(el => el.Id).ToList();

            //Element el = MovableElement.FamInstToMove;
            if (FamInstToMove !=null)
            elementsIds.Add(FamInstToMove.Id);

            ExclusionFilter exclusionFilter = new ExclusionFilter(elementsIds);

            modelCollector.WherePasses(BBIntersectsFilter);
            List<Element> includedElements = modelCollector.WherePasses(exclusionFilter).ToElements().ToList();

            return includedElements;
        }

        public List<Element> GetLinkElementsByBB()
        {
            List<Element> includedElements = new List<Element>();

            FilteredElementCollector linkCollector = null;

            foreach (RevitLinkInstance m_currentInstance in Data.AllLinks)
            {
                if (m_currentInstance != null)
                {
                    // Get the handle to the element in the link
                    Document linkedDoc = m_currentInstance.GetLinkDocument();
                    linkCollector = new FilteredElementCollector(linkedDoc, Data.AllLinkedElementsIds);

                    includedElements.AddRange(linkCollector.WherePasses(BBIntersectsFilter).ToElements().ToList());

                }
            }

            return includedElements;
        }

        bool CheckElementsCount(List<Element> elements)
        {
            if (elements.Count > 500)
            {
                IsModelsElementsCountValid = false;
                return false;
            }

            return true;
        }
    }
}
