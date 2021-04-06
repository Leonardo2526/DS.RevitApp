using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS_SystemTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;


namespace AddProjectParameters
{
    /// <summary>
    /// Interaction logic for AddParametersToSFPOptions.xaml
    /// </summary>
    public partial class SelectGroup : Window
    {
        public UIApplication App;

        //Lists for log
        List<string> ParametersAdded = new List<string>();
        List<string> GroupsNamesList = new List<string>();
        List<string> ParametersNamesList = new List<string>();
        public List<string> SelectesParameters = new List<string>();

        string selectedGroupName;

        //Get current date and time    
        readonly string CurDate = DateTime.Now.ToString("yyMMdd");
        readonly string CurDateTime = DateTime.Now.ToString("yyMMdd_HHmmss");

        public SelectGroup(UIApplication app)
        {
            InitializeComponent();
            this.App = app;

            //Fill groups names comboBox
            GetGroupsNames();
        }


        void ApplySelectedParameters()
        {
            string delimiter = "\n";
            string StringOutput = SelectesParameters.Aggregate((i, j) => i + delimiter + j);
            MessageBox.Show(StringOutput);

            /*
            if (selectedGroupName != "")
            {
                //LoadParametersToSPF();
                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Fill a group name!");
            }
            */
        }

        void GetGroupsNames()
        {
            App.Application.SharedParametersFilename = StartForm.SPFPath;
            DefinitionFile def = App.Application.OpenSharedParameterFile();

            IEnumerator<DefinitionGroup> enumerate = def.Groups.GetEnumerator();

            while (enumerate.MoveNext())  
            {
                string item = enumerate.Current.Name;  
                GroupsNamesList.Add(item);
                GroupsNames.Items.Add(item);
            }
            enumerate.Reset();
        }

        void GetParametersNames()
        {
            App.Application.SharedParametersFilename = StartForm.SPFPath;
            DefinitionGroup def = App.Application.OpenSharedParameterFile().
                Groups.get_Item(selectedGroupName);

            IEnumerator<Definition> enumerate = def.Definitions.GetEnumerator();

            while (enumerate.MoveNext())
            {
                string item = enumerate.Current.Name;
                ParametersNames.Items.Add(item);
            }
            enumerate.Reset();
        }


        void WriteLogToFile(DS_Tools dS_Tools)
        {
            dS_Tools.DS_StreamWriter("Path to shared parameters file: " + "\n" + StartForm.SPFPath + "\n");
            dS_Tools.DS_StreamWriter("Group: " + selectedGroupName + "\n");
            dS_Tools.DS_StreamWriter("Added parameters:");

            //Saved file names with empty models
            if (ParametersAdded.Count != 0)
            {
                dS_Tools.DS_StreamWriter(dS_Tools.DS_ListToString(ParametersAdded));
            }

            dS_Tools.DS_FileExistMessage();
        }

        private void GroupsNames_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ParametersNames.Items.Clear();
            selectedGroupName = GroupsNames.SelectedItem.ToString();
            GetParametersNames();
        }

        private void ParametersNames_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void ApplySelection_Click(object sender, RoutedEventArgs e)
        {
            if (ParametersNames.SelectedItems.Count != 0)
            {

                foreach (object it in ParametersNames.SelectedItems)
                {
                    SelectesParameters.Add(it.ToString());
                }

                ParametersNames.SelectedItems.Clear();

                this.Close();
            }
            else
                MessageBox.Show("No files selected!");

            ApplySelectedParameters();
        }

        private void ApplyAllSelection_Click(object sender, RoutedEventArgs e)
        {
            foreach (object it in ParametersNames.Items)
            {
                SelectesParameters.Add(it.ToString());
            }

            this.Close();
            ApplySelectedParameters();
        }
    }
}
