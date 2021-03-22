using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace DS.RevitApp.FamiliesUpdate
{
    public class ExternalEventHandler : IExternalEventHandler
    {
        public List<string> FileList = new List<string>();
        public List<string> FamiliesList = new List<string>();

        public UIApplication App;
        public ExternalEventHandler(UIApplication app)
        {
            this.App = app;
        }

        public string GetName()
        {
            return "";
        }

        public void Execute(UIApplication app)
        {
            Main main = new Main(app);
            main.ExecuteLoadProcess(FileList, FamiliesList);
        }
    }

}
