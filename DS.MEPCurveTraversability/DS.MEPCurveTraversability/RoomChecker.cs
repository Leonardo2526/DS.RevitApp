using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Intersections;
using OLMP.RevitAPI.Tools.Rooms.Traversability;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace DS.MEPCurveTraversability
{
    internal class RoomChecker
    {
        private readonly UIDocument _uiDoc;
        private readonly IEnumerable<RevitLinkInstance> _links;
        private readonly IElementMultiFilter _elementMultiFilter;
        private readonly ITIntersectionFactory<Element, Solid> _elementIntersectFactory;
        private readonly SolidElementIntersectionFactoryBase<Room> _solidElementIntersectFactory;
        private readonly Document _doc;

        public RoomChecker(
            UIDocument uiDoc, IEnumerable<RevitLinkInstance> links,
            IElementMultiFilter elementMultiFilter,
            ITIntersectionFactory<Element, Solid> elementIntersectFactory,
            SolidElementIntersectionFactoryBase<Room> solidElementIntersectFactory
            )
        {
            _uiDoc = uiDoc;
            _links = links;
            _elementMultiFilter = elementMultiFilter;
            _elementIntersectFactory = elementIntersectFactory;
            _solidElementIntersectFactory = solidElementIntersectFactory;
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

        public bool Initiate(MEPCurve mEPCurve)
        {
            var minIntersectionVolume = 3.CubicCMToFeet();


            var elementMultiFilter = new ElementMutliFilter(_doc);
            var excludeFields = new List<string>()
            {
                "электр",
                //"2"
            };

            //get rooms
            _elementMultiFilter.SlowFilters.Add((new RoomFilter(), null));
            var rooms = _elementMultiFilter.ApplyToAllDocs().SelectMany(kv => kv.Value.ToElements(kv.Key)).OfType<Room>();
            var pointIntersectionFactory = new ElementPointIntersectionFactory(_doc, _links, elementMultiFilter)
            {
                Logger = Logger
            }.SetExcludedElementIds(new List<ElementId>() { mEPCurve.Id });

            var openingIntersectionFactory = new SolidOpeningIntersectionFactoty(_doc, _links, elementMultiFilter)
            { MinVolume = minIntersectionVolume };

            IRoomTraverable<Solid> roomSolidFactory(IEnumerable<Room> rooms)
                => new SolidRoomTraversable(_doc, _links, rooms, _solidElementIntersectFactory, _elementIntersectFactory, openingIntersectionFactory)
                {
                    Logger = Logger,
                    ExcludeFields = excludeFields,
                    WindowMessenger = WindowMessenger,
                    MinVolume = minIntersectionVolume
                };

            IRoomTraverable<XYZ> roomPointFactory(IEnumerable<Room> rooms)
                => new PointRoomTraversable(_doc, _links, rooms)
                {
                    Logger = Logger,
                    ExcludeFields = excludeFields,
                    WindowMessenger = WindowMessenger,
                    PointIntersectionFactory = pointIntersectionFactory,
                };


            return new MEPCurveRoomTraversable(_doc, roomPointFactory, roomSolidFactory, rooms)
            {
                Logger = Logger,
                TransactionFactory = TransactionFactory,
                IsSolidTraversabilityEnabled = true,
                WindowMessenger = null
            }.IsTraversable(mEPCurve);
        }

    }
}
