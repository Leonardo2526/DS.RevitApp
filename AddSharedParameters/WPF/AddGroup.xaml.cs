using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Windows;

namespace AddSharedParameters
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
            App.Application.SharedParametersFilename = EntryCommand.SPFPath;

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
