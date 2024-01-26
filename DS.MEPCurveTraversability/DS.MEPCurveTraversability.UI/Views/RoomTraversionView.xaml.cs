using Autodesk.Revit.DB;
using DS.MEPCurveTraversability.Presenters;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DS.MEPCurveTraversability.UI
{
    /// <summary>
    /// Interaction logic for RoomTraversionViewModel.xaml
    /// </summary>
    public partial class RoomTraversionView : MetroWindow
    {
        private readonly RoomTraversionViewModel _viewModel;
        private readonly CheckDocsConfigView _checkDocsConfigView;

        public RoomTraversionView(
            RoomTraversionViewModel viewModel, 
            CheckDocsConfigView checkDocsConfigView)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            _viewModel = viewModel;
            _checkDocsConfigView = checkDocsConfigView;
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

        private void RoomNamesCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RoomNamesCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            ItemToAdd.Text = string.Empty;
            _viewModel.ItemToAdd = string.Empty;
        }
    }
}
