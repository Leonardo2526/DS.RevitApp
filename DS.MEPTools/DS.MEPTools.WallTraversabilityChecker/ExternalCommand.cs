using System;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using DS.MEPTools.OpeningsCreator;
using DS.MEPTools.WallTraversabilityChecker;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Openings;
using DS.RevitLib.Utils.Various;
using Serilog;

namespace DS.MEPTools.WallTraversabilityChecker;

/// <inheritdoc />
[Transaction(TransactionMode.Manual)]
public class ExternalCommand : IExternalCommand
{
    /// <inheritdoc />
    public Result Execute(ExternalCommandData commandData,
        ref string message, ElementSet elements)
    {
        var uiApp = commandData.Application;
        var application = uiApp.Application;
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;

        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .CreateLogger();
        var trf = new ContextTransactionFactory(doc, RevitContextOption.Inside);

       new WallsChecker(uiDoc)
       {
            Logger = logger,
            TransactionFactory = trf

       }.Initiate();

        return Result.Succeeded;
    }
}