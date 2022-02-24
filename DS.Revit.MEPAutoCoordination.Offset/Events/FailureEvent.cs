using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Linq;

namespace DS.CollisionsElliminator
{
    class FailureEvent
    {
        readonly Application App;

        public FailureEvent(Application app)
        {
            App = app;
        }

        public bool IsErrorConnection = false;

        public void RegisterEvent()
        {
            try
            {
                App.FailuresProcessing += App_FailuresProcessing;

            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", ex.ToString());
            }
        }

        private void App_FailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            var transactionName = e.GetFailuresAccessor().GetTransactionName();

            if (e.GetFailuresAccessor().GetTransactionName().ToLower().Contains("automep_"))
            {
                var messages = e.GetFailuresAccessor().GetFailureMessages(FailureSeverity.Error);
                if (messages != null && messages.Any())
                {
                    var type = messages.FirstOrDefault().GetCurrentResolutionType();
                    messages.FirstOrDefault().SetCurrentResolutionType(type);
                    e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
                    IsErrorConnection = true;
                }
            }
            //TaskDialog.Show("Revit", "Modified elements");
        }

        public void ExitEvent()
        {
            try
            {
                App.FailuresProcessing -= App_FailuresProcessing;

            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", ex.ToString());
            }
        }
    }
}
