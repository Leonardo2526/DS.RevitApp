using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
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
        private ExternalEvent m_ExEvent;
        public UIApplication App;

        public static string GroupName;

        public AddParametersToSFPOptions(UIApplication app, ExternalEvent exEvent)
        {
            InitializeComponent();
            this.App = app;
            m_ExEvent = exEvent;
        }


        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (GroupNameText.Text != "")
            {
                GroupName = GroupNameText.Text;
                //Start loading process
                m_ExEvent.Raise();
            }
            else
            {
                System.Windows.MessageBox.Show("Fill a group name!");
            }
        }
    }
}
