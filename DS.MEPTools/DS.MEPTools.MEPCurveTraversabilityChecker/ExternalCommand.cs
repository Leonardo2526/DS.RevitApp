using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.MEPTools.Core;
using DS.MEPTools.Core.Rooms;
using DS.MEPTools.WallTraversabilityChecker;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Various;
using Serilog;

namespace DS.MEPTools.MEPCurveTraversabilityChecker;

/// <inheritdoc />
[Transaction(TransactionMode.Manual)]
public class ExternalCommand : IExternalCommand
{
    /// <inheritdoc />
    public Result Execute(ExternalCommandData commandData,
        ref string message, ElementSet elements)
    {
        bool checkRooms = true;

        var uiApp = commandData.Application;
        var application = uiApp.Application;
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;

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

        if (checkRooms)
        {
            var roomIntersectFactory = new SolidRoomIntersectionFactory(doc, elementFilter)
            { Logger = logger, TransactionFactory = null };
            var roomCheker = new RoomChecker(
                uiDoc,
                elementFilter,
                elementIntersectFactory,
                roomIntersectFactory)
            {
                Logger = logger,
                //TransactionFactory = trf,
                WindowMessenger = messenger
            }.Initiate(mEPCurve);

            if (!roomCheker) { return Result.Failed; }
        }

        new WallsChecker(uiDoc, elementIntersectFactory)
        {
            Logger = logger,
            //TransactionFactory = trf,
            WindowMessenger = messenger

        }.Initiate(mEPCurve);

        return Result.Succeeded;
    }
}