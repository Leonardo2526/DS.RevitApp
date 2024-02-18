using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Collisons;
using MoreLinq;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Filtering;
using OLMP.RevitAPI.Tools.Filtering.Intersections;
using OLMP.RevitAPI.Tools.Filtering.Intersections.Builders;
using OLMP.RevitAPI.Tools.Various;
using Rhino;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using UnitSystem = Rhino.UnitSystem;

namespace DS.FindCollisions
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        private ILogger _logger;
        private static readonly double _cmToFeet =
      RhinoMath.UnitScale(UnitSystem.Centimeters, UnitSystem.Feet);
        private static readonly double _mmToFeet =
        RhinoMath.UnitScale(UnitSystem.Millimeters, UnitSystem.Feet);

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


            var docsToApply1 = new List<Document>();
            docsToApply1.Add(activeDoc);
            //docsToApply1.AddRange(allLinkDocs);

            //specify doc2
            var docsToApply2 = new List<Document>();
            docsToApply2.Add(activeDoc);
            docsToApply2.AddRange(allLinkDocs);

            var minVolume = 0 * _cmToFeet; 
            var intersectionFactory = new SolidElementIntersectionFactoryBuilder(
                docsToApply1,
                docsToApply2,
                activeDoc,
                allLoadedLinks)
            {
                Logger = _logger,
                IncudeTypes1 = new List<Type>()
                {
                    typeof(MEPCurve),
                    //typeof(FamilyInstance),
                    //typeof(HostObject),
                },
                IncudeTypes2 = new List<Type>()
                {
                    typeof(FamilyInstance),
                    typeof(HostObject),
                },
                IntersectionRules = new()
                {
                    (e1, e2) =>CollisionUtils.
                    GetBestIntersectionSolid(e1, e2, allLoadedLinks,
                        out Solid elem1Solid, out Solid elem2Solid, minVolume) != null,

                    (e1, e2) => !e1.IsConnected(e2, true),

                    (e1, e2) =>
                    !ElementFilterUtils.HasFreeConnectorIntersections(e1, e2, allLoadedLinks) &
                    !ElementFilterUtils.HasFreeConnectorIntersections(e2, e1, allLoadedLinks)

                }
            }
                .Create();


            var collisions = intersectionFactory.GetIntersections();
            PrintCollsions(collisions, _logger);
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