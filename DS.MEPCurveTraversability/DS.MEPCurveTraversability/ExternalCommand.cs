using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.MEPCurveTraversability.Interactors;
using DS.MEPCurveTraversability.Presenters;
using DS.MEPCurveTraversability.UI;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Various;
using Rhino;
using Serilog;
using Serilog.Core;
using System.Collections.Generic;
using UnitSystem = Rhino.UnitSystem;

namespace DS.MEPCurveTraversability;

/// <inheritdoc />
[Transaction(TransactionMode.Manual)]
public class ExternalCommand : IExternalCommand
{
    private static readonly double _mmToFeet =
            RhinoMath.UnitScale(UnitSystem.Millimeters, UnitSystem.Feet);

    /// <inheritdoc />
    public Result Execute(ExternalCommandData commandData,
        ref string message, ElementSet elements)
    {
        //var view = new TraceSettingsView();
        var viewModel = new WallCheckerViewModel(App.WallIntersectionSettingsKR);
        var view = new WallIntersectionSettingsView(viewModel);
        return Result.Succeeded;

        var uiApp = commandData.Application;
        var application = uiApp.Application;
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;

        var links = doc.GetLoadedLinks();

        var docAR = doc;
        var docKR = doc;
        var linksAR = links;
        var linksKR = links;

        var logger = App.Logger;
        var messenger = App.Messenger;
        var trf = new ContextTransactionFactory(doc, RevitContextOption.Inside);
        var elementFilter = new ElementMutliFilter(doc, links);

        if (new ElementSelector(uiDoc).Pick() is not MEPCurve mEPCurve) { return Result.Failed; }

        var checkServiceAR = GetCheckService(
            uiDoc,
            doc,
            links,
            docAR,
            linksAR,
            logger,
            trf,
            messenger,
            elementFilter,
            App.WallIntersectionSettingsAR);
        if (!checkServiceAR.Initiate(mEPCurve)) { return Result.Failed; }

        var checkServiceKR = GetCheckService(
            uiDoc,
            doc,
            links,
            docKR,
            linksKR,
            logger,
            trf,
            messenger,
            elementFilter,
            App.WallIntersectionSettingsKR);
        checkServiceKR.Initiate(mEPCurve);
        if (!checkServiceKR.Initiate(mEPCurve)) { return Result.Failed; }

        return Result.Succeeded;
    }

    private static TraversabilityService GetCheckService(
        UIDocument uiDoc,
        Document doc,
        List<RevitLinkInstance> links,
        Document activeDocToCheck,
        List<RevitLinkInstance> linksToCheck,
        ILogger logger,
        ContextTransactionFactory trf,
        IWindowMessenger messenger,
        ElementMutliFilter elementFilter,
        WallIntersectionSettings intersectionSettings)
    {
        IElementMultiFilter serviceElementFilter;
        if (activeDocToCheck != null && linksToCheck != null)
        { serviceElementFilter = new ElementMutliFilter(activeDocToCheck, linksToCheck); }
        else if (linksToCheck != null)
        { serviceElementFilter = new ElementMutliFilter(linksToCheck); }
        else
        { serviceElementFilter = new ElementMutliFilter(activeDocToCheck); }

        var elementIntersectFactory = new SolidElementIntersectionFactory(doc, serviceElementFilter)
        { Logger = logger, TransactionFactory = null };

        return new TraversabilityService(uiDoc, links, elementFilter, elementIntersectFactory, intersectionSettings)
        {
            Logger = logger,
            TransactionFactory = trf,
            WindowMessenger = messenger
        };
    }
}