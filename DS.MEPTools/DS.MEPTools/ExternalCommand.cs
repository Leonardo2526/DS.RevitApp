using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.MEPTools.OpeningsCreator;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Openings;
using DS.RevitLib.Utils.Various;
using Serilog;
using Serilog.Core;
using System;
using System.Linq;
using System.Text;

namespace DS.MEPTools
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application application = uiapp.Application;
            UIDocument uiDoc = uiapp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            var mEPCurve = new ElementSelector(uiDoc).Pick() as MEPCurve ?? throw new ArgumentNullException();
            var wall = new ElementSelector(uiDoc).Pick() as Wall ?? throw new ArgumentNullException();

            var logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Debug()
                    .CreateLogger();
            var trf = new ContextTransactionFactory(doc, RevitLib.Utils.RevitContextOption.Inside);
            var solid = mEPCurve.GetSolidWithInsulation();
            var result2 = GetResultBySolid(doc, solid, wall, logger, trf);
            logger?.Information("Result: " + result2);

            return Autodesk.Revit.UI.Result.Succeeded;
        }

        private static bool GetResultByMEPCurve(Document doc, MEPCurve mEPCurve, Wall wall, Logger logger, ContextTransactionFactory trf)
        {
            var profileCreator = new RectangleMEPCurveProfileCreator(doc)
            {
                Offset = 100.MMToFeet(),
                TransactionFactory = trf
            };
            var wBuilder = new WallOpeningProfileValidator<MEPCurve>(doc, profileCreator)
            {
                InsertsOffset = 500.MMToFeet(),
                WallOffset = 1000.MMToFeet(),
                Logger = logger,
                TransactionFactory = trf
            };

            //var result1 = wBuilder.GetRule(wall);
            //logger?.Information("Result: " + result1);
            //return Autodesk.Revit.UI.Result.Succeeded;
            (MEPCurve, Element) mArg = (mEPCurve, wall);

            Func<(MEPCurve, Element), bool> func = (f) =>
            {
                if (f.Item2 is not Wall wall) { return false; }
                return wBuilder.IsValid((wall, f.Item1));
            };

            //(Solid, Element) arg = (null, wall);
            var b = func.Invoke(mArg);
            if (wBuilder.ValidationResults.Any())
            { logger?.Information(wBuilder.ValidationResults.ToString()); }

            return b;
        }

        private static bool GetResultBySolid(Document doc, Solid solid, Wall wall, Logger logger, ContextTransactionFactory trf)
        {
            var profileCreator = new RectangleSolidProfileCreator(doc)
            {
                Offset = 100.MMToFeet(),
                TransactionFactory = trf, 
                Logger = logger
            };
            var wBuilder = new WallOpeningProfileValidator<Solid>(doc, profileCreator)
            {
                InsertsOffset = 500.MMToFeet(),
                WallOffset = 1000.MMToFeet(),
                JointsOffset = 500.MMToFeet(),
                Logger = logger,
                TransactionFactory = trf
            };

            (Solid, Element) mArg = (solid, wall);

            Func<(Solid, Element), bool> func = (f) =>
            {
                if (f.Item2 is not Wall wall) { return false; }
                return wBuilder.IsValid((wall, f.Item1));
            };

            var b = func.Invoke(mArg);
            if (wBuilder.ValidationResults.Any())
            {
                var sb = new StringBuilder();
                wBuilder.ValidationResults.ToList().ForEach(r => sb.Append(r.ErrorMessage));
                logger?.Information(sb.ToString());
            }

            return b;
        }
    }
}
