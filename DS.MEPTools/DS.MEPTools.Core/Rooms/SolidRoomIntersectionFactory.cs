using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.MEPTools.Core.Rooms
{
   /// <inheritdoc/>
    public class SolidRoomIntersectionFactory(Document doc, IElementMultiFilter elementMultiFilter) :
        SolidElementIntersectionFactoryBase<Room>(doc, elementMultiFilter)
    {
        /// <inheritdoc/>
        public override IEnumerable<Room> GetIntersections(Solid solid)
        {
            //config quick filters
            _elementMultiFilter.Reset();

            if (ElementIdsSet?.Count > 0)
            { _elementMultiFilter.ElementIdsSet = [.. ElementIdsSet];}

            var boundingBoxFilter = GetBoundingBoxIntersectsFilter(solid);
            _elementMultiFilter.QuickFilters.Add(boundingBoxFilter);

            //config slow filters
            _elementMultiFilter.SlowFilters.Add((new RoomFilter(), null));

            var roomBoxDocs = _elementMultiFilter.ApplyToAllDocs();
            var rooms = roomBoxDocs.SelectMany(kv => kv.Value.ToElements(kv.Key)).OfType<Room>();
            rooms = rooms.Where(r => r.Contains(solid, _doc));

            if (Logger != null)
            {
                Logger.Information("Rooms intersections with solid found:");
                rooms.ForEach(r => Logger.Information($"{r.Name}"));
            }

            return rooms;
        }
    }
}
