using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace DS.Revit.MEPAutoCoordination.Offset
{
    [Transaction(TransactionMode.Manual)]
    internal class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit,
           ref string message, ElementSet elements)
        {
            TaskDialog.Show("Revit", "Hello World");
            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}