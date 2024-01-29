using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Collisions;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Intersections;
using OLMP.RevitAPI.Tools.Openings;
using OLMP.RevitAPI.Tools.OpeningsCreator;
using Rhino;
using Rhino.Geometry;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DS.MEPCurveTraversability.Interactors
{
    public class WallIntersectionValidatorFactory : IValidatorFactory<MEPCurve>
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly IEnumerable<RevitLinkInstance> _allLoadedLinks;
        private readonly IElementMultiFilter _localFilter;
        private readonly IWallIntersectionSettings _wallIntersectionSettings;

        public WallIntersectionValidatorFactory(
            UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> allLoadedLinks,
            IElementMultiFilter localFilter,
            IWallIntersectionSettings wallIntersectionSettings)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _allLoadedLinks = allLoadedLinks;
            _localFilter = localFilter;
            _wallIntersectionSettings = wallIntersectionSettings;
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

        public IValidator<MEPCurve> GetValidator()
        {
            var solidIntersectionFactory = new SolidElementIntersectionFactory(_doc, _localFilter)
            { Logger = Logger, TransactionFactory = TransactionFactory };

            //build filters
            Func<Wall, Vector3d, bool> directionFilter = null;
            if (_wallIntersectionSettings.NormalAngleLimit < RhinoMath.ToRadians(90))
            {
                directionFilter = (wall, dir) =>
                wall.IsTraversable(dir,
                _doc,
                _allLoadedLinks,
                _wallIntersectionSettings.NormalAngleLimit);
            }

            Func<Solid, Wall, bool> openingFilter = null;
            if (_wallIntersectionSettings.CheckOpenings)
            {
                openingFilter = (wall, solid) =>
                IsOpeningTraversable(
                    _doc,
                    _allLoadedLinks,
                    wall,
                    solid,
                    _wallIntersectionSettings,
                    TransactionFactory,
                    Logger);
            }

            return new WallIntersectionValidator(
              _uiDoc,
              _allLoadedLinks,
              solidIntersectionFactory)
            {
                DirectionFilter = directionFilter,
                OpeningFilter = openingFilter,
                Logger = Logger,
                TransactionFactory = TransactionFactory,
                WindowMessenger = WindowMessenger
            };
        }

        private bool IsOpeningTraversable(
            Document activeDoc,
            IEnumerable<RevitLinkInstance> links,
              Solid solid,
              Wall wall,
              IWallIntersectionSettings wallIntersectionSettings, 
              ITransactionFactory transactionFactory, 
              ILogger logger)
        {
            var profileCreator = new RectangleSolidProfileCreator(activeDoc, links)
            {
                Offset = wallIntersectionSettings.OpeningOffset,
                TransactionFactory = transactionFactory,
                Logger = logger
            };
            var wBuilder = new WallOpeningProfileValidator<Solid>(activeDoc, links, profileCreator)
            {
                InsertsOffset = wallIntersectionSettings.InsertsOffset,
                WallOffset = wallIntersectionSettings.WallOffset,
                JointsOffset = wallIntersectionSettings.JointsOffset,
                Logger = logger,
                TransactionFactory = transactionFactory
            };

            var mArg = (wall as Wall, solid);

            var b = wBuilder.IsValid(mArg);
            if (!b)
            {
                var sb = new StringBuilder();
                wBuilder.ValidationResults.ToList().ForEach(r => sb.Append(r.ErrorMessage));
                logger?.Information(sb.ToString());
            }

            return b;
        }

    }
}
