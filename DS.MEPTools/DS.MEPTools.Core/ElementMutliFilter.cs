using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DS.MEPTools.Core
{
    /// <summary>
    /// Instansiate the object to get elements with multi filters. 
    /// </summary>
    /// <param name="activeDoc"></param>
    public class ElementMutliFilter(Document activeDoc) : IElementMultiFilter
    {
        /// <inheritdoc/>
        public List<ElementId> ElementIdsSet { get; set; }

        /// <inheritdoc/>
        public List<(ElementQuickFilter filter, Func<Transform, ElementQuickFilter> getLinkFilter)> QuickFilters { get; } = [];

        /// <inheritdoc/>
        public List<(ElementSlowFilter filter, Func<Transform, ElementSlowFilter> getLinkFilter)> SlowFilters { get; } = [];


        /// <inheritdoc/>
        public IEnumerable<ElementId> ApplyToActiveDoc()
            => GetElementsIdsFromDoc(activeDoc, QuickFilters.Select(t => t.filter), SlowFilters.Select(t => t.filter));

        /// <inheritdoc/>
        public Dictionary<RevitLinkInstance, IEnumerable<ElementId>> ApplyToLinks()
        {
            var elementIds = new Dictionary<RevitLinkInstance, IEnumerable<ElementId>>();

            var links = activeDoc.GetLoadedLinks();
            if (links == null) { return elementIds; }

            foreach (var link in links)
            {
                var linkDoc = link.GetLinkDocument();
                var linkTransform = link.GetTotalTransform();

                var quickLinkFilters = 
                    GetLinkFilters(linkTransform, QuickFilters);
                var slowLinkFilters = 
                    GetLinkFilters(linkTransform, SlowFilters);

                var linkElmentsIds = GetElementsIdsFromDoc(linkDoc, quickLinkFilters, slowLinkFilters);
                if (linkElmentsIds.Count() > 0) { elementIds.Add(link, linkElmentsIds); }
            }

            return elementIds;

            static IEnumerable<TFilter> GetLinkFilters<TFilter>(Transform linkTransform,
                IEnumerable<(TFilter, Func<Transform, TFilter>)> filtersTranformTuple) where TFilter : ElementFilter
            {
                var linkFilters = new List<TFilter>();

                if (linkTransform.AlmostEqual(Transform.Identity))
                {
                    linkFilters.AddRange(filtersTranformTuple.Select(t => t.Item1));
                }
                else
                {
                    foreach (var ft in filtersTranformTuple)
                    {
                        if (ft.Item2 == null)
                        {
                            linkFilters.Add(ft.Item1);
                        }
                        else
                        {
                            linkFilters.Add(ft.Item2.Invoke(linkTransform));
                        }
                    }
                }

                return linkFilters;
            }
        }

        /// <inheritdoc/>
        public Dictionary<Document, IEnumerable<ElementId>> ApplyToAllDocs()
        {
            var elementIds = new Dictionary<Document, IEnumerable<ElementId>>();

            var activeDocIds = ApplyToActiveDoc();
            var linksDocIds = ApplyToLinks();
            if (activeDocIds.Count() > 0) { elementIds.Add(activeDoc, activeDocIds); }
            linksDocIds.ForEach(l => elementIds.Add(l.Key.GetLinkDocument(), l.Value));

            return elementIds;
        }

        private IEnumerable<ElementId> GetElementsIdsFromDoc(Document doc,
            IEnumerable<ElementQuickFilter> quickFilters,
            IEnumerable<ElementSlowFilter> slowFilters)
        {
            var currentDocIds = ElementIdsSet?.Where(id => doc.GetElement(id) != null).ToList();

            if ((quickFilters.Count() == 0 && slowFilters.Count() == 0) || currentDocIds?.Count == 0)
            { return new List<ElementId>(); }

            var collector = currentDocIds is null ? 
                new FilteredElementCollector(doc) : 
                new FilteredElementCollector(doc, currentDocIds);
            quickFilters.ForEach(filter => collector.WherePasses(filter));
            slowFilters.ForEach(filter => collector.WherePasses(filter));

            return collector.ToElementIds();
        }

        /// <inheritdoc/>
        public void Reset()
        {
            ElementIdsSet = null;
            QuickFilters.Clear();
            SlowFilters.Clear();
        }
    }
}
