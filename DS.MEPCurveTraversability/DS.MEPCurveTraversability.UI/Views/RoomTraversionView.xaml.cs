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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

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


            SwitchItemsButSender<ContentControl>(SolidCheckBox, viewModel.CheckSolid);

            SwitchItemsButSender<ContentControl>(FieldsCheckBox, viewModel.CheckNames);
            ItemToAdd.IsEnabled = viewModel.CheckNames;
            CollectionsListBox1.IsEnabled = viewModel.CheckNames;

            ShowDialog();
        }

        #region ItemsCheckers
        private void ConfigDocs_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _checkDocsConfigView.ShowDialog();
        }

        private void SolidCheckBox_Checked(object sender, RoutedEventArgs e)
            => SwitchItemsButSender<ContentControl>(sender as ContentControl, true);

        private void SolidCheckBox_Unchecked(object sender, RoutedEventArgs e)
          => SwitchItemsButSender<ContentControl>(sender as ContentControl, false);

        private void FieldsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ItemToAdd.IsEnabled = true;
            CollectionsListBox1.IsEnabled = true;
            SwitchItemsButSender<ContentControl>(sender as ContentControl, true);
        }

        private void FieldsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ItemToAdd.IsEnabled = false;
            CollectionsListBox1.IsEnabled = false;
            SwitchItemsButSender<ContentControl>(sender as ContentControl, false);
        }

        private void AllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _viewModel.CheckEndPoints = true;
            _viewModel.CheckSolid = true;
            _viewModel.CheckNames = true;
        }

        private void AllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _viewModel.CheckEndPoints = false;
            _viewModel.CheckSolid = false;
            _viewModel.CheckNames = false;
        }


        #endregion


        private static void SwitchItemsButSender<T>(
            ContentControl contentControl,
            bool swichOption) where T : ContentControl
        {
            var parent = contentControl.GetParentObject();
            var ucs = parent.FindChildren<T>().Where(e => e.Name != contentControl.Name);
            ucs.ToList().ForEach(uc => uc.IsEnabled = swichOption);
        }


        private void TrySwitchWithCheckBox<T>(object sender) where T : UIElement
        {
            var checkBox = (CheckBox)sender;
            SwitchChildren<T>(checkBox.GetParentObject(), checkBox.IsChecked == true);

        }
        private void SwitchChildren<T>(DependencyObject parent, bool swichOption) where T : UIElement
        {
            var ucs = parent.FindChildren<T>();
            ucs.ToList().ForEach(uc => uc.IsEnabled = swichOption);
        }

    }
}
