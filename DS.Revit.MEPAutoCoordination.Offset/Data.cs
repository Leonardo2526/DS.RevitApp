using Autodesk.Revit.DB;
using DS.Revit.Utils.External;
using System;
using System.Collections.Generic;
using System.Linq;
using DS.Revit.Utils.MEP;
using DS.Revit.Utils;

namespace DS.Revit.MEPAutoCoordination.Offset
{

    static class Data
    {
        public static Document Doc { get; set; }
        public static double MaxZCoordinate;

        public static double MinZCoordinate;

        //AllModelElements
        public static List<Element> AllModelElements { get; set; }

        public static List<ElementId> AllModelElementsIds
        {
            get
            {
                return AllModelElements.Select(el => el.Id).ToList();
            }
        }
        public static List<ElementId> NotConnectedToElem1ElementIds;

        public static List<Element> AllLinkedElements { get; set; }
    
        public static List<ElementId> AllLinkedElementsIds 
        { 
            get
            { return AllLinkedElements.Select(x => x.Id).ToList(); }
        }
        public static List<RevitLinkInstance> AllLinks { get; set; }

        public static double SearchStep = 100;
        public static double ElementClearence = 100;
        public static double ElementClearenceInFeets
        {
            get
            {
                return UnitUtils.Convert(ElementClearence,
                                           DisplayUnitType.DUT_MILLIMETERS,
                                           DisplayUnitType.DUT_DECIMAL_FEET);
            }
        }

        public static MEPCurve Elem1Curve { get; set; }
        public static MEPCurve Elem2Curve { get; set; }

        public static string Elem1SystemName;

        public static double Elem1Width;
        public static double Elem1Height;
        public static double Elem2Width;
        public static double Elem2Height;

        public static Line Elem1StartCenterLine;
        public static List<Line> Elem1StartGeneralLines;

        public static bool Elem1IsRectangular;
        public static bool Elem2IsRectangular;

        public static XYZ MoveVector;

        public static double Elem1A;
        public static double Elem1AX;
        public static double Elem1AY;

        public static List<Element> ConnectedToElem1Elements
        {
            get 
            {
                return ConnectorUtils.GetConnectedElements(Elem1Curve);
            }
        }


        public static void GetAllData()
        {
            LinesUtils linesUtils = new LinesUtils(null);

            Elem1StartCenterLine = linesUtils.CreateCenterLine(new ElementCenterLine(Elem1Curve));
            Elem1StartGeneralLines = linesUtils.CreateGeneralLines(new ElementGeneralLines(Elem1Curve));

            MaxZCoordinate = MaxZCoordinate - ElementClearenceInFeets;

            List<ElementId> connectedIds = ConnectedToElem1Elements.Select(el => el.Id).ToList();

            NotConnectedToElem1ElementIds = Data.AllModelElementsIds;
            foreach (ElementId elementId in connectedIds)
            {
                NotConnectedToElem1ElementIds.Remove(elementId);
                NotConnectedToElem1ElementIds.Remove(Elem1Curve.Id);
            }
        }

        public static XYZ GetNormOffset(double offsetNorm, int dxy, int dz)
        {
            ElementUtils.GetPoints(Elem1Curve, out XYZ startPoint1, out XYZ endPoint1, out XYZ centerPointElement1);
            ElementUtils.GetPoints(Elem2Curve, out XYZ startPoint2, out XYZ endPoint2, out XYZ centerPointElement2);

            double alfa;
            double beta;
            double offsetNormF;

            double fullOffsetX = 0;
            double fullOffsetY = 0;
            double fullOffsetZ = 0;
            Elem1A = 0;
            Elem1AX = 0;
            Elem1AY = 0;

            offsetNormF = UnitUtils.Convert(offsetNorm / 1000,
                                   DisplayUnitType.DUT_METERS,
                                   DisplayUnitType.DUT_DECIMAL_FEET);

            ElementSize elementSize = new ElementSize(Elem1Curve, Elem2Curve);
            elementSize.GetSizes();

            //int side = 1;
            //if (!CheckProjection())
            //    side = -1;

            //dxy = dxy * side;

            if (Math.Round(startPoint1.X, 3) == Math.Round(endPoint1.X, 3))
            {
                fullOffsetX = (Elem2Width + Elem1Width) / 2 + dxy * (centerPointElement2.X - centerPointElement1.X) + offsetNormF;
            }
            else if (Math.Round(startPoint1.Y, 3) == Math.Round(endPoint1.Y, 3))
            {
                fullOffsetY = (Elem2Width + Elem1Width) / 2 + dxy * (centerPointElement2.Y - centerPointElement1.Y) + offsetNormF;
            }
            else
            {
                Elem1A = (endPoint1.Y - startPoint1.Y) / (endPoint1.X - startPoint1.X);

                alfa = Math.Atan(Elem1A);
                double angle = alfa * (180 / Math.PI);
                beta = 90 * (Math.PI / 180) - alfa;
                angle = beta * (180 / Math.PI);

                Elem1AX = Math.Cos(beta);
                Elem1AY = Math.Sin(beta);

                double H = centerPointElement2.Y + Elem1A * (centerPointElement1.X - centerPointElement2.X);

                double deltaCenter = (centerPointElement1.Y - H) * Math.Cos(alfa);

                double fullOffset = ((Elem2Width + Elem1Width) / 2 + deltaCenter + offsetNormF);

                //Get full offset of element B from element A              
                fullOffsetX = fullOffset * Elem1AX;
                fullOffsetY = -fullOffset * Elem1AY;
            }


            fullOffsetZ = (Elem2Height + Elem1Height) / 2 + dz * (centerPointElement2.Z - centerPointElement1.Z) + offsetNormF;


            XYZ XYZoffset = new XYZ(dxy * (fullOffsetX), dxy * (fullOffsetY), dz * (fullOffsetZ));

            return XYZoffset;
        }
    }

}
