using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Basis;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Points;
using DS.GraphUtils.Entities;
using DS.MEPTools.Core.Rooms.Traversability;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Various.Bases;
using QuickGraph;
using Rhino.Geometry;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DS.MEPTools.Core
{
    public class BasisPointTaggedEdgeValidator : ITaggedEdgeValidator<TaggedGVertex<Point3d>, Basis3d>
    {
        private readonly IPoint3dConverter _pointConverter;

        public BasisPointTaggedEdgeValidator(IPoint3dConverter pointConverter = null)
        {
            _pointConverter = pointConverter;
        }

        public IEnumerable<ValidationResult> ValidationResults => throw new NotImplementedException();
        public IRoomTraverable<XYZ> PointRoomTraverable { get; set; }
        public Tuple<IRoomTraverable<Solid>, ISolidOffsetExtractor> TupleSolidRoomTraverable { get; set; }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        public bool IsValid(TaggedEdge<TaggedGVertex<Point3d>, Basis3d> edge)
        {          
            var p1 = _pointConverter is null ? 
                edge.Source.Tag.ToXYZ() :
                _pointConverter.ConvertToUCS1(edge.Source.Tag).ToXYZ();
            var p2 = _pointConverter is null ?
               edge.Target.Tag.ToXYZ() :
               _pointConverter.ConvertToUCS1(edge.Target.Tag).ToXYZ();

            if (PointRoomTraverable is not null &&
               !PointRoomTraverable.IsTraversable(p1) ||
               !PointRoomTraverable.IsTraversable(p2))
            {
                return false;
            }

            if (TupleSolidRoomTraverable.Item1 is not null)
            {
                var uCS1Basis = _pointConverter.ConvertToUCS1(edge.Tag).ToXYZ();
                var solid = TupleSolidRoomTraverable.Item2.Extract(p1, p2, uCS1Basis);
                if (!TupleSolidRoomTraverable.Item1.IsTraversable(solid))
                { return false; }
            }

            return true;
        }
    }
}
