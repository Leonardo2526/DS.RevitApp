using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

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
                StartForm startForm = new StartForm(revit.Application);
                startForm.AddParametersToProject();

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
