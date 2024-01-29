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
        private readonly IElementMultiFilter _globalFilter;
        private readonly IElementMultiFilter _localFilter;
        private readonly DocSettingsAR _docSettingsAR;
        private readonly Document _doc;

        public SolidRoomValidatorFactory(
            UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> links,
            IElementMultiFilter globalFilter,
            IElementMultiFilter localFilter,
            DocSettingsAR docSettingsAR
            )
        {
            _uiDoc = uiDoc;
            _links = links;
            _globalFilter = globalFilter;
            _localFilter = localFilter;
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

        /// <summary>
        /// Specifies if room names should conatain content fields fully or not.
        /// </summary>
        public bool StrictFieldCompliance { get; set; }

        public IValidator<MEPCurve> GetValidator()
        {
            //get rooms
            _localFilter.Reset();           
            _localFilter.SlowFilters.Add((new RoomFilter(), null));
            var rooms = _localFilter.ApplyToAllDocs().SelectMany(kv => kv.Value.ToElements(kv.Key)).OfType<Room>();

            //build solid room factory
            var solidRoomIntersectFactory = new SolidRoomIntersectionFactory(_doc, _links, _localFilter)
            { Logger = Logger, TransactionFactory = null };

            //build solid element factory
            var elementItersectionFactory = new SolidElementIntersectionFactory(_doc, _globalFilter)
            { Logger = Logger, TransactionFactory = TransactionFactory };

            var openingIntersectionFactory = new SolidOpeningIntersectionFactoty(_doc, _links, _globalFilter)
            { MinVolume = MinVolume };

            return new SolidRoomTraversable(
                _doc,
                _links,
                rooms,
                solidRoomIntersectFactory,
                elementItersectionFactory,
                openingIntersectionFactory)
            {
                Logger = Logger,
                ExcludeFields = ExcludeFields,
                WindowMessenger = WindowMessenger, 
                StrictFieldCompliance = StrictFieldCompliance,
                MinVolume = MinVolume
            };
        }
    }
}
