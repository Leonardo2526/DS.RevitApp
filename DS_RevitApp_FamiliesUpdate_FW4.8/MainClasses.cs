using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS_Forms;
using DS_SystemTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DS.RevitApp.FamiliesUpdate
{
    public class Main
    {
        public UIApplication App;

        public Main(UIApplication app)
        {
            this.App = app;
        }

        //Lists for log
        List<string> LoadReport = new List<string>();

        //Get current date and time    
        readonly string CurDate = DateTime.Now.ToString("yyMMdd");
        readonly string CurDateTime = DateTime.Now.ToString("yyMMdd_HHmmss");

        public void ExecuteLoadProcess(List<string> files, List<string> families)
        {
            Docs docs = new Docs(App);
            int i = 0;
            //Through each file iterating
            foreach (string file in files)
            {
                i++;
                docs.OpenDoc(file, out Document doc);

                //Logs write
                LoadReport.Add(i + ". File path: " + doc.PathName + "\n");

                TransactionCommit(doc, families);
            }


            DS_Tools dS_Tools = new DS_Tools
            {
                DS_LogName = CurDateTime + "_Log.txt",
                DS_LogOutputPath = DialogForms.SourcePath + "\\" + "Log" + "\\"
            };
            WriteLogToFile(dS_Tools);

            TaskDialog.Show("Revit", "Process completed successfully!");

            MyApplication.thisApp.m_MyForm.Close();

        }

        private void TransactionCommit(Document doc, List<string> families)
        {
            using (Transaction transNew = new Transaction(doc, "RealLoading"))
            {
                try
                {
                    transNew.Start();

                    //Create new lists for log
                    List<string> successList = new List<string>();
                    List<string> errorList = new List<string>();

                    //Through each family file iterating
                    foreach (string family in families)
                    {
                        this.Load(doc, family, successList, errorList);
                    }

                    AddIteratingResultsToLists(successList, errorList);
                }


                catch (Exception e)
                {
                    transNew.RollBack();
                    MessageBox.Show(e.ToString());
                }

                transNew.Commit();
                doc.Close(true);
            }
        }

        public void Load(Document doc, string familyPath, List<string> successList, List<string> errorList)
        {
            if (doc.LoadFamily(familyPath, new FamilyLoadOptions(), out Family family))
            {
                successList.Add(family.Name);
            }
            else
            {
                errorList.Add(familyPath);
            }
        }

        void WriteLogToFile(DS_Tools dS_Tools)
        {
            dS_Tools.DS_StreamWriter("Directory for checking '*.rvt' files: " + "\n" + DialogForms.SourcePath + "\n");
            dS_Tools.DS_StreamWriter("Directory for families '*.rfa' files: " + "\n" + DialogForms.FamilyPath + "\n");

            //Saved file names with empty models
            if (LoadReport.Count != 0)
            {
                dS_Tools.DS_StreamWriter(dS_Tools.DS_ListToString(LoadReport));
            }

            dS_Tools.DS_FileExistMessage();
        }

        void AddIteratingResultsToLists(List<string> successList, List<string> errorList)
        {
            if (successList.Count != 0)
            {
                LoadReport.Add("- Family files loaded successfully: ");
                LoadReport.AddRange(successList);
                LoadReport.Add("");
            }

            if (errorList.Count != 0)
            {
                LoadReport.Add("- Families haven't been loaded, because there are no distincions in the origin file: ");
                LoadReport.AddRange(errorList);
                LoadReport.Add("");
            }
        }
    }

    public class FamilyLoadOptions : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            return true;
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            source = FamilySource.Family;
            return true;
        }
    }

    public class Docs

    {
        public UIApplication App;
        public List<string> FileFullNames_Filtered = new List<string>();

        public Docs(UIApplication app)
        {
            this.App = app;
        }

        public int FileSize { get; set; }
        public int FileDate { get; set; }

        public void OpenDoc(string SourcePath, out Document doc)
        {
            ModelPath mp = ModelPathUtils.ConvertUserVisiblePathToModelPath(SourcePath);

            OpenOptions options1 = new OpenOptions
            {
                //DetachFromCentralOption = DetachFromCentralOption.DetachAndDiscardWorksets
            };

            doc = App.Application.OpenDocumentFile(mp, options1);
        }

        public void DirIterate(string CheckedPath, string ext)
        //Output files names list and it's direcories to Log
        {
            try
            {
                //Check folders
                foreach (string dir in Directory.GetDirectories(CheckedPath))
                {
                    GetFiles(dir, ext);
                    DirIterate(dir, ext);
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void GetFiles(string dir, string ext)
        {
            //Get files list from current directory
            List<string> newList = FileFilter(dir, ext);
            FileFullNames_Filtered.AddRange(newList);

            /*
            foreach (string FileName in FileFullNames_Filtered)
            {
                ProjectFilesList
                string NWCFileName = Path.GetFileNameWithoutExtension(FileName);
            }
            */
        }

        List<string> FileFilter(string dir, string ext)

        {
            //Extensions list
            var FileExt = new List<string> { ext };
            List<string> FileFullNames = new List<string>();

            if (StartForms.MaskApply == false)
            {
                FileFullNames = Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly).
                Where(s => FileExt.Contains(Path.GetExtension(s).TrimStart((char)46).ToLowerInvariant())).ToList();
            }
            else
            {
                FileFullNames = Directory.EnumerateFiles(dir, "*_*_*_*_*_*_*_*", SearchOption.TopDirectoryOnly).
                Where(s => FileExt.Contains(Path.GetExtension(s).TrimStart((char)46).ToLowerInvariant())).ToList();
            }

            return FileFullNames;
        }

        public void DirFamiliesIterate(string CheckedPath, string ext)
        //Output files names list and it's direcories to Log
        {
            try
            {
                //Check folders
                foreach (string dir in Directory.GetDirectories(CheckedPath))
                {
                    GetFamilies(dir, ext);
                    DirFamiliesIterate(dir, ext);
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void GetFamilies(string dir, string ext)
        {
            //Extensions list
            var FileExt = new List<string> { ext };

            //Get files list from current directory
            List<string> newList = Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly).
              Where(s => FileExt.Contains(Path.GetExtension(s).TrimStart((char)46).ToLowerInvariant())).ToList();

            FileFullNames_Filtered.AddRange(newList);
        }
    }

    class DialogForms
    {
        public static string SourcePath { get; set; }
        public static string FamilyPath { get; set; }

        public void AssignSourcePath()
        {
            DS_Form newForm = new DS_Form
            {
                TopLevel = true
            };
           
            SourcePath = newForm.DS_OpenInputFolderDialogForm().ToString();
            if (SourcePath == "")
            {
                newForm.Close();
                return;
            }
        }

        public void AssignFamilyPath()
        {
            DS_Form newForm = new DS_Form
            {
                TopLevel = true
            };

            FamilyPath = newForm.DS_OpenInputFolderDialogForm().ToString();
            if (FamilyPath == "")
            {
                newForm.Close();
                return;
            }
        }

    }

}
