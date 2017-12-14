using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Xml;

using Microarea.Library.Licence;
using Microarea.TaskBuilderNet.Core.Generic;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.Lexan;

using Microarea.Console.SecurityAdminPlugIn;

namespace SecurityObjectsReleaseCompare
{
    //=========================================================================
    public partial class MainForm : Form
    {
        public const string XML_TAG_REPORT = "Report";
        public const string XML_TAG_OBJECT = "Object";

        public string Product = string.Empty;

        private string oldPath = string.Empty;
        private string newPath = string.Empty;

        private ArrayList filefromZip = null;

        private DataTable newObjectsDataTable = null;

        //---------------------------------------------------------------------
        public MainForm()
        {
            InitializeComponent();
            InitNewObjectsDataTable();
        }

        //---------------------------------------------------------------------
        public void InitNewObjectsDataTable()
        {
            newObjectsDataTable = new DataTable("NewObject");
            newObjectsDataTable.Columns.Add("NameSpace", Type.GetType("System.String"));
            newObjectsDataTable.Columns.Add("Type", Type.GetType("System.String"));
            newObjectsDataTable.Columns.Add("Cultures", Type.GetType("System.String"));

        }

        //---------------------------------------------------------------------
        public void AddRow(Microarea.TaskBuilderNet.Core.NameSolver.NewObject obj)
        {
            if (obj == null)
                return;

            DataRow dr = newObjectsDataTable.NewRow();

            dr["NameSpace"] = obj.NameSpace;
            dr["Type"] = DefaultSecurityRolesEngine.GetTypeFromNewObject(obj);
            dr["Cultures"] = string.Empty;

            newObjectsDataTable.Rows.Add(dr);

        }

        //---------------------------------------------------------------------
        private void OldPathButton_Click(object sender, EventArgs e)
        {
            DialogResult res = openFileDialog.ShowDialog();
            this.OldReleasePathTextBox.Text = openFileDialog.FileName;
            this.oldPath = OldReleasePathTextBox.Text;
        }

        //---------------------------------------------------------------------
        private void NewPathButton_Click(object sender, EventArgs e)
        {
            DialogResult res = openFileDialog.ShowDialog();
            this.NewReleasePathTextBox.Text = openFileDialog.FileName;
            this.newPath = NewReleasePathTextBox.Text;
        }

        //---------------------------------------------------------------------
        private void CompareReleaseButton_Click(object sender, EventArgs e)
        {
            Product = ProductNameComboBox.Text;

            if (string.IsNullOrEmpty(oldPath) ||
                string.IsNullOrEmpty(newPath) || string.IsNullOrEmpty(Product))
                return;


            newObjectsDataTable.Rows.Clear();

            LoadFiles(oldPath, NameSolverStrings.DocumentObjectsXml);
            ExtractObjFromDocumentObjectsFiles(filefromZip);
            LoadFiles(oldPath, NameSolverStrings.MenuExtension);
            ExtractObjFromMenuFiles(filefromZip);

            LoadFiles(newPath, NameSolverStrings.DocumentObjectsXml);
            ExtractObjFromDocumentObjectsFiles(filefromZip);
            LoadFiles(newPath, NameSolverStrings.MenuExtension);
            ExtractObjFromMenuFiles(filefromZip);

            AddCulture();
            CreateLogFile();

            MessageBox.Show("End procedure");
        }


        //---------------------------------------------------------------------------
        private void CreateLogFile()
        {
            DataSet ds = new DataSet("NewObjects");

            if (newObjectsDataTable == null)
                return;

            ds.Tables.Add(newObjectsDataTable);



            string fileName = @"C:\NewObjects " + Product + ".xml";

            ds.WriteXml(fileName);
        }


