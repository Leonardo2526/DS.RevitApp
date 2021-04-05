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

        string GroupName;

        //Lists for log
        List<string> ParametersAdded = new List<string>();
        List<string> ParametersNames = new List<string>();
        List<string> ParametersTypes = new List<string>();


        //Get current date and time    
        readonly string CurDate = DateTime.Now.ToString("yyMMdd");
        readonly string CurDateTime = DateTime.Now.ToString("yyMMdd_HHmmss");

        public AddParametersToSFPOptions(UIApplication app)
        {
            InitializeComponent();
            this.App = app;
            GetParameterTypes();

            TypesNames.Text = ParameterType.Text.ToString();
        }

       

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (GroupNameText.Text != "")
            {
                GroupName = GroupNameText.Text;

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
           
            string selectedTypeName = TypesNames.SelectedItem.ToString();

            ParameterType type = (ParameterType)Enum.Parse(typeof(ParameterType), selectedTypeName, true);

            string[] names = File.ReadAllLines(StartForm.ParametersNamesPath);
            try
            {
                foreach (string name in names)
                {
                    //Add curent parameter name
                    AddParameterToSPF(name, type, true);
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

        public void AddParameterToSPF(string name, ParameterType type, bool visible)
        {
            App.Application.SharedParametersFilename = StartForm.SPFPath;

            ExternalDefinitionCreationOptions opt = new ExternalDefinitionCreationOptions(name, type)
            {
                Visible = visible
            };

            DefinitionFile def = App.Application.OpenSharedParameterFile();
          
            try 
            {
                def.Groups.Create(GroupName).Definitions.Create(opt);
            }
            catch
            {
                def.Groups.get_Item(GroupName).Definitions.Create(opt);
            }

            //Add parameter name to list
            ParametersAdded.Add(name);
        }

        void WriteLogToFile(DS_Tools dS_Tools)
        {
            dS_Tools.DS_StreamWriter("Path to shared parameters file: " + "\n" + StartForm.SPFPath + "\n");
            dS_Tools.DS_StreamWriter("Group: " + GroupName + "\n");
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
                //ParametersTypes.Add(typeName.ToString());
                TypesNames.Items.Add(typeName.ToString());
            }
        }

        private void TypesNames_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
