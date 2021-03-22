using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


using DS_RVTtoNWC;
using Form = System.Windows.Forms.Form;
using TextBox = System.Windows.Forms.TextBox;

namespace DS_FilterForm
{
    public partial class DS_Filter : Form
    {
        public int fileSize { get; private set; }
        public int fileDate { get; private set; }
        public string sourcePath { get; set; }
        public string destinationPath { get; set; }

        public UIApplication uiapp;

        public DS_Filter(UIApplication uiap)
        {
            InitializeComponent();
            uiapp = uiap;
        }

        private TableLayoutPanel tableLayoutPanel1;
        private TextBox textBox_File_size;
        private CheckBox checkBox_File_size;
        private FlowLayoutPanel flowLayoutPanel2;
        private Button ApplyFilter;
        private Button NoFilter;
        private FlowLayoutPanel flowLayoutPanel_Filters;
        private Label label_File_size;
        private CheckBox checkBox_File_date;
        private TextBox textBox_File_date;
        private Label label_File_date;
        private Button button1;

        public void MainProgramInitiate()
        {
            DS_MainClass dS_MainClass = new DS_MainClass
            {
                SourcePath = sourcePath,
                DestinationPath = destinationPath,
                FileSize = fileSize,
                FileDate = fileDate
            };

            dS_MainClass.DS_MainProgram(uiapp);
        }


        public void ApplyFilter_Click(object sender, EventArgs e)
        {

            if (checkBox_File_size.Checked && textBox_File_size.Text == "")
            {
                MessageBox.Show("Enter any value to the field");
                return;
            }

            if (checkBox_File_date.Checked && textBox_File_date.Text == "")
            {
                MessageBox.Show("Enter any value to the field");
                return;
            }

            this.Close();

            MainProgramInitiate();
        }

        private void NoFilter_Click_1(object sender, EventArgs e)
        {
            this.Close();
            MainProgramInitiate();
        }

        private void checkBox_File_size_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_File_size.Checked)
            {
                textBox_File_size.Visible = true;
                label_File_size.Visible = true;
            }
            else
            {
                fileSize = 0;
                textBox_File_size.Clear();
                textBox_File_size.Visible = false;
                label_File_size.Visible = false;
            }

        }

        private void checkBox_File_date_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_File_date.Checked)
            {
                textBox_File_date.Visible = true;
                label_File_date.Visible = true;
            }
            else
            {
                fileDate = 0;
                textBox_File_date.Text = "";
                textBox_File_date.Visible = false;
                label_File_date.Visible = false;
            }

        }

        private void textBox_File_size_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox_File_size.Text, out int ParsedValue) && textBox_File_size.Text != "")
            {
                MessageBox.Show("This is a number only field");
                return;
            }
            fileSize = ParsedValue * 1000000;
        }

        private void textBox_File_date_TextChanged(object sender, EventArgs e)
        {
            {
                if (!int.TryParse(textBox_File_date.Text, out int ParsedValue) && textBox_File_date.Text != "")
                {
                    MessageBox.Show("This is a number only field");
                    return;
                }

                fileDate = ParsedValue;
            }
        }

        private void label_File_size_Click(object sender, EventArgs e)
        {

        }
    }
}
