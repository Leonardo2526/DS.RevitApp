using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using DS.MEPTools.Core;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using MoreLinq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DS.MEPTools.Core.Rooms.Traversability
{

    /// <summary>
    /// The objects to check <see cref="Solid"/> traversability through <see cref="Rooms"/>.
    /// </summary>
    public class SolidRoomTraversable(
        Document activeDoc,
        IEnumerable<Room> rooms,
        SolidElementIntersectionFactoryBase<Room> solidElementIntersectionFactory,
        ITIntersectionFactory<Element, Solid> intersectionFactory) : IRoomTraverable<Solid>
    {

        private readonly double _minIntersectionVolume =
            3.CubicCMToFeet();
        private readonly Document _activeDoc =
            activeDoc;
        private readonly SolidElementIntersectionFactoryBase<Room> _solidElementIntersectionFactory =
            solidElementIntersectionFactory;
        private readonly ITIntersectionFactory<Element, Solid> _elementIntersectionFactory =
            intersectionFactory;

        /// <inheritdoc/>
        public IEnumerable<Room> Rooms { get; } = rooms;

        /// <summary>
        /// Fields to exclude from <see cref="Rooms"/>.
        /// </summary>
        public IEnumerable<string> ExcludeFields { get; set; }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Window messenger to show important information to user.
        /// </summary>
        public IWindowMessenger WindowMessenger { get; set; }

        /// <summary>
        /// A factory to commit transactions.
        /// </summary>
        public ITransactionFactory TransactionFactory { get; set; }

        /// <inheritdoc/>
        public bool IsTraversable(Solid item)
        {
            _solidElementIntersectionFactory.ElementIdsSet = [.. Rooms.Select(r => r.Id).ToList()];
            var foundRooms = _solidElementIntersectionFactory.GetIntersections(item);

            if (foundRooms is null || foundRooms.Count() == 0)
            {
                Logger?.Information($"Check solid is out of the rooms.");
                WindowMessenger?.Show("Элемент находится вне пределов помещений.", "Ошибка");
                return false;
            }

            if (ExcludeFields is not null)
            {
                var exludedRooms = foundRooms.Where(room => ExcludeFields.Any(f => room.Name.Contains(f.ToLower())));
                if (exludedRooms is not null && exludedRooms.Any())
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Элемент не может проходить через данные помещения:");
                    exludedRooms.ForEach(room => sb.AppendLine($"имя: {room.Name}, id: {room.Id}"));
                    WindowMessenger?.Show(sb.ToString(), "Ошибка");
                    Logger?.Information($"Check solid is in the room from exclusion list.");
                    return false;
                }
            }

            var incsideRoomsElements = GetElementsInsideRoooms(
                foundRooms,
                _activeDoc,
                _elementIntersectionFactory)
                .Select(e => e.Id);

            List<Solid> intersectionSolids = CalculateIntersectionSolids(
                _activeDoc,
                item,
                foundRooms,
                incsideRoomsElements,
                _elementIntersectionFactory,
                _minIntersectionVolume);

            var sumVolumeSolids = intersectionSolids.Sum(s => s.Volume);
            var deltaV = item.Volume - sumVolumeSolids;

            Logger?.Information($"MEPCurve solid volume is: {item.Volume}");
            Logger?.Information($"Intersection solids sum volume is: {sumVolumeSolids}");
            Logger?.Information($"Difference is: {deltaV}");

            var isTraversable = Math.Abs(deltaV) < _minIntersectionVolume;
            if (!isTraversable)
            { WindowMessenger?.Show("Элемент находится вне пределов помещений.", "Ошибка"); }

            return isTraversable;
        }

        private List<Solid> CalculateIntersectionSolids(
            Document activeDoc,
            Solid mEPCurveSolid,
            IEnumerable<Room> rooms,
            IEnumerable<ElementId> incsideRoomsElementsIds,
            ITIntersectionFactory<Element, Solid> elementIntersectionFactory,
            double minVolume)
        {
            var solids = new List<Solid>();

            var intersectionElements = elementIntersectionFactory.GetIntersections(mEPCurveSolid).
                Where(el => !incsideRoomsElementsIds.Contains(el.Id));
            var elementsIntersectionSolids = RevitLib.Utils.Solids.SolidUtils.GetIntersections(
                mEPCurveSolid,
                intersectionElements,
                activeDoc,
                minVolume);
            var roomsIntersectionsSolids = RevitLib.Utils.Solids.SolidUtils.GetIntersections(
                mEPCurveSolid,
                rooms,
                activeDoc,
                minVolume);
            solids.AddRange(elementsIntersectionSolids);
            solids.AddRange(roomsIntersectionsSolids);

            return solids;
        }

        private IEnumerable<Element> GetElementsInsideRoooms(IEnumerable<Room> rooms,
            Document doc, ITIntersectionFactory<Element, Solid> elementIntersectionFactory)
        {
            var roomsElements = new List<Element>();

            foreach (var room in rooms)
            {
                var elements = room.GetInsideElements(doc, elementIntersectionFactory).ToList();
                foreach (var elem in elements)
                {
                    if (!roomsElements.Select(r => r.Id).Contains(elem.Id))
                    { roomsElements.Add(elem); }
                }
                Logger?.Information($"Elements in room '{room.Name}': {elements.Count()}");
            }

            Logger?.Information($"Total rooms elements: {roomsElements.Count()}");

            //_uiDoc.Selection.SetElementIds( roomsElements );

            return roomsElements;
        }
    }
}
