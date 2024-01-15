using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using MoreLinq;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DS.MEPTools.Core.Rooms.Traversability
{
    /// <summary>
    /// The objects to check <see cref="Autodesk.Revit.DB.XYZ"/> traversability through <see cref="Rooms"/>.
    /// </summary>
    public class PointRoomTraversable(Document activeDoc, IEnumerable<Room> rooms) : IRoomTraverable<XYZ>
    {
        private readonly Document _activeDoc = activeDoc;

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

        /// <inheritdoc/>
        public bool IsTraversable(XYZ item)
        {
            var foundRooms = Rooms.Where(r => r.IsPointInLinkRoom(item, _activeDoc));
            var id = Rooms.Where(el => el.Id.IntegerValue == 7305320);

            if (foundRooms is null || foundRooms.Count() == 0)
            {
                Logger?.Information($"Point {item} is out of the rooms.");
                WindowMessenger?.Show("Конечная точка находится вне пределов помещений.", "Ошибка");
                return false;
            }

            if (ExcludeFields is not null)
            {
                var exludedRooms = foundRooms.Where(room => ExcludeFields.Any(f => room.Name.Contains(f.ToLower())));
                if (exludedRooms is not null && exludedRooms.Any())
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Конечная точка не может находиться в данных помещениях:");
                    exludedRooms.ForEach(room => sb.AppendLine($"имя: {room.Name}, id: {room.Id}"));
                    WindowMessenger?.Show(sb.ToString(), "Ошибка");
                    Logger?.Information($"Point {item} is in the room from exclusion list.");
                    return false;
                }
            }

            return true;
        }
    }
}
