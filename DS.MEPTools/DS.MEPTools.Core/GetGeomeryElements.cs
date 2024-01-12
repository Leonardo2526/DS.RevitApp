using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.MEPTools.Core.Rooms;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using MoreLinq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.MEPTools.Core
{
    public class GetGeomeryElements
    {
        private readonly UIDocument _uiDoc;
        private readonly IElementMultiFilter _elementMultiFilter;
        private readonly Document _doc;

        public GetGeomeryElements(UIDocument uiDoc, IElementMultiFilter elementMultiFilter)
        {
            _uiDoc = uiDoc;
            _elementMultiFilter = elementMultiFilter;
            _doc = _uiDoc.Document;
        }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// A factory to commit transactions.
        /// </summary>
        public ITransactionFactory TransactionFactory { get; set; }


        public IEnumerable<Element> GetElements(MEPCurve mEPCurve)
        {
            var idsSet = new List<ElementId>() { new ElementId(702920) };
            var factory = new SolidElementIntersectionFactory(_doc, _elementMultiFilter)
            {
                Logger = Logger,
                TransactionFactory = TransactionFactory, 
                //ElementIdsSet = idsSet
            };
            factory.ExcludedElementIds.Add(mEPCurve.Id);
            factory.ExculdedTypes.Add(typeof(InsulationLiningBase));
            var intersectionElements = factory.GetIntersections(mEPCurve.Solid());



            intersectionElements.ForEach(e => Logger?.Information(e.Id.ToString()));
            Logger?.Information($"Total elements count is: {intersectionElements.Count()}.");

            return intersectionElements;
        }

        public IEnumerable<Element> GetElementsOld(MEPCurve mEPCurve)
        {
            var elements = new List<Element>();


            var typeFilter = new ElementIsElementTypeFilter(true);
            var types = new List<Type>()
            {
                typeof(FamilyInstance),
                typeof(HostObject)
            };
            var multiclassFilter = new ElementMulticlassFilter(types);
            var excludeCategoryfilter =
                    new ElementCategoryFilter(BuiltInCategory.OST_GenericAnnotation, true);
            var intersectsElementFilter = new ElementIntersectsElementFilter(mEPCurve);
            //var typesToExclude = new List<Type>()
            //{
            //    typeof(AnnotationSymbol),
            //};
            //var invertedMulticlassFilter = new ElementMulticlassFilter(typesToExclude, true);

            // Create a category filter for Doors

            //var totalBox = new BoundingBoxXYZ();
            //TransactionFactory.CreateAsync(() => totalBox.Show(_doc), "ShowBox");

            _elementMultiFilter.QuickFilters.Add((typeFilter, null));
            _elementMultiFilter.QuickFilters.Add((multiclassFilter, null));
            _elementMultiFilter.QuickFilters.Add((excludeCategoryfilter, null));
            _elementMultiFilter.SlowFilters.Add((intersectsElementFilter, null));
            var elementIdsDocs = _elementMultiFilter.ApplyToAllDocs();
            var elementIds = elementIdsDocs.SelectMany(ed => ed.Value);
            //var elementIds = _elementMultiFilter.ApplyToActiveDoc();
            _uiDoc.Selection.SetElementIds(elementIds.ToList());

            elementIds.ForEach(id => elements.Add(_doc.GetElement(id)));
            //var noGeomElements = elements.Where(e => !e.IsGeometryElement());
            //var stairs = noGeomElements.Where(e => e.Category.Name == "Stairs");
            //var windows = noGeomElements.Where(e => e.Category.Name == "Windows").ToList();
            //var Framing = noGeomElements.Where(e => e.Category.Name == "Structural Framing").ToList();
            //var wb = windows[0].get_BoundingBox(null);
            //foreach (var nGe in noGeomElements)
            //{ var b = nGe.IsGeometryElement(); }
            //Logger?.Information($"No geomety elements count is: {noGeomElements.Count()}.");
            //var noGeomNameTypes = noGeomElements.Select(e => e.Category.Name).Distinct();
            //Logger?.Information("NoGeomety elements categories: ");
            //noGeomNameTypes.ForEach(t => Logger?.Information(t));
            //elementIds.ForEach(id => Logger?.Information(_doc.GetElement(id).Id.ToString()));
            Logger?.Information($"Total elements count is: {elementIds.Count()}.");

            return elements;
        }
    }
}
