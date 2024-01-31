using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
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
        private readonly IElementMultiFilter _globalFilter;
        private readonly DocSettingsAR _docSettingsAR;
        private readonly DocSettingsKR _docSettingsKR;
        private readonly IElementMultiFilter _localARFilter;
        private readonly IElementMultiFilter _localKRFilter;

        public ValidatorFactory(
            UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> allLoadedLinks,
            IElementMultiFilter globalFilter,
            DocSettingsAR docSettingsAR,
            DocSettingsKR docSettingsKR)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _allLoadedLinks = allLoadedLinks;
            _globalFilter = globalFilter;
            _docSettingsAR = docSettingsAR;
            _docSettingsKR = docSettingsKR;
            _localARFilter = GetLocalFilter(allLoadedLinks, _docSettingsAR.Docs);
            _localKRFilter = GetLocalFilter(allLoadedLinks, _docSettingsKR.Docs);
        }



        #region Properties

        /// <summary>
        /// Ids to exclude from intersections.
        /// </summary>
        public List<ElementId> ExcludedElementIds { get; set; }

        /// <summary>
        /// Types to exclude from intersections.
        /// </summary>
        public List<Type> ExculdedTypes { get; set; }

        /// <summary>
        /// Ids to exclude from intersections.
        /// </summary>
        public List<BuiltInCategory> ExcludedCategories { get; set; }

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
                    ExcludedElementIds = ExcludedElementIds, 
                    ExculdedTypes = ExculdedTypes, 
                    ExcludedCategories = ExcludedCategories,
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
                    ExcludedElementIds = ExcludedElementIds,
                    ExculdedTypes = ExculdedTypes,
                    ExcludedCategories = ExcludedCategories,
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
                    ExcludedElementIds = ExcludedElementIds,
                    ExculdedTypes = ExculdedTypes,
                    ExcludedCategories = ExcludedCategories,
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
                ExcludedElementIds = ExcludedElementIds,
                ExculdedTypes = ExculdedTypes,
                ExcludedCategories = ExcludedCategories,
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
