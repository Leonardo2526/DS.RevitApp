using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.MEPTools.OpeningsCreator;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Openings;
using DS.RevitLib.Utils.Various;
using Serilog;
using System;
using System.Threading.Tasks;

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
            var profileCreator = new RectangleProfileCreator(doc)
            {
                Offset = 100.MMToFeet(),
                TransactionFactory = trf
            };
            var wBuilder = new WallOpeningRuleBuilder(doc, mEPCurve, profileCreator)
            {
                InsertsOffset = 500.MMToFeet(),
                WallOffset = 1000.MMToFeet(),
                Logger = logger,
                TransactionFactory = trf
            };

            //var result1 = wBuilder.GetRule(wall);
            //logger?.Information("Result: " + result1);
            //return Autodesk.Revit.UI.Result.Succeeded;
            (Solid, Element) arg = (null, wall);
            var result2 = wBuilder.GetRuleFunc().Invoke(arg);
            logger?.Information("Result: " + result2);

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}
