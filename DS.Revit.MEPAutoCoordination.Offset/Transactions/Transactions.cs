using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace DS.Revit.MEPAutoCoordination.Offset
{

    interface ITransaction
    {
        void CreateTransaction();
    }

    interface IModelCurveTransaction
    {
        ModelCurve GetModelCurve();
    }

    class TransactionUtils
    {
        public void MoveElement(ITransaction transaction)
        {
            transaction.CreateTransaction();
        }

        public void CreateModelCurve(IModelCurveTransaction transaction)
        {
            transaction.GetModelCurve();
        }

    }



    class MoveElementTransaction : ITransaction
    {
        readonly Document Doc;
        readonly ElementId ElemId;
        readonly XYZ Vector;


        public MoveElementTransaction(Document doc, ElementId elemId, XYZ vector)
        {
            Doc = doc;
            ElemId = elemId;
            Vector = vector;
        }

        public void CreateTransaction()
        {
            using (Transaction transNew = new Transaction(Doc, "automep_MoveElement"))
            {
                try
                {
                    transNew.Start();
                    ElementTransformUtils.MoveElement(Doc, ElemId, Vector);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }
            //UIDocument uIDocument = new UIDocument(Doc);
            //uIDocument.RefreshActiveView();
        }

    }

    class CreateModelCurveTransaction : IModelCurveTransaction
    {
        readonly Document Doc;
        readonly XYZ StartPoint;
        readonly XYZ EndPoint;

        public CreateModelCurveTransaction(Document doc, XYZ startPoint, XYZ endPoint)
        {
            Doc = doc;
            StartPoint = startPoint;
            EndPoint = endPoint;
        }

        public ModelCurve GetModelCurve()
        {
            ModelCurve line = null;

            Line geomLine = Line.CreateBound(StartPoint, EndPoint);

            // Create a geometry plane in Revit application
            XYZ p1 = StartPoint;
            XYZ p2 = EndPoint;
            XYZ p3 = new XYZ();

            if (Math.Abs(p1.X - p2.X) < 0.01 & Math.Abs(p1.Y - p2.Y) < 0.01)
                p3 = p2 + XYZ.BasisY;
            else
                p3 = p2 + XYZ.BasisZ;
            Plane geomPlane = Plane.CreateByThreePoints(p1, p2, p3);

            using (Transaction transNew = new Transaction(Doc, "automep_CreateModelLine"))
            {
                try
                {
                    transNew.Start();

                    // Create a sketch plane in current document
                    SketchPlane sketch = SketchPlane.Create(Doc, geomPlane);

                    // Create a ModelLine element using the created geometry line and sketch plane
                    line = Doc.Create.NewModelCurve(geomLine, sketch) as ModelCurve;
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }

                transNew.Commit();
            }
            //UIDocument uIDocument = new UIDocument(Doc);
            //uIDocument.RefreshActiveView();
            return line;
        }
    }

}
