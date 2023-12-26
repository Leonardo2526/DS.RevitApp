using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.Openings;
using Rhino.Geometry;
using Serilog;
using System;
using System.Collections.Generic;

namespace DS.MEPTools.OpeningsCreator
{
    /// <summary>
    ///  A base object is used to create rectangle 
    ///  opening profile of <typeparamref name="TIntersectionItem"/> on the <see cref="Wall"/>.
    /// </summary>
    /// <typeparam name="TIntersectionItem"></typeparam>
    public abstract class RectangleProfileCreatorBase<TIntersectionItem> : IOpeningProfileCreator<TIntersectionItem>
    {
        /// <summary>
        /// Curent active <see cref="Document"/>.
        /// </summary>
        protected Document _activeDoc;

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        protected ILogger _logger;

        private double _offset;

        /// <summary>
        /// Instansiate a base object is used to create rectangle 
        /// opening profile of <typeparamref name="TIntersectionItem"/> on the <see cref="Wall"/>.
        /// </summary>
        /// <param name="doc"></param>
        public RectangleProfileCreatorBase(Document doc)
        {
            _activeDoc = doc;
        }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger
        { get => _logger; set => _logger = value; }

        /// <summary>
        /// Opening clearance.
        /// </summary>
        public double Offset { get => _offset; set => _offset = value; }

        /// <summary>
        /// A factory to commit transactions.
        /// </summary>
        public ITransactionFactory TransactionFactory { get; set; } = null;

        /// <inheritdoc/>
        public CurveLoop CreateProfile(Wall wall, TIntersectionItem intersectionItem)
        {
            Solid intersectionSolid = GetIntersectionSolid(wall, intersectionItem);
            if (intersectionSolid == null)
            {
                _logger?.Warning($"Wall {wall.Id} and MEPCurve {intersectionItem} have no intersections.");
                return null;
            }

            //var wSolid = wall.Solid;
            var wPlane = wall.GetMainPlanarFace(_activeDoc).GetPlane().ToRhinoPlane();

            List<Point3d> points = GetBoxPoints(wall, intersectionSolid);
            if (points is null || points.Count == 0) { return null; }
            if (TransactionFactory != null)
            {
                var label = 50.MMToFeet();
                TransactionFactory.CreateAsync(() =>
                points.ForEach(p => p.ToXYZ().ShowPoint(_activeDoc, label)), "ShowBoundPoints");
            }

            var box = new Box(wPlane, points);
            var corners = box.GetCorners();
            var c1 = corners[0];
            var c2 = box.FurthestPoint(c1);
            var rect = new Rectangle3d(wPlane, c1, c2);

            if (!rect.TryExtend(_offset, out var exRectangle))
            { throw new Exception(""); }

            var lines = exRectangle.ToLines();

            var cloop = new CurveLoop();
            lines.ForEach(l => cloop.Append(l.ToXYZ()));

            return cloop;
        }

        private List<Point3d> GetBoxPoints(Wall wall, Solid intersectionSolid)
        {
            var centroid = intersectionSolid.ComputeCentroid();
            var wLine = wall.GetCenterLine();
            var xLine = Autodesk.Revit.DB.Line.CreateUnbound(centroid, wLine.Direction);
            (XYZ px1, XYZ px2) = intersectionSolid.GetEdgeProjectPoints(xLine);
            var zLine = Autodesk.Revit.DB.Line.CreateUnbound(centroid, XYZ.BasisZ);
            (XYZ pz1, XYZ pz2) = intersectionSolid.GetEdgeProjectPoints(zLine);

            var points = new List<Point3d>()
            { px1.ToPoint3d(), px2.ToPoint3d(), pz1.ToPoint3d(), pz2.ToPoint3d()};
            return points;
        }

        /// <summary>
        /// Get intersection <see cref="Solid"/> between <paramref name="wall"/> and <paramref name="intersectionItem"/>.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="intersectionItem"></param>
        /// <returns>
        /// Intersection <see cref="Solid"/>.
        /// </returns>
        protected abstract Solid GetIntersectionSolid(Wall wall, TIntersectionItem intersectionItem);
    }
}