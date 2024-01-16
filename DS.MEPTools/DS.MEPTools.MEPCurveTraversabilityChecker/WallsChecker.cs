using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Filters;
using DS.MEPTools.OpeningsCreator;
using DS.RevitLib.Utils.Collisions;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Various;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP;
using DS.MEPTools.Core;
using System.Security.Cryptography;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using MoreLinq;

namespace DS.MEPTools.WallTraversabilityChecker
{
    internal class WallsChecker
    {
        private readonly UIDocument _uiDoc;
        private readonly ITIntersectionFactory<Element, Solid> _intersectionFactory;
        private readonly Document _doc;

        public WallsChecker(UIDocument uiDoc, ITIntersectionFactory<Element, Solid> intersectionFactory)
        {
            _uiDoc = uiDoc;
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
            var mEPCurveSolid = mEPCurve.GetSolidInLink(_doc);

            //get collisions
            var collisions = new List<(Solid, Element)>();
            var interesections = _intersectionFactory.GetIntersections(mEPCurveSolid);
            var interesectionWalls = interesections.OfType<Wall>();
            interesectionWalls.ForEach(e => collisions.Add((mEPCurveSolid, e)));

            var filter = GetFilter(_doc, mEPCurve, Logger, TransactionFactory);
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
            MEPCurve mEPCurve,
            ILogger logger,
            ITransactionFactory trf)
        {
            var dir = mEPCurve.Direction().ToVector3d();
            var mEPCurveSolid = mEPCurve.GetSolidWithInsulation();

            var rools = new List<Func<(Solid, Element), bool>>
            {
                SolidElementRulesFilterSet.WallTraversableDirectionRule(doc, dir),
                SolidElementRulesFilterSet.WallOpeningTraversableRule(doc, mEPCurveSolid, logger, trf),
                //SolidElementRulesFilterSet.WallConstructionRule(doc)
            };
            return new RulesFilterFactory<Solid, Element>(rools).GetFilter();
        }


    }
}
