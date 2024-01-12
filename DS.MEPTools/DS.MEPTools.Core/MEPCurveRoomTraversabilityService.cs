using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using DS.ClassLib.VarUtils.Collisons;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.MEPTools.Core
{
    /// <summary>
    /// The service to check <see cref="MEPCurve"/>s traversability through <see cref="Room"/>s.
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="roomIntersectionFactory"></param>
    /// <param name="elementIntersectionFactory"></param>
    public class MEPCurveRoomTraversabilityService(Document doc,
         ITIntersectionFactory<Room, Solid> roomIntersectionFactory,
         ITIntersectionFactory<Element, Solid> elementIntersectionFactory)
    {
        private readonly Document _doc = doc;
        private readonly ITIntersectionFactory<Room, Solid> _roomIntersectionFactory = roomIntersectionFactory;
        private readonly ITIntersectionFactory<Element, Solid> _elementIntersectionFactory = elementIntersectionFactory;

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// A factory to commit transactions.
        /// </summary>
        public ITransactionFactory TransactionFactory { get; set; }

        /// <summary>
        /// Check solid traversability.
        /// </summary>
        public bool IsSolidTraversabilityEnabled { get; set; } = true;

        /// <summary>
        /// Check if <paramref name="mEPCurve"/> can be traverse through <see cref="Room"/>s.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="mEPCurve"/> can be traverse through <see cref="Room"/>s.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public bool IsTraversable(MEPCurve mEPCurve)
        {
            var mEPCurvesolid = mEPCurve.Solid();
            var rooms = _roomIntersectionFactory.GetIntersections(mEPCurvesolid).OfType<Room>();

            if (rooms.Count() == 0 ||
                !IsPointTraversable(mEPCurve, rooms) ||
                (IsSolidTraversabilityEnabled && !IsSolidTraversable(mEPCurve, rooms)))
            {
                Logger?.Warning($"The MEPCurve {mEPCurve.Id} is outside rooms.");
                return false;
            }

            Logger?.Warning($"The MEPCurve {mEPCurve.Id} is traversable.");
            return true;
        }

        /// <summary>
        /// Check if <paramref name="rooms"/> contains both <paramref name="mEPCurve"/> edge points.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="rooms"></param>
        /// <returns></returns>
        public bool IsPointTraversable(MEPCurve mEPCurve, IEnumerable<Room> rooms)
        {
            var centerLine = mEPCurve.GetCenterLine();
            var p1 = centerLine.GetEndPoint(0);
            var p2 = centerLine.GetEndPoint(1);

            var tp1 = rooms.Any(r => IsPointInRoom(p1, r));
            if (tp1 == false)
            { Logger?.Information($"Point {p1} is out the rooms."); return false; }
            var tp2 = rooms.Any(r => IsPointInRoom(p2, r));
            if (tp2 == false)
            { Logger?.Information($"Point {p2} is out the rooms."); return false; }

            return tp1 && tp2;

            bool IsPointInRoom(XYZ point, Room room)
            {
                if (room.IsPointInRoom(point))
                { Logger?.Information($"Point {point} is in room '{room.Name}' (id: {room.Id})"); return true; }
                else { return true; }
            }
        }

        /// <summary>
        /// Check if <paramref name="roomDocs"/> contains all <paramref name="mEPCurve"/> solids.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="rooms"></param>
        /// <param name="roomDocs"></param>
        /// <returns></returns>
        public bool IsSolidTraversable(MEPCurve mEPCurve, IEnumerable<Room> rooms)
        {
            var roomsElementsIds = GetElementsInsideRoooms(rooms, _doc, _elementIntersectionFactory);

            var mEPCurveSolid = mEPCurve.Solid();
            List<Solid> intersectionSolids = CalculateSolids(_doc, mEPCurve, mEPCurveSolid, rooms, roomsElementsIds);

            var sumVolumeSolids = intersectionSolids.Sum(s => s.Volume);
            var deltaV = mEPCurveSolid.Volume - sumVolumeSolids;

            Logger?.Information($"MEPCurve solid volume is: {mEPCurveSolid.Volume}");
            Logger?.Information($"Intersection solids sum volume is: {sumVolumeSolids}");
            Logger?.Information($"Difference is: {deltaV}");

            return Math.Abs(deltaV) < 0.001;
        }

        private List<Solid> CalculateSolids(
            Document doc,
            MEPCurve mEPCurve, Solid mEPCurveSolid,
            IEnumerable<Room> rooms, IEnumerable<ElementId> roomsElementsIds)
        {
            var solids = new List<Solid>();

            var insideElementSolids = CalculateInsideElementSolids(doc, roomsElementsIds, mEPCurve);
            var roomSolids = CalculateRoomSolids(mEPCurve, mEPCurveSolid, rooms);
            solids.AddRange(insideElementSolids);
            solids.AddRange(roomSolids);

            return solids;

            List<Solid> CalculateInsideElementSolids(
                Document doc,
                IEnumerable<ElementId> roomsElementsIds,
                MEPCurve mEPCurve)
            {
                var elemSolids = new List<Solid>();

                var intersectionFactory = new ElementIntersectionFactory(doc);
                var collisionDetector = new ElementCollisionDetector(doc, intersectionFactory)
                { ExcludedIds = roomsElementsIds };
                var collisions = collisionDetector.GetCollisions(mEPCurve);
                collisions.ForEach(collision => { elemSolids.Add(collision.GetIntersectionSolid()); });

                return elemSolids;
            }

            List<Solid> CalculateRoomSolids(
                MEPCurve mEPCurve, Solid mEPCurveSolid, IEnumerable<Room> rooms)
            {
                var roomSolids = new List<Solid>();

                var s1 = mEPCurveSolid;

                foreach (var room in rooms)
                {
                    var elem2 = room;
                    var s2 = elem2.GetSolidInLink(doc);
                    var s = DS.RevitLib.Utils.Solids.SolidUtils.GetIntersection(s1, s2);
                    roomSolids.Add(s);
                }


                return roomSolids;
            }
        }

        private IEnumerable<ElementId> GetElementsInsideRoooms(IEnumerable<Room> rooms,
            Document doc, ITIntersectionFactory<Element, Solid> elementIntersectionFactory)
        {
            var roomsElementsIds = new List<ElementId>();

            foreach (var room in rooms)
            {
                var elementIds = room.GetInsideElements(doc, elementIntersectionFactory).ToList();
                foreach (var id in elementIds)
                {
                    if (!roomsElementsIds.Contains(id))
                    { roomsElementsIds.Add(id); }
                }
                Logger?.Information($"Elements in room '{room.Name}': {elementIds.Count()}");
            }

            Logger?.Information($"Total rooms elements: {roomsElementsIds.Count()}");

            //_uiDoc.Selection.SetElementIds( roomsElements );

            return roomsElementsIds;
        }

    }
}
