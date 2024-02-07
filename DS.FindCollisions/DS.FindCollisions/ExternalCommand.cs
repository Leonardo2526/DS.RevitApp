using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Collisons;
using MoreLinq;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.FindCollisions
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;

            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document activeDoc = uiApp.ActiveUIDocument.Document;

            var allLoadedLinks = activeDoc.GetLoadedLinks() ?? new List<RevitLinkInstance>();
            var allFilteredDocs = new List<Document>() { activeDoc };
            var allLinkDocs = allLoadedLinks.Select(l => l.GetLinkDocument());
            allFilteredDocs.AddRange(allLinkDocs);

            var logger = AppSettings.Logger;
            var messenger = AppSettings.Messenger;
            //var appContainer = AppSettings.AppContainer;
            var trf = new ContextTransactionFactory(activeDoc, RevitContextOption.Inside);

            //create global filter
            var excludedCategories = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_TitleBlocks,
                BuiltInCategory.OST_GenericAnnotation,
                BuiltInCategory.OST_TelephoneDevices,
                BuiltInCategory.OST_Materials,
                BuiltInCategory.OST_GenericModel,
                BuiltInCategory.OST_Massing
            };
            var types = new List<Type>()
            {
                //typeof(MEPCurve),
                typeof(FamilyInstance),
                typeof(HostObject)
            };
            var multiclassFilter = new ElementMulticlassFilter(types);

            var exculdedTypes = new List<Type>()
            {
                typeof(InsulationLiningBase)
            };
            var excludedMulticlassFilter = new ElementMulticlassFilter(exculdedTypes, true);

            var globalFilter = new DocumentFilter(allFilteredDocs, activeDoc, allLoadedLinks);
            globalFilter.QuickFilters =
            [
                (new ElementMulticategoryFilter(excludedCategories, true), null),
                (new ElementIsElementTypeFilter(true), null),
                (multiclassFilter, null),
                (excludedMulticlassFilter, null),
            ];

            var docFilter1 = globalFilter.Clone();
            docFilter1.Docs.Clear();
            docFilter1.Docs = new List<Document> { activeDoc };
            //docFilter1.Docs.AddRange(allLinkDocs);

            var docFilter2 = globalFilter.Clone();
            docFilter2.Docs.Clear();
            //docFilter2.Docs.AddRange(new List<Document>() { activeDoc });
            docFilter2.Docs.AddRange(allLinkDocs);
            //docFilter2.Docs.AddRange(allFilteredDocs);


            //var intersectionFilter = new DocumentFilter(docFilter2.Docs,
            // activeDoc,
            // allLoadedLinks)
            //{
            //    QuickFilters = [(new ElementIsElementTypeFilter(true), null)]
            //};
            //var intersectionFactory =
            //    new SimpleSolidElementIntersectionFactory(
            //        activeDoc,
            //        allLoadedLinks,
            //        intersectionFilter);

            var docIntersectionFactory =
            new DocIntersectionFactory(activeDoc, allLoadedLinks, docFilter2, null)
            { Logger = logger };
            //return Autodesk.Revit.UI.Result.Succeeded;

            var collisions = docIntersectionFactory.GetIntersections(docFilter1);           
            //PrintCollsions(collisions, logger);
            ShowCollisions(uiDoc, collisions);

            return Autodesk.Revit.UI.Result.Succeeded;
        }

        private static void ShowCollisions(UIDocument uiDoc, IEnumerable<(Element, Element)> collisions)
        {
            var intersectionElements = new List<Element>();
            collisions.ForEach(e =>
            {
                intersectionElements.Add(e.Item1);
                intersectionElements.Add(e.Item2);
            });
            var ids = intersectionElements.Select(e => e.Id).ToList();
            uiDoc.Selection.SetElementIds(ids);
        }

        private void PrintCollsions(IEnumerable<(Element, Element)> collisions, ILogger logger)
        {
            int i = 0;
            foreach (var collision in collisions)
            {
                i++;
                logger.Information($"Collision {i}: " +
                    $"{collision.Item1.Id}({collision.Item1.GetType().Name}) - " +
                    $"{collision.Item2.Id}({collision.Item2.GetType().Name}).");
            }
            logger?.Information($"Total collisions count is: {collisions.Count()}.");
        }
    }
}