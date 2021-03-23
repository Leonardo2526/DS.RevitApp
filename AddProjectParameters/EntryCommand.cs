using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AddProjectParameters
{
    [Transaction(TransactionMode.Manual)]
    public class EntryCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit,
         ref string message, ElementSet elements)
        {
            try
            {
                MyApplication.thisApp.ShowForm(revit.Application);
                /*
                UserControl1 userControl = new UserControl1();
                userControl.Show();
                */
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

        }
    }



}
