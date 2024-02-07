﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Collisons;
using MoreLinq;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Filtering.Intersections;
using OLMP.RevitAPI.Tools.Various;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DS.FindCollisions
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        private ILogger _logger;

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

            _logger = AppSettings.Logger;
            var messenger = AppSettings.Messenger;
            var appContainer = AppSettings.AppContainer;
            var trf = new ContextTransactionFactory(activeDoc, RevitContextOption.Inside);

            //specify doc1
            var docsToApply1 = new List<Document>();
            docsToApply1.Add(activeDoc);
            //docsToApply1.AddRange(allLinkDocs);
            var elementFilter1 = OLMP.RevitAPI.Tools.Filtering.ElementFilterUtils
              .CreateMEPCurveFilter(docsToApply1, activeDoc, allLoadedLinks);
            var filteredElements1 = elementFilter1
                .Apply()
                .FilteredElements;
            var docElements1 = filteredElements1
                .SelectMany(kv => kv.Value.ToElements(kv.Key));


            //specify doc2
            var docsToApply2 = new List<Document>();
            docsToApply2.Add(activeDoc);
            //docsToApply2.AddRange(allLinkDocs);
            var elementFilter2 = OLMP.RevitAPI.Tools.Filtering.ElementFilterUtils
            .CreateMEPCurveFilter(docsToApply2, activeDoc, allLoadedLinks);
            var filteredElements2 = elementFilter2
                .Apply()
                .FilteredElements;
            var docElements2 = filteredElements2
                .SelectMany(kv => kv.Value.ToElements(kv.Key));
           
            //find intersections
           var intersectionFactory = 
                new BestDocIntersectionFactory(activeDoc, allLoadedLinks, filteredElements1)
            { 
                    Logger = _logger 
                };
            intersectionFactory.NotifyOnSetProcessName += IntersectionFactory_NotifyOnSetProcessName;
            intersectionFactory.NotifyOnProcessUpdate += IntersectionFactory_NotifyOnProcessUpdate;
            //return Autodesk.Revit.UI.Result.Succeeded;

            var collisions = intersectionFactory.GetIntersections(filteredElements2);
            PrintCollsions(collisions, _logger);
            ShowCollisions(uiDoc, collisions);

            return Autodesk.Revit.UI.Result.Succeeded;
        }

        private void IntersectionFactory_NotifyOnSetProcessName(string message)
        {
            _logger?.Information(message);
        }

        private void IntersectionFactory_NotifyOnProcessUpdate(string message, int i, int count)
        {
            _logger?.Information(message);
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