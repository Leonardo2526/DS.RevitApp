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
using System.Collections.Generic;
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
        //var application = uiApp.Application;
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

        var settingsAR = appContainer.GetInstance<DocSettingsAR>();
        settingsAR.TrySetFilteredAutoDocs(doc, allLoadedLinks);
        var settingsKR = appContainer.GetInstance<DocSettingsKR>();
        settingsKR.TrySetFilteredAutoDocs(doc, allLoadedLinks);

        var validators = new ValidatorFactory(
            uiDoc,
            allLoadedLinks,
            globalFilter,
            settingsAR,
            settingsKR)
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
}