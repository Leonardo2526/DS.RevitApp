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

namespace AddProjectParameters
{
    public partial class StartForm : Window
    {
        private ExternalEvent m_ExEvent;
        private ExternalEventHandler m_Handler;
        public UIApplication App;

        public static string SPFPath;
        public static string ParametersNamesPath;


        public StartForm(UIApplication app, ExternalEvent exEvent, ExternalEventHandler handler)
        {
            InitializeComponent();
            this.App = app;
            m_ExEvent = exEvent;
            m_Handler = handler;
        }



        private void Button_AddParametersToSFP_Click(object sender, RoutedEventArgs e)
        {
            DS_Form newForm = new DS_Form
            {
                TopLevel = true
            };
            
            SPFPath = newForm.DS_OpenFileDialogForm_txt("Select shared parameter file for loading.").ToString();
            if (SPFPath == "")
            {
                newForm.Close();
                return;
            }

            ParametersNamesPath = newForm.DS_OpenFileDialogForm_txt("Select file with parameters names.").ToString();
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
            if (SPFPath == "")
            {
                DS_Form newForm = new DS_Form
                {
                    TopLevel = true
                };

                SPFPath = newForm.DS_OpenFileDialogForm_txt().ToString();
                if (SPFPath == "")
                {
                    newForm.Close();
                    return;
                }
            }
            

                    //Start loading process
            m_ExEvent.Raise();
        }

        private void Button_StartLoading_Click(object sender, RoutedEventArgs e)
        {
           
        }

    }
}
