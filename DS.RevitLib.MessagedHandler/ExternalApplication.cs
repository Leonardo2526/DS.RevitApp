using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DS.RevitLib.MessageHandler
{
    public class ExternalApplication : IExternalApplication
    {
        /// Implement this method to subscribe to event.
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Register event. 
                application.ControlledApplication.FailuresProcessing += ControlledApplication_FailuresProcessing;
                application.DialogBoxShowing += new EventHandler<DialogBoxShowingEventArgs>(a_DialogBoxShowing);
                //TaskDialog.Show("DS message", "Message handler1 acivated!");

            }
            catch (Exception)
            {
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void a_DialogBoxShowing(object sender, DialogBoxShowingEventArgs e)
        {
            TaskDialogShowingEventArgs showingEventArgs = e as TaskDialogShowingEventArgs;

            string s = string.Empty;

            if (null != showingEventArgs)
            {
                s = string.Format(
                  ", dialog id {0}, message '{1}'",
                  showingEventArgs.DialogId, showingEventArgs.Message);

                bool isConfirm = showingEventArgs.DialogId.Equals(
                  "TaskDialog_Missing_Third_Party_Updaters");

                if (isConfirm)
                {
                    showingEventArgs.OverrideResult(
                    (int)DialogResult.Yes);
                    s += ", auto-confirmed.";
                }
            }
            Debug.Print(
              "DialogBoxShowing: help id {0}, cancellable {1}{2}",
              e.DialogId,
              e.Cancellable ? "Yes" : "No",
              s);
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // remove the event.
            application.ControlledApplication.FailuresProcessing -= ControlledApplication_FailuresProcessing;
            return Result.Succeeded;
        }

        private void ControlledApplication_FailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            FailuresAccessor fa = e.GetFailuresAccessor();
            var failList = fa.GetFailureMessages();

            var messagesHandler = new MessagesHandler(e);
        }

    }

}
