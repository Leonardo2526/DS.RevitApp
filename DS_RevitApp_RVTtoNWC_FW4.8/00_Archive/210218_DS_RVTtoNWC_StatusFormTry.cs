using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS_FilterForm;
using DS_Forms;
using DS_SystemTools;
using DS_StatusForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using View = Autodesk.Revit.DB.View;

namespace DS_RVTtoNWC
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DS_MainClass : IExternalCommand

    {
        //Get current date and time  
        readonly string CurDate = DateTime.Now.ToString("yyMMdd");
        readonly string CurDateTime = DateTime.Now.ToString("yyMMdd_HHmmss");

        public int FileSize { get; set; }
        public int FileDate { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

        List<string> FileFiltered = new List<string>();
        List<string> EmptyModels = new List<string>();

        List<string> DirNWClist = new List<string>();
        List<string> NewDirNWClist = new List<string>();



        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get application and documnet objects
            UIApplication uiapp = commandData.Application;

            uiapp.Application.FailuresProcessing += Application_FailuresProcessing;

            try
            {
                DS_StartForms();

                DS_Filter filter = new DS_Filter(uiapp)
                {
                    TopLevel = true,
                    sourcePath = SourcePath,
                    destinationPath = DestinationPath
                };

                filter.Show();
            }

            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        public void DS_StartForms()
        {
            DS_Form newForm = new DS_Form
            {
                TopLevel = true
            };

            SourcePath = newForm.DS_OpenInputFolderDialogForm("Set folder for checking '*.rvt' files").ToString();
            if (SourcePath == "")
            {
                return;
            }
            newForm.Close();

            DestinationPath = newForm.DS_OpenOutputFolderDialogForm("Set output folder for '*.nwc' files").ToString();
            if (DestinationPath == "")
            {
                return;
            }

            newForm.Close();

            
        }

        public void DS_MainProgram(UIApplication uiapp)
        //Intiating main process
        {
            DS_Tools dS_Tools = new DS_Tools
            {
                DS_LogName = CurDateTime + "_Log.txt",
                DS_LogOutputPath = DestinationPath + "\\" + "Log" + "\\"
            };
            //Get all exist directories in DestinationPath
            GetDirNamesNWC();

            Status status = new Status
            {
                TopLevel = true
            };

            GetFiles(uiapp, SourcePath, dS_Tools);

            DirIterate(uiapp, SourcePath, dS_Tools, status);

            status.Close();

            //Saved file names with pathes write to log
            if (FileFiltered.Count != 0)
            {
                dS_Tools.DS_StreamWriter("NWC files have been created: ");
                dS_Tools.DS_StreamWriter(dS_Tools.DS_ListToString(FileFiltered));
                MessageBox.Show("Process has been completed successfully!");
            }

            else
            {
                dS_Tools.DS_StreamWriter("No NWC files have been created.");
                MessageBox.Show("Process has been completed with errors. \n See log file.");
            }           

            //Saved file names with empty models
            if (EmptyModels.Count != 0)
            {
                dS_Tools.DS_StreamWriter("\n Files with no geometry have been found: ");
                dS_Tools.DS_StreamWriter(dS_Tools.DS_ListToString(EmptyModels));
            }

            //Saved file names with empty models
            if (NewDirNWClist.Count != 0)
            {
                dS_Tools.DS_StreamWriter("\n New directories have been created: ");
                dS_Tools.DS_StreamWriter(dS_Tools.DS_ListToString(NewDirNWClist));
            }
            dS_Tools.DS_FileExistMessage();
        }

        public void DirIterate(UIApplication uiapp, string CheckedPath, DS_Tools dS_Tools, Status status)
        //Output files names list and it's direcories to Log
        {
            try
            {
                //Check folders
                foreach (string dir in Directory.GetDirectories(CheckedPath))
                {
                    status.dir = dir;
                    status.ShowDialog();

                    GetFiles(uiapp, dir, dS_Tools);
                    DirIterate(uiapp, dir, dS_Tools, status);
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void GetFiles(UIApplication uiapp, string dir, DS_Tools dS_Tools)
        {
            //Get files list from current directory
            string ext = "rvt";
            List<string> FileFullNames_Filtered = FileFilter(dir, ext);

            foreach (string FileName in FileFullNames_Filtered)
            {
                string NWCFileName = Path.GetFileNameWithoutExtension(FileName);

                NWC_Export(uiapp, FileName, NWCFileName);              
            }
        }
         
        List<string> FileFilter(string dir, string ext)

        {
            //Extensions list
            var FileExt = new List<string> { ext };

            List<string> FileFullNames = Directory.EnumerateFiles(dir, "*_*_*_*_*_*_*_*", SearchOption.TopDirectoryOnly).
            Where(s => FileExt.Contains(Path.GetExtension(s).TrimStart((char)46).ToLowerInvariant())).ToList();

            List<string> FileFullNames_Filtered = new List<string>();

            FileFullNames_Filtered.AddRange(FileFullNames);

            foreach (string fn in FileFullNames)
            {
                FileInfo f = new FileInfo(fn);
                if (FileSize != 0 && f.Length < FileSize)
                    FileFullNames_Filtered.Remove(fn);
                if (FileDate != 0 && f.LastWriteTime < DateTime.Now.AddDays(-FileDate))
                    FileFullNames_Filtered.Remove(fn);
                if (fn.Contains("Архив")| fn.Contains("восстановление"))
                    FileFullNames_Filtered.Remove(fn);
            }

            return FileFullNames_Filtered;
        }

        public void NWC_Export(UIApplication uiapp, string FileName, string NWCFileName)
        {
            ModelPath mp = ModelPathUtils.ConvertUserVisiblePathToModelPath(FileName);

            OpenOptions options1 = new OpenOptions
            {
                DetachFromCentralOption = DetachFromCentralOption.DetachAndDiscardWorksets
            };

            Document doc = uiapp.Application.OpenDocumentFile(mp, options1);
            
            DS_IsEmptyModel dS_IsEmptyModel = new DS_IsEmptyModel();
            if (dS_IsEmptyModel.IsEmptyModel(doc))
            {
                EmptyModels.Add(FileName);
                return;
            }

            string dirName = GetDirNWC(NWCFileName);

            // Get a reference to each file in that directory.
            string OutputPath = DestinationPath + "\\" + dirName + "\\";

            NavisworksExportOptions nweo = new NavisworksExportOptions
            {
                ExportScope = NavisworksExportScope.View,
                ViewId = NW_View(doc).Id
            };
            doc.Export(OutputPath, NWCFileName, nweo);

            FileFiltered.Add(OutputPath + NWCFileName + ".nwc");
            doc.Close(false); 
        }


        public View NW_View(Document doc)
        {

            FilteredElementCollector viewCollector = new FilteredElementCollector(doc);
            viewCollector.OfClass(typeof(View3D));

            DS_Tools dS_ViewTools = new DS_Tools
            {
                DS_LogName = CurDateTime + "_ViewLog.txt"
            };

            //Navisworks view searching
            foreach (Element viewElement in viewCollector)
            {
                View3D view = (View3D)viewElement;

                if (view.Name.Contains("Navisworks"))
                    return view;
            }

            //{3D} view searching
            foreach (Element viewElement in viewCollector)
            {
                View3D view = (View3D)viewElement;

                if (view.Name.Contains("{3D}"))
                    return view;
            }

            dS_ViewTools.DS_FileExistMessage();

            return null;
        }

        void Application_FailuresProcessing(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
        {
            FailuresAccessor fa = e.GetFailuresAccessor();
            IList<FailureMessageAccessor> failList = new List<FailureMessageAccessor>();
            failList = fa.GetFailureMessages(); // Inside event handler, get all warnings
            foreach (FailureMessageAccessor failure in failList)
            {

                // check FailureDefinitionIds against ones that you want to dismiss, FailureDefinitionId failID = failure.GetFailureDefinitionId();
                // prevent Revit from showing Unenclosed room warnings
                FailureDefinitionId failID = failure.GetFailureDefinitionId();
                if (failID == BuiltInFailures.WorksharingFailures.DuplicateNamesChanged)
                {
                    fa.DeleteWarning(failure);
                }
            }
        }

        public string GetDirNWC(string NWCFileName)
        //Output files names list and it's direcories to Log
        {
            string DefaultOutputPath = DestinationPath + "\\" + NWCFileName + "\\";

           
            try
            {
                //Check folders
                foreach (string dirname in DirNWClist)
                {
                    if (NWCFileName.Contains(dirname))
                    return dirname;
                }
                Directory.CreateDirectory(DefaultOutputPath);
                NewDirNWClist.Add(DefaultOutputPath);
                return NWCFileName;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return "";
        }

        public void GetDirNamesNWC()
        //Write directory names in DestinationPath to list
        {
            try
            {
                //Check folders
                foreach (string dir in Directory.GetDirectories(DestinationPath))
                {
                    string dirName = new DirectoryInfo(dir).Name;
                    DirNWClist.Add(dirName);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static List<string> FieldsGet(string line)
        {
            int i = 0;
            int indS = 0;
            int[] ind = new int[line.Length];

            List<string> field = new List<string>();

            //Through each symbol in file name iterating
            foreach (char sign in line)
            {
                indS += 1;

                if (sign.ToString() == "_")
                {
                    i += 1;

                    //Record the index of findings
                    ind[i] = indS;

                    //LIst record of fields
                    field.Add(line.Substring(ind[i - 1], ind[i] - ind[i - 1] - 1));
                }
            }
            field.Add(line);
            return field;
        }

    }
     
    public class DS_IsEmptyModel
    //Check if active document has physical elements
    {
        public bool IsEmptyModel(Document doc)
        {
            // Use shortcut WhereElementIsNotElementType() to find wall instances only
            FilteredElementCollector collector = new FilteredElementCollector(doc).WhereElementIsNotElementType();

            foreach (Element e in collector)
            {
                if (IsPhysicalElement(e))
                    return false;
            }
            return true;
        }

        bool IsPhysicalElement(Element e)
        {
            if (e.Category == null) return false;
            if (e.ViewSpecific) return false;
            if (e.LevelId == null) return false;

            // exclude specific unwanted categories
            if (((BuiltInCategory)e.Category.Id.IntegerValue) == BuiltInCategory.OST_HVAC_Zones) return false;

            return e.Category.CategoryType == CategoryType.Model && e.Category.CanAddSubcategory;
        }

    }

}
