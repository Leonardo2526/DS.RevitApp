using DS.MEPCurveTraversability.Presenters;
using MahApps.Metro.Controls;
using System.Windows;

namespace DS.MEPCurveTraversability.UI
{
    /// <summary>
    /// Interaction logic for CheckDocsConfigView.xaml
    /// </summary>
    public partial class CheckDocsConfigView : MetroWindow
    {

        public CheckDocsConfigView(ExchangeItemsViewModel configViewModel)
        {
            InitializeComponent();
            this.DataContext = configViewModel;
            ConfigViewModel = configViewModel;
        }

        public ExchangeItemsViewModel ConfigViewModel { get; }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
    }
}
