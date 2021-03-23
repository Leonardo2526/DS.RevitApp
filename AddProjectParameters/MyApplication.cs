using Autodesk.Revit.UI;
using System.Windows.Forms;

namespace AddProjectParameters
{
    public class MyApplication : IExternalApplication
    {
        // class instance
        internal static MyApplication thisApp = null;
        // ModelessForm instance
        public StartForm m_MyForm;


        public Result OnShutdown(UIControlledApplication application)
        {
            if (m_MyForm != null && m_MyForm.IsVisible)
            {
                m_MyForm.Close();
            }

            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            m_MyForm = null;   // no dialog needed yet; the command will bring it
            thisApp = this;  // static access to this application instance

            return Result.Succeeded;
        }

        public void ShowForm(UIApplication uiapp)
        {
            MessageBox.Show("New");
            // If we do not have a dialog yet, create and show it
            if (m_MyForm == null || !m_MyForm.IsActive)
            {
                // A new handler to handle request posting by the dialog
                ExternalEventHandler handler = new ExternalEventHandler(uiapp);

                // External Event for the dialog to use (to post requests)
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // We give the objects to the new dialog;
                // The dialog becomes the owner responsible fore disposing them, eventually.
                m_MyForm = new StartForm(uiapp, exEvent, handler);
                m_MyForm.Show();
            }
        }

    }


}
