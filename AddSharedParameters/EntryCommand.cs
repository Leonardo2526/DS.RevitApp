using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS_Forms;
using System;
using System.Collections.Generic;

namespace AddSharedParameters
{
    [Transaction(TransactionMode.Manual)]
    public class EntryCommand : IExternalCommand
    {
        public static string SPFPath;
        public static string ParametersNamesPath;
        public static string FileWithProjectsPathes;

        public static List<string> SelectesParameters = new List<string>();
        public static List<string> FilesPathes = new List<string>();

        readonly string CurDate = DateTime.Now.ToString("yyMMdd");
        readonly string CurDateTime = DateTime.Now.ToString("yyMMdd_HHmmss");



        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            //Get application and documnet objects
            UIApplication uiapp = commandData.Application;

            DS_StartForms(uiapp);

            return Result.Succeeded;
        }

        public void DS_StartForms(UIApplication uiapp)
        {
            DS_Form newForm = new DS_Form
            {
                TopLevel = true
            };

            SPFPath = newForm.DS_OpenFileDialogForm_txt("", "Select shared parameter file for loading.").ToString();
            if (SPFPath == "")
            {
                newForm.Close();
                return;
            }

            ParametersNamesPath = newForm.DS_OpenFileDialogForm_txt("", "Select file with parameters names.").ToString();
            if (ParametersNamesPath == "")
            {
                newForm.Close();
                return;
            }

            AddParametersToSFPOptions addParametersToSFPOptions = new AddParametersToSFPOptions(uiapp);
            addParametersToSFPOptions.Show();

        }

    }



}
