using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                //TaskDialog.Show("DS message", "Message handler1 acivated!");

            }
            catch (Exception)
            {
                return Result.Failed;
            }

            return Result.Succeeded;
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
