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

namespace DS.MEPTools.WallTraversabilityChecker
{
    internal class WallsChecker
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;

        public WallsChecker(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
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

        public void Initiate()
        {
            if (new ElementSelector(_uiDoc).Pick() is not MEPCurve mEPCurve) { return; }
            if (new ElementSelector(_uiDoc).Pick() is not Wall wall) { return; }

            var collisions = new List<(Solid, Element)>();
            var collision = (mEPCurve.Solid(), wall);
            collisions.Add(collision);

            //if (_checkTraverseDirection)
            //{
            //    var dir = mEPCurve.Direction().ToVector3d();
            //    var rools = new List<Func<(Solid, Element), bool>>
            //    {SolidElementRulesFilterSet.WallTraversableDirectionRule(dir)};
            //    Func<(Solid, Element), bool> ruleCollisionFilter = new RulesFilterFactory<Solid, Element>(rools).GetFilter();
            //    collisions = collisions.Where(ruleCollisionFilter).ToList();
            //}

            var filter = GetFilter(_doc, mEPCurve, wall, Logger, TransactionFactory);
            collisions = collisions.SkipWhile(filter).ToList();

            if (collisions.Count == 0)
            {
                Logger?.Information($"The wall is traversable.");
            }
            else
            {
                Logger?.Information($"The wall is not traversable.");
            }
        }

        private Func<(Solid, Element), bool> GetFilter(
            Document doc,
            MEPCurve mEPCurve,
            Wall wall,
            ILogger logger,
            ITransactionFactory trf)
        {
            var dir = mEPCurve.Direction().ToVector3d();
            var mEPCurveSolid = mEPCurve.GetSolidWithInsulation();

            var rools = new List<Func<(Solid, Element), bool>>
            {
                SolidElementRulesFilterSet.WallTraversableDirectionRule(doc, dir),
                SolidElementRulesFilterSet.WallOpeningTraversableRule(doc, mEPCurveSolid, wall, logger, trf),
                //SolidElementRulesFilterSet.WallConstructionRule(doc)
            };
            return new RulesFilterFactory<Solid, Element>(rools).GetFilter();
        }


    }
}
