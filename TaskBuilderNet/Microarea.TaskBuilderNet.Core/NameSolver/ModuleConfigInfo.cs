using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.NameSolver
{
    //=========================================================================
    public class ModuleFolderInfo
    {
        public StringCollection FilesExtensions = new StringCollection();
        private string subDir;
        private bool outputDir;

        #region Properties
        public string SubDir
        {
            get
            {
                if (outputDir)
                    return string.Empty;

                return subDir;
            }
        }

        public string DebugOutputDir
        {
            get
            {
                if (!outputDir)
                    return string.Empty;

                return subDir + Path.DirectorySeparatorChar + NameSolverStrings.Debug;
            }
        }

        public string ReleaseOutputDir
        {
            get
            {
                if (!outputDir)
                    return string.Empty;

                return subDir + Path.DirectorySeparatorChar + NameSolverStrings.Release;
            }
        }

        public bool OutputDir { get { return outputDir; } }
        #endregion

        //---------------------------------------------------------------------
        public ModuleFolderInfo(string subDir, bool outputDir)
        {
            this.subDir = subDir;
            this.outputDir = outputDir;

            FilesExtensions.AddRange(DefaultFilesExtensions());
        }

        /// <summary>
        /// Restituisce un array di stringhe contenente le FilesExtensions impostate di default;
        /// il default è *.dll, *.exe e *.exe.manifest.
        /// Fare attenzione che le "FilesExtensions" in realtà sono "FilesMask" (una file
        /// extension ad esempio è ".dll", ma questo metodo restituisce "*.dll").
        /// </summary>
        /// <returns>Array di stringhe contenente le FilesExtensions di default.</returns>
        //---------------------------------------------------------------------
        public static string[] DefaultFilesExtensions()
        {
            return
                new string[]
                {
                    "*" + NameSolverStrings.DllExtension,
                    "*" + NameSolverStrings.ExeExtension,
                    "*" + NameSolverStrings.ManifestExeExtension
                };
        }
    }

    /// <summary>
    /// Classe che legge il file Module.config e ne mantiene i dati.
    /// </summary>
    //=========================================================================
    public sealed class ModuleConfigInfo : IModuleConfigInfo
    {
        private string moduleName = string.Empty;
        private BaseModuleInfo parentModuleInfo;
        private string moduleConfigFile = string.Empty;
        private string title = string.Empty;
        private string localizedTitle = null;
        private string destinationFolder = string.Empty;
        private bool optional;
        private int menuViewOrder = int.MaxValue;
        private ArrayList moduleFolders = new ArrayList();
        private ArrayList libraries;
        private string signature = string.Empty;
        private int release = 0;

        #region Properties
        public IBaseModuleInfo ParentModuleInfo { get { return parentModuleInfo; } }
        public IList ModuleFolders { get { return moduleFolders; } }
        public IList Libraries { get { return libraries; } }
        public string ModuleConfigFile { get { return moduleConfigFile; } }
        public string ModuleName
        {
            get
            {
                return moduleName;
            }
            set
            {
                if (moduleName != value && !string.IsNullOrEmpty(value))
                    moduleConfigFile = Path.Combine(Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(moduleConfigFile)), value), Path.GetFileName(moduleConfigFile));

                moduleName = value;
            }
        }
        public string Title
        {
            get
            {
                if (localizedTitle == null)
                {
                    localizedTitle = LocalizableXmlDocument.LoadXMLString
                    (
                        title,
                        moduleConfigFile,
                        parentModuleInfo.DictionaryFilePath
                    );
                }

                return localizedTitle;
            }
        }
        public string DestinationFolder { get { return destinationFolder; } }
        public int MenuViewOrder { get { return menuViewOrder; } }
        /// <summary>
        /// Indica se il modulo è opzionale.
        /// </summary>
        public bool Optional { get { return optional; } }
        //Signature e release del modulo, dal vecchio DatabaseObjects.xml
        public string Signature { get { return signature; } set { signature = value; } }
        public int Release { get { return release; } }
        #endregion

        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="aModuleConfigFile">Path del file module.config</param>
        //---------------------------------------------------------------------
        public ModuleConfigInfo(string moduleName, BaseModuleInfo parentModuleInfo, string moduleConfigFile)
        {
            this.moduleName = moduleName;
            this.title = moduleName;
            this.moduleConfigFile = moduleConfigFile;
            this.parentModuleInfo = parentModuleInfo;
        }

        /// <summary>
        /// Legge il file module.config.
        /// </summary>
        /// <returns>Il successo della lettura</returns>
        //---------------------------------------------------------------------
        public bool Parse()
        {
            if (parentModuleInfo == null || parentModuleInfo.ParentApplicationInfo == null || !File.Exists(moduleConfigFile))
                return false;

            XmlDocument moduleConfigDocument = null;

            try
            {
                moduleConfigDocument = new XmlDocument();
                moduleConfigDocument.Load(moduleConfigFile);

                XmlElement root = moduleConfigDocument.DocumentElement;
                if (root == null)
                {
                    Debug.Fail("Sintassi del file " + moduleConfigFile);
                    return false;
                }

                // Title
                title = root.GetAttribute(ModuleConfigXML.Attribute.Localize);
                if (title == null || title.Length == 0)
                    title = moduleName;

                //DestinationFolder è la cartella dentro MicroareaClient
                //dove vanno a finire le dll
                destinationFolder = root.GetAttribute(ModuleConfigXML.Attribute.DestinationFolder);

                //optional
                optional = false;
                string tmp = root.GetAttribute(ModuleConfigXML.Attribute.Optional);
                try
                {
                    if (tmp != string.Empty)
                        optional = bool.Parse(tmp);
                }
                catch (Exception err)
                {
                    Debug.Fail(err.Message);
                }

                //MenuViewOrder
                tmp = root.GetAttribute(ModuleConfigXML.Attribute.MenuViewOrder);
                try
                {
                    if (tmp != string.Empty)
                        menuViewOrder = Int32.Parse(tmp);
                }
                catch (Exception err)
                {
                    Debug.Fail(err.Message);
                }

                //cartelle di default
                //cartella bin estensioni exe e dll
                ModuleFolderInfo moduleFolderInfo = new ModuleFolderInfo(NameSolverStrings.Bin, true);

                //Estensioni di file aggiuntive
                XmlNodeList rootFileExtensionsList = root.GetElementsByTagName(ModuleConfigXML.Element.FilesExtension);
                if (rootFileExtensionsList != null && rootFileExtensionsList.Count > 0)
                {
                    //scorro i FilesExtension
                    //non devono essere presenti le estensioni dll exe exe.manifest
                    foreach (XmlElement FilesExtensionElement in rootFileExtensionsList)
                    {
                        //tipo di file da copiare
                        string extension = FilesExtensionElement.GetAttribute(ModuleConfigXML.Attribute.Value);

                        moduleFolderInfo.FilesExtensions.Add(extension);
                    }
                }

                moduleFolders.Add(moduleFolderInfo);

                //cartella di help
                if (Directory.Exists(this.ParentModuleInfo.Path + "\\" + NameSolverStrings.Help + "\\Bin"))
                {
                    DirectoryInfo helpBin = new DirectoryInfo(this.ParentModuleInfo.Path + "\\" + NameSolverStrings.Help + "\\Bin");
                    DirectoryInfo[] helpLangs = helpBin.GetDirectories();
                    foreach (DirectoryInfo helpLang in helpLangs)
                    {
                        DirectoryInfo[] helpEditions = helpLang.GetDirectories();
                        foreach (DirectoryInfo helpEdition in helpEditions)
                        {
                            ModuleFolderInfo helpFolderInfo = new ModuleFolderInfo(NameSolverStrings.Help + "\\Bin\\" + helpLang.Name + "\\" + helpEdition.Name, false);
                            helpFolderInfo.FilesExtensions.Add("*.chm");
                            moduleFolders.Add(helpFolderInfo);
                        }
                    }
                }

                //cartella di progetto
                moduleFolderInfo = new ModuleFolderInfo(String.Empty, false);

                //Estensioni di file aggiuntive
                rootFileExtensionsList = root.GetElementsByTagName(ModuleConfigXML.Element.FilesExtension);
                if (rootFileExtensionsList != null && rootFileExtensionsList.Count > 0)
                {
                    //scorro i FilesExtension
                    //non devono essere presenti le estensioni dll exe exe.manifest
                    foreach (XmlElement FilesExtensionElement in rootFileExtensionsList)
                    {
                        //tipo di file da copiare
                        string extension = FilesExtensionElement.GetAttribute(ModuleConfigXML.Attribute.Value);

                        moduleFolderInfo.FilesExtensions.Add(extension);
                    }
                }

                moduleFolders.Add(moduleFolderInfo);

                //Signature e release di modulo
                XmlNodeList dbObjectsInfoNodes = root.GetElementsByTagName(ModuleConfigXML.Element.DbObjectsInfo);
                if (dbObjectsInfoNodes != null && dbObjectsInfoNodes.Count > 0 && dbObjectsInfoNodes[0] != null)
                {
                    signature = ((XmlElement)dbObjectsInfoNodes[0]).GetAttribute(ModuleConfigXML.Attribute.Signature);
                    Int32.TryParse(((XmlElement)dbObjectsInfoNodes[0]).GetAttribute(ModuleConfigXML.Attribute.Release), out release);
                }

                //Nodo Components
                XmlNodeList componentsNodes = root.GetElementsByTagName(ModuleConfigXML.Element.Components);

                // decidere se il nodo Components deve esiste necessariamente oppure se, come adesso,
                // non serve la sua esistenza.
                if (componentsNodes != null && componentsNodes.Count > 0)
                {
                    XmlNodeList libraryDependenciesElements = ((XmlElement)componentsNodes[0]).GetElementsByTagName(ModuleConfigXML.Element.Library);

                    if (libraryDependenciesElements != null && libraryDependenciesElements.Count > 0)
                    {
                        libraries = new ArrayList();

                        foreach (XmlElement library in libraryDependenciesElements)
                        {
                            LibraryInfo libraryInfo = new LibraryInfo(parentModuleInfo);
                            libraryInfo.Name = library.GetAttribute(ModuleConfigXML.Attribute.Name);
                            libraryInfo.AggregateName = library.GetAttribute(ModuleConfigXML.Attribute.AggregateName);

                            string sourceFolder = library.GetAttribute("sourcefolder");
                            string libfullPath = Path.Combine(this.parentModuleInfo.Path, sourceFolder);
                            if (String.IsNullOrEmpty(sourceFolder))
                            {
                                int lastSepIdx = libfullPath.LastIndexOf(Path.DirectorySeparatorChar);
                                if (lastSepIdx < (libfullPath.Length - 1))
                                    libraryInfo.Path = libfullPath.Substring(lastSepIdx + 1);
                            }
                            else
                                libraryInfo.Path = sourceFolder;

                            if (sourceFolder == string.Empty)
                                sourceFolder = NameSolverStrings.Bin;
                            else
                                sourceFolder += Path.DirectorySeparatorChar + NameSolverStrings.Bin;

                            moduleFolderInfo = new ModuleFolderInfo(sourceFolder, true);

                            //Estensioni di file aggiuntive
                            XmlNodeList fileExtensionsList = library.GetElementsByTagName(ModuleConfigXML.Element.FilesExtension);

                            if (fileExtensionsList != null && fileExtensionsList.Count > 0)
                            {
                                //scorro i FilesExtension
                                //non devono essere presenti le estensioni dll exe exe.manifest
                                foreach (XmlElement FilesExtensionElemnt in fileExtensionsList)
                                {
                                    //tipo di file da copiare
                                    string extension = FilesExtensionElemnt.GetAttribute(ModuleConfigXML.Attribute.Value);

                                    moduleFolderInfo.FilesExtensions.Add(extension);
                                }
                            }

                            moduleFolders.Add(moduleFolderInfo);

                            libraries.Add(libraryInfo);
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                Debug.Fail(e.Message);
                return false;
            }
            catch (Exception err)
            {
                Debug.Fail(err.Message);
                return false;
            }

            return true;
        }

        //---------------------------------------------------------------------
        public string GetOutPutPath(bool debug)
        {
            foreach (ModuleFolderInfo mfi in ModuleFolders)
            {
                if (mfi.OutputDir)
                    return debug ? mfi.DebugOutputDir : mfi.ReleaseOutputDir;
            }

            return string.Empty;
        }

        //---------------------------------------------------------------------
        public bool Save()
        {
            XmlDocument xDoc = new XmlDocument();

            XmlDeclaration xmlDeclaration = xDoc.CreateXmlDeclaration(NameSolverStrings.XmlDeclarationVersion, NameSolverStrings.XmlDeclarationEncoding, null);
            XmlElement rootNode = xDoc.CreateElement(ModuleConfigXML.Element.ModuleInfo);
            xDoc.AppendChild(rootNode);
            xDoc.InsertBefore(xmlDeclaration, xDoc.DocumentElement);
            xDoc.AppendChild(rootNode);

            rootNode.SetAttribute(ModuleConfigXML.Attribute.Localize, moduleName);

            XmlElement typeNode = xDoc.CreateElement(ModuleConfigXML.Element.Components);
            rootNode.AppendChild(typeNode);

            xDoc.Save(moduleConfigFile);
            return true;
        }
    }
}