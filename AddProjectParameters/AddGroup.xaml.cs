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
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AddProjectParameters
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class GroupAddWindow : Window
    {
        public UIApplication App;
        string GroupName;

        public GroupAddWindow(UIApplication app)
        {
            InitializeComponent();
            this.App = app;
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (GroupNameText.Text != "")
            {
                GroupName = GroupNameText.Text;

                AddGroupToSPF();

                this.Close();

                AddParametersToSFPOptions addParametersToSFPOptions = new AddParametersToSFPOptions(App);
                addParametersToSFPOptions.Show();
            }
            else
            {
                System.Windows.MessageBox.Show("Fill a group name!");
            }
        }

        public void AddGroupToSPF()
        {
            App.Application.SharedParametersFilename = StartForm.SPFPath;

            DefinitionFile def = App.Application.OpenSharedParameterFile();

            try
            {
                def.Groups.Create(GroupName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.Message);
            }
        }

    }
}
