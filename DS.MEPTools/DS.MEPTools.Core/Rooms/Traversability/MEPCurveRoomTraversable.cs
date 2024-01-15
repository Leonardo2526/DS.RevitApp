using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.MEPTools.Core.Rooms.Traversability
{
    /// <summary>
    /// The service to check <see cref="MEPCurve"/>s traversability through <see cref="Room"/>s.
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="roomPointTraversabilityFactory"></param>
    /// <param name="roomSolidTraversabilityFactory"></param>
    /// <param name="rooms"></param>
    public class MEPCurveRoomTraversable(Document doc,
        Func<IEnumerable<Room>, IRoomTraverable<XYZ>> roomPointTraversabilityFactory,
        Func<IEnumerable<Room>, IRoomTraverable<Solid>> roomSolidTraversabilityFactory,
        IEnumerable<Room> rooms) : IRoomTraverable<MEPCurve>
    {

        private readonly Document _doc = doc;
        private readonly Func<IEnumerable<Room>, IRoomTraverable<XYZ>> _roomPointTraversabilityFactory = 
            roomPointTraversabilityFactory;
        private readonly Func<IEnumerable<Room>, IRoomTraverable<Solid>> _roomSolidTraversabilityFactory = 
            roomSolidTraversabilityFactory;


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
        /// Check solid traversability.
        /// </summary>
        public bool IsSolidTraversabilityEnabled { get; set; } = true;

        /// <inheritdoc/>
        public IEnumerable<Room> Rooms { get; set; } = rooms;

        /// <inheritdoc/>
        public bool IsTraversable(MEPCurve item)
        {
            var mEPCurvesolid = item.Solid();

            if (Rooms.Count() == 0)
            {
                Logger?.Warning($"The MEPCurve {item.Id} is outside of the rooms.");
                WindowMessenger?.Show("Не найдено помещений, содержащих данный элемент", "Ошибка");
                return false;
            }

            var roomPointTraversable = _roomPointTraversabilityFactory.Invoke(Rooms);
            var roomSolidTraverable = _roomSolidTraversabilityFactory.Invoke(Rooms);

            if (!IsPointTraversable(item, roomPointTraversable) ||
                (IsSolidTraversabilityEnabled && !roomSolidTraverable.IsTraversable(mEPCurvesolid)))
            {
                Logger?.Warning($"The MEPCurve {item.Id} isn't traversable through the rooms.");
                WindowMessenger?.Show("Не найдено помещений, содержащих данный элемент", "Ошибка");
                return false;
            }

            Logger?.Warning($"The MEPCurve {item.Id} is traversable.");
            return true;
        }

        /// <summary>
        /// Check if rooms contains both <paramref name="mEPCurve"/> edge points.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="roomTraverable"></param>
        /// <returns></returns>
        public bool IsPointTraversable(MEPCurve mEPCurve, IRoomTraverable<XYZ> roomTraverable)
        {
            var centerLine = mEPCurve.GetCenterLine();
            var p1 = centerLine.GetEndPoint(0);
            var p2 = centerLine.GetEndPoint(1);

            return roomTraverable.IsTraversable(p1) && roomTraverable.IsTraversable(p2);
        }
    }
}
