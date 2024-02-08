using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Collisons;
using MoreLinq;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Filtering;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.FindCollisions
{
    [Transaction(TransactionMode.Manual)]
    public class FilterExternalCommand : IExternalCommand
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
            var appContainer = AppSettings.AppContainer;
            var trf = new ContextTransactionFactory(activeDoc, RevitContextOption.Inside);

            var docsToApply = new List<Document>();
            //docsToApply.Add(activeDoc);
            docsToApply.AddRange(allLinkDocs);
            var elementFilter = OLMP.RevitAPI.Tools.Filtering.ElementFilterUtils
                .CreateHostInstFilter(docsToApply, activeDoc, allLoadedLinks);
            var filteredElements = elementFilter
                .Apply()
                .FilteredElements;
            var elements2 = filteredElements.SelectMany(kv => kv.Value.ToElements(kv.Key));

            var typeFilter1 = 
                new ElementMulticlassFilter(new List<Type>() { typeof(MEPCurve) });
            var f1 = elementFilter.Clone().With(typeFilter1).Apply();
            var mEPCurvesIds = f1.FilteredElements;
            var mEPCurves = mEPCurvesIds.SelectMany(kv => kv.Value.ToElements(kv.Key));

            var typeFilter2 =
              new ElementMulticlassFilter(new List<Type>() { typeof(FamilyInstance) });
            var f2 = elementFilter.Clone().With(typeFilter2).Apply();
            var filteredFamInst = f2.FilteredElements;
            var famInst = filteredFamInst.SelectMany(kv => kv.Value.ToElements(kv.Key));

            var ids = mEPCurvesIds.SelectMany(e => e.Value).ToList();
            uiDoc.Selection.SetElementIds(ids);

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