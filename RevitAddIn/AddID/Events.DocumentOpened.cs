using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.ApplicationServices;  
using Autodesk.Revit.DB.Events;

namespace AddIn
{
    public class Application_DocumentOpened : IExternalApplication
    {
        /// Implement this method to subscribe to event.
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Register event. 
                application.ControlledApplication.DocumentOpened += new EventHandler
                     <Autodesk.Revit.DB.Events.DocumentOpenedEventArgs>(application_DocumentOpened);
            }
            catch (Exception)
            {
                return Result.Failed;
            }

            return Result.Succeeded;
        }
              
        /// Implement OnShutdown method of IExternalApplication interface to unregister subscribed event
        public Result OnShutdown(UIControlledApplication application)
        {
            // remove the event.
            application.ControlledApplication.DocumentOpened -= application_DocumentOpened;
            return Result.Succeeded;
        }
       
        /// This sample demonstrates how to subscribe to the DocumentOpened event and modify the model in the event handler method. 
  
        public void application_DocumentOpened(object sender, DocumentOpenedEventArgs args)
        {
            // get document from event args.
            Document doc = args.Document;

            // Following code snippet demonstrates support of DocumentOpened event to modify the model.
            // Because DocumentOpened supports model changes, it allows user to update document data.
            // Here, this sample assigns a specified value to ProjectInformation.Address property. 
            // User can change other properties of document or create(delete) something as he likes.
            // 
            // Please note that ProjectInformation property is empty for family document.
            // So please don't run this sample on family document.
            using (Transaction transaction = new Transaction(doc, "Edit Address"))
            {
                if (transaction.Start() == TransactionStatus.Started)
                {
                    doc.ProjectInformation.Address =
                        "United States - Massachusetts - Waltham - 1560 Trapelo Road";
                    transaction.Commit();
                }
            }
        }

    }


}
