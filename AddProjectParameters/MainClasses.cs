using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RvtApplication = Autodesk.Revit.ApplicationServices.Application;


using DS_Forms;
using DS_SystemTools;

namespace AddProjectParameters
{
    class Main
    {
        public UIApplication App;

        public Main(UIApplication app)
        {
            this.App = app;
        }

        public void IterateThroughtFiles()
        {
            int i = 0;
            try
            {
                //Through each file iterating
                foreach (string file in StartForm.FilesPathes)
                {
                    i++;
                    Document doc = App.Application.OpenDocumentFile(file);
                    CategorySet cats = SetCatergoty(doc);

                    foreach (string parameter in StartForm.SelectesParameters)
                        AddParameterToProject(doc, cats, false, BuiltInParameterGroup.PG_DATA, parameter);

                    doc.Close(true);
                    //Logs write
                    //LoadReport.Add(i + ". File path: " + doc.PathName + "\n");

                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        
           

            /*
            DS_Tools dS_Tools = new DS_Tools
            {
                DS_LogName = CurDateTime + "_Log.txt",
                DS_LogOutputPath = DialogForms.SourcePath + "\\" + "Log" + "\\"
            };
            WriteLogToFile(dS_Tools);
            */

            TaskDialog.Show("Revit", "Process completed successfully!");

            MyApplication.thisApp.m_MyForm.Close();

        }

        CategorySet SetCatergoty(Document doc)
        {
            //Category set forming
            Category wall = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Walls);
            Category door = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Doors);
            CategorySet cats = App.Application.Create.NewCategorySet();
            cats.Insert(wall);
            cats.Insert(door);

            return cats;
            //CreateProjectParameter(App.Application, doc, "NewParameter1", ParameterType.Text, true, cats1, BuiltInParameterGroup.PG_DATA, false);        
             
        }


        void AddParameterToProject(Document doc, CategorySet cats, bool inst, BuiltInParameterGroup group, string parameterName)
        {
            App.Application.SharedParametersFilename = StartForm.SPFPath;

            ExternalDefinition def = App.Application.OpenSharedParameterFile().Groups.
                get_Item(SelectParameters.SelectedGroupName).Definitions.get_Item(parameterName) as ExternalDefinition;

            Autodesk.Revit.DB.Binding binding = App.Application.Create.NewTypeBinding(cats);
            //if (inst) binding = App.Application.Create.NewInstanceBinding(cats);

            TransactionCommit(doc, def, binding, group);
        }


        public void CreateProjectParameter(RvtApplication app, Document doc, string name, 
            ParameterType type, bool visible, CategorySet cats, BuiltInParameterGroup group, bool inst)
        {
            //InternalDefinition def = new InternalDefinition();
            //Definition def = new Definition();

            string oriFile = app.SharedParametersFilename;
            string tempFile = Path.GetTempFileName() + ".txt";
            using (File.Create(tempFile)) { }
            app.SharedParametersFilename = tempFile;

            ExternalDefinitionCreationOptions opt = new ExternalDefinitionCreationOptions(name, type)
            {
                Visible = visible
            };

            ExternalDefinition def = app.OpenSharedParameterFile().Groups.Create("AddParametersToSFPOptions.GroupName").
                Definitions.Create(opt) as ExternalDefinition;
            app.SharedParametersFilename = oriFile;
            File.Delete(tempFile);

            Autodesk.Revit.DB.Binding binding = app.Create.NewTypeBinding(cats);
            if (inst) binding = app.Create.NewInstanceBinding(cats);

            TransactionCommit(doc, def, binding, group);

            TaskDialog.Show("Revit", "Process completed successfully!");

        }

       

        private void TransactionCommit(Document doc, ExternalDefinition def, Autodesk.Revit.DB.Binding binding , BuiltInParameterGroup group)
        {
            using (Transaction transNew = new Transaction(doc, "Create project parameter"))
            {
                try
                {
                    transNew.Start();
                    // The name of the transaction was given as an argument
                    //if (transNew.Start("Create project parameter") != TransactionStatus.Started) 
                        //return ;

                    BindingMap map = doc.ParameterBindings;
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
    }

}