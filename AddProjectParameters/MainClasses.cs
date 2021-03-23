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
            //TransactionCommit(App.Application, doc);
            Category wall = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Walls);
            Category door = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Doors);
            CategorySet cats1 = App.Application.Create.NewCategorySet();
            cats1.Insert(wall);
            cats1.Insert(door);

            RawCreateProjectParameter_2(App.Application, doc, "NewParameter1", ParameterType.Text, true, cats1, BuiltInParameterGroup.PG_DATA, false);

            TaskDialog.Show("Revit", "Process completed successfully!");

            MyApplication.thisApp.m_MyForm.Close();

        }

        public void RawCreateProjectParameter_2(RvtApplication app, Document doc, string name, ParameterType type, bool visible, CategorySet cats, BuiltInParameterGroup group, bool inst)
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

            using (Transaction t = new Transaction(doc))
            {
                t.Start("RealLoading");
                BindingMap map = doc.ParameterBindings;
                map.Insert(def, binding, group);
                t.Commit();

            }
         
        }




        public static void RawCreateProjectParameter_1(RvtApplication app, Document doc, string name, ParameterType type, bool visible, CategorySet cats, BuiltInParameterGroup group, bool inst)
        {
            string oriFile = app.SharedParametersFilename;
            string tempFile = Path.GetTempFileName() + ".txt";
            using (File.Create(tempFile)) { }
            app.SharedParametersFilename = tempFile;

            var defOptions = new ExternalDefinitionCreationOptions(name, type)
            {
                Visible = visible
            };
            ExternalDefinition def = app.OpenSharedParameterFile().Groups.Create("TemporaryDefintionGroup").Definitions.Create(defOptions) as ExternalDefinition;

            app.SharedParametersFilename = oriFile;
            File.Delete(tempFile);

            Autodesk.Revit.DB.Binding binding = app.Create.NewTypeBinding(cats);
            if (inst) binding = app.Create.NewInstanceBinding(cats);

            BindingMap map = doc.ParameterBindings;

            if (!map.Insert(def, binding, group))
            {
                Trace.WriteLine($"Failed to create Project parameter '{name}' :(");
            }
        }

        private void TransactionCommit(RvtApplication app, Document doc)
        {
            using (Transaction transNew = new Transaction(doc, "RealLoading"))
            {
                try
                {
                    //transNew.Start();
                    // The name of the transaction was given as an argument
                    if (transNew.Start("Create project parameter") != TransactionStatus.Started) 
                        return ;

                    Category materials = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Materials);
                    CategorySet cats = app.Create.NewCategorySet();
                    cats.Insert(materials);

                    RawCreateProjectParameter_1(app, doc, "projectParameterName", ParameterType.Text, true,
                        cats, BuiltInParameterGroup.PG_IDENTITY_DATA, true);

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