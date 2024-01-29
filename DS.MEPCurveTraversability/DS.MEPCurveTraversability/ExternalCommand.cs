using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Iterators;
using DS.MEPCurveTraversability.Interactors.Settings;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Various;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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
        var allLoadedLinks = doc.GetLoadedLinks();

        var logger = AppSettings.Logger;
        var messenger = AppSettings.Messenger;
        var trf = new ContextTransactionFactory(doc, RevitContextOption.Inside);
        var elementFilter = new ElementMutliFilter(doc, allLoadedLinks);

        if (new ElementSelector(uiDoc).Pick() is not MEPCurve mEPCurve)
        { return Result.Failed; }

        var settingsKR = DocSettingsKR.GetInstance();
        settingsKR.TryUpdateDocs(doc, allLoadedLinks);
        var settingsAR = DocSettingsAR.GetInstance();
        settingsAR.Docs?.Clear();
        settingsAR.AutoDocsDetectionFields = new List<string>() { "АР", "Тест" };
        settingsAR.TryUpdateDocs(doc, allLoadedLinks);
        //return Result.Succeeded;

        var validatorsSet = new MEPCurveValidatorSet(
            uiDoc,
            allLoadedLinks,
            mEPCurve,
            elementFilter,
            settingsAR,
            settingsKR)
        {
            WindowMessenger = null,
            Logger = logger,
            TransactionFactory = trf
        }
            .Create();

        var iterator = validatorsSet.GetEnumerator();
        var validatorIterator = new ValidatorIterator<MEPCurve>(iterator)
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