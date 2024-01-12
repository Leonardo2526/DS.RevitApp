using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils;
using Serilog;
using System;
using DS.MEPTools.Core;
using DS.RevitLib.Utils.Various;
using DS.MEPTools.Core.Rooms;
using Serilog.Core;
using System.Diagnostics;

namespace DS.MEPTools.RoomTraversabilityChecker
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;

            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiApp.ActiveUIDocument.Document;

            var logger = new LoggerConfiguration()
           .MinimumLevel.Information()
           .WriteTo.Debug()
           .CreateLogger();
            var trf = new ContextTransactionFactory(doc, RevitContextOption.Inside);
            var elementMultiFilter = new ElementMutliFilter(doc);

            //MEPCurve mEPCurve1 = null;
            //if (new ElementSelector(uiDoc).Pick() is not MEPCurve mEPCurve1) { return Result.Failed; }

            //new GetGeomeryElements(uiDoc, elementMultiFilter)
            //{
            //    Logger = logger,
            //    TransactionFactory = trf
            //}.GetElements(mEPCurve1);
            //return Autodesk.Revit.UI.Result.Succeeded;


            if (new ElementSelector(uiDoc).Pick() is not MEPCurve mEPCurve) { return Result.Failed; }

            var roomIntersectFactory = new SolidRoomIntersectionFactory(doc, elementMultiFilter)
            { Logger = logger, TransactionFactory = null };
            var elementIntersectFactory = new SolidElementIntersectionFactory(doc, elementMultiFilter)
            { Logger = logger, TransactionFactory = null };
            new MEPCurveRoomTraversabilityService(doc, roomIntersectFactory, elementIntersectFactory)
            {
                Logger = logger,
                TransactionFactory = trf, 
                IsSolidTraversabilityEnabled = true
            }.IsTraversable(mEPCurve);

            //new RoomSolidChecker(uiDoc)
            //{
            //    Logger = logger,
            //    TransactionFactory = trf
            //}.
            //Initiate();
            //GetRooms();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}