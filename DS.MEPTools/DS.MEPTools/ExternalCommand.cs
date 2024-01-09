using System;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using DS.MEPTools.OpeningsCreator;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Openings;
using DS.RevitLib.Utils.Various;
using Serilog;

namespace DS.MEPTools;

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

        var mepCurve = new ElementSelector(uiDoc).Pick() as MEPCurve ?? throw new ArgumentNullException();
        var wall = new ElementSelector(uiDoc).Pick() as Wall ?? throw new ArgumentNullException();
        //var floor = new ElementSelector(uiDoc).Pick() as Floor ?? throw new ArgumentNullException();
        //wall.Solid();
        //var con = wall.GetBestConnected();

        //var joined1 = JoinGeometryUtils.GetJoinedElements(doc, wall);
        //var joined2 = JoinGeometryUtils.GetJoinedElements(doc, floor);
        //return Result.Succeeded;

        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .CreateLogger();
        var trf = new ContextTransactionFactory(doc);
        var solid = mepCurve.GetSolidWithInsulation();
        var result2 = GetResultBySolid(doc, solid, wall, logger, trf);
        logger.Information("Result: " + result2);

        return Result.Succeeded;
    }

    private static bool GetResultByMepCurve(Document doc, MEPCurve mEpCurve, Element wall, ILogger logger,
        ITransactionFactory trf)
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
        (MEPCurve, Element) mArg = (mEpCurve, wall);

        //(Solid, Element) arg = (null, wall);
        var b = Func(mArg);
        if (wBuilder.ValidationResults.Any()) logger?.Information(wBuilder.ValidationResults.ToString());

        return b;

        bool Func((MEPCurve, Element) f) => f.Item2 is Wall wall1 && wBuilder.IsValid((wall1, f.Item1));
    }

    private static bool GetResultBySolid(Document doc,
        Solid solid,
        Element wall,
        ILogger logger,
        ITransactionFactory trf)
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
            JointsOffset = 0,
            Logger = logger,
            TransactionFactory = trf
        };

        (Solid, Element) mArg = (solid, wall);

        var b = Func(mArg);
        if (!wBuilder.ValidationResults.Any()) return b;
        var sb = new StringBuilder();
        wBuilder.ValidationResults.ToList().ForEach(r => sb.Append(r.ErrorMessage));
        logger?.Information(sb.ToString());

        return b;

        bool Func((Solid, Element) f) => f.Item2 is Wall wall1 && wBuilder.IsValid((wall1, f.Item1));
    }
}