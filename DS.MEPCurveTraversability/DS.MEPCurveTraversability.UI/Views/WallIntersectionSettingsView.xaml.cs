using DS.MEPCurveTraversability.Presenters;
using MahApps.Metro.Controls;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DS.MEPCurveTraversability.UI
{
    public partial class WallIntersectionSettingsView : MetroWindow
    {
        private readonly CheckDocsConfigView _checkDocsConfigView;

        public WallIntersectionSettingsView(
            WallCheckerViewModel wallCheckerViewModel,
            CheckDocsConfigView checkDocsConfigView)
        {
            InitializeComponent();
            this.DataContext = wallCheckerViewModel;
            _checkDocsConfigView = checkDocsConfigView;
            ShowDialog();
        }

        private void ConfigDocs_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _checkDocsConfigView.ShowDialog();
        }

        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            var ucs = this.WallIntersectionSettingsPanel.FindChildren<UserControl>();
            ucs.ToList().ForEach(uc => uc.IsEnabled = true);
            Item1.IsEnabled = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var ucs = this.WallIntersectionSettingsPanel.FindChildren<UserControl>();
            ucs.ToList().ForEach(uc => uc.IsEnabled = false);
            Item1.IsEnabled = false;
        }
    }
}
