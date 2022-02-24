using Autodesk.Revit.DB;
using DS.Revit.ExternalUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.Revit.MEPAutoCoordination.Offset
{

    class Data
    {
        public static Document Doc { get; set; }
        public static double MaxZCoordinate;

        public static double MinZCoordinate;

        //AllModelElements
        public static List<Element> AllModelElements { get; set; }

        public static List<ElementId> AllModelElementsIds;
        public static List<ElementId> NotConnectedToElem1ElementIds;

        public static List<Element> AllLinkedElements { get; set; }
    
        public static List<ElementId> AllLinkedElementsIds 
        { 
            get
            { return AllLinkedElements.Select(x => x.Id).ToList(); }
            set { }
        }
        public static List<RevitLinkInstance> AllLinks { get; set; }

        public static double SearchStep = 100;
        public static double ElementClearence = 100;
        public static double ElementClearenceInFeets;

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

        public static List<ElementId> ConnectedToElem1Elements;


        public void GetAllData()
        {
            var dims1 = IvanovUtils.GetMEPCurveDimensions(Elem1Curve);
            var dims2 = IvanovUtils.GetMEPCurveDimensions(Elem2Curve);
            if (Math.Abs(dims1.area - dims2.area) < 0.01)
                CheckLength();

            //Get all model elements Ids
            AllModelElementsIds = new List<ElementId>();
            foreach (Element element in AllModelElements)
                AllModelElementsIds.Add(element.Id);

            if (AllModelElementsIds.Count != 0)
            {
                FilteredElementCollector collector = new FilteredElementCollector(Doc, AllModelElementsIds);
                ElementCategoryFilter elementCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_TelephoneDevices, true);

                AllModelElements = collector.WherePasses(elementCategoryFilter).ToElements().ToList();

                AllModelElementsIds = new List<ElementId>();
                foreach (Element element in AllModelElements)
                    AllModelElementsIds.Add(element.Id);
            }


            GetLinkedElements();

            ElementClearenceInFeets = UnitUtils.Convert(ElementClearence / 1000,
                                           DisplayUnitType.DUT_METERS,
                                           DisplayUnitType.DUT_DECIMAL_FEET);

            LinesUtils linesUtils = new LinesUtils(null);

            Elem1StartCenterLine = linesUtils.CreateCenterLine(new ElementCenterLine(Elem1Curve));
            Elem1StartGeneralLines = linesUtils.CreateGeneralLines(new ElementGeneralLines(Elem1Curve));

            MaxZCoordinate = MaxZCoordinate - ElementClearenceInFeets;

            ConnectedToElem1Elements = GetConnectedElemenets(Elem1Curve);

            NotConnectedToElem1ElementIds = Data.AllModelElementsIds;
            foreach (ElementId elementId in ConnectedToElem1Elements)
            {
                NotConnectedToElem1ElementIds.Remove(elementId);
                NotConnectedToElem1ElementIds.Remove(Elem1Curve.Id);
            }
        }

        public static XYZ GetNormOffset(double offsetNorm, int dxy, int dz)
        {
            ElementUtils elementUtils = new ElementUtils();
            elementUtils.GetPoints(Elem1Curve, out XYZ startPoint1, out XYZ endPoint1, out XYZ centerPointElement1);
            elementUtils.GetPoints(Elem2Curve, out XYZ startPoint2, out XYZ endPoint2, out XYZ centerPointElement2);

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


        public XYZ GetStartOffset(double offset)
        {
            ElementUtils elementUtils = new ElementUtils();
            elementUtils.GetPoints(Elem1Curve, out XYZ startPointA, out XYZ endPointA, out XYZ centerPointElementA);

            double offsetF = UnitUtils.Convert(offset / 1000,
                                   DisplayUnitType.DUT_METERS,
                                   DisplayUnitType.DUT_DECIMAL_FEET);

            XYZ XYZoffset = new XYZ(centerPointElementA.X + offsetF, centerPointElementA.Y + offsetF, centerPointElementA.Z + offsetF);

            return XYZoffset;
        }

        bool CheckProjection()
        {
            ElementUtils elementUtils = new ElementUtils();
            elementUtils.GetPoints(Elem1Curve, out XYZ Elem1StartPoint, out XYZ Elem1EndPoint, out XYZ Elem1CenterPoint);
            elementUtils.GetPoints(Elem2Curve, out XYZ Elem2StartPoint, out XYZ Elem2EndPoint, out XYZ Elem2CenterPoint);

            XYZ Elem2Vector = Elem2EndPoint - Elem2StartPoint;
            XYZ deltaVector = Elem1CenterPoint - Elem2CenterPoint;

            double angleRad = 90.0 / (180 / Math.PI) - Elem2Vector.AngleTo(deltaVector);
            double angle = angleRad * (180 / Math.PI);

            double gyp = Elem2CenterPoint.DistanceTo(Elem1CenterPoint);
            double projection = gyp * Math.Sin(angleRad);

            if (projection < 0)
                return false;

            return true;
        }

        /// <summary>
        /// Get sizes of Elem1 and Elem2 and set class properties
        /// </summary>


        void CheckLength()
        {
            string type1 = Elem1Curve.GetType().ToString();
            string type2 = Elem2Curve.GetType().ToString();

            if (!type1.Contains("FamilyInstance") && !type2.Contains("FamilyInstance"))
            {
                LocationCurve lc1 = Elem1Curve.Location as LocationCurve;
                LocationCurve lc2 = Elem2Curve.Location as LocationCurve;
                double l1 = lc1.Curve.ApproximateLength;
                double l2 = lc2.Curve.ApproximateLength;
                if (l2 < l1)
                {
                    Elem1Curve = Elem2Curve;
                    Elem2Curve = Elem1Curve;
                }

            }
        }

        void GetLinkedElements()
        {
            if (AllLinkedElementsIds.Count != 0)
            {
                FilteredElementCollector collector = new FilteredElementCollector(Doc, AllLinkedElementsIds);
                ElementCategoryFilter elementCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_TelephoneDevices, true);

                AllLinkedElements = collector.WherePasses(elementCategoryFilter).ToElements().ToList();
                AllLinkedElementsIds = AllLinkedElements.Select(x => x.Id).ToList();
            }


        }

        public List<Connector> GetConnectors(MEPCurve mepCurve)
        {
            //1. Get connector set of MEPCurve
            ConnectorSet connectorSet = mepCurve.ConnectorManager.Connectors;

            //2. Initialise empty list of connectors
            List<Connector> connectorList = new List<Connector>();

            //3. Loop through connector set and add to list
            foreach (Connector connector in connectorSet)
            {
                connectorList.Add(connector);
            }
            return connectorList;
        }

        public List<ElementId> GetConnectedElemenets(MEPCurve mepCurve)
        {
            List<Connector> connectors = GetConnectors(mepCurve);

            List<ElementId> connectedElements = new List<ElementId>();
            foreach (Connector connector in connectors)
            {
                ConnectorSet connectorSet = connector.AllRefs;

                foreach (Connector con in connectorSet)
                {
                    ElementId elementId = con.Owner.Id;
                    if (elementId != Elem1Curve.Id)
                        connectedElements.Add(elementId);
                }


            }


            return connectedElements;
        }
    }

}
