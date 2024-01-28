using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.MEPCurveTraversability.Interactors.Settings;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Various;

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

        var settingsKR = DocSettingsKR.GetInstance().
            TryUpdateDocs(doc, allLoadedLinks) as DocSettingsKR;
        var settingsAR = DocSettingsAR.GetInstance().
            TryUpdateDocs(doc, allLoadedLinks) as DocSettingsAR;

        var validatorsSet = new MEPCurveValidatorSet(
            uiDoc,
            allLoadedLinks,
            mEPCurve,
            elementFilter,
            settingsAR,
            settingsKR)
            { 
                WindowMessenger = messenger, 
                Logger = logger,
                TransactionFactory = trf
            }
            .Create();


        validatorsSet.TrueForAll(v => v.IsValid(mEPCurve));

        return Result.Succeeded;
    }
}