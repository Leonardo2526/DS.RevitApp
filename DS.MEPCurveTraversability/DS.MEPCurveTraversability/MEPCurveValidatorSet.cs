using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.MEPCurveTraversability.Interactors;
using DS.MEPCurveTraversability.Interactors.Settings;
using DS.MEPCurveTraversability.Interactors.ValidatorFactories;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using Serilog;
using System.Collections.Generic;

namespace DS.MEPCurveTraversability
{
    public class MEPCurveValidatorSet : List<IValidator<MEPCurve>>
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly IEnumerable<RevitLinkInstance> _allLoadedLinks;
        private readonly MEPCurve _mEPCurve;
        private readonly IElementMultiFilter _elementMultiFilter;
        private readonly DocSettingsAR _docSettingsAR;
        private readonly DocSettingsKR _docSettingsKR;

        public MEPCurveValidatorSet(
            UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> allLoadedLinks,
            MEPCurve mEPCurve,
            IElementMultiFilter elementMultiFilter,
            DocSettingsAR docSettingsAR,
            DocSettingsKR docSettingsKR)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _allLoadedLinks = allLoadedLinks;
            _mEPCurve = mEPCurve;
            _elementMultiFilter = elementMultiFilter;
            _docSettingsAR = docSettingsAR;
            _docSettingsKR = docSettingsKR;
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


        public MEPCurveValidatorSet Create()
        {
            var arValidators = new List<IValidator<MEPCurve>>
            {
                new WallIntersectionValidatorFactory(
                    _uiDoc,
                    _allLoadedLinks,
                    _elementMultiFilter,
                    _docSettingsAR.Docs,
                    _docSettingsAR.WallIntersectionSettings)
                {
                    WindowMessenger = WindowMessenger,
                    Logger = Logger,
                    TransactionFactory = null
                }.GetValidator()
            };

            if (_docSettingsAR.RoomTraversionSettings.CheckEndPoints)
            {
                arValidators.Add(new PointRoomValidatorFactory(_uiDoc, _allLoadedLinks, _elementMultiFilter)
                {
                    ExcludedIds = new List<ElementId>() { _mEPCurve.Id },
                    ExcludeFields = _docSettingsAR.RoomTraversionSettings.ExcludeFields,
                    WindowMessenger = WindowMessenger,
                    Logger = Logger,
                    TransactionFactory = null,
                }.GetValidator());
            }

            if (_docSettingsAR.RoomTraversionSettings.CheckSolid)
            {
                arValidators.Add(new SolidRoomValidatorFactory(
                    _uiDoc,
                    _allLoadedLinks,
                    _elementMultiFilter,
                    _docSettingsAR)
                {
                    ExcludeFields = _docSettingsAR.RoomTraversionSettings.ExcludeFields,
                    MinVolume = _docSettingsAR.RoomTraversionSettings.MinResidualVolume,
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
              _elementMultiFilter,
              _docSettingsKR.Docs,
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
