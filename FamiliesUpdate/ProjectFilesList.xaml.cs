using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.UI;
using DS_SystemTools;

namespace DS.RevitApp.FamiliesUpdate
{
    /// <summary>
    /// Логика взаимодействия для ProjectFilesList.xaml
    /// </summary>
    public partial class ProjectFilesList : Window
    {

        List<string> FileFullNames = new List<string>();
        public List<string> SelItems = new List<string>();

        public ProjectFilesList(List<string> list)
        {
            InitializeComponent();
            FileFullNames = list;
            FilesList.ItemsSource = FileFullNames;
        }

        private void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ApplySelection_Click(object sender, RoutedEventArgs e)
        {
            if (FilesList.SelectedItems.Count !=0)
            {
              
                foreach (object it in FilesList.SelectedItems)
                {
                    SelItems.Add(it.ToString());
                }

                FilesList.SelectedItems.Clear();
              
                this.Close();
            }
            else
                MessageBox.Show("No files selected!");
        }

        private void ApplyAllSelection_Click(object sender, RoutedEventArgs e)
        {
            foreach (object it in FilesList.Items)
            {
                SelItems.Add(it.ToString());
            }
           
            this.Close();
        }
    }
}
