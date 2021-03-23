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

namespace AddProjectParameters
{
    class Main
    {
        public UIApplication App;

        public Main(UIApplication app)
        {
            this.App = app;
        }


        public void ExecuteLoadProcess()
        {
            Document doc = App.ActiveUIDocument.Document;

            //Category set forming
            Category wall = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Walls);
            Category door = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Doors);
            CategorySet cats1 = App.Application.Create.NewCategorySet();
            cats1.Insert(wall);
            cats1.Insert(door);

            CreateProjectParameter(App.Application, doc, "NewParameter1", ParameterType.Text, true, cats1, BuiltInParameterGroup.PG_DATA, false);

            TaskDialog.Show("Revit", "Process completed successfully!");

            MyApplication.thisApp.m_MyForm.Close();

        }

        public void CreateProjectParameter(RvtApplication app, Document doc, string name, ParameterType type, bool visible, CategorySet cats, BuiltInParameterGroup group, bool inst)
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

            ExternalDefinition def = app.OpenSharedParameterFile().Groups.Create("TemporaryDefintionGroup").Definitions.Create(opt) as ExternalDefinition;
            app.SharedParametersFilename = oriFile;
            File.Delete(tempFile);

            Autodesk.Revit.DB.Binding binding = app.Create.NewTypeBinding(cats);
            if (inst) binding = app.Create.NewInstanceBinding(cats);

            TransactionCommit(doc, def, binding, group);

        }



        private void TransactionCommit(Document doc, ExternalDefinition def, Autodesk.Revit.DB.Binding binding , BuiltInParameterGroup group)
        {
            using (Transaction transNew = new Transaction(doc, "RealLoading"))
            {
                try
                {
                    // The name of the transaction was given as an argument
                    if (transNew.Start("Create project parameter") != TransactionStatus.Started) 
                        return ;

                    BindingMap map = doc.ParameterBindings;
                    map.Insert(def, binding, group);
                }


                catch (Exception e)
                {
                    transNew.RollBack();
                    MessageBox.Show(e.ToString());
                }

                transNew.Commit();
                //doc.Close(false);
            }
        }
    }

}