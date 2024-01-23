using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.MEPCurveTraversability.Interactors;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Various;
using OLMP.RevitAPI.UI;
using Serilog;

namespace DS.MEPCurveTraversability;

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

        var links = doc.GetLoadedLinks();

        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .CreateLogger();
        var trf = new ContextTransactionFactory(doc, RevitContextOption.Inside);
        var messenger = new TaskDialogMessenger();
        var elementFilter = new ElementMutliFilter(doc);
        var elementIntersectFactory = new SolidElementIntersectionFactory(doc, elementFilter)
        { Logger = logger, TransactionFactory = null };

        if (new ElementSelector(uiDoc).Pick() is not MEPCurve mEPCurve) { return Result.Failed; }

        new TraversabilityService(uiDoc, links, elementFilter, elementIntersectFactory)
        {
            Logger = logger,
            TransactionFactory = trf,
            WindowMessenger = messenger
        }.
        Initiate(mEPCurve);

        return Result.Succeeded;
    }
}