        //--------------------------------------------------------------------
        private void AddCulture()
        {
            ArrayList countries = new ArrayList();

            countries.Add("IT");
            countries.Add("BG");
            countries.Add("GR");
            countries.Add("PL");
            countries.Add("RO");
            countries.Add("HU");
            countries.Add("SZ");

            CompressedFile zip = new CompressedFile(newPath, CompressedFile.OpenMode.Read);

            foreach (DataRow dr in newObjectsDataTable.Rows)
            {
                string objectCountries = string.Empty;

                foreach (string country in countries)
                {
                    ProviderNoInstallation lp = new ProviderNoInstallation(Product, zip, country);
                    ActivationObject obj = new ActivationObject(lp);

                    string app = dr["NameSpace"].ToString().Substring(0, dr["NameSpace"].ToString().IndexOf("."));
                    string mod = dr["NameSpace"].ToString().Substring(app.Length + 1);
                    mod = mod.Substring(0, mod.IndexOf("."));

                    if (obj.IsInConfiguration(app, mod))
                    {
                        if (objectCountries == string.Empty)
                            objectCountries = country;
                        else
                            objectCountries = objectCountries + "," + country;
                    }
                }

                zip.Close();
                dr.BeginEdit();
                dr["Cultures"] = objectCountries;
                dr.EndEdit();
            }
        }

        //--------------------------------------------------------------------
        private void ExtractObjFromDocumentObjectsFiles(ArrayList stremArray)
        {

            DocumentsObjectInfo documentsObjectInfo = null;
            Microarea.TaskBuilderNet.Core.NameSolver.NewObject obj = null;
           
            foreach (Stream st in stremArray)
            {
                documentsObjectInfo = new DocumentsObjectInfo(st);
                documentsObjectInfo.Parse();

                foreach (DocumentInfo doc in documentsObjectInfo.Documents)
                {
                    obj = new Microarea.TaskBuilderNet.Core.NameSolver.NewObject(doc.NameSpace.ToString(), doc.IsBatch, doc.IsDataEntry, doc.IsFinder, false);

                    DataRow[] rows = newObjectsDataTable.Select("NameSpace ='" + obj.NameSpace + "' AND Type= '" + DefaultSecurityRolesEngine.GetTypeFromNewObject(obj) + "'");


                    if (rows == null || rows.Length == 0)
                        AddRow(obj);
                    else
                        newObjectsDataTable.Rows.Remove(rows[0]);
                }
            }
        }

        //--------------------------------------------------------------------
        private void ExtractObjFromMenuFiles(ArrayList stremArray)
        {

            XmlDocument document = null;
            Microarea.TaskBuilderNet.Core.NameSolver.NewObject newObj = null;

            foreach (Stream st in stremArray)
            {
                document = new XmlDocument();
                document.Load(st);

                XmlNodeList reportNodes = document.SelectNodes("descendant::" + XML_TAG_REPORT);
                foreach (XmlNode report in reportNodes)
                {
                    XmlNodeList objNodes = report.SelectNodes("child::" + XML_TAG_OBJECT);
                    foreach (XmlNode obj in objNodes)
                    {
                        newObj = new Microarea.TaskBuilderNet.Core.NameSolver.NewObject(obj.InnerText, false, false, false, true);

                        DataRow[] rows = newObjectsDataTable.Select("NameSpace ='" + newObj.NameSpace + "' AND Type='" + DefaultSecurityRolesEngine.GetTypeFromNewObject(newObj) + "'");

                        if (rows == null || rows.Length == 0)
                            AddRow(newObj);
                        else
                            newObjectsDataTable.Rows.Remove(rows[0]);
                    }
                }
            }
        }

        //--------------------------------------------------------------------
        private void LoadFiles(string zipFileName, string fileToFind)
        {
            if (filefromZip == null)
                filefromZip = new ArrayList();
            else
                filefromZip.Clear();

            CompressedFile zip = new ZipFile(zipFileName, CompressedFile.OpenMode.Read);
            CompressedEntry[] entries = zip.GetAllEntries();

            foreach (CompressedEntry theEntry in entries)
            {
                if (theEntry.IsDirectory)
                    continue; // controllo non sia una dir 

                if (String.Compare(fileToFind, Path.GetFileName(theEntry.Name)) == 0 ||
                    String.Compare(fileToFind, Path.GetExtension(theEntry.Name)) == 0)
                    filefromZip.Add(theEntry.CurrentStream);
            }
            zip.Close();

        }

