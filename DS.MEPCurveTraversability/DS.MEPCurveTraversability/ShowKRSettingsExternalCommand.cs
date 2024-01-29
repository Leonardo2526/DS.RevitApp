using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.MEPCurveTraversability.Interactors;
using DS.MEPCurveTraversability.Interactors.Settings;
using DS.MEPCurveTraversability.Presenters;
using DS.MEPCurveTraversability.UI;
using OLMP.RevitAPI.Core.Extensions;
using OLMP.RevitAPI.Tools.Extensions;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows.Controls;
using UnitSystem = Rhino.UnitSystem;

namespace DS.MEPCurveTraversability;

/// <inheritdoc />
[Transaction(TransactionMode.Manual)]
public class ShowKRSettingsExternalCommand : IExternalCommand
{
    private static readonly double _mmToFeet =
            RhinoMath.UnitScale(UnitSystem.Millimeters, UnitSystem.Feet);
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
        _allDocs = doc.GetDocuments();
        var allDocNames = _allDocs.Select(d => d.Title);

        _settings = DocSettingsKR.GetInstance();
        //_settings.AutoDocsDetectionFields = new List<string>();
        //_settings.AutoDocsDetectionFields = new List<string>() {"Тест" };
        _settings.TryUpdateDocs(doc, links);

        var targetDocNames = _settings.Docs.Select(d => d.Title);

        var sourceDocNames = allDocNames.Except(targetDocNames);
        var exchangeKRItemsViewModel = new ExchangeItemsViewModel(sourceDocNames, targetDocNames);
        var checkDocsView = new CheckDocsConfigView(exchangeKRItemsViewModel);
        checkDocsView.Closing += CheckDocsView_Closing;


        var viewModel = new WallCheckerViewModel(_settings.WallIntersectionSettings)
        { Title = "КР" };
        var view = new WallIntersectionSettingsView(viewModel, checkDocsView);

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