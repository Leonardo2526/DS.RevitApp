using DS.ClassLib.VarUtils;
using MoreLinq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace DS.MEPCurveTraversability.Presenters
{
    public class ExchangeItemsViewModel
    {
        private readonly IEnumerable<string> _source;
        private readonly IEnumerable<string> _target;

        public ExchangeItemsViewModel(IEnumerable<string> source, IEnumerable<string> target)
        {
            _source = source;
            source.ForEach(ObservableSource.Add);
            _target = target;
            target.ForEach(ObservableTarget.Add);
        }

        public string SelectedSource { get; set; }
        public string SelectedTarget { get; set; }

        public ObservableCollection<string> ObservableSource { get; } = new();
        public ObservableCollection<string> ObservableTarget { get; } = new();

        #region Commands

        public ICommand AddItem => new RelayCommand(p =>
        {
            string sourceItem = SelectedSource is null ?
           ObservableSource.First() : SelectedSource;

            ObservableTarget.Add(sourceItem);
            ObservableSource.Remove(sourceItem);
        }, _ => ObservableSource.Count > 0);

        public ICommand AddAllItems => new RelayCommand(p =>
        {
            ObservableSource.ToList().ForEach(d => ObservableTarget.Add(d));
            ObservableSource.Clear();
        }, _ => ObservableSource.Count > 0);

        public ICommand RemoveItem => new RelayCommand(p =>
        {
            string targetItem = SelectedTarget is null ?
            ObservableTarget.Last() : SelectedTarget;

            ObservableTarget.Remove(targetItem);
            ObservableSource.Add(targetItem);

        }, _ => ObservableTarget.Count > 0);

        public ICommand RemoveAllItems => new RelayCommand(p =>
        {
            ObservableTarget.ToList().ForEach(d => ObservableSource.Add(d));
            ObservableTarget.Clear();
        }, _ => ObservableTarget.Count > 0);

        #endregion



    }
}