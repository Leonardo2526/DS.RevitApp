using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Filters;
using DS.MEPTools.OpeningsCreator;
using DS.RevitLib.Utils.Collisions;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Various;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP;
using DS.MEPTools.Core;
using System.Security.Cryptography;
using Autodesk.Revit.DB.Architecture;
using DS.MEPTools.Core.Rooms.Traversability;
using DS.MEPTools.Core.Rooms;
using Rhino.UI;
using Serilog.Core;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;

namespace DS.MEPTools.WallTraversabilityChecker
{
    internal class RoomChecker
    {
        private readonly UIDocument _uiDoc;
        private readonly IElementMultiFilter _elementMultiFilter;
        private readonly ITIntersectionFactory<Element, Solid> _elementIntersectFactory;
        private readonly SolidElementIntersectionFactoryBase<Room> _roomIntersectFactory;
        private readonly Document _doc;

        public RoomChecker(
            UIDocument uiDoc, 
            IElementMultiFilter elementMultiFilter, 
            ITIntersectionFactory<Element, Solid> elementIntersectFactory,
            SolidElementIntersectionFactoryBase<Room> roomIntersectFactory
            )
        {
            _uiDoc = uiDoc;
            _elementMultiFilter = elementMultiFilter;
            _elementIntersectFactory = elementIntersectFactory;
            _roomIntersectFactory = roomIntersectFactory;
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
            var elementMultiFilter = new ElementMutliFilter(_doc);
            var excludeFields = new List<string>()
            {
                "электр",
                //"2"
            };

            //get rooms
            _elementMultiFilter.SlowFilters.Add((new RoomFilter(), null));
            var rooms = _elementMultiFilter.ApplyToAllDocs().SelectMany(kv => kv.Value.ToElements(kv.Key)).OfType<Room>();

         

            IRoomTraverable<Solid> roomSolidFactory(IEnumerable<Room> rooms)
                => new SolidRoomTraversable(_doc, rooms, _roomIntersectFactory, _elementIntersectFactory)
                {
                    Logger = Logger,
                    ExcludeFields = excludeFields,
                    WindowMessenger = WindowMessenger
                };

            IRoomTraverable<XYZ> roomPointFactory(IEnumerable<Room> rooms)
                => new PointRoomTraversable(_doc, rooms)
                {
                    Logger = Logger,
                    ExcludeFields = excludeFields,
                    WindowMessenger = WindowMessenger
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
