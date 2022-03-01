using Autodesk.Revit.DB;
using DS.Revit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DS.Revit.MEPAutoCoordination.Offset
{
    class PointUtils
    {

        public void GetGeneralPoints(IGeneralPointExtractor pointExtractor, out List<XYZ> startPoints, out List<XYZ> endPoints)
        {
            pointExtractor.GetGeneralPoints(out List<XYZ> SP, out List<XYZ> EP);
            startPoints = SP;
            endPoints = EP;
        }

        /// <summary>
        /// Get min and max points for list of lines
        /// </summary>
        public void FindMinMaxPointByLines(List<Line> allCurrentPositionLines, out XYZ minPoint, out XYZ maxPoint)
        {
            List<XYZ> points = new List<XYZ>();

            foreach (Line line in allCurrentPositionLines)
            {
                points.Add(line.GetEndPoint(0));
                points.Add(line.GetEndPoint(1));
            }

            FindMinMaxPointByPoints(points, out minPoint, out maxPoint);
        }

        public void FindMinMaxPointByPoints(List<XYZ> points, out XYZ minPoint, out XYZ maxPoint)
        {
            List<double> xlist = new List<double>();
            List<double> ylist = new List<double>();
            List<double> zlist = new List<double>();

            foreach (XYZ point in points)
            {
                xlist.Add(point.X);
                ylist.Add(point.Y);
                zlist.Add(point.Z);
            }

            minPoint = new XYZ(xlist.MinBy(a => a), ylist.MinBy(a => a), zlist.MinBy(a => a));
            maxPoint = new XYZ(xlist.OrderByDescending(a => a).First(),
                ylist.OrderByDescending(a => a).First(), zlist.OrderByDescending(a => a).First());
        }


        public XYZ GetOffsetByMoveVector(XYZ moveVector, double offset)
        {
            XYZ normVector = moveVector.Normalize();
            double offsetF = UnitUtils.Convert(offset / 1000,
                                   DisplayUnitType.DUT_METERS,
                                   DisplayUnitType.DUT_DECIMAL_FEET);

            XYZ XYZoffset = normVector.Multiply(offsetF);
            return XYZoffset;
        }

        public XYZ GetNormalVector(XYZ vector)
        {
            double x;
            double y;
            double z;

            x = Math.Round(vector.X / (Math.Abs(vector.X)));
            y = Math.Round(vector.Y / (Math.Abs(vector.Y)));
            z = Math.Round(vector.Z / (Math.Abs(vector.Z)));

            if (Math.Abs(vector.X) <= 0.01)
                x = 0;
            if (Math.Abs(vector.Y) <= 0.01)
                y = 0;
            if (Math.Abs(vector.Z) <= 0.01)
                z = 0;

            return new XYZ(x, y, z);
        }

        public void GetClearancePoints(Element element, out List<XYZ> startCLRPoints, out List<XYZ> endCLRPoints)
        {
            ClearancePoint clearancePoint = new ClearancePoint();
            clearancePoint.GetClearancePoints(element, out List<XYZ> startCLR, out List<XYZ> endCLR);
            startCLRPoints = startCLR;
            endCLRPoints = endCLR;
        }

        public List<XYZ> GetElementsPoints(List<Element> elements)
        {
            List<XYZ> points = new List<XYZ>();
            PointUtils pointUtils = new PointUtils();

            foreach (Element element in elements)
            {
                IGeneralPointExtractor pointExtractor = new GeneralPointExtractor(element);
                GetGeneralPoints(pointExtractor, out List<XYZ> startPoints, out List<XYZ> endPoints);

                points.AddRange(startPoints);
                points.AddRange(endPoints);
            }

            return points;
        }

        public List<XYZ> GetMinMaxSolidsPoints(List<Solid> solids)
        {
            List<XYZ> points = new List<XYZ>();

            foreach (Solid solid in solids)
            {
                Transform transform = solid.GetBoundingBox().Transform;

                Solid solidTransformed = SolidUtils.CreateTransformed(solid, transform);

                XYZ minPoint = solidTransformed.GetBoundingBox().Min;
                XYZ maxPoint = solidTransformed.GetBoundingBox().Max;

                XYZ minTrPoint = transform.OfPoint(minPoint);
                XYZ maxTrPoint = transform.OfPoint(maxPoint);

                points.Add(minTrPoint);
                points.Add(maxTrPoint);
            }

            return points;
        }

        public void GetStaticListsPoinsOld(List<XYZ> glStartPoints, List<XYZ> glEndPoints, XYZ staticPoint,
          out List<XYZ> movablePoints, out List<XYZ> staticPoints)
        {
            if (Math.Abs(glStartPoints[0].X - staticPoint.X) < 0.01)
            {
                staticPoints = glStartPoints;
                movablePoints = glEndPoints;
            }
            else
            {
                staticPoints = glEndPoints;
                movablePoints = glStartPoints;
            }
        }

        public void GetStaticListsPoins(List<XYZ> glStartPoints, List<XYZ> glEndPoints, XYZ centerPoint1, XYZ centerPoint2,
            XYZ staticPoint,
         out List<XYZ> movablePoints, out List<XYZ> staticPoints)
        {
            //Check in case of center line
            if (Math.Abs(staticPoint.DistanceTo(glStartPoints[0])) < 0.01)
            {
                staticPoints = glStartPoints;
                movablePoints = glEndPoints;
                return;
            }
            else if (Math.Abs(staticPoint.DistanceTo(glEndPoints[0])) < 0.01)
            {
                staticPoints = glEndPoints;
                movablePoints = glStartPoints;
                return;
            }

            //Check in all other cases lines
            XYZ vector = staticPoint - glStartPoints[0];           
            XYZ centerLineVector = centerPoint1 - centerPoint2;

            double angleRad = vector.AngleTo(centerLineVector);
            double angle = Math.Round(angleRad * (180 / Math.PI));

            if (Math.Abs(angle - 90) < 3)
            {
                staticPoints = glStartPoints;
                movablePoints = glEndPoints;
            }
            else
            {
                staticPoints = glEndPoints;
                movablePoints = glStartPoints;
            }

        }
    }

        interface IGeneralPointExtractor
        {
            Element element { get; set; }
            void GetGeneralPoints(out List<XYZ> startPoints, out List<XYZ> endPoints);
        }

        class GeneralPointExtractor : IGeneralPointExtractor
        {

            public Element element { get; set; }

            public GeneralPointExtractor(Element elem)
            {
                element = elem;
            }

            public void GetGeneralPoints(out List<XYZ> startPoints, out List<XYZ> endPoints)
            {
                //Get element's solid
                List<Solid> solids = ElementUtils.GetSolids(element);

                Solid elementSolid = null;
                foreach (Solid solid in solids)
                    elementSolid = solid;

                startPoints = new List<XYZ>();
                endPoints = new List<XYZ>();

                int i = 0;

                foreach (Face face in elementSolid.Faces)
                {

                    Mesh mesh = face.Triangulate();
                    for (i = 0; i < mesh.Vertices.Count - 1; i++)
                    {
                        if ((mesh.Vertices[i].DistanceTo(mesh.Vertices[i + 1]) > 0.1))
                        {
                            startPoints.Add(mesh.Vertices[i]);
                            endPoints.Add(mesh.Vertices[i + 1]);
                        }

                    }

                }

            }
        }


        class ClearancePoint
        {
            PointUtils pointUtils = new PointUtils();

            XYZ GetClearancePoint(XYZ point, XYZ centerPoint)

            {
                XYZ diffVector = (point - centerPoint).Normalize();
                XYZ normalVector = pointUtils.GetNormalVector(diffVector);

                double offsetCorrectionF = UnitUtils.Convert(10.0 / 1000,
                                    DisplayUnitType.DUT_METERS,
                                    DisplayUnitType.DUT_DECIMAL_FEET);

                XYZ vector = normalVector.Multiply(Data.ElementClearenceInFeets - offsetCorrectionF);

                double AX = 1;
                double AY = 1;
                if (Math.Abs(Data.Elem1AX) > 0.01)
                    AX = Math.Abs(Data.Elem1AX);
                if (Math.Abs(Data.Elem1AY) > 0.01)
                    AY = Math.Abs(Data.Elem1AY);

                vector = new XYZ(vector.X * AX, vector.Y * AY, vector.Z);

                return point + vector;
            }


            /// <summary>
            /// Get clearance points list considering reference general point position.
            /// Taking account start and end points to excule wrong points.
            /// </summary>
            /// <param name="glPoints">General point</param>
            List<XYZ> GetCLRPointsList(List<XYZ> glPoints, XYZ startPoint, XYZ endPoint)
            {
                List<XYZ> CLRPoints = new List<XYZ>();

                foreach (XYZ glPoint in glPoints)
                {
                    XYZ clearancePoint = new XYZ();
                    if (glPoint.DistanceTo(startPoint) < glPoint.DistanceTo(endPoint))
                        clearancePoint = GetClearancePoint(glPoint, startPoint);
                    else
                        clearancePoint = GetClearancePoint(glPoint, endPoint);

                    CLRPoints.Add(clearancePoint);
                }

                return CLRPoints;
            }

            public void GetClearancePoints(Element element, out List<XYZ> startCLRPoints, out List<XYZ> endCLRPoints)
            {
                ElementUtils.GetPoints(element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPointElement);

                IGeneralPointExtractor pointExtractor = new GeneralPointExtractor(element);
                pointUtils.GetGeneralPoints(new GeneralPointExtractor(element), out List<XYZ> glStartPoints, out List<XYZ> glEndPoints);

                //Fill start and end points lists
                startCLRPoints = GetCLRPointsList(glStartPoints, startPoint, endPoint);
                endCLRPoints = GetCLRPointsList(glEndPoints, startPoint, endPoint);
            }


        }
    
}
