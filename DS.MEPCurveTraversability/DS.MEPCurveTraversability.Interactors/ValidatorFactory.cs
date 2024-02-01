using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using DS.MEPCurveTraversability.Interactors.Settings;
using DS.MEPCurveTraversability.Interactors.ValidatorFactories;
using OLMP.RevitAPI.Core.Extensions;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using Serilog;
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
        private readonly ITIntersectionFactory<Element, XYZ> _pointIntersectionFactory;
        private readonly IDocumentFilter _localARFilter;
        private readonly IDocumentFilter _localKRFilter;

        public ValidatorFactory(
            UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> allLoadedLinks,
            IDocumentFilter globalFilter,
            DocSettingsAR docSettingsAR,
            DocSettingsKR docSettingsKR,
             ITIntersectionFactory<Element, XYZ> pointIntersectionFactory)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _allLoadedLinks = allLoadedLinks;
            _globalFilter = globalFilter;
            _docSettingsAR = docSettingsAR;
            _docSettingsKR = docSettingsKR;
            _pointIntersectionFactory = pointIntersectionFactory;
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
            var arValidators = new List<IValidator>
            {
                new WallIntersectionValidatorFactory(
                    _uiDoc,
                    _allLoadedLinks,
                    _localARFilter,
                    _docSettingsAR.WallIntersectionSettings)
                { 
                    WindowMessenger = WindowMessenger,
                    Logger = Logger,
                    TransactionFactory = null
                }.GetValidator()
            };

            if (_docSettingsAR.RoomTraversionSettings.CheckEndPoints)
            {              
                arValidators.Add(new PointRoomValidatorFactory(
                    _uiDoc, _allLoadedLinks, _globalFilter, _localARFilter, _pointIntersectionFactory)
                {                   
                    ExcludeFields = _docSettingsAR.RoomTraversionSettings.ExcludeFields,
                    StrictFieldCompliance = _docSettingsAR.RoomTraversionSettings.StrictFieldCompliance,
                    WindowMessenger = WindowMessenger,
                    Logger = Logger,
                    TransactionFactory = null,
                }.GetValidator());
            }

            if (_docSettingsAR.RoomTraversionSettings.CheckSolid)
            {
                arValidators.Add(new SolidRoomValidatorFactory(
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
            AddRange(arValidators);

            Add(
            new WallIntersectionValidatorFactory(
              _uiDoc,
              _allLoadedLinks,
              _localKRFilter,
              _docSettingsKR.WallIntersectionSettings)
            {              
                WindowMessenger = WindowMessenger,
                Logger = Logger,
                TransactionFactory = null
            }.GetValidator()
                );

            return this;
        }
    }


}
