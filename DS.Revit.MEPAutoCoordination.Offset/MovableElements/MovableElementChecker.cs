using Autodesk.Revit.DB;
using DS.Revit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DS.Revit.MEPAutoCoordination.Offset
{

    class MovableElementChecker
    {
        readonly XYZ MoveVector;
        readonly Element element;

        public MovableElementChecker(XYZ moveVector, Element elem)
        {
            MoveVector = moveVector;
            element = elem;
        }

        PointUtils pointUtils = new PointUtils();
        
        XYZ StartPoint = new XYZ();
        XYZ EndPoint = new XYZ();
        XYZ CenterPoint = new XYZ();
        XYZ Vector = new XYZ();

        public double AngleRad;
        public double Angle;

        public static double MinCurveLength 
        {
            get
            {
                return UnitUtils.Convert(
                    50, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);
            }
        }
       

        public void GetData()
        {
            ElementUtils.GetPoints(element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint);
            StartPoint = startPoint;
            EndPoint = endPoint;
            CenterPoint = centerPoint;

            XYZ Vector = EndPoint - StartPoint;
            AngleRad = MoveVector.AngleTo(Vector);
            Angle = Math.Round(AngleRad * (180 / Math.PI));
        }

        public string GetTypeName()
        {
            Type type = element.GetType();

            string familyInstance = "FamilyInstance";
            string system = "System";

            if (type.ToString().Contains(familyInstance))
                return familyInstance;
            else if (type.ToString().Contains(system))
                return system;

            return "";
        }

        /// <summary>
        /// Check angle between move vector and center line of element.
        /// Return true if element's angle will not obstacle for moving.
        /// </summary>
        public bool CheckAngle()
        {
            if (Angle == 90|| Angle == 270)
                return true;

            return false;
        }

        /// <summary>
        /// Check length of element.
        /// Return true if element's length will not obstacle for moving.
        /// </summary>
        public bool CheckLength(double angleRad, out XYZ moveVectorForFamInst)
        {
            moveVectorForFamInst = null;

            MEPCurve mEPCurve = element as MEPCurve;
            LocationCurve lc = mEPCurve.Location as LocationCurve;
            double curvelength = lc.Curve.ApproximateLength;

            double moveVectorLength = Math.Abs(MoveVector.GetLength() / Math.Cos(angleRad));

            if (curvelength -moveVectorLength < MinCurveLength)
                {
                moveVectorForFamInst = Obstacle.GetMoveVector(curvelength, MinCurveLength);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Check refernce position of move vector and element.
        /// Return true if element's refernce position is in opposite side from moving.
        /// </summary>
        public bool CheckPosition()
        {
            ElementUtils.GetPoints(Data.Elem1Curve, out XYZ Elem1StartPoint, out XYZ Elem1EndPoint, out XYZ Elem1CenterPoint);

            XYZ elementVector = CenterPoint - Elem1CenterPoint;

            double angleRad = MoveVector.AngleTo(elementVector);
            double angle = angleRad * (180 / Math.PI);

            double gyp = CenterPoint.DistanceTo(Elem1CenterPoint);
            double projection = gyp * Math.Cos(angleRad);

            if (projection > 0)
                return false;

            return true;
        }
    }
}
