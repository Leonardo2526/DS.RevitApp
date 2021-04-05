using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace AddProjectParameters
{
    public partial class StartForm : Window
    {
        private ExternalEvent m_ExEvent;
        private ExternalEventHandler m_Handler;
        public UIApplication App;

        public StartForm(UIApplication app, ExternalEvent exEvent, ExternalEventHandler handler)
        {
            InitializeComponent();
            this.App = app;
            m_ExEvent = exEvent;
            m_Handler = handler;
        }

        private void Button_LoadPathes_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_AddParameter_Click(object sender, RoutedEventArgs e)
        {
            //Start loading process
            m_ExEvent.Raise();
        }

        private void Button_StartLoading_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
