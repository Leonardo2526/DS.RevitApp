using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using DS_Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace DS.RevitApp.FamiliesUpdate
{
    public partial class StartForms : Window
    {
        private ExternalEvent m_ExEvent;
        private ExternalEventHandler m_Handler;

        private ProjectFilesList ProjectFilesList;
        private FamiliesFilesList FamiliesFilesList;


        List<string> FileList = new List<string>();
        List<string> FamiliesList = new List<string>();

        public static bool MaskApply;

        public UIApplication App;
        public StartForms(UIApplication app, ExternalEvent exEvent, ExternalEventHandler handler)
        {
            InitializeComponent();
            this.App = app;
            m_ExEvent = exEvent;
            m_Handler = handler;
        }

        private void Button_Projects_Click(object sender, RoutedEventArgs e)
        {
            DialogForms dialogForms = new DialogForms();
            dialogForms.AssignSourcePath();

            if (DialogForms.SourcePath == "")
                return;
            

            Docs docs = new Docs(App);

            string ext = "rvt";
            docs.GetFiles(DialogForms.SourcePath, ext);
            docs.DirIterate(DialogForms.SourcePath, ext);
            FileList = docs.FileFullNames_Filtered;

            if (FileList.Count ==0)
            {
                System.Windows.MessageBox.Show("No files found!");
                return;
            }

            ProjectFilesList = new ProjectFilesList(FileList);
            ProjectFilesList.Show();
        }

        private void Button_Families_Click(object sender, RoutedEventArgs e)
        {
            DialogForms dialogForms = new DialogForms();
            dialogForms.AssignFamilyPath();

            if (DialogForms.FamilyPath == "")
                return;

            Docs docs = new Docs(App);

            string ext = "rfa";
            docs.GetFamilies(DialogForms.FamilyPath, ext);
            docs.DirFamiliesIterate(DialogForms.FamilyPath, ext);
            FamiliesList = docs.FileFullNames_Filtered;

            if (FamiliesList.Count == 0)
            {
                System.Windows.MessageBox.Show("No families found!");
                return;
            }

            FamiliesFilesList = new FamiliesFilesList(FamiliesList);
            FamiliesFilesList.Show();
        }

        private void Button_StartLoading_Click(object sender, RoutedEventArgs e)
        {
            if (FileList.Count == 0)
            {
                System.Windows.MessageBox.Show("Chose project files (*.rvt) at first.");
                return;
            }

            else if (FamiliesList.Count == 0)
            {
                System.Windows.MessageBox.Show("Chose families files (*.rfa) at first.");
                return;
            }

            //Create list with selected items
            //Files
            FileList.Clear();
            FileList.AddRange(ProjectFilesList.SelItems);

            //Families
            FamiliesList.Clear();
            FamiliesList.AddRange(FamiliesFilesList.SelItems);
           
            m_Handler.FileList = FileList;
            m_Handler.FamiliesList = FamiliesList;

            //Start loading process
            m_ExEvent.Raise();
        }

        private void FamiliesUpdate_Closed(object sender, EventArgs e)
        {
            // we own both the event and the handler
            // we should dispose it before we are closed
            m_ExEvent.Dispose();
            m_ExEvent = null;
            m_Handler = null;
            Close();
        }

        private void Mask_Checked(object sender, RoutedEventArgs e)
        {
            MaskApply = true;
        }

        private void Mask_Unchecked(object sender, RoutedEventArgs e)
        {
            MaskApply = false;
        }
    }
   
}
