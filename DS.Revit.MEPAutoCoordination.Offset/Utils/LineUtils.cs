using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.Revit.MEPAutoCoordination.Offset
{


    interface IElementCenterLine
    {
        Line CreateCenterLine(XYZ offset, bool show);
        Line CreateCenterLineByStaticPoint(XYZ offset, XYZ staticPoint, bool show);
    }

    class ElementCenterLine : IElementCenterLine
    {
        readonly Element Elem;

        public ElementCenterLine(Element elem)
        {
            Elem = elem;
        }

        public Line CreateCenterLine(XYZ offset, bool show)
        {
            ElementUtils ElemUtils = new ElementUtils();
            TransactionUtils transactionUtils = new TransactionUtils();

            ElemUtils.GetPoints(Elem, out XYZ startPoint, out XYZ endPoint, out XYZ centerPointElement);

            if (offset == null)
                offset = new XYZ();

            if (show)
                transactionUtils.CreateModelCurve(new CreateModelCurveTransaction(Elem.Document, startPoint + offset, endPoint + offset));

            return Line.CreateBound(startPoint + offset, endPoint + offset);
        }

        public Line CreateCenterLineByStaticPoint(XYZ offset, XYZ staticPoint, bool show)
        {
            ElementUtils ElemUtils = new ElementUtils();
            TransactionUtils transactionUtils = new TransactionUtils();

            ElemUtils.GetPoints(Elem, out XYZ startPoint, out XYZ endPoint, out XYZ centerPointElement);

            List<XYZ> glStartPoints = new List<XYZ>();
            glStartPoints.Add(startPoint);

            List<XYZ> glEndPoints = new List<XYZ>();
            glEndPoints.Add(endPoint);

            PointUtils pointUtils = new PointUtils();
            pointUtils.GetStaticListsPoins(glStartPoints, glEndPoints, startPoint, endPoint, staticPoint,
            out List<XYZ> movablePoints, out List<XYZ> staticPoints);

            if (offset == null)
                offset = new XYZ();

            if (show)
                transactionUtils.CreateModelCurve(new CreateModelCurveTransaction(Elem.Document, staticPoints[0], movablePoints[0] + offset));

            return Line.CreateBound(staticPoints[0], movablePoints[0] + offset);
        }
    }


    interface IElementGeneralLines
    {
        List<Line> CreateGeneralLines(XYZ offset, bool show);
        List<Line> CreateGeneralLinesByStaticPoint(XYZ offset, XYZ staticPoint, bool show);
    }

    class ElementGeneralLines : IElementGeneralLines
    {
        readonly Element Elem;

        public ElementGeneralLines(Element elem)
        {
            Elem = elem;
        }

        public List<Line> CreateGeneralLines(XYZ offset, bool show)
        {

            IGeneralPointExtractor pointExtractor = new GeneralPointExtractor(Elem);
            PointUtils pointsUtils = new PointUtils();
            pointsUtils.GetGeneralPoints(pointExtractor, out List<XYZ> glStartPoints, out List<XYZ> glEndPoints);

            LinesCreator linesCreator = new LinesCreator();

            return linesCreator.CreateLines(Elem, glStartPoints, glEndPoints, offset, show);
        }

        public List<Line> CreateGeneralLinesByStaticPoint(XYZ offset, XYZ staticPoint, bool show)
        {

            IGeneralPointExtractor pointExtractor = new GeneralPointExtractor(Elem);
            PointUtils pointsUtils = new PointUtils();
            pointsUtils.GetGeneralPoints(pointExtractor, out List<XYZ> glStartPoints, out List<XYZ> glEndPoints);

            LinesCreator linesCreator = new LinesCreator();

            return linesCreator.CreateLinesByStaticPoint(Elem, glStartPoints, glEndPoints, offset, staticPoint, show);
        }
    }


    interface IElementClearanceLines
    {
        List<Line> CreateClearenceLines(XYZ offset, bool show);
    }

    class ElementClearanceLines : IElementClearanceLines
    {
        readonly Element Elem;

        public ElementClearanceLines(Element elem)
        {
            Elem = elem;
        }

        public List<Line> CreateClearenceLines(XYZ offset, bool show)
        {
            PointUtils pointsUtils = new PointUtils();
            TransactionUtils transactionUtils = new TransactionUtils();

            pointsUtils.GetClearancePoints(Elem, out List<XYZ> clStartPoints, out List<XYZ> clEndPoints);

            LinesCreator linesCreator = new LinesCreator();

            return linesCreator.CreateLines(Elem, clStartPoints, clEndPoints, offset, show);
        }
    }

    class LinesCreator
    {
        public List<Line> CreateLines(Element Elem, List<XYZ> glStartPoints, List<XYZ> glEndPoints, XYZ offset, bool show)
        {
            TransactionUtils transactionUtils = new TransactionUtils();

            List<Line> generalLines = new List<Line>();

            if (offset == null)
                offset = new XYZ();

            int j;
            for (j = 0; j < glStartPoints.Count; j++)
            {
                XYZ glPoint1 = new XYZ(glStartPoints[j].X + offset.X, glStartPoints[j].Y + offset.Y, glStartPoints[j].Z + offset.Z);
                XYZ glPoint2 = new XYZ(glEndPoints[j].X + offset.X, glEndPoints[j].Y + offset.Y, glEndPoints[j].Z + offset.Z);

                Line generalLine;

                if (Data.Elem1IsRectangular && j < glStartPoints.Count - 1)
                {
                    generalLine = Line.CreateBound(glStartPoints[j] + offset, glEndPoints[j + 1] + offset);
                    generalLines.Add(generalLine);

                    if (show)
                        transactionUtils.CreateModelCurve(new CreateModelCurveTransaction(Data.Doc, glStartPoints[j] + offset, glEndPoints[j + 1] + offset));

                }
                else if (!Data.Elem1IsRectangular)
                {
                    generalLine = Line.CreateBound(glPoint1, glPoint2);
                    generalLines.Add(generalLine);

                    if (show)
                        transactionUtils.CreateModelCurve(new CreateModelCurveTransaction(Elem.Document, glPoint1, glPoint2));
                }

            }

            return generalLines;
        }



        public List<Line> CreateLinesByStaticPoint(Element Elem, List<XYZ> glStartPoints, List<XYZ> glEndPoints, XYZ offset, XYZ staticPoint, bool show)
        {
            PointUtils pointUtils = new PointUtils();
            ElementUtils ElemUtils = new ElementUtils();

            ElemUtils.GetPoints(Elem, out XYZ startPoint, out XYZ endPoint, out XYZ centerPointElement);

            pointUtils.GetStaticListsPoins(glStartPoints, glEndPoints, startPoint, endPoint,
                staticPoint,
            out List<XYZ> movablePoints, out List<XYZ> staticPoints);

            TransactionUtils transactionUtils = new TransactionUtils();

            List<Line> generalLines = new List<Line>();

            if (offset == null)
                offset = new XYZ();

            int j;
            for (j = 0; j < glStartPoints.Count; j++)
            {
                XYZ glPoint1 = staticPoints[j];
                XYZ glPoint2 = new XYZ(movablePoints[j].X + offset.X, movablePoints[j].Y + offset.Y, movablePoints[j].Z + offset.Z);

                Line generalLine;

                if (Data.Elem1IsRectangular && j < glStartPoints.Count - 1)
                {
                    generalLine = Line.CreateBound(staticPoints[j], movablePoints[j + 1] + offset);
                    generalLines.Add(generalLine);

                    if (show)
                        transactionUtils.CreateModelCurve(new CreateModelCurveTransaction(Data.Doc, staticPoints[j], movablePoints[j + 1] + offset));

                }
                else if (!Data.Elem1IsRectangular)
                {
                    generalLine = Line.CreateBound(glPoint1, glPoint2);
                    generalLines.Add(generalLine);

                    if (show)
                        transactionUtils.CreateModelCurve(new CreateModelCurveTransaction(Elem.Document, glPoint1, glPoint2));
                }

            }

            return generalLines;
        }
    }

    class LinesUtils
    {
        readonly XYZ Offset;

        public LinesUtils(XYZ offset)
        {
            Offset = offset;
        }

        public Line CreateCenterLine(IElementCenterLine elementCenterLine, bool show = false)
        {
            return elementCenterLine.CreateCenterLine(Offset, show);
        }

        /// <summary>
        /// Create general lines of element with offset option from original point of element. 
        /// Lines are visible if true option is activated. Default value is false.
        /// </summary>
        public List<Line> CreateGeneralLines(IElementGeneralLines elementGeneralLines, bool show = false)
        {
            return elementGeneralLines.CreateGeneralLines(Offset, show);
        }

        public List<Line> CreateGeneralLinesByStaticPoint(IElementGeneralLines elementGeneralLines, XYZ staticPoint, bool show = false)
        {
            return elementGeneralLines.CreateGeneralLinesByStaticPoint(Offset, staticPoint, show);
        }
        public Line CreateCenterLineByStaticPoint(IElementCenterLine elementCenterLine, XYZ staticPoint, bool show = false)
        {
            return elementCenterLine.CreateCenterLineByStaticPoint(Offset, staticPoint, show);
        }

        /// <summary>
        /// Create lines of element with offset option from original point of element by clearence from general lines. 
        /// Lines are visible if true option is activated. Default value is false.
        /// </summary>
        public List<Line> CreateClearenceLines(IElementClearanceLines elementClearanceLines, bool show = false)
        {
            return elementClearanceLines.CreateClearenceLines(Offset, show);
        }

        public List<Line> CreateAllElementLines(Element element, XYZ moveVector, bool show = false)
        {
            List<Line> allLines = new List<Line>();

            Line centerline = CreateCenterLine(new ElementCenterLine(element), show);
            List<Line> generalLines = CreateGeneralLines(new ElementGeneralLines(element), show);
            List<Line> clearenceLines = CreateClearenceLines(new ElementClearanceLines(element), show);

            allLines.Add(centerline);
            allLines.AddRange(generalLines);
            allLines.AddRange(clearenceLines);

            return allLines;
        }

        public List<Line> CreateAllObstacledElementLines(Element element, XYZ point, XYZ moveVector, bool show = false)
        {
            List<Line> allLines = new List<Line>();
            List<Line> generalLines = new List<Line>();
            LinesUtils linesUtils = new LinesUtils(moveVector);
            Line centerline = null;

            generalLines = CreateGeneralLinesByStaticPoint(
                new ElementGeneralLines(element), point, show);

            centerline = CreateCenterLineByStaticPoint(
                new ElementCenterLine(element), point, show);


            allLines.Add(centerline);
            allLines.AddRange(generalLines);
            //allLines.AddRange(clearenceLines);

            return allLines;
        }
    }

    /// <summary>
    /// Example of LinesUtils methods implementation
    /// </summary>
    abstract class ElementLinesImplementation
    {
        void Main()
        {
            Element element = null;
            XYZ offset = new XYZ();

            LinesUtils linesUtils = new LinesUtils(offset);
            XYZ moveVector = new XYZ();

            Line centerline = linesUtils.CreateCenterLine(new ElementCenterLine(element));
            List<Line> generalLines = linesUtils.CreateGeneralLines(new ElementGeneralLines(element));
            List<Line> clearenceLines = linesUtils.CreateClearenceLines(new ElementClearanceLines(element));

            List<Line> allElementLines = linesUtils.CreateAllElementLines(element, moveVector);
        }
    }
}
