using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddProjectParameters
{
    class Main
    {
        public UIApplication App;

        public Main(UIApplication app)
        {
            this.App = app;
        }

        public void ExecuteLoadProcess(List<string> files, List<string> families)
        {
            TaskDialog.Show("Revit", "Process completed successfully!");

            MyApplication.thisApp.m_MyForm.Close();
        }
    }
}
