using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Iterators;
using DS.MEPCurveTraversability.Interactors;
using DS.MEPCurveTraversability.Interactors.Settings;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Intersections;
using OLMP.RevitAPI.Tools.Various;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        var allLoadedLinks = doc.GetLoadedLinks() ?? new List<RevitLinkInstance>();
        var allFilteredDocs = new List<Document>() { doc };
        allFilteredDocs.AddRange(allLoadedLinks.Select(l => l.GetLinkDocument()));

        var logger = AppSettings.Logger;
        var messenger = AppSettings.Messenger;
        var appContainer = AppSettings.AppContainer;
        var trf = new ContextTransactionFactory(doc, RevitContextOption.Inside);

        //create global filter
        var excludedCategories = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_GenericAnnotation,
            BuiltInCategory.OST_TelephoneDevices,
            BuiltInCategory.OST_Materials,
            BuiltInCategory.OST_GenericModel,
            BuiltInCategory.OST_Massing
        };
        var globalFilter = new DocumentFilter(allFilteredDocs, doc, allLoadedLinks);
        globalFilter.QuickFilters =
        [
            (new ElementMulticategoryFilter(excludedCategories, true), null),
            (new ElementIsElementTypeFilter(true), null),
        ];

        if (new ElementSelector(uiDoc).Pick() is not MEPCurve mEPCurve)
        { return Result.Failed; }

        var docIndexSettings = appContainer.GetInstance<DocIndexSettings>();
        var settings = docIndexSettings.GetSettings(
            doc,
            application,
            appContainer.GetInstance<DocSettingsAR>,
            appContainer.GetInstance<DocSettingsKR>);
        var docSettingsAR = settings.OfType<DocSettingsAR>().FirstOrDefault();
        docSettingsAR.TrySetFilteredAutoDocs(doc, allLoadedLinks);
        var docSettingsKR = settings.OfType<DocSettingsKR>().FirstOrDefault();
        docSettingsKR.TrySetFilteredAutoDocs(doc, allLoadedLinks);

        var validators = new ValidatorFactory(
            uiDoc,
            allLoadedLinks,
            globalFilter,
            docSettingsAR,
            docSettingsKR)
        {
            WindowMessenger = null,
            Logger = logger,
            TransactionFactory = trf
        }
            .Create();

        var mEPCurveValidators = validators.OfType<IValidator<MEPCurve>>();
        var validatorIterator = new ValidatorIterator<MEPCurve>(mEPCurveValidators)
        { Logger = logger, StopOnFirst = false };

        //Validation

        //Test(logger, mEPCurve, validatorIterator);
        var isValid = validatorIterator.IsValid(mEPCurve);
        if (isValid)
        { logger?.Information("MEPCurve is traversable"); }
        else
        { logger?.Warning("MEPCurve isn't traversable"); }

        var resultMessage = ValidationResultsConverter.
            ToString(validatorIterator.ValidationResults);

        if (!string.IsNullOrEmpty(resultMessage)) { messenger?.Show(resultMessage); }

        return Result.Succeeded;
    }

    private static void Test(
        ILogger logger,
        MEPCurve mEPCurve,
        ValidatorIterator<MEPCurve> validatorIterator)
    {
        var time1 = DateTime.Now;

        
        int total = 0;
        for (int i = 0; i < 317; i++)
        {
            //Debug.WriteLine("----- STEP " + i);
            var isValid = validatorIterator.IsValid(mEPCurve);
            //if (isValid)
            //{ logger?.Information("MEPCurve is traversable"); }
            //else
            //{ logger?.Warning("MEPCurve isn't traversable"); }
            total++;
        }
        var time2 = DateTime.Now;
        TimeSpan totalInterval = time2 - time1;
        logger?.Information($"Total steps is {total}");
        logger?.Information($"Task resolved in {(int)totalInterval.TotalMilliseconds} ms successfully!");
    }
}