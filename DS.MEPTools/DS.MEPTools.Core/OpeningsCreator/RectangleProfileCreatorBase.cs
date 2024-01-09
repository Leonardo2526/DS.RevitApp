using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
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
        /// Current active <see cref="Document"/>.
        /// </summary>
        protected readonly Document ActiveDoc;

        /// <summary>
        /// Instantiate a base object is used to create rectangle 
        /// opening profile of <typeparamref name="TIntersectionItem"/> on the <see cref="Wall"/>.
        /// </summary>
        /// <param name="doc"></param>
        protected RectangleProfileCreatorBase(Document doc)
        {
            ActiveDoc = doc;
        }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Opening clearance.
        /// </summary>
        public double Offset { get; set; }

        /// <summary>
        /// A factory to commit transactions.
        /// </summary>
        public ITransactionFactory TransactionFactory { get; set; }

        /// <inheritdoc/>
        public CurveLoop CreateProfile(Wall wall, TIntersectionItem mepCurve)
        {
            var intersectionSolid = GetIntersectionSolid(wall, mepCurve);
            if (intersectionSolid == null)
            {
                Logger?.Warning($"Wall {wall.Id} and MEPCurve {mepCurve} have no intersections.");
                return null;
            }

            //var wSolid = wall.Solid;
            var wPlane = wall.GetMainPlanarFace(ActiveDoc).GetPlane().ToRhinoPlane();

            var points = GetBoxPoints(wall, intersectionSolid);
            if (TransactionFactory != null)
            {
                var label = 50.MMToFeet();
                TransactionFactory.CreateAsync(() =>
                points.ForEach(p => p.ToXYZ().ShowPoint(ActiveDoc, label)), "ShowBoundPoints");
            }

            var box = new Box(wPlane, points);
            var corners = box.GetCorners();
            var c1 = corners[0];
            var c2 = box.FurthestPoint(c1);
            var rect = new Rectangle3d(wPlane, c1, c2);

            if (!rect.TryExtend(Offset, out var exRectangle))
            { throw new Exception(""); }

            var lines = exRectangle.ToLines();

            var cLoop = new CurveLoop();
            lines.ForEach(l => cLoop.Append(l.ToXYZ()));

            return cLoop;
        }

        private static List<Point3d> GetBoxPoints(Element wall, Solid intersectionSolid)
        {
            var centroid = intersectionSolid.ComputeCentroid();
            var wLine = wall.GetCenterLine();
            var xLine = Autodesk.Revit.DB.Line.CreateUnbound(centroid, wLine.Direction);
            var (px1, px2) = intersectionSolid.GetEdgeProjectPoints(xLine);
            var zLine = Autodesk.Revit.DB.Line.CreateUnbound(centroid, XYZ.BasisZ);
            var (pz1, pz2) = intersectionSolid.GetEdgeProjectPoints(zLine);

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