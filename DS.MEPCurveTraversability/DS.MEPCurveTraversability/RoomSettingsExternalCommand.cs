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
public class RoomSettingsExternalCommand : IExternalCommand
{
    private IEnumerable<Document> _allDocs;
    private DocSettingsAR _settings;

    /// <inheritdoc />
    public Result Execute(ExternalCommandData commandData,
        ref string message, ElementSet elements)
    {
        var uiApp = commandData.Application;
        var application = uiApp.Application;
        var uiDoc = uiApp.ActiveUIDocument;

        var doc = uiDoc.Document;
        var links = doc.GetLoadedLinks2();
        _allDocs = doc.GetDocuments();
        var allDocNames = _allDocs.Select(d => d.Title);

        var appContainer = AppSettings.AppContainer;
        var settings = appContainer.GetInstance<DocSettingsAR>();

        new ViewBuilder().ShowRoomSettingsView(doc, links, settings);

        return Result.Succeeded;
    }

    private void CheckDocsView_Closing(object sender, EventArgs e)
    {
        if (sender is not CheckDocsConfigView view) { return; }

        var targedNames = view.ConfigViewModel.ObservableTarget;
        var docs = _allDocs.Where(d => targedNames.Any(n => d.Title == n));
        _settings.Docs.Clear();
        _settings.Docs.AddRange(docs);

        return;
    }
}