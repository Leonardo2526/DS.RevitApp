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

        var doc = uiDoc.Document;
        var links = doc.GetLoadedLinks2();
        var appContainer = AppSettings.AppContainer;
        var settings = appContainer.GetInstance<DocSettingsAR>();

        new ViewBuilder().ShowSettingsView(doc, links, settings, "АР");

        return Result.Succeeded;
    }
}