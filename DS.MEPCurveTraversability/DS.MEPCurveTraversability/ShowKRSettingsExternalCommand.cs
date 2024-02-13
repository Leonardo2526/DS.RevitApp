using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.MEPCurveTraversability.Interactors;
using DS.MEPCurveTraversability.Interactors.Settings;
using DS.MEPCurveTraversability.Presenters;
using DS.MEPCurveTraversability.UI;
using OLMP.RevitAPI.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.MEPCurveTraversability;

/// <inheritdoc />
[Transaction(TransactionMode.Manual)]
public class ShowKRSettingsExternalCommand : IExternalCommand
{
    private IEnumerable<Document> _allDocs;
    private DocSettingsKR _settings;

    /// <inheritdoc />
    public Result Execute(ExternalCommandData commandData,
        ref string message, ElementSet elements)
    {
        var uiApp = commandData.Application;
        var application = uiApp.Application;
        var uiDoc = uiApp.ActiveUIDocument;

        var doc = uiDoc.Document;
        var links = doc.GetLoadedLinks2();
        var appContainer = AppSettings.AppContainer;

        var docIndexSettings = appContainer.GetInstance<DocIndexSettings>();
        var settings = docIndexSettings.GetSettings(
            doc,
            application,
            appContainer.GetInstance<DocSettingsAR>,
            appContainer.GetInstance<DocSettingsKR>);
        var docSettingsKR = settings.OfType<DocSettingsKR>().FirstOrDefault();

        new ViewBuilder().ShowSettingsView(doc, links, docSettingsKR, "КР");

        return Result.Succeeded;
    }
}