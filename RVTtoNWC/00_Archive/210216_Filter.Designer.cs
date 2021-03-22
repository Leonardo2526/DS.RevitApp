
namespace DS_FilterForm
{
    partial class DS_Filter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.ApplyFilter = new System.Windows.Forms.Button();
            this.NoFilter = new System.Windows.Forms.Button();
            this.flowLayoutPanel_Filters = new System.Windows.Forms.FlowLayoutPanel();
            this.checkBox_File_size = new System.Windows.Forms.CheckBox();
            this.textBox_File_size = new System.Windows.Forms.TextBox();
            this.label_File_size = new System.Windows.Forms.Label();
            this.checkBox_File_date = new System.Windows.Forms.CheckBox();
            this.textBox_File_date = new System.Windows.Forms.TextBox();
            this.label_File_date = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel_Filters.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel_Filters, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(450, 271);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.ApplyFilter);
            this.flowLayoutPanel2.Controls.Add(this.NoFilter);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 211);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(444, 57);
            this.flowLayoutPanel2.TabIndex = 2;
            // 
            // ApplyFilter
            // 
            this.ApplyFilter.Location = new System.Drawing.Point(3, 3);
            this.ApplyFilter.Name = "ApplyFilter";
            this.ApplyFilter.Size = new System.Drawing.Size(111, 42);
            this.ApplyFilter.TabIndex = 0;
            this.ApplyFilter.Text = "ApplyFilter\r\n";
            this.ApplyFilter.UseVisualStyleBackColor = true;
            this.ApplyFilter.Click += new System.EventHandler(this.ApplyFilter_Click);
            // 
            // NoFilter
            // 
            this.NoFilter.Location = new System.Drawing.Point(120, 3);
            this.NoFilter.Name = "NoFilter";
            this.NoFilter.Size = new System.Drawing.Size(141, 42);
            this.NoFilter.TabIndex = 1;
            this.NoFilter.Text = "Continue without filer";
            this.NoFilter.UseVisualStyleBackColor = true;
            this.NoFilter.Click += new System.EventHandler(this.NoFilter_Click_1);
            // 
            // flowLayoutPanel_Filters
            // 
            this.flowLayoutPanel_Filters.Controls.Add(this.checkBox_File_size);
            this.flowLayoutPanel_Filters.Controls.Add(this.textBox_File_size);
            this.flowLayoutPanel_Filters.Controls.Add(this.label_File_size);
            this.flowLayoutPanel_Filters.Controls.Add(this.checkBox_File_date);
            this.flowLayoutPanel_Filters.Controls.Add(this.textBox_File_date);
            this.flowLayoutPanel_Filters.Controls.Add(this.label_File_date);
            this.flowLayoutPanel_Filters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel_Filters.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel_Filters.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel_Filters.Name = "flowLayoutPanel_Filters";
            this.flowLayoutPanel_Filters.Size = new System.Drawing.Size(444, 202);
            this.flowLayoutPanel_Filters.TabIndex = 3;
            // 
            // checkBox_File_size
            // 
            this.checkBox_File_size.AutoSize = true;
            this.checkBox_File_size.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.checkBox_File_size.Location = new System.Drawing.Point(3, 3);
            this.checkBox_File_size.Name = "checkBox_File_size";
            this.checkBox_File_size.Size = new System.Drawing.Size(92, 24);
            this.checkBox_File_size.TabIndex = 2;
            this.checkBox_File_size.Text = "File size";
            this.checkBox_File_size.UseVisualStyleBackColor = true;
            this.checkBox_File_size.CheckedChanged += new System.EventHandler(this.checkBox_File_size_CheckedChanged);
            // 
            // textBox_File_size
            // 
            this.textBox_File_size.Location = new System.Drawing.Point(3, 33);
            this.textBox_File_size.Name = "textBox_File_size";
            this.textBox_File_size.Size = new System.Drawing.Size(100, 26);
            this.textBox_File_size.TabIndex = 3;
            this.textBox_File_size.Tag = "";
            this.textBox_File_size.Visible = false;
            this.textBox_File_size.TextChanged += new System.EventHandler(this.textBox_File_size_TextChanged);
            // 
            // label_File_size
            // 
            this.label_File_size.AutoSize = true;
            this.label_File_size.Location = new System.Drawing.Point(3, 62);
            this.label_File_size.Name = "label_File_size";
            this.label_File_size.Size = new System.Drawing.Size(256, 20);
            this.label_File_size.TabIndex = 4;
            this.label_File_size.Text = "Enter the smallest size of a file, MB";
            this.label_File_size.Visible = false;
            this.label_File_size.Click += new System.EventHandler(this.label_File_size_Click);
            // 
            // checkBox_File_date
            // 
            this.checkBox_File_date.AutoSize = true;
            this.checkBox_File_date.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.checkBox_File_date.Location = new System.Drawing.Point(3, 85);
            this.checkBox_File_date.Name = "checkBox_File_date";
            this.checkBox_File_date.Size = new System.Drawing.Size(96, 24);
            this.checkBox_File_date.TabIndex = 5;
            this.checkBox_File_date.Text = "File date";
            this.checkBox_File_date.UseVisualStyleBackColor = true;
            this.checkBox_File_date.CheckedChanged += new System.EventHandler(this.checkBox_File_date_CheckedChanged);
            // 
            // textBox_File_date
            // 
            this.textBox_File_date.Location = new System.Drawing.Point(3, 115);
            this.textBox_File_date.Name = "textBox_File_date";
            this.textBox_File_date.Size = new System.Drawing.Size(100, 26);
            this.textBox_File_date.TabIndex = 6;
            this.textBox_File_date.Tag = "";
            this.textBox_File_date.Visible = false;
            this.textBox_File_date.TextChanged += new System.EventHandler(this.textBox_File_date_TextChanged);
            // 
            // label_File_date
            // 
            this.label_File_date.AutoSize = true;
            this.label_File_date.Location = new System.Drawing.Point(3, 144);
            this.label_File_date.Name = "label_File_date";
            this.label_File_date.Size = new System.Drawing.Size(394, 20);
            this.label_File_date.TabIndex = 7;
            this.label_File_date.Text = "Enter the last file write date (in days from current date).";
            this.label_File_date.Visible = false;
            // 
            // DS_Filter
            // 
            this.ClientSize = new System.Drawing.Size(450, 271);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.button1);
            this.Name = "DS_Filter";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Filters applying";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel_Filters.ResumeLayout(false);
            this.flowLayoutPanel_Filters.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
    }
}