        //--------------------------------------------------------------------
        public static string GetReportDefaultSecurityRoles(string reportFileName)
        {
            if (reportFileName == null || reportFileName.Length == 0 || !File.Exists(reportFileName))
                return String.Empty;

            Microarea.Library.Lexan.Parser reportParser = new Microarea.Library.Lexan.Parser(Parser.SourceType.FromFile);

            if (!reportParser.Open(reportFileName))
                return String.Empty;

            string defaultSecurityRoles = String.Empty;

            if (reportParser.SkipToToken(Token.DEFAULTSECURITYROLES, true, false))

                reportParser.ParseString(out defaultSecurityRoles);

            reportParser.Close();
            if (defaultSecurityRoles == null || defaultSecurityRoles.Length == 0)
                return String.Empty;

            return defaultSecurityRoles;
        }

        //--------------------------------------------------------------------
        public static bool ReportReplaceToken(string reportFileName, int line, int pos, string source, string target)
        {
            if (reportFileName == null || reportFileName.Length == 0 || !File.Exists(reportFileName))
                return false;

            try
            {
                StreamReader sr = File.OpenText(reportFileName);
                string report = sr.ReadToEnd();
                sr.Close();
                //-----
                int idx1 = Microarea.TaskBuilderNet.Core.Generic.Functions.IndexOfOccurrence(report, "\n", (int)line, 0);
                if (idx1 < 0) return false;

                int idx2 = report.IndexOf(source, idx1);
                if (idx2 < 0) return false;

                string left = idx2 > 0 ? report.Substring(0, idx2) : string.Empty;
                //string src = source.Length > 0 ? report.Substring(idx2, source.Length) : string.Empty;
                string right = report.Substring(idx2 + source.Length);

                string body = left + target + right;

                //salvo il report migrato
                StreamWriter sw = new StreamWriter(reportFileName, false, System.Text.Encoding.UTF8);
                sw.Write(body);
                sw.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }

        //--------------------------------------------------------------------
        public static bool SetReportDefaultSecurityRoles(string reportFileName, string roles)
        {
            if (reportFileName == null || reportFileName.Length == 0 || !File.Exists(reportFileName))
                return false;

            string report = string.Empty;

            string properties =
@"Properties
	Begin
		Title ""Accounting Settings Check"" 
		Subject """"
		Author """" 
		ReportProducer """" 
		Comments """" 
        DefaultSecurityRoles """ + roles + @""" 
	End
    PageInfo ";

            string defaultRolesEntry =
@"DefaultSecurityRoles """ + roles + @"""
End";

            string defaultRolesValue =
@" """ + roles + @"""";

            long line;
            int pos;
            string source;

            Microarea.Library.Lexan.Parser reportParser = new Microarea.Library.Lexan.Parser(Parser.SourceType.FromFile);
            if (!reportParser.Open(reportFileName))
                return false;

            //cerco la sezione properties che potrebbe anche non esserci
            Token[] tks = new Token[2] { Token.PROPERTIES, Token.PAGE_INFO };
            bool found = reportParser.SkipToToken(tks, false, false);
            if (!found)
                return false;
            if (reportParser.LookAhead(Token.PAGE_INFO))
            {
                //manca intera sezione Properties
                line = reportParser.CurrentLine;
                pos = reportParser.CurrentPos;
                source = reportParser.CurrentLexeme;
                reportParser.Close();

                return ReportReplaceToken(reportFileName, (int)line, pos, source, properties);
            }

            reportParser.SkipToken();   //toglie PROPERTIES
            reportParser.SkipToken();   //toglie BEGIN

            tks = new Token[2] { Token.END, Token.DEFAULTSECURITYROLES };
            found = reportParser.SkipToToken(tks, false, true);
            if (!found)
                return false;
            if (reportParser.LookAhead(Token.END))
            {
                //manca entry DEFAULTSECURITYROLES
                line = reportParser.CurrentLine;
                pos = reportParser.CurrentPos;
                source = reportParser.CurrentLexeme;
                reportParser.Close();

                return ReportReplaceToken(reportFileName, (int)line, pos, source, defaultRolesEntry);
            }

            //esiste entry DEFAULTSECURITYROLES, sostituiamo il suo valore se e vuoto
            reportParser.SkipToken(); //toglie DEFAULTSECURITYROLES

            Token tk = reportParser.LookAhead(); //sync necessario per la posizione
            line = reportParser.CurrentLine;
            pos = reportParser.CurrentPos;

            string currentRoles;
            reportParser.ParseString(out currentRoles);
            reportParser.Close();

            if (currentRoles.Length == 0)
            {
                return ReportReplaceToken(reportFileName, (int)line, pos, "\"" + currentRoles + "\"", defaultRolesValue);
            }
            else
            {
                //ruoli esistenti: decidere se sostituirli ugualmente o segnalarlo 
                //return ReportReplaceToken(reportFileName, (int)line, pos, "\"" + currentRoles + "\"", defaultRolesValue);
            }

            return false;
        }

        //--------------------------------------------------------------------
        private void button1_Click(object sender, EventArgs e)
        {

            if (!File.Exists(RoleFileTextBox.Text))
                return;

            ImportExportRole import = new ImportExportRole(RoleFileTextBox.Text);
            import.Parse();

            ImportExportRole importAdvanced = new ImportExportRole(this.AdvancedRoleTextBox.Text);
            importAdvanced.Parse();

            if (!Directory.Exists(ApplicationFolderTextBox.Text))
                return;

            foreach (string mod in Directory.GetDirectories(ApplicationFolderTextBox.Text))
            {
                string reportPath = Path.Combine(ApplicationFolderTextBox.Text, mod.Substring(mod.LastIndexOf('\\') + 1));
                reportPath = Path.Combine(reportPath, "Report");

                if (!Directory.Exists(reportPath))
                    continue;

                string[] wrmFiles = Directory.GetFiles(reportPath, "*.wrm");

                foreach (string wrmFile in wrmFiles)
                {
                    string reportName = wrmFile.Substring(wrmFile.LastIndexOf('\\') + 1);

                    reportName = reportName.Substring(0, reportName.IndexOf("."));

                    //Tolgo i blank e faccio i raplace con degli _
                    reportName = reportName.Trim();
                    reportName = reportName.Replace(" ", "_");

                    string reportNameSpace = String.Concat(ApplicationFolderTextBox.Text.Substring(ApplicationFolderTextBox.Text.LastIndexOf('\\') + 1), ".", mod.Substring(mod.LastIndexOf('\\') + 1), ".", reportName);
                    string roles = GetRolesFromObjectNameSpace(import, importAdvanced, reportNameSpace, 4);
                    if (roles != string.Empty)
                        SetReportDefaultSecurityRoles(wrmFile, roles);
                }
            }
        }

        //--------------------------------------------------------------------
        private string GetRolesFromObjectNameSpace(ImportExportRole import, ImportExportRole importAdvanced, string reportNameSpace, int objectType)
        {
            string roles = string.Empty;

            foreach (Role role in import.Roles)
            {
                foreach (Grant grant in role.Grats)
                {
                    if (string.Compare(grant.NameSpace, reportNameSpace, true) == 0 && grant.TypeId == objectType)
                    {
                        if (roles == string.Empty)
                            roles = role.RoleName;
                        else
                            roles = roles + "," + role.RoleName;
                        break;
                    }
                }

            }

            foreach (Role role in importAdvanced.Roles)
            {
                foreach (Grant grant in role.Grats)
                {
                    if (string.Compare(grant.NameSpace, reportNameSpace, true) == 0 && grant.TypeId == objectType)
                    {
                        if (role.RoleName.Substring(0, 1) == "c" ||
                            role.RoleName == "aMailConnector Parameter Manager" ||
                            role.RoleName == "aXtech Parameter Manager" ||
                            role.RoleName == "aUnprotected Report Manager")
                            if (roles == string.Empty)
                                roles = role.RoleName;
                            else
                                roles = roles + "," + role.RoleName;
                        break;
                    }
                }

            }

            return roles;
        }

        //--------------------------------------------------------------------
        private void OpenFolderButton_Click(object sender, EventArgs e)
        {
            DialogResult res = this.folderBrowserDialog1.ShowDialog();
            this.ApplicationFolderTextBox.Text = folderBrowserDialog1.SelectedPath;
        }

        //--------------------------------------------------------------------
        private void RoleFileButton_Click(object sender, EventArgs e)
        {
            DialogResult res = openFileDialog.ShowDialog();
            this.RoleFileTextBox.Text = openFileDialog.FileName;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        //--------------------------------------------------------------------
        private void button1_Click_1(object sender, EventArgs e)
        {
            DialogResult res = openFileDialog.ShowDialog();
            this.AdvancedRoleTextBox.Text = openFileDialog.FileName;
        }

        //--------------------------------------------------------------------
        private void button2_Click(object sender, EventArgs e)
        {
            if (!File.Exists(RoleFileTextBox.Text))
                return;

            ImportExportRole import = new ImportExportRole(RoleFileTextBox.Text);
            import.Parse();

            ImportExportRole importAdvanced = new ImportExportRole(this.AdvancedRoleTextBox.Text);
            importAdvanced.Parse();

            if (!Directory.Exists(ApplicationFolderTextBox.Text))
                return;

            foreach (string mod in Directory.GetDirectories(ApplicationFolderTextBox.Text))
            {
                string docObjectsPath = Path.Combine(ApplicationFolderTextBox.Text, mod.Substring(mod.LastIndexOf('\\') + 1));
                docObjectsPath = Path.Combine(docObjectsPath, "ModuleObjects");

                if (!Directory.Exists(docObjectsPath))
                    continue;

                if (!File.Exists(Path.Combine(docObjectsPath, NameSolverStrings.DocumentObjectsXml)))
                    continue;

                XmlDocument doc = new XmlDocument();
                doc.Load(Path.Combine(docObjectsPath, NameSolverStrings.DocumentObjectsXml));

                XmlNodeList nodes = doc.SelectNodes("descendant::Document");

                foreach (XmlNode node in nodes)
                {
                    XmlNodeList viewModes = node.SelectNodes("child::ViewModes");
                    foreach (XmlNode view in viewModes)
                    {
                        int type = 0;
                        XmlNode viewModeNode = view.SelectSingleNode("child::Mode[@name='Default']");
                        if (viewModeNode.Attributes["type"] == null)
                            type = 5;
                        else
                            GetTypeFromtypeName(viewModeNode.Attributes["type"].Value);


                        string roles = GetRolesFromObjectNameSpace(import, importAdvanced, node.Attributes["namespace"].Value, type);
                        if (roles != string.Empty)
                        {
                            if (node.Attributes["defaultsecurityroles"] != null)
                                node.Attributes["defaultsecurityroles"].Value = roles;
                            else
                            {
                                XmlAttribute attr = doc.CreateAttribute("defaultsecurityroles", string.Empty);
                                attr.Value = roles;
                                node.Attributes.Append(attr);
                            }
                        }
                    }
                }

                doc.Save(Path.Combine(docObjectsPath, NameSolverStrings.DocumentObjectsXml));
            }
        }

        //---------------------------------------------------------------------
        public int GetTypeFromtypeName(string type)
        {
            if (string.Compare(type, "batch", true) == 0)
                return 7;

            if (string.Compare(type, "finder", true) == 0)
                return 21;

            return 5;
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {

        }
    }
}

