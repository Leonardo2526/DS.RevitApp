using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.MEPCurveTraversability.Interactors.Settings;
using DS.MEPCurveTraversability.Presenters;
using DS.MEPCurveTraversability.UI;
using OLMP.RevitAPI.Core.Extensions;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;

namespace DS.MEPCurveTraversability.Interactors
{
    public class ViewBuilder
    {
        public void ShowSettingsView(
            Document doc,
            IEnumerable<RevitLinkInstance> allLoadedLinks,
            DocSettingsBase settings, string viewTitle)
        {
            var allDocs = doc.GetDocuments();
            var allDocNames = allDocs.Select(d => d.Title);

            settings.TrySetFilteredAutoDocs(doc, allLoadedLinks);

            var targetDocNames = settings.Docs.Select(d => d.Title);

            var sourceDocNames = allDocNames.Except(targetDocNames);
            var exchangeKRItemsViewModel = new ExchangeItemsViewModel(sourceDocNames, targetDocNames);
            var checkDocsView = new CheckDocsConfigView(exchangeKRItemsViewModel);
            checkDocsView.Closing += CheckDocsView_Closing;


            var viewModel = new WallCheckerViewModel(settings.WallIntersectionSettings)
            { Title = viewTitle };
            var view = new WallIntersectionSettingsView(viewModel, checkDocsView);

            void CheckDocsView_Closing(object sender, EventArgs e)
            {
                if (sender is not CheckDocsConfigView view) { return; }

                var targedNames = view.ConfigViewModel.ObservableTarget;
                var docs = allDocs.Where(d => targedNames.Any(n => d.Title == n));
                settings.Docs.Clear();
                settings.Docs.AddRange(docs);

                return;
            }
        }

        /// <inheritdoc />
        public void ShowRoomSettingsView(Document doc,
            IEnumerable<RevitLinkInstance> allLoadedLinks,
            DocSettingsAR settings)
        {
            var allDocs = doc.GetDocuments();
            var allDocNames = allDocs.Select(d => d.Title);

            settings.TrySetFilteredAutoDocs(doc, allLoadedLinks);

            var targetDocNames = settings.Docs.Select(d => d.Title);

            var sourceDocNames = allDocNames.Except(targetDocNames);
            var exchangeKRItemsViewModel = new ExchangeItemsViewModel(sourceDocNames, targetDocNames);
            var checkDocsView = new CheckDocsConfigView(exchangeKRItemsViewModel);
            checkDocsView.Closing += CheckDocsView_Closing;


            var viewModel = new RoomTraversionViewModel(settings.RoomTraversionSettings)
            { Title = "АР" };
            var view = new RoomTraversionView(viewModel, checkDocsView);

            void CheckDocsView_Closing(object sender, EventArgs e)
            {
                if (sender is not CheckDocsConfigView view) { return; }

                var targedNames = view.ConfigViewModel.ObservableTarget;
                var docs = allDocs.Where(d => targedNames.Any(n => d.Title == n));
                settings.Docs.Clear();
                settings.Docs.AddRange(docs);

                return;
            }
        }
    }
}
