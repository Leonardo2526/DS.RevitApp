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
          
            SwitchChildren<UserControl>(
                WallIntersectionCheckBox.GetParentObject(), 
                WallIntersectionCheckBox.IsChecked == true);

            ShowDialog();
        }

        private void ConfigDocs_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _checkDocsConfigView.ShowDialog();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            SwitchChildren<UserControl>(checkBox.GetParentObject(), checkBox.IsChecked == true);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            SwitchChildren<UserControl>(checkBox.GetParentObject(), checkBox.IsChecked == true);
        }

        private void SwitchChildren<T>(DependencyObject parent, bool swichOption) where T : UIElement
        {
            var ucs = parent.FindChildren<T>();
            ucs.ToList().ForEach(uc => uc.IsEnabled = swichOption);
        }
    }
}
