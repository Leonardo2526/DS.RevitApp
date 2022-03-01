using Autodesk.Revit.DB;
using DS.Revit.Utils;
using System;

namespace DS.Revit.MEPAutoCoordination.Offset
{

    public interface ICollisionType
    {
        bool Check();
    }

    public class CollisionType2 : ICollisionType
    {
        readonly Element Elem1;
        readonly Element Elem2;

        public CollisionType2(Element elem1, Element elem2)
        {
            Elem1 = elem1;
            Elem2 = elem2;
        }


        public bool Check()
        {
            bool elementForMove = false;


            ElementUtils.GetPoints(Elem1, out XYZ startPointA, out XYZ endPointA, out XYZ centerPointA);
            ElementUtils.GetPoints(Elem2, out XYZ startPointB, out XYZ endPointB, out XYZ centerPointB);

            if (Math.Abs(endPointB.X - startPointB.X) < 0.01 && Math.Abs(endPointB.Y - startPointB.Y) < 0.01)
                return false;

            if (Math.Abs(endPointA.X - startPointA.X) < 0.01 && Math.Abs(endPointA.Y - startPointA.Y) < 0.01)
                return false;

            double tgA = (endPointA.Y - startPointA.Y) / (endPointA.X - startPointA.X);
            double tgB = (endPointB.Y - startPointB.Y) / (endPointB.X - startPointB.X);

            double radA = Math.Atan(tgA);
            double angleA = radA * (180 / Math.PI);

            double radB = Math.Atan(tgB);
            double angleB = radB * (180 / Math.PI);

            double deltaAndle = Math.Abs(angleA - angleB);

            if (deltaAndle < 15 | (180 - deltaAndle) < 15)
                elementForMove = true;

            return elementForMove;
        }
    }
}
