using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS_SystemTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace AddProjectParameters
{
    class Main
    {
        public UIApplication App;

        public Main(UIApplication app)
        {
            this.App = app;
        }

        //Get current date and time    
        readonly string CurDate = DateTime.Now.ToString("yyMMdd");
        readonly string CurDateTime = DateTime.Now.ToString("yyMMdd_HHmmss");

        List<string> FilesUpdated = new List<string>();

        public void IterateThroughtFiles()
        {
            int i = 0;

            CategorySet cats = SetCatergoty();

            try
            {
                //Through each file iterating
                foreach (string file in StartForm.FilesPathes)
                {
                    i++;
                    Document doc = App.Application.OpenDocumentFile(file);


                    //Logs write
                    FilesUpdated.Add(i + ". File path: " + doc.PathName);

                    foreach (string parameter in StartForm.SelectesParameters)
                        AddParameterToProject(doc, cats, false, BuiltInParameterGroup.PG_DATA, parameter);

                    doc.Close(true);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }



            DS_Tools dS_Tools = new DS_Tools
            {
                DS_LogName = CurDateTime + "_Log.txt",
                DS_LogOutputPath = Path.GetDirectoryName(StartForm.SPFPath) + "\\" + "Log" + "\\"
            };
            WriteLogToFile(dS_Tools);

            TaskDialog.Show("Revit", "Process completed successfully!");

            MyApplication.thisApp.m_MyForm.Close();

        }


        CategorySet SetCatergoty()
        {
            Document doc = App.ActiveUIDocument.Document;
            Categories categories = doc.Settings.Categories;
            CategorySet cats = App.Application.Create.NewCategorySet();

            //Category set forming
            foreach (Category c in categories)
            {
                if (c.CategoryType == CategoryType.Model && c.AllowsBoundParameters)
                    cats.Insert(c);
            }

            return cats;
        }


        void AddParameterToProject(Document doc, CategorySet cats, bool inst, BuiltInParameterGroup group, string parameterName)
        {
            App.Application.SharedParametersFilename = StartForm.SPFPath;

            ExternalDefinition def = App.Application.OpenSharedParameterFile().Groups.
                get_Item(SelectParameters.SelectedGroupName).Definitions.get_Item(parameterName) as ExternalDefinition;

            Autodesk.Revit.DB.Binding binding = App.Application.Create.NewTypeBinding(cats);

            TransactionCommit(doc, def, binding, group);

        }


        private void TransactionCommit(Document doc, ExternalDefinition def, Autodesk.Revit.DB.Binding binding, BuiltInParameterGroup group)
        {
            using (Transaction transNew = new Transaction(doc, "Create project parameter"))
            {
                try
                {
                    transNew.Start();

                    BindingMap map = doc.ParameterBindings;
                    map.ReInsert(def, binding, group);
                    map.Insert(def, binding, group);
                }


                catch (Exception e)
                {
                    transNew.RollBack();
                    MessageBox.Show(e.ToString());
                }

                transNew.Commit();
            }
        }

        void WriteLogToFile(DS_Tools dS_Tools)
        {
            dS_Tools.DS_StreamWriter("Path to shared parameters file: " + "\n" + StartForm.SPFPath + "\n");
            dS_Tools.DS_StreamWriter("Path to shared parameters file: " + "\n" + StartForm.FileWithProjectsPathes + "\n");
            dS_Tools.DS_StreamWriter("Group: " + SelectParameters.SelectedGroupName + "\n");
            dS_Tools.DS_StreamWriter("Selected parameters: " + "\n" + dS_Tools.DS_ListToString(StartForm.SelectesParameters) + "\n");
            dS_Tools.DS_StreamWriter("Files updated: " + "\n" + dS_Tools.DS_ListToString(FilesUpdated) + "\n");

            dS_Tools.DS_FileExistMessage();
        }

    }



}