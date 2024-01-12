using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.MEPTools.Core
{
    /// <inheritdoc/>
    public class SolidElementIntersectionFactory(Document doc, IElementMultiFilter elementMultiFilter) : 
        SolidElementIntersectionFactoryBase<Element>(doc, elementMultiFilter)
    {

        #region Properties


        /// <summary>
        /// Ids to exclude from intersections.
        /// </summary>
        public List<ElementId> ExcludedElementIds { get; set; } = [];


        /// <summary>
        /// Types to exclude from intersections.
        /// </summary>
        public List<Type> ExculdedTypes { get; } = [];

        /// <summary>
        /// Ids to exclude from intersections.
        /// </summary>
        public List<BuiltInCategory> ExcludedCategories { get; } =
            new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_GenericAnnotation,
                BuiltInCategory.OST_GenericModel
            };

        #endregion

        /// <inheritdoc/>
        public override IEnumerable<Element> GetIntersections(Solid solid)
        {
            //config filters
            _elementMultiFilter.Reset();

            if (ElementIdsSet?.Count > 0)
            { _elementMultiFilter.ElementIdsSet = ElementIdsSet; }

            var boundingBoxFilter = GetBoundingBoxIntersectsFilter(solid);
            _elementMultiFilter.QuickFilters.Add(boundingBoxFilter);

            var typeFilter = new ElementIsElementTypeFilter(true);
            _elementMultiFilter.QuickFilters.Add((typeFilter, null));

            var types = new List<Type>()
            {
                typeof(FamilyInstance),
                typeof(HostObject)
            };
            var multiclassFilter = new ElementMulticlassFilter(types);
            _elementMultiFilter.QuickFilters.Add((multiclassFilter, null));

            if (ExculdedTypes.Count > 0)
            { _elementMultiFilter.QuickFilters.Add((new ElementMulticlassFilter(ExculdedTypes, true), null)); }

            var excludeCategoryfilter =
                    new ElementMulticategoryFilter(ExcludedCategories, true);
            _elementMultiFilter.QuickFilters.Add((excludeCategoryfilter, null));

            if (ExcludedElementIds.Count > 0)
            { _elementMultiFilter.QuickFilters.Add((new ExclusionFilter(ExcludedElementIds), null)); }

            var intersectsElementFilter = GetSolidIntersectsFilter(solid);
            _elementMultiFilter.SlowFilters.Add(intersectsElementFilter);

            var elementIdsDocs = _elementMultiFilter.ApplyToAllDocs();
            return elementIdsDocs.SelectMany(kv => ToElements(kv.Key, kv.Value));
        }

        private (ElementSlowFilter, Func<Transform, ElementSlowFilter>)
          GetSolidIntersectsFilter(Solid sourceSolid)
            =>
            (new ElementIntersectsSolidFilter(sourceSolid), (linkTransform) =>
            new ElementIntersectsSolidFilter(SolidUtils.CreateTransformed(sourceSolid, linkTransform.Inverse)));
    }
}

