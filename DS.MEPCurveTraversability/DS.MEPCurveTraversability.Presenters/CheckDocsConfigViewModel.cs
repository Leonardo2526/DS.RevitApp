using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.MEPCurveTraversability.Interactors;
using Rhino;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UnitSystem = Rhino.UnitSystem;

namespace DS.MEPCurveTraversability.Presenters
{
    public class CheckDocsConfigViewModel
    {
        private readonly (Document, IEnumerable<RevitLinkInstance>) _availableDocLinks;
        private (Document, IEnumerable<RevitLinkInstance>) _checkerDocsLinks;
        private readonly List<Document> _allAvailableDocs;

        public CheckDocsConfigViewModel(
            (Document, IEnumerable<RevitLinkInstance>) availableDocLinks,
            (Document, IEnumerable<RevitLinkInstance>) checkerDocsLinks)
        {
            _availableDocLinks = availableDocLinks;
            _checkerDocsLinks = checkerDocsLinks;
            _allAvailableDocs = new List<Document>()
            { _availableDocLinks.Item1};
            var linkDocs = availableDocLinks.Item2.Select(l => l.GetLinkDocument()).ToList();
            _allAvailableDocs.AddRange(linkDocs);  

            var checkerDocs = ToDocs(checkerDocsLinks);
            checkerDocs.ForEach(d => CheckerDocNames.Add(d.Title));

            var availableDocs = ToDocs(availableDocLinks);
            foreach (var doc in availableDocs)
            {
                if (!checkerDocs.Any(d => d.Title == doc.Title))
                { AvailableDocNames.Add(doc.Title); }
            }

            //CheckerDocNames.CollectionChanged += CheckerDocNames_CollectionChanged;
        }


        public string Text { get; set; } = "Test";

        public string DocNameToAdd { get; set; }
        public string DocNameToRemove { get; set; }

        public ObservableCollection<string> AvailableDocNames { get; } = new();
        public ObservableCollection<string> CheckerDocNames { get; } = new();

        #region Commands

        public ICommand AddItem => new RelayCommand(p =>
        {
            CheckerDocNames.Add(DocNameToAdd);
            AvailableDocNames.Remove(DocNameToAdd);
        }, _ => DocNameToAdd != null);

        public ICommand RemoveItem => new RelayCommand(p =>
        {
            string nameToRemove = DocNameToRemove is null ?
            CheckerDocNames.Last() : DocNameToRemove;

            CheckerDocNames.Remove(nameToRemove);
            AvailableDocNames.Add(nameToRemove);

        }, _ => CheckerDocNames.Count > 0);

        public ICommand CloseWindow => new RelayCommand(p =>
        {
            var allDocs = _allAvailableDocs.Where(d => CheckerDocNames.Any(n => d.Title == n));
            var docsLinks = ToDocLinks(allDocs, _availableDocLinks.Item2);
            _checkerDocsLinks.Item1 = docsLinks.Item1;
            _checkerDocsLinks.Item2 = docsLinks.Item2;
        }, _ => true);

        #endregion

        private static List<Document> ToDocs(
            (Document, IEnumerable<RevitLinkInstance>) docLinks)
        {
            var docs = new List<Document>();

            if (docLinks.Item1 == null && docLinks.Item2 == null)
            { return docs; }

            if (docLinks.Item1 != null)
            { docs.Add(docLinks.Item1); }

            if (docLinks.Item2 != null)
            { docs.AddRange(docLinks.Item2.Select(r => r.GetLinkDocument())); }

            return docs;
        }

        private static (Document, IEnumerable<RevitLinkInstance>) ToDocLinks(
            IEnumerable<Document> docs, 
            IEnumerable<RevitLinkInstance> sourceLinks)
        {
            Document doc = docs.FirstOrDefault(d => !d.IsLinked);
            var links = sourceLinks.Where(l => docs.Any(d => d.Title == l.GetLinkDocument().Title));

            return (doc, links);
        }

    }
}

