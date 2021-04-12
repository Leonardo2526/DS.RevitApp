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
    public partial class SelectParameters : Window
    {
        private ExternalEvent m_ExEvent;
        private ExternalEventHandler m_Handler;
        public UIApplication App;

        //Lists for log
        List<string> ParametersAdded = new List<string>();
        List<string> GroupsNamesList = new List<string>();
        

        public static string SelectedGroupName;

        //Get current date and time    
        readonly string CurDate = DateTime.Now.ToString("yyMMdd");
        readonly string CurDateTime = DateTime.Now.ToString("yyMMdd_HHmmss");

        public SelectParameters(UIApplication app, ExternalEvent exEvent, ExternalEventHandler handler)
        {
            InitializeComponent();
            this.App = app;
            m_ExEvent = exEvent;
            m_Handler = handler;

            //Fill groups names comboBox
            GetGroupsNames();
        }


        void ApplySelectedParameters()
        {
           
            if (StartForm.SelectesParameters.Count != 0)
            {
                //Start loading process
                m_ExEvent.Raise();

                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Select parameters!");
            }
            
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
                Groups.get_Item(SelectedGroupName);

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
            dS_Tools.DS_StreamWriter("Group: " + SelectedGroupName + "\n");
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
            SelectedGroupName = GroupsNames.SelectedItem.ToString();
            GetParametersNames();

            if (ParametersNames.Items.Count != 0)
            {
                ApplySelection.IsEnabled = true;
                ApplyAllSelection.IsEnabled = true;
            }
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
                    StartForm.SelectesParameters.Add(it.ToString());
                }

                ParametersNames.SelectedItems.Clear();

                this.Close();
            }

            ApplySelectedParameters();
        }

        private void ApplyAllSelection_Click(object sender, RoutedEventArgs e)
        {
            if (ParametersNames.Items.Count != 0)
            {
                foreach (object it in ParametersNames.Items)
                {
                    StartForm.SelectesParameters.Add(it.ToString());
                }
                this.Close();
            }

            ApplySelectedParameters();
        }
    }
}
