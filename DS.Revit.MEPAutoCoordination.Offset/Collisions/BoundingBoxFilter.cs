using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using DS.Revit.Utils;

namespace DS.Revit.MEPAutoCoordination.Offset
{

    class BoundingBoxFilter
    {
        public BoundingBoxIntersectsFilter GetBoundingBoxFilter(IBoundingBoxFilter boundingBoxFilter)
        {
            return boundingBoxFilter.GetBoundingBoxFilter();
        }

    }

    interface IBoundingBoxFilter
    {
        BoundingBoxIntersectsFilter GetBoundingBoxFilter();
    }


    class LinesBoundingBox : IBoundingBoxFilter
    {
        public List<Line> Lines;

        public LinesBoundingBox (List<Line> lines)
        {
            Lines = lines;
        }

        /// <summary>
        /// Get bounding box by list of lines
        /// </summary>
        public BoundingBoxIntersectsFilter GetBoundingBoxFilter()
        {
            PointUtils pointUtils = new PointUtils();
            pointUtils.FindMinMaxPointByLines(Lines, out XYZ minPoint, out XYZ maxPoint);

            XYZ minRefPoint = new XYZ(minPoint.X, minPoint.Y, minPoint.Z);
            XYZ maxRefPoint = new XYZ(maxPoint.X, maxPoint.Y, maxPoint.Z);

            Outline myOutLn = new Outline(minRefPoint, maxRefPoint);

            return new BoundingBoxIntersectsFilter(myOutLn);
        }
    }

    class SolidsBoundingBox : IBoundingBoxFilter
    {
        public List<Solid> Solids;

        public SolidsBoundingBox(List<Solid> solids)
        {
            Solids = solids;
        }

        public BoundingBoxIntersectsFilter GetBoundingBoxFilter()
        {

            PointUtils pointUtils = new PointUtils();

            List<XYZ> points = new List<XYZ>();

            points.AddRange(pointUtils.GetMinMaxSolidsPoints(Solids));
            
            pointUtils.FindMinMaxPointByPoints(points, out XYZ minPoint, out XYZ maxPoint);

            XYZ minRefPoint = new XYZ(minPoint.X, minPoint.Y, minPoint.Z);
            XYZ maxRefPoint = new XYZ(maxPoint.X, maxPoint.Y, maxPoint.Z);

            Outline myOutLn = new Outline(minRefPoint, maxRefPoint);

            //TransactionUtils transactionUtils = new TransactionUtils();
            //transactionUtils.CreateModelCurve(new CreateModelCurveTransaction(Data.Elem1.Document, minRefPoint, maxRefPoint));
          
            return new BoundingBoxIntersectsFilter(myOutLn);
        }

      
    }

    class ElementBoundingBox
    {
        public BoundingBoxIntersectsFilter GetElementBoundingBox(Element element)
        {
            PointUtils pointUtils = new PointUtils();

            ElementUtils.GetPoints(element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint);

            List<XYZ> points = new List<XYZ>()
            {
                startPoint,
                endPoint
            };

            pointUtils.FindMinMaxPointByPoints(points, out XYZ minPoint, out XYZ maxPoint);

            Outline myOutLn = new Outline(minPoint, maxPoint);

            //TransactionUtils transactionUtils = new TransactionUtils();
            //transactionUtils.CreateModelCurve(new CreateModelCurveTransaction(Data.Elem1.Document, minRefPoint, maxRefPoint));

            return new BoundingBoxIntersectsFilter(myOutLn);

        }
    }

    abstract class BoundingBoxFilterImplementation
    {
        void Main()
        {
            BoundingBoxFilter boundingBoxFilter = new BoundingBoxFilter();

            boundingBoxFilter.GetBoundingBoxFilter(new LinesBoundingBox(new List<Line>()));
            boundingBoxFilter.GetBoundingBoxFilter(new SolidsBoundingBox(new List<Solid>()));

        }
       


    }
}
