using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.MEPCurveTraversability.Interactors;
using DS.MEPCurveTraversability.Interactors.Settings;
using DS.MEPCurveTraversability.Interactors.ValidatorFactories;
using OLMP.RevitAPI.Core.Extensions;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace DS.MEPCurveTraversability
{
    public class MEPCurveValidatorSet : List<IValidator<MEPCurve>>
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly IEnumerable<RevitLinkInstance> _allLoadedLinks;
        private readonly MEPCurve _mEPCurve;
        private readonly IElementMultiFilter _globalFilter;
        private readonly DocSettingsAR _docSettingsAR;
        private readonly DocSettingsKR _docSettingsKR;
        private readonly IElementMultiFilter _localARFilter;
        private readonly IElementMultiFilter _localKRFilter;

        public MEPCurveValidatorSet(
            UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> allLoadedLinks,
            MEPCurve mEPCurve,
            IElementMultiFilter globalFilter,
            DocSettingsAR docSettingsAR,
            DocSettingsKR docSettingsKR)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _allLoadedLinks = allLoadedLinks;
            _mEPCurve = mEPCurve;
            _globalFilter = globalFilter;
            _docSettingsAR = docSettingsAR;
            _docSettingsKR = docSettingsKR;
            _localARFilter = GetLocalFilter(allLoadedLinks, _docSettingsAR.Docs);
            _localKRFilter = GetLocalFilter(allLoadedLinks, _docSettingsKR.Docs);
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
                    _uiDoc, _allLoadedLinks, _globalFilter, _localARFilter)
                {
                    ExcludedIds = new List<ElementId>() { _mEPCurve.Id },
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
                    _uiDoc,
                    _allLoadedLinks,
                    _globalFilter, 
                    _localARFilter,
                    _docSettingsAR)
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

        private IElementMultiFilter GetLocalFilter(
            IEnumerable<RevitLinkInstance> allDocLinks,
            IEnumerable<Document> localDocs)
        {
            var settingsDoc = localDocs.FirstOrDefault(d => !d.IsLinked);
            var settingsLinks = localDocs.Select(d => d.TryGetLink(allDocLinks)).Where(l => l != null);
            return new ElementMutliFilter(settingsDoc, settingsLinks);
        }

    }


}
