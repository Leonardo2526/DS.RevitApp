using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.MEPCurveTraversability.Interactors;
using DS.MEPCurveTraversability.Interactors.Settings;
using DS.MEPCurveTraversability.Presenters;
using DS.MEPCurveTraversability.UI;
using OLMP.RevitAPI.Core.Extensions;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.MEPCurveTraversability;

/// <inheritdoc />
[Transaction(TransactionMode.Manual)]
public class ShowARSettingsExternalCommand : IExternalCommand
{
    /// <inheritdoc />
    public Result Execute(ExternalCommandData commandData,
        ref string message, ElementSet elements)
    {
        var uiApp = commandData.Application;
        var application = uiApp.Application;
        var uiDoc = uiApp.ActiveUIDocument;

        var docs = application.Documents;
        var doc = uiDoc.Document;
        var code = doc.GetHashCode();

        var links = doc.GetLoadedLinks2();
        var appContainer = AppSettings.AppContainer;

        var docIndexSettings = appContainer.GetInstance<DocIndexSettings>();
        var settings = docIndexSettings.GetSettings(
            doc,
            application,
            appContainer.GetInstance<DocSettingsAR>,
            appContainer.GetInstance<DocSettingsKR>);
        var docSettingsAR = settings.OfType<DocSettingsAR>().FirstOrDefault();

        new ViewBuilder().ShowSettingsView(doc, links, docSettingsAR, "АР");

        return Result.Succeeded;
    }
}