using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS_SystemTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace AddProjectParameters
{
    /// <summary>
    /// Interaction logic for AddParametersToSFPOptions.xaml
    /// </summary>
    public partial class AddParametersToSFPOptions : Window
    {
        public UIApplication App;

        //Lists for log
        List<string> ParametersAdded = new List<string>();
        List<string> ParametersNames = new List<string>();
        List<string> ParametersTypes = new List<string>();
        List<string> GroupsNamesList = new List<string>();

        string selectedGroupName;
        string selectedTypeName;

        //Get current date and time    
        readonly string CurDate = DateTime.Now.ToString("yyMMdd");
        readonly string CurDateTime = DateTime.Now.ToString("yyMMdd_HHmmss");

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

            if (selectedGroupName != "" & selectedTypeName !="")
            {
                LoadParametersToSPF();
                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Fill a group name!");
            }
        }

        public void LoadParametersToSPF()
        {
            App.Application.SharedParametersFilename = StartForm.SPFPath;
            DefinitionGroup def = App.Application.OpenSharedParameterFile().
                Groups.get_Item(selectedGroupName);

            ParameterType type = (ParameterType)Enum.Parse(typeof(ParameterType), selectedTypeName, true);

            string[] names = File.ReadAllLines(StartForm.ParametersNamesPath);
            try
            {
                foreach (string name in names)
                {
                    //Add curent parameter name
                    AddParameterToSPF(def, name, type, true);
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
                DS_LogOutputPath = Path.GetDirectoryName(StartForm.SPFPath) + "\\" + "Log" + "\\"
            };
            WriteLogToFile(dS_Tools);
        }

        public void AddParameterToSPF(DefinitionGroup def, string name, ParameterType type, bool visible)
        {
            ExternalDefinitionCreationOptions opt = new ExternalDefinitionCreationOptions(name, type)
            {
                Visible = visible
            };
          
            def.Definitions.Create(opt);

            //Add parameter name to list
            ParametersAdded.Add(name);
        }

        public void GetGroupsNames()
        {
            App.Application.SharedParametersFilename = StartForm.SPFPath;
            DefinitionFile def = App.Application.OpenSharedParameterFile();

            IEnumerator<DefinitionGroup> enume = def.Groups.GetEnumerator();

            while (enume.MoveNext())   // пока не будет возвращено false
            {
                string item = enume.Current.Name;     // берем элемент на текущей позиции
                GroupsNamesList.Add(item);
                GroupsNames.Items.Add(item);
            }
            enume.Reset(); // сбрасываем указатель в начало массива

            //GroupsNamesList.AddRange((IEnumerable<string>)def.Groups);
           

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
