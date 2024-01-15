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
using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;
using DS.MEPTools.Core.Rooms.Traversability;
using System.Linq;
using DS.RevitLib.Utils.Extensions;

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

            var messenger = new TaskDialogMessenger();

            //MEPCurve mEPCurve1 = null;
            //if (new ElementSelector(uiDoc).Pick() is not MEPCurve mEPCurve1) { return Result.Failed; }

            //new GetGeomeryElements(uiDoc, elementMultiFilter)
            //{
            //    Logger = logger,
            //    TransactionFactory = trf
            //}.GetElements(mEPCurve1);
            //return Autodesk.Revit.UI.Result.Succeeded;

            var excludeFields = new List<string>()
            {
                "электр",
                //"2"
            };

            //get rooms
            var roomsFilter = new ElementMutliFilter(doc);
            roomsFilter.SlowFilters.Add((new RoomFilter(), null));
            var rooms = roomsFilter.ApplyToAllDocs().SelectMany(kv => kv.Value.ToElements(kv.Key)).OfType<Room>();


            if (new ElementSelector(uiDoc).Pick() is not MEPCurve mEPCurve) { return Result.Failed; }

            var roomIntersectFactory = new SolidRoomIntersectionFactory(doc, elementMultiFilter)
            { Logger = logger, TransactionFactory = null };
            var elementIntersectFactory = new SolidElementIntersectionFactory(doc, elementMultiFilter)
            { Logger = logger, TransactionFactory = null };

            IRoomTraverable<Solid> roomSolidFactory(IEnumerable<Room> rooms)
                => new SolidRoomTraversable(doc, rooms, roomIntersectFactory, elementIntersectFactory)
                {
                    Logger = logger,
                    ExcludeFields = excludeFields, 
                    WindowMessenger = messenger
                };

            IRoomTraverable<XYZ> roomPointFactory(IEnumerable<Room> rooms)
                => new PointRoomTraversable(doc, rooms)
                {
                    Logger = logger,
                    ExcludeFields = excludeFields,
                    WindowMessenger = messenger
                };


            //new MEPCurveRoomTraversabilityServiceOld(doc, roomIntersectFactory, elementIntersectFactory)
            //{
            //    Logger = logger,
            //    TransactionFactory = trf,
            //    IsSolidTraversabilityEnabled = true
            //}.IsTraversable(mEPCurve);


            new MEPCurveRoomTraversable(doc, roomPointFactory, roomSolidFactory, rooms)
            {
                Logger = logger,
                TransactionFactory = trf,
                IsSolidTraversabilityEnabled = false,
                WindowMessenger = null
            }.IsTraversable(mEPCurve);


            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}