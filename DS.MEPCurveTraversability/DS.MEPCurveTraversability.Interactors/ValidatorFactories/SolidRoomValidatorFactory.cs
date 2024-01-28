using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using DS.MEPCurveTraversability.Interactors.Settings;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Intersections;
using OLMP.RevitAPI.Tools.Rooms;
using OLMP.RevitAPI.Tools.Rooms.Traversability;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using OLMP.RevitAPI.Core.Extensions;

namespace DS.MEPCurveTraversability.Interactors.ValidatorFactories
{
    public class SolidRoomValidatorFactory : IValidatorFactory<MEPCurve>
    {
        private readonly UIDocument _uiDoc;
        private readonly IEnumerable<RevitLinkInstance> _links;
        private readonly IElementMultiFilter _elementMultiFilter;
        private readonly DocSettingsAR _docSettingsAR;
        private readonly Document _doc;

        public SolidRoomValidatorFactory(
            UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> links,
            IElementMultiFilter elementMultiFilter,
            DocSettingsAR docSettingsAR
            )
        {
            _uiDoc = uiDoc;
            _links = links;
            _elementMultiFilter = elementMultiFilter;
            _docSettingsAR = docSettingsAR;
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

        /// <summary>
        /// Fields to exclude from <see cref="Room"/>s.
        /// </summary>
        public IEnumerable<string> ExcludeFields { get; set; }

        /// <summary>
        /// Minimum volume to get intersections beteween <see cref="Solid"/>s.
        /// </summary>
        public double MinVolume { get; set; }

        public IValidator<MEPCurve> GetValidator()
        {
            //build solid element factory
            var settingsDoc = _docSettingsAR.Docs.FirstOrDefault(d => !d.IsLinked);
            var settingsLinks = _docSettingsAR.Docs.Select(d => d.TryGetLink(_links)).Where(l => l != null);
            var elementFilter = new ElementMutliFilter(_doc, _links);
            //var elementFilter = new ElementMutliFilter(settingsDoc, settingsLinks);
            var solidIntersectionFactory = new SolidElementIntersectionFactory(_doc, elementFilter)
            { Logger = Logger, TransactionFactory = TransactionFactory };

            var elementRoomFilter = new ElementMutliFilter(settingsDoc, settingsLinks);

            //build solid room factory
            var solidRoomIntersectFactory = new SolidRoomIntersectionFactory(_doc, _links, elementRoomFilter)
            { Logger = Logger, TransactionFactory = null };

            //get rooms
            elementRoomFilter.SlowFilters.Add((new RoomFilter(), null));
            var rooms = elementRoomFilter.ApplyToAllDocs().SelectMany(kv => kv.Value.ToElements(kv.Key)).OfType<Room>();

            var openingIntersectionFactory = new SolidOpeningIntersectionFactoty(_doc, _links, _elementMultiFilter)
            { MinVolume = MinVolume };

            return new SolidRoomTraversable(
                _doc,
                _links,
                rooms,
                solidRoomIntersectFactory,
                solidIntersectionFactory,
                openingIntersectionFactory)
            {
                Logger = Logger,
                ExcludeFields = ExcludeFields,
                WindowMessenger = WindowMessenger,
                MinVolume = MinVolume
            };
        }
    }
}
