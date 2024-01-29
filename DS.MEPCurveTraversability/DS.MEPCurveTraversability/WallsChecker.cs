using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using DS.ClassLib.VarUtils.Filters;
using MoreLinq;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.MEP;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.MEPCurveTraversability
{
    internal class WallsChecker
    {
        private readonly UIDocument _uiDoc;
        private readonly IEnumerable<RevitLinkInstance> _links;
        private readonly ITIntersectionFactory<Element, Solid> _intersectionFactory;
        private readonly Document _doc;

        public WallsChecker(UIDocument uiDoc, IEnumerable<RevitLinkInstance> links, ITIntersectionFactory<Element, Solid> intersectionFactory)
        {
            _uiDoc = uiDoc;
            _links = links;
            _intersectionFactory = intersectionFactory;
            _doc = uiDoc.Document;
        }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// A factory to commit transactions.
        /// </summary>
        public ITransactionFactory TransactionFactory { get; set; }

        /// <summary>
        /// Window messenger to show important information to user.
        /// </summary>
        public IWindowMessenger WindowMessenger { get; set; }

        public bool Initiate(MEPCurve mEPCurve)
        {
            var mEPCurveSolid = mEPCurve.Solid(_links);

            //get collisions
            var collisions = new List<(Solid, Element)>();
            var interesections = _intersectionFactory.GetIntersections(mEPCurveSolid);
            var interesectionWalls = interesections.OfType<Wall>();
            interesectionWalls.ForEach(e => collisions.Add((mEPCurveSolid, e)));

            var filter = GetFilter(_doc, _links, mEPCurve, Logger, TransactionFactory);
            var isWallCollisions = collisions.SkipWhile(filter).Any(c => c.Item2 is Wall);
            if (isWallCollisions)
            {
                Logger?.Information($"The walls are not traversable.");
                WindowMessenger?.Show($"Некорректный проход через стены.");
                return false;
            }

            return true;
        }

        private Func<(Solid, Element), bool> GetFilter(
            Document doc,
            IEnumerable<RevitLinkInstance> links,
            MEPCurve mEPCurve,
            ILogger logger,
            ITransactionFactory trf)
        {
            var dir = mEPCurve.Direction().ToVector3d();
            var mEPCurveSolid = mEPCurve.GetSolidWithInsulation();

            var rools = new List<Func<(Solid, Element), bool>>
            {
                SolidElementRulesFilterSet.WallTraversableDirectionRule(doc, links, dir),
                SolidElementRulesFilterSet.WallOpeningTraversableRule(doc, links, mEPCurveSolid, logger, trf),
                //SolidElementRulesFilterSet.WallConstructionRule(doc)
            };
            return new RulesFilterFactory<Solid, Element>(rools).GetFilter();
        }


    }
}
