using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.CollisionsElliminator
{
    class ElementMover
    {
        /// <summary>
        /// Move element by move vector.
        /// </summary>
        public static bool Move(ElementId elemetnID, Application App, XYZ MoveVector)
        {
            TransactionUtils transactionUtils = new TransactionUtils();

            ITransaction moveElementTransaction = new MoveElementTransaction(Data.Elem1.Document, elemetnID, MoveVector);

            FailureEvent failureProcess = new FailureEvent(App);
            failureProcess.RegisterEvent();

            transactionUtils.MoveElement(moveElementTransaction);

            if (!failureProcess.IsErrorConnection)
            {
                failureProcess.ExitEvent();
                return true;
            }
            else
            {
                failureProcess.ExitEvent();
                return false;
            }
        }
    }
}
