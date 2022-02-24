using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using iUtils;
using MEPAutoCoordination;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.CollisionsElliminator
{
    class Data
    {
        readonly Document Doc;
        readonly ElementToFindCollision Element1;
        readonly ElementToFindCollision Element2;

        public static double MaxZCoordinate;

        public static double MinZCoordinate;

        public Data(Document doc, ElementToFindCollision elem1, ElementToFindCollision elem2, double maxZCoordinate, double minZCoordinate)
        {
            Doc = doc;
            Element1 = elem1;
            Element2 = elem2;
            MaxZCoordinate = maxZCoordinate;
            MinZCoordinate = minZCoordinate;
        }

        //AllModelElements
        public static List<Element> AllModelElements;
        public static List<ElementId> AllModelElementsIds;
        public static List<ElementId> NotConnectedToElem1ElementIds;

        public static List<Element> AllLinkedElements;
        public static List<ElementId> AllLinkedElementsIds;
        public static List<RevitLinkInstance> AllLinks;

        public static double SearchStep = 100;
        public static double ElementClearence = 100;
        public static double ElementClearenceInFeets;

        public static Element Elem1;
        public static Element Elem2;

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

        ElementUtils elementUtils = new ElementUtils();

        public void GetAllData()
        {
            
            //Set element as Elem1 with less size
            Elem1 = Element1.MEPCurve;
            Elem2 = Element2.MEPCurve;

            var dims1 = Utils.GetMEPCurveDimensions(Element1.MEPCurve);
            var dims2 = Utils.GetMEPCurveDimensions(Element2.MEPCurve);
            if (Math.Abs(dims1.area - dims2.area) < 0.01)
                CheckLength();

            //Get all model elements
            AllModelElements = MEPAutoCoordination.MEPAutoCoordinationCommand.
           Container.GetInstance<AllElementsGeometryService>().CurrentDocElements.
           Select(x => x.Element).Where(x=>x.IsValidObject).ToList();

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

            ElementClearenceInFeets =  UnitUtils.Convert(ElementClearence/1000,
                                           DisplayUnitType.DUT_METERS,
                                           DisplayUnitType.DUT_DECIMAL_FEET);

            LinesUtils linesUtils = new LinesUtils(null);

            Elem1StartCenterLine = linesUtils.CreateCenterLine(new ElementCenterLine(Elem1));
            Elem1StartGeneralLines = linesUtils.CreateGeneralLines(new ElementGeneralLines(Elem1));

            MaxZCoordinate = MaxZCoordinate - ElementClearenceInFeets;

            ConnectedToElem1Elements = GetConnectedElemenets(Element1.MEPCurve);

            NotConnectedToElem1ElementIds = Data.AllModelElementsIds;
            foreach (ElementId elementId in ConnectedToElem1Elements)
            {
                NotConnectedToElem1ElementIds.Remove(elementId);
                NotConnectedToElem1ElementIds.Remove(this.Element1.MEPCurve.Id);
            }
        }
       
        public XYZ GetNormOffset(double offsetNorm, int dxy, int dz)
        {

            elementUtils.GetPoints(Elem1, out XYZ startPoint1, out XYZ endPoint1, out XYZ centerPointElement1);
            elementUtils.GetPoints(Elem2, out XYZ startPoint2, out XYZ endPoint2, out XYZ centerPointElement2);

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

            ElementSize elementSize = new ElementSize(Element1.MEPCurve, Element2.MEPCurve);
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
            elementUtils.GetPoints(Elem1, out XYZ startPointA, out XYZ endPointA, out XYZ centerPointElementA);

            double offsetF = UnitUtils.Convert(offset / 1000,
                                   DisplayUnitType.DUT_METERS,
                                   DisplayUnitType.DUT_DECIMAL_FEET);

            XYZ XYZoffset = new XYZ(centerPointElementA.X + offsetF, centerPointElementA.Y + offsetF, centerPointElementA.Z + offsetF);

            return XYZoffset;
        }

        bool CheckProjection()
        {
            ElementUtils elementUtils = new ElementUtils();
            elementUtils.GetPoints(Data.Elem1, out XYZ Elem1StartPoint, out XYZ Elem1EndPoint, out XYZ Elem1CenterPoint);
            elementUtils.GetPoints(Data.Elem2, out XYZ Elem2StartPoint, out XYZ Elem2EndPoint, out XYZ Elem2CenterPoint);

            XYZ Elem2Vector = Elem2EndPoint - Elem2StartPoint;
            XYZ deltaVector = Elem1CenterPoint - Elem2CenterPoint;
         
            double angleRad = 90.0/(180 / Math.PI)- Elem2Vector.AngleTo(deltaVector);
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
            string type1 = Element1.MEPCurve.GetType().ToString();
            string type2 = Element2.MEPCurve.GetType().ToString();

            if (!type1.Contains("FamilyInstance") && !type2.Contains("FamilyInstance"))
            {
                LocationCurve lc1 = Element1.MEPCurve.Location as LocationCurve;
                LocationCurve lc2 = Element2.MEPCurve.Location as LocationCurve;
                double l1 = lc1.Curve.ApproximateLength;
                double l2 = lc2.Curve.ApproximateLength;
                if (l2 < l1)
                {
                    Elem1 = Element2.MEPCurve;
                    Elem2 = Element1.MEPCurve;
                }

            }
        }

        void GetLinkedElements()
        {
            //Get all linked models
            AllLinks = MEPAutoCoordination.MEPAutoCoordinationCommand.Container.
                GetInstance<AllElementsGeometryService>().AlllLinks.ToList();

            //Get all linked elements
            var LinksElems = MEPAutoCoordination.MEPAutoCoordinationCommand.
           Container.GetInstance<AllElementsGeometryService>().LinksElems;

            AllLinkedElements = LinksElems.SelectMany(x => x.Value).Select(x => x.Element).ToList();
            AllLinkedElementsIds = LinksElems.SelectMany(x => x.Value).Select(x => x.Element.Id).ToList();

            if (AllLinkedElementsIds.Count !=0)
            {
                FilteredElementCollector collector = new FilteredElementCollector(Doc, AllLinkedElementsIds);
                ElementCategoryFilter elementCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_TelephoneDevices, true);

                AllLinkedElements = collector.WherePasses(elementCategoryFilter).ToElements().ToList();
                AllLinkedElementsIds = LinksElems.SelectMany(x => x.Value).Select(x => x.Element.Id).ToList();
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
                    if (elementId != Elem1.Id)
                        connectedElements.Add(elementId);
                }

                   
            }


            return connectedElements;
        }
    }

}
