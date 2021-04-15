using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS_SystemTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;


namespace AddSharedParameters
{
    /// <summary>
    /// Interaction logic for AddParametersToSFPOptions.xaml
    /// </summary>
    public partial class AddParametersToSFPOptions : Window
    {
        public UIApplication App;

        //Lists for log
        List<string> ParametersAdded = new List<string>();
        List<string> GroupsNamesList = new List<string>();

        string selectedGroupName;
        string selectedTypeName;

        //Get current date and time    
        readonly string CurDate = DateTime.Now.ToString("yyMMdd");
        readonly string CurDateTime = DateTime.Now.ToString("yyMMdd_HHmmss");

        DefinitionFile defF;

        public AddParametersToSFPOptions(UIApplication app)
        {
            InitializeComponent();
            this.App = app;

            //Fill groups names comboBox
            GetGroupsNames();
            GroupsNames.Text = GroupsNamesList[0];

            //Fill parameter types comboBox
            GetParameterTypes();
            TypesNames.Text = ParameterType.Text.ToString();
        }



        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            selectedGroupName = GroupsNames.SelectedItem.ToString();
            selectedTypeName = TypesNames.SelectedItem.ToString();

            if (selectedGroupName != "" & selectedTypeName != "")
            {
                LoadParametersToSPF();
                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Fill a group name!");
            }
        }

        List<string> CheckExistDefinitions(List<string> names)
        {
            List<string> existParameters = new List<string>();

            // iterate the Definition groups of this file
            foreach (DefinitionGroup myGroup in defF.Groups)
            {
                // iterate the difinitions
                foreach (Definition definition in myGroup.Definitions)
                {
                    if (names.Contains(definition.Name))
                        existParameters.Add(definition.Name);
                }
            }
            return existParameters;
        }

        public void LoadParametersToSPF()
        {
            App.Application.SharedParametersFilename = EntryCommand.SPFPath;
            DefinitionGroup defG = App.Application.OpenSharedParameterFile().
                Groups.get_Item(selectedGroupName);

            ParameterType type = (ParameterType)Enum.Parse(typeof(ParameterType), selectedTypeName, true);

            List<string> names = File.ReadAllLines(EntryCommand.ParametersNamesPath).ToList();

            //Check if parameters with this names alredy exists
            if (CheckExistDefinitions(names).Count != 0)
            {
                string delimiter = "\n";
                string StringOutput = CheckExistDefinitions(names).Aggregate((i, j) => i + delimiter + j);
                MessageBox.Show("Error occured! Parameters alredy exists: \n" + StringOutput);
                return;
            }

            try
            {
                foreach (string name in names)
                {
                    //Add curent parameter name
                    AddParameterToSPF(defG, name, type, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.Message);
            }

            //Log writer initiation
            DS_Tools dS_Tools = new DS_Tools
            {
                DS_LogName = CurDateTime + "_AddParametersToSPF_Log.txt",
                DS_LogOutputPath = Path.GetDirectoryName(EntryCommand.SPFPath) + "\\" + "Log" + "\\"
            };

            MessageBox.Show("Completed successfully!");

            WriteLogToFile(dS_Tools);
        }

        public void AddParameterToSPF(DefinitionGroup defG, string name, ParameterType type, bool visible)
        {

            ExternalDefinitionCreationOptions opt = new ExternalDefinitionCreationOptions(name, type)
            {
                Visible = visible
            };

            defG.Definitions.Create(opt);

            //Add parameter name to list
            ParametersAdded.Add(name);
        }

        public void GetGroupsNames()
        {
            App.Application.SharedParametersFilename = EntryCommand.SPFPath;
            defF = App.Application.OpenSharedParameterFile();

            IEnumerator<DefinitionGroup> enume = defF.Groups.GetEnumerator();

            while (enume.MoveNext())   // пока не будет возвращено false
            {
                string item = enume.Current.Name;     // берем элемент на текущей позиции
                GroupsNamesList.Add(item);
                GroupsNames.Items.Add(item);
            }
            enume.Reset(); // сбрасываем указатель в начало массива
        }

        void WriteLogToFile(DS_Tools dS_Tools)
        {
            dS_Tools.DS_StreamWriter("Path to shared parameters file: " + "\n" + EntryCommand.SPFPath + "\n");
            dS_Tools.DS_StreamWriter("Group: " + selectedGroupName + "\n");
            dS_Tools.DS_StreamWriter("Added parameters:");

            //Saved file names with empty models
            if (ParametersAdded.Count != 0)
            {
                dS_Tools.DS_StreamWriter(dS_Tools.DS_ListToString(ParametersAdded));
            }

            dS_Tools.DS_FileExistMessage();
        }


        public void GetParameterTypes()
        {
            foreach (var typeName in Enum.GetValues(typeof(ParameterType)))
            {
                TypesNames.Items.Add(typeName.ToString());
            }
        }

        private void TypesNames_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void Button_AddGroup_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            GroupAddWindow groups = new GroupAddWindow(App);
            groups.Show();

        }
    }
}
