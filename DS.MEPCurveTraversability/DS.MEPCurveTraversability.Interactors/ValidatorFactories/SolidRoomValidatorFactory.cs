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

    public class SolidRoomValidatorFactory(
        Document activeDoc,
        IEnumerable<RevitLinkInstance> links,
        IElementMultiFilter globalFilter,
        IElementMultiFilter localFilter
            ) : IValidatorFactory<MEPCurve>
    {
        private readonly Document _doc = activeDoc;
        private readonly IEnumerable<RevitLinkInstance> _links = links;
        private readonly IElementMultiFilter _globalFilter = globalFilter;
        private readonly IElementMultiFilter _localFilter = localFilter;

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

        /// <inheritdoc/>
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
