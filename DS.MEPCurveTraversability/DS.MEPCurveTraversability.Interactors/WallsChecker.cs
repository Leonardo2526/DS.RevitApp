using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using DS.ClassLib.VarUtils.Filters;
using DS.MEPCurveTraversability.Core;
using MoreLinq;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.MEP;
using Rhino;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;

namespace DS.MEPCurveTraversability.Interactors
{
    internal class WallsChecker : IValidator<MEPCurve>
    {
        private readonly UIDocument _uiDoc;
        private readonly IEnumerable<RevitLinkInstance> _links;
        private readonly ITIntersectionFactory<Element, Solid> _intersectionFactory;
        private readonly IWallIntersectionSettings _intersectionSettings;
        private readonly Document _doc;
        private readonly List<ValidationResult> _validationResults = new();

        public WallsChecker(
            UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> links,
            ITIntersectionFactory<Element, Solid> intersectionFactory, 
            IWallIntersectionSettings wallIntersectionSettings)
        {
            _uiDoc = uiDoc;
            _links = links;
            _intersectionFactory = intersectionFactory;
            _intersectionSettings = wallIntersectionSettings;
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

        public IEnumerable<ValidationResult> ValidationResults => _validationResults;
     

        public bool IsValid(MEPCurve item)
        {
            _validationResults.Clear();
            var mEPCurveSolid = item.Solid(_links);

            //get collisions
            var collisions = new List<(Solid, Element)>();
            var interesections = _intersectionFactory.GetIntersections(mEPCurveSolid);
            var interesectionWalls = interesections.OfType<Wall>();
            interesectionWalls.ForEach(e => collisions.Add((mEPCurveSolid, e)));

            var filter = GetFilter(_doc, _links, item, _intersectionSettings);
            var isWallCollisions = collisions.SkipWhile(filter).Any(c => c.Item2 is Wall);
            if (isWallCollisions)
            {
                var message = $"Некорректный проход через стены.";
                _validationResults.Add(new ValidationResult(message));
                Logger?.Information($"The walls are not traversable.");
                WindowMessenger?.Show(message);
                return false;
            }

            return true;
        }

        private Func<(Solid, Element), bool> GetFilter(
            Document doc,
            IEnumerable<RevitLinkInstance> links,
            MEPCurve mEPCurve,
             IWallIntersectionSettings intersectionSettings)
        {
            var dir = mEPCurve.Direction().ToVector3d();
            var mEPCurveSolid = mEPCurve.GetSolidWithInsulation();

            var wallRuleBuilder = new WallRuleBuilder(doc, links, intersectionSettings);
            var rools = new List<Func<(Solid, Element), bool>>();

            if(intersectionSettings.NormalAngleLimit < RhinoMath.ToRadians(90))
            { rools.Add((f) => wallRuleBuilder.IsTraversableDirection((f.Item1, f.Item2), dir)); }

            if (intersectionSettings.CheckOpenings)
            { rools.Add((f) => wallRuleBuilder.IsOpeningTraversable(f.Item1, f.Item2)); }


            return new RulesFilterFactory<Solid, Element>(rools).GetFilter();
        }


    }
}
