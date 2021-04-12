using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


using DS_Forms;
using DS_SystemTools;
using System.IO;
using System.Security.AccessControl;

namespace AddProjectParameters
{
    public partial class StartForm : Window
    {
        private ExternalEvent m_ExEvent;
        private ExternalEventHandler m_Handler;
        public UIApplication App;

        public static string SPFPath;
        public static string ParametersNamesPath;
        public static string FileWithProjectsPathes;

        public static List<string> SelectesParameters = new List<string>();
        public static List<string> FilesPathes = new List<string>();


        public StartForm(UIApplication app, ExternalEvent exEvent, ExternalEventHandler handler)
        {
            InitializeComponent();
            this.App = app;
            m_ExEvent = exEvent;
            m_Handler = handler;

            FilesPathes.Clear();
            SelectesParameters.Clear();
        }



        private void Button_AddParametersToSFP_Click(object sender, RoutedEventArgs e)
        {
            DS_Form newForm = new DS_Form
            {
                TopLevel = true
            };
            
            SPFPath = newForm.DS_OpenFileDialogForm_txt("", "Select shared parameter file for loading.").ToString();
            if (SPFPath == "")
            {
                newForm.Close();
                return;
            }

            ParametersNamesPath = newForm.DS_OpenFileDialogForm_txt("", "Select file with parameters names.").ToString();
            if (ParametersNamesPath == "")
            {
                newForm.Close();
                return;
            }

            AddParametersToSFPOptions addParametersToSFPOptions = new AddParametersToSFPOptions(App);
            addParametersToSFPOptions.Show();
        }

        private void Button_AddParametersToProject_Click(object sender, RoutedEventArgs e)
        {
            bool FileError = false;
            DS_Form newForm = new DS_Form
            {
                TopLevel = true
            };

            FileWithProjectsPathes = newForm.DS_OpenFileDialogForm_txt("", "Select file with pathes to *.rvt projects.").ToString();
            if (FileWithProjectsPathes == "")
            {
                newForm.Close();
                return;
            }
            CreateFilesPathesList(ref FileError);

            if (FileError == true)
                return;

            SPFPath = newForm.DS_OpenFileDialogForm_txt("", "Select shared parameter file for loading.").ToString();
                if (SPFPath == "")
                {
                    newForm.Close();
                    return;
                }
                this.Close();

            SelectParameters selectParameters = new SelectParameters(App, m_ExEvent, m_Handler);
            selectParameters.Show();
        }

        void CreateFilesPathesList(ref bool FileError)
        {

            string[] names = File.ReadAllLines(FileWithProjectsPathes);
            try
            {
                foreach (string name in names)
                {
                    char[] MyChar = { (char)34 };
                    string trimedName = name.Trim(MyChar);

                    if (File.Exists(trimedName))
                    {
                        if (HasWritePermissionOnDir(trimedName) == true && IsDocOpened(trimedName) == false)
                            FilesPathes.Add(trimedName);
                        else
                        {
                            MessageBox.Show("You haven't  permission to write this file: \n" + trimedName);
                            FileError = true;
                            return;
                        }

                    }                     
                    else
                    {
                        MessageBox.Show("No such file path exist : \n" + trimedName);
                        FileError = true;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.Message);
                FileError = true;
            }
        }

        private bool HasWritePermissionOnDir(string path)
        {
            var writeAllow = false;
            var writeDeny = false;
            var accessControlList = Directory.GetAccessControl(path);
            if (accessControlList == null)
                return false;
            var accessRules = accessControlList.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
            if (accessRules == null)
                return false;

            foreach (FileSystemAccessRule rule in accessRules)
            {
                if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write) continue;

                if (rule.AccessControlType == AccessControlType.Allow)
                    writeAllow = true;
                else if (rule.AccessControlType == AccessControlType.Deny)
                    writeDeny = true;
            }

            return writeAllow && !writeDeny;
        }

        private bool IsDocOpened(string docName)
        {
            string CurrentDocName = App.ActiveUIDocument.Document.PathName;
            if (docName == CurrentDocName)
                return true;
            return false;
        }
    }
}
