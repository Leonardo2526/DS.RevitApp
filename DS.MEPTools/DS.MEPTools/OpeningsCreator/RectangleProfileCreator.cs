using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Openings;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Extensions;
using Autodesk.Revit.DB.Electrical;
using DS.RevitLib.Utils;
using Rhino.UI;
using DS.RevitLib.Utils.Geometry;
using DS.RevitLib.Utils.Lines;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Creation.Transactions;

namespace DS.MEPTools.OpeningsCreator
{
    public class RectangleProfileCreator : IOpeningProfileCreator
    {
        private readonly Document _activeDoc;
        private double _offset;

        public RectangleProfileCreator(Document doc)
        {
            _activeDoc = doc;
        }

        public double Offset { get => _offset; set => _offset = value; }
        public ITransactionFactory TransactionFactory { get; set; } = null;

        //public Rectangle3d Profile { get; private set; }

        public CurveLoop CreateProfile(Wall wall, MEPCurve mEPCurve)
        {
            //var wSolid = wall.Solid;
            var wPlane = wall.GetMainPlanarFace().GetPlane().ToRhinoPlane();

            List<Point3d> points = GetBoxPoints(wall, mEPCurve, _activeDoc);
            if(points is null || points.Count == 0) { return null; }
            if(TransactionFactory != null)
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

           var lines =  exRectangle.ToLines();

            var cloop = new CurveLoop();
            lines.ForEach(l => cloop.Append(l.ToXYZ()));

            //for (int i = 0; i < corners.Length/2 - 1; i++)
            //{
            //    var p1 = corners[i];
            //    var p2 = corners[i + 1];
            //    var line = new Rhino.Geometry.Line(p1, p2).ToXYZ();
            //    cloop.Append(line);
            //}

            return cloop;
        }

        private List<Point3d> GetBoxPoints(Wall wall, MEPCurve mEPCurve, Document activeDoc)
        {
            var intersectionSolid = (wall, mEPCurve).GetIntersectionSolidWithInsulation(0, activeDoc);
            if(intersectionSolid == null) { return new List<Point3d>(); }

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
    }
}
