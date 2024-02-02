using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using DS.MEPCurveTraversability.Interactors.Settings;
using DS.MEPCurveTraversability.Interactors.ValidatorFactories;
using OLMP.RevitAPI.Core.Extensions;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Intersections;
using Rhino.UI;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.MEPCurveTraversability.Interactors
{
    public class ValidatorFactory : List<IValidator>
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly IEnumerable<RevitLinkInstance> _allLoadedLinks;
        private readonly IDocumentFilter _globalFilter;
        private readonly DocSettingsAR _docSettingsAR;
        private readonly DocSettingsKR _docSettingsKR;
        private readonly IDocumentFilter _localARFilter;
        private readonly IDocumentFilter _localKRFilter;

        public ValidatorFactory(
            UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> allLoadedLinks,
            IDocumentFilter globalFilter,
            DocSettingsAR docSettingsAR,
            DocSettingsKR docSettingsKR)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _allLoadedLinks = allLoadedLinks;
            _globalFilter = globalFilter;
            _docSettingsAR = docSettingsAR;
            _docSettingsKR = docSettingsKR;
            _localARFilter = _docSettingsAR.GetLocalFilter(globalFilter);
            _localKRFilter = _docSettingsKR.GetLocalFilter(globalFilter);
        }



        #region Properties

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

        #endregion


        public ValidatorFactory Create()
        {
            WithARWallIntersectionValidator().
                WithPointRoomValidator().
                WithSolidRoomValidator().
                WithKRWallIntersectionValidator();
            return this;
        }

        public ValidatorFactory WithSolidRoomValidator()
        {
            if (_docSettingsAR.RoomTraversionSettings.CheckSolid)
            {
                Add(new SolidRoomValidatorFactory(
                     _doc,
                    _allLoadedLinks,
                    _globalFilter,
                    _localARFilter)
                {
                    ExcludeFields = _docSettingsAR.RoomTraversionSettings.ExcludeFields,
                    MinVolume = _docSettingsAR.RoomTraversionSettings.MinResidualVolume,
                    StrictFieldCompliance = _docSettingsAR.RoomTraversionSettings.StrictFieldCompliance,
                    WindowMessenger = WindowMessenger,
                    Logger = Logger,
                    TransactionFactory = null,
                }.GetValidator());
            }
            return this;
        }

        public ValidatorFactory WithARWallIntersectionValidator()
            => WithWallIntersectionValidator(_localARFilter,
                _docSettingsAR.WallIntersectionSettings);

        public ValidatorFactory WithKRWallIntersectionValidator()
            => WithWallIntersectionValidator(_localKRFilter,
                _docSettingsKR.WallIntersectionSettings);

        private ValidatorFactory WithWallIntersectionValidator(
            IDocumentFilter localFilter,
            IWallIntersectionSettings wallIntersectionSettings)
        {
            Add(new WallIntersectionValidatorFactory(
                                _uiDoc,
                                _allLoadedLinks,
                                localFilter,
                                wallIntersectionSettings)
            {
                WindowMessenger = WindowMessenger,
                Logger = Logger,
                TransactionFactory = null
            }.GetValidator());
            return this;
        }

        public ValidatorFactory WithPointRoomValidator(bool canBetweenRooms = false)
        {
            if (_docSettingsAR.RoomTraversionSettings.CheckEndPoints)
            {
                var pointIntersectionFactory = canBetweenRooms ? 
                    GetPointElementFactory(_doc, _allLoadedLinks, _localARFilter, Logger) : 
                    null;
                Add(new PointRoomValidatorFactory(
                    _uiDoc, _allLoadedLinks, _globalFilter, _localARFilter)
                {
                    ExcludeFields = _docSettingsAR.RoomTraversionSettings.ExcludeFields,
                    StrictFieldCompliance = _docSettingsAR.RoomTraversionSettings.StrictFieldCompliance,
                    PointIntersectionFactory = pointIntersectionFactory,
                    WindowMessenger = WindowMessenger,
                    Logger = Logger,
                    TransactionFactory = null,
                }.GetValidator());
            }
            return this;
        }

        public ITIntersectionFactory<Element, XYZ> GetPointElementFactory(
            Document doc,
            IEnumerable<RevitLinkInstance> allLoadedLinks,
            IDocumentFilter localFilter,
            ILogger logger)
        {
            var types = new List<Type>()
            {
                typeof(Wall),
                typeof(Opening),
                typeof(Floor),
                typeof(Ceiling)
            };
            var multiclassFilter = new ElementMulticlassFilter(types);

            var arFilter = localFilter.Clone();
            arFilter.QuickFilters ??= new();
            arFilter.QuickFilters.Add((multiclassFilter, null));
            var pointIntersectionFactory =
            new ElementPointIntersectionFactory(doc, allLoadedLinks, localFilter)
            {
                Logger = logger,
            };

            return pointIntersectionFactory;
        }
    }


}
