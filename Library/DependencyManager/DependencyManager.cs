using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.Library.Licence;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

using PEAnalyzer = Microarea.Library.PEFileAnalyzer.PEFileAnalyzer;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.Library.DependencyManager
{
    /// <summary>
    /// Classe per memorizzare nel Dictionary il nome dell'applicazione, del modulo
    /// e della libreria a cui appartiene la Library oggetto di analisi.
    /// </summary>
    //=========================================================================
    public class LibraryData : IComparable
    {
        private readonly string application;
        private readonly string module;
        private readonly ILibraryInfo lib;

        //---------------------------------------------------------------------
        public LibraryData(string applicationName, string moduleName, ILibraryInfo lib)
        {
            this.application = applicationName;
            this.module = moduleName;
            this.lib = lib;
        }
        public string Application { get { return this.application; } }
        public string Module { get { return this.module; } }
        public string Name { get { return this.lib.Name; } }
        public string Policy { get { return this.lib.DeploymentPolicy; } }

        public string GetDllRelPath()
        {
            string[] tokens = new string[6];
            int i = 0;
            tokens[i++] = NameSolverStrings.TaskBuilderApplications;
            tokens[i++] = this.application;
            tokens[i++] = this.module;
            tokens[i++] = this.lib.Path;
            tokens[i++] = NameSolverStrings.Bin;
            tokens[i++] = NameSolverStrings.Release;
            string relDir = string.Join(Path.DirectorySeparatorChar.ToString(), tokens);
            return string.Concat(relDir, Path.DirectorySeparatorChar, lib.Name, NameSolverStrings.DllExtension);
        }

        public string GetModuleAttributeValue()
        {
            return string.Concat(this.application, ".", this.module);
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            LibraryData other = obj as LibraryData;
            if (object.ReferenceEquals(other, null))
                return 1;

            // should be sorted with this sintax: 
            // <[Library|Dependency] module="ERP.InventoryAccounting" name="InventoryAccountingAddOnsItems" policy="addon">

            int moduleComp = this.GetModuleAttributeValue().CompareTo(other.GetModuleAttributeValue());
            if (moduleComp != 0)
                return moduleComp;

            int nameComp = this.lib.Name.CompareTo(other.lib.Name);
            if (nameComp != 0)
                return nameComp;

            return this.Policy.CompareTo(other.Policy);
        }

        #endregion

        public override bool Equals(object obj)
        {
            LibraryData other = obj as LibraryData;
            if (object.ReferenceEquals(other, null))
                return false;
            return this.CompareTo(other) == 0;
        }

        public override int GetHashCode()
        {
            return string.Concat(this.GetModuleAttributeValue(), "_", this.lib.Name, "_", this.Policy).GetHashCode();
        }
    }

    /// <summary>
    /// Classe che legge i file Module.Config (la cui struttura è nel PathFinder), 
    /// utilizza il componente PEAnalyzer per leggere l'header della dll presa in esame
    /// e serializza l'elenco delle stesse in un file _Solution_.DependenciesMap.xml.
    /// </summary>
    /// <remarks>2.7 READY, full backward compatible.</remarks>
    //=========================================================================
    public class LibraryDependency
    {
        #region Private members
        private BasePathFinder pathFinder;
        private string productName;
        private string path;
        private Dictionary<string, LibraryData> libraryMap;
        private Dictionary<string, LibraryData> fullLibraryMap;
        private Dictionary<string, List<string>> aggregations;

        // (BOZZA DI) Analisi script di database, per ora inutilizzato ($, 12/7/2007)
        //private DependentItemCatalogue catalogue;

        /// <remarks>
        /// Definisco un nuovo oggetto di tipo Diagnostic che leggo per
        /// intero alla fine, piuttosto che fare uno Show per ogni item
        /// aggiunto perchè in caso di molti errori è fastidioso.
        /// </remarks>
        private Diagnostic diagnostic;
        #endregion

        #region Public properties
        public Diagnostic Diagnostic { get { return diagnostic; } }
        #endregion

        #region Public methods
        /// <summary>
        /// Costruzione delle dipendenze e generazione del file DependenciesMap.xml
        /// di tutte le applicazioni TaskBuilder.
        /// </summary>
        /// <param name="prodInfo">Una istanza di ProductInfo inizializzata.</param>
        /// <param name="path">Il percorso assoluto dove salvare la mappa delle dipendenze.</param>
        /// <param name="pathFinder">Una istanza di BasePathFinder già inizializzata.</param>
        //---------------------------------------------------------------------
        public void MakeDependencyMap(ProductInfo prodInfo, string path, BasePathFinder pathFinder)
        {
            if (prodInfo == null ||
                prodInfo.Articles == null ||
                prodInfo.Articles.Length == 0 ||
                prodInfo.ProductName.Length == 0)
            {
                diagnostic.SetError(Strings.NoAppsDetected);
                return;
            }

            this.pathFinder = pathFinder;
            this.productName = prodInfo.ProductName;
            this.path = path;
            diagnostic = new Diagnostic(productName);
            libraryMap = new Dictionary<string, LibraryData>(StringComparer.InvariantCultureIgnoreCase);
            fullLibraryMap = new Dictionary<string, LibraryData>(StringComparer.InvariantCultureIgnoreCase);
            aggregations = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);

            // Elenco di tutte le Applications contenute nella cartella di applicazione
            // Servono per poter descrivere i verticali che dipendono da
            // altri verticali, tutti TaskBuilder applications.

            StringCollection applications = new StringCollection();
            pathFinder.GetApplicationsList(ApplicationType.TaskBuilderApplication, out applications);
            List<string> productApplications = GetProductApplications(prodInfo);

            IDictionary<string, string> productApplications1 = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string productApplication in productApplications)
                productApplications1.Add(productApplication, productApplication);

            if (applications.Count > 0)
            {
                Dictionary<string, ILibraryInfo> aggInfos = GetAggregatedLibraryInfos(productApplications);
                foreach (string application in applications)
                {
                    IBaseApplicationInfo baseAppInfo = pathFinder.GetApplicationInfoByName(application);
                    Read(baseAppInfo, fullLibraryMap, aggInfos);

                    if (productApplications1.ContainsKey(application))
                        if (Read(baseAppInfo, libraryMap, aggInfos))
                            DetectAggregations(baseAppInfo);
                }

#if DEBUG
                if (aggInfos != null && aggInfos.Count != 0)
                    CompareAggregationList(aggInfos, aggregations);
#endif

                Write();

                libraryMap.Clear();
                fullLibraryMap.Clear();
                aggregations.Clear();
            }
            else
                diagnostic.SetError(Strings.NoAppsDetected);
        }

#if DEBUG
        private static void CompareAggregationList(Dictionary<string, ILibraryInfo> aggInfos, Dictionary<string, List<string>> aggregations)
        {
            Debug.Assert(aggInfos.Count == aggregations.Count);
            foreach (string libName in aggregations.Keys)
            {
                string[] toks = libName.Split('.');
                string key = toks[toks.Length - 1];
                Debug.Assert(aggInfos.ContainsKey(key));
            }
        }
#endif
        #endregion

        #region Private methods
        /// <summary>
        /// Restituisce un elenco di nomi di cartelle di applicazione di
        /// TaskBuilder applications definite dai Sales Module di un prodotto.
        /// </summary>
        /// <param name="prodInfo">Un oggetto ProductInfo inizializzato.</param>
        /// <returns>A list of application names</returns>
        //---------------------------------------------------------------------
        private List<string> GetProductApplications(ProductInfo prodInfo)
        {
            List<string> applications = new List<string>();

            foreach (ArticleInfo articleInfo in prodInfo.Articles)
                if (articleInfo.Modules != null)
                    foreach (Microarea.Library.Licence.ModuleInfo modInfo in articleInfo.Modules)
                        if (string.Compare(modInfo.Container, NameSolverStrings.TaskBuilderApplications, true, CultureInfo.InvariantCulture) == 0 &&
                            modInfo.Type == Microarea.Library.Licence.ModuleInfo.ModuleType.Module &&
                            !applications.Contains(modInfo.Application))
                            applications.Add(modInfo.Application);

            return applications;
        }
        /*
        //---------------------------------------------------------------------
        private void AddAggregatedModulesInfo(BaseApplicationInfo baseAppInfo)
        {
            // (BOZZA DI) Analisi script di database, per ora inutilizzato ($, 12/7/2007)
            ArrayList list = new ArrayList();
            foreach (DictionaryEntry de in aggregations)
            {
                ArrayList al = (ArrayList)de.Value;
                foreach (string aggModule in al)
                    if (!list.Contains(aggModule))
                        list.Add(aggModule);
            }

            list.Sort();

            // Analisi degli script di creazione del database
            catalogue = new DependentItemCatalogue();
            XmlDocument doc;
            DatabaseItem item;

            foreach (string aggModule in list)
            {
                BaseModuleInfo module = new BaseModuleInfo(aggModule, baseAppInfo);
                string dbScriptPath = Path.Combine(module.GetDatabaseScriptPath(), "Create");
                string dbScriptFile = Path.Combine(dbScriptPath, "CreateInfo.xml");

                if (!Directory.Exists(dbScriptPath) || !File.Exists(dbScriptFile))
                    continue;

                doc = new XmlDocument();
                doc.Load(dbScriptFile);
                item = new DatabaseItem(module.ParentApplicationName, module.Name, doc.DocumentElement);
                item.GetReferences();
                catalogue.SetDependentItemNode(item);
            }
        }
        */
        //---------------------------------------------------------------------
        private void DetectAggregations(IBaseApplicationInfo baseAppInfo)
        {
            foreach (string library in libraryMap.Keys)
            {
                LibraryData libData = libraryMap[library];
                string key = libData.Application + "." + libData.Module + "." + library;

                List<string> modules = new List<string>();

                if (baseAppInfo != null && baseAppInfo.Modules != null)
                    foreach (BaseModuleInfo module in baseAppInfo.Modules)
                        if (module.ModuleConfigInfo.Libraries != null)
                            foreach (LibraryInfo lib in module.ModuleConfigInfo.Libraries)
                                if (string.Compare(lib.AggregateName, library, StringComparison.InvariantCultureIgnoreCase) == 0 &&
                                    !modules.Contains(module.Name))
                                {
                                    modules.Add(module.Name);
                                }

                if (modules.Count > 0)
                {
                    modules.Sort();
                    aggregations.Add(key, modules);
                }
                //else
                //    Debug.WriteLine("Library skipped (no modules): " + library);
            }
        }

        // return the list of aggregated dlls for a product
        private Dictionary<string, ILibraryInfo> GetAggregatedLibraryInfos(List<string> productApplications)
        {
            Dictionary<string, ILibraryInfo> aggDic = new Dictionary<string, ILibraryInfo>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string application in productApplications)
            {
                IBaseApplicationInfo baseAppInfo = pathFinder.GetApplicationInfoByName(application);
                if (baseAppInfo == null || baseAppInfo.Modules == null)
                    continue;
                foreach (IBaseModuleInfo moduleInfo in baseAppInfo.Modules)
                    if (moduleInfo.ModuleConfigInfo.Libraries != null)
                        foreach (ILibraryInfo lib in moduleInfo.ModuleConfigInfo.Libraries)
                        {
                            if (lib.AggregateName == null ||
                                lib.AggregateName.Length == 0 ||
                                string.Compare(lib.AggregateName, lib.Name, StringComparison.InvariantCultureIgnoreCase) != 0)
                                continue;
                            aggDic.Add(lib.AggregateName, lib);
                        }
            }
            return aggDic;
        }

        /// <summary>
        /// Per tutti i moduli contenuti nell'applicazione leggo il relativo
        /// file Module.config (con la struttura fornita dal parse di
        /// PathFinder) e memorizzo i nodi di tipo Library, il nome del modulo
        /// e della cartella di appartenenza.
        /// </summary>
        /// <returns>true se viene individuato almeno un modulo con DLL aggregate, false altrimenti.</returns>
        //---------------------------------------------------------------------
        private bool Read(IBaseApplicationInfo baseAppInfo, Dictionary<string, LibraryData> ht, Dictionary<string, ILibraryInfo> aggInfos)
        {
            string binRel = NameSolverStrings.Bin + Path.DirectorySeparatorChar + NameSolverStrings.Release + Path.DirectorySeparatorChar;
            bool aggregated = false;

            if (baseAppInfo != null && baseAppInfo.Modules != null)
                foreach (BaseModuleInfo moduleInfo in baseAppInfo.Modules)
                    if (moduleInfo.ModuleConfigInfo.Libraries != null)
                        foreach (LibraryInfo lib in moduleInfo.ModuleConfigInfo.Libraries)
                        {
                            //// "addon" type dlls are excluded...
                            //if (string.Compare(lib.DeploymentPolicy, "addon", StringComparison.InvariantCultureIgnoreCase) == 0)
                            //    continue;

                            // Attribute AggregateName might exist but not be used, as in the case of
                            // Mago.Net 2.9.x, where module.config sources are shared with the 3.x version

                            string dllNameWithEstension = lib.Name + NameSolverStrings.DllExtension;
                            string dllServerPath = Path.Combine(lib.FullPath, string.Concat(binRel, dllNameWithEstension));
                            bool existsOnServer = File.Exists(dllServerPath);

                            if (!existsOnServer)
                            {
                                // the library might be declared for another edition, and thus be cited in the module.config...
                                // ...or might have been aggregated, and should be found in the aggregated module with the aggregated name

                                if (lib.AggregateName.Length == 0) // AggregateName attribute does not exist
                                    continue; // might be declared in module.config just for another edition

                                if (string.Compare(lib.Name, lib.AggregateName, StringComparison.InvariantCultureIgnoreCase) != 0)
                                {
                                    if (aggInfos == null || aggInfos.Count == 0)
                                        continue;

                                    // the dll could have been aggregated
                                    // server dll should be found in the aggregated module, with the aggregated name
                                    if (!aggInfos.ContainsKey(lib.AggregateName))
                                    {
                                        Debug.WriteLine("");
                                        Debug.WriteLine(string.Format("{0} does not exist\r\nseems aggregated into {1}, which is not declared in any Module.config", dllServerPath, lib.AggregateName));
                                        continue;
                                    }
                                    Debug.Assert(aggInfos.ContainsKey(lib.AggregateName));
                                    ILibraryInfo aggLib = aggInfos[lib.AggregateName];
                                    string aggDllServerPath = Path.Combine(aggLib.FullPath, string.Concat(binRel, aggLib.Name + NameSolverStrings.DllExtension));
                                    if (!File.Exists(aggDllServerPath))
                                    {
                                        Debug.WriteLine("");
                                        Debug.WriteLine(string.Format("{0} does not exist\r\neems aggregated into {1}\r\nwhich itself does not exist.", dllServerPath, aggDllServerPath));
                                        continue;
                                    }

                                    // aggregated dll exists (and aggregating does not), so we assume that the dll has
                                    // been aggregated and add the details of the aggregated one
                                    // server dll should be found in the aggregated module, with the aggregated name
                                    string aggAppName = aggLib.ParentModuleInfo.ParentApplicationName;
                                    string aggModName = aggLib.ParentModuleName;

                                    aggregated = true;

                                    // name="CoreServices"
                                    // aggregatename="CoreDocuments"
                                    if (lib.AggregateName.StartsWith(lib.ParentModuleName, true, CultureInfo.InvariantCulture) &&
                                        !ht.ContainsKey(lib.AggregateName))
                                        ht.Add
                                        (
                                            lib.AggregateName,
                                            new LibraryData(aggAppName, aggModName, aggLib)
                                        );
                                    continue;
                                }

                                Debug.WriteLine("");
                                Debug.WriteLine(string.Format("{0} should be aggregated but does not exist.", dllServerPath));
                                continue; // might be declared in module.config just for another edition
                            }

                            // if we are here, the dll exists on server, and has not been aggregated

                            // L'attributo AggregateName è stato introdotto con Mago.Net v. 3.0.
                            if (lib.AggregateName.Length == 0) // AggregateName attribute does not exist
                            {
                                if (!ht.ContainsKey(lib.Name))
                                    // Aggiungo la libreria nella mappa.
                                    // Il controllo di non esistenza in teoria è inutile
                                    // perchè ogni libreria ha (deve avere) un nome diverso,
                                    // ma se un Module.config è frutto di copia_e_incolla
                                    // e non è aggiornato, il metodo Add si schianta.
                                    ht.Add
                                    (
                                        lib.Name,
                                        new LibraryData(baseAppInfo.Name, moduleInfo.Name, lib)
                                    );
                                else
                                    diagnostic.SetError(string.Format(Strings.InvalidLibrary, lib.Name, moduleInfo.Name));
                            }
                            else // AggregateName attribute exists
                            {
                                // ...ed inserite solamente quelle dichiarate dal modulo,
                                // non "tirate dentro" da altri moduli.
                                if (string.Compare(lib.Name, lib.AggregateName, StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    // no matter whether it is aggregated or not, we have to add it with its name
                                    aggregated = true;

                                    if (ht.ContainsKey(lib.AggregateName))
                                        ht.Remove(lib.AggregateName);
                                    // SalesDocuments viene inserito da SalesBatchDocuments,
                                    // ma viene sostituito dalla libreria "legittima"

                                    ht.Add
                                    (
                                        lib.AggregateName,
                                        new LibraryData(baseAppInfo.Name, moduleInfo.Name, lib)
                                    );
                                }
                                else
                                {
                                    if (!ht.ContainsKey(lib.Name))
                                        ht.Add
                                        (
                                            lib.Name,
                                            new LibraryData(baseAppInfo.Name, moduleInfo.Name, lib)
                                        );
                                    else
                                        diagnostic.SetError(string.Format(Strings.InvalidLibrary, lib.Name, moduleInfo.Name));
                                }
                            }
                        } // foreach (LibraryInfo lib in moduleInfo.ModuleConfigInfo.Libraries)

            return aggregated;
        }
        /// <summary>
        /// Scrive le informazioni in un DOM.
        /// </summary>
        /// <remarks>
        /// Vengono analizzate sempre le dll buildate in debug (GetOutPutPath(debug))
        /// perchè le informazioni di dipendenza delle dll attualmente sono estrapolabili
        /// solo dalle versioni di debug.
        /// Questo non è mai stato considerato una limitazione poiché le dipendenze delle
        /// dll di Microarea non cambiano tra debug e release, ma cambiano solo quelle di
        /// sistema (Bruna, 02/04/2004).
        /// </remarks>
        //---------------------------------------------------------------------
        private void Write()
        {
            XmlDocument xDoc = new XmlDocument();
            XmlDeclaration newDec =
                xDoc.CreateXmlDeclaration
                (
                    NameSolverStrings.XmlDeclarationVersion,
                    NameSolverStrings.XmlDeclarationEncoding,
                    null
                );
            xDoc.AppendChild(newDec);

            XmlElement newSeq = xDoc.CreateElement(DependenciesMapXML.Element.DependenciesMap);
            xDoc.AppendChild(newSeq);

            List<string> list;
            XmlElement newLib, newDep = null;

            // <Aggregations>
            if (aggregations.Count > 0)
            {
                newLib = xDoc.CreateElement(DependenciesMapXML.Element.Aggregations);

                list = new List<string>(aggregations.Count);
                list.AddRange(aggregations.Keys);
                list.Sort();

                foreach (string key in list)
                {
                    // key: "ERP.Accounting.AccountingComponents"
                    List<string> modulesList = aggregations[key];

                    if (modulesList.Count > 0)
                    {
                        string modules = string.Empty;
                        foreach (string module in modulesList)
                            modules += module + ",";
                        modules = modules.Remove(modules.Length - 1);

                        newDep = xDoc.CreateElement(DependenciesMapXML.Element.AggregateName);
                        newDep.SetAttribute(DependenciesMapXML.Attribute.Name, key.Split('.')[2]);
                        newDep.SetAttribute(DependenciesMapXML.Attribute.Application, key.Split('.')[0]);
                        newDep.SetAttribute(DependenciesMapXML.Attribute.Module, key.Split('.')[1]);
                        newDep.SetAttribute(DependenciesMapXML.Attribute.Modules, modules);
                        newLib.AppendChild(newDep);
                    }
                }

                xDoc.DocumentElement.AppendChild(newLib);
            }
            // </Aggregations>

            // (BOZZA DI) Analisi script di database, per ora inutilizzato ($, 12/7/2007)
            /*
            // </DbDependencies>
            if (catalogue != null)
            {
                //TODO$: Stringa cablata! (Write)
                newLib = xDoc.CreateElement("DbDependencies");

                ArrayList al = new ArrayList();
                foreach (DependentItemNode node in catalogue)
                {
                    if (node.References.Count > 0)
                        foreach (DependentItemNode reference in node.References.Values)
                            if (!al.Contains(reference.LookUpName))
                                al.Add(reference.LookUpName);
                }

                xDoc.DocumentElement.AppendChild(newLib);
            }
            // </DbDependencies>
            */

            // <Library>
            string stdPath = pathFinder.GetStandardPath();

            List<LibraryData> libs = new List<LibraryData>(libraryMap.Count);
            libs.AddRange(libraryMap.Values);
            libs.Sort();

            // use sorted collection
            foreach (LibraryData levOneLib in libs)
            {
                string fileName = Path.Combine(stdPath, levOneLib.GetDllRelPath());
                bool existsOnServer = File.Exists(fileName);
                //if (string.Compare(levOneLib.Policy, "addon", true, CultureInfo.InvariantCulture) != 0)
                //	Debug.Assert(existsOnServer); // commented: might happen also with enterprise edition stuff read from Module.config when creating professional edition
                if (!existsOnServer)
                    continue;

                if (!PEAnalyzer.IsValidPEFile(fileName))
                {
                    string msg = "File has not valid PE format: '" + fileName + "'";
                    Debug.Fail(msg);
                    diagnostic.SetError(msg); // not wise?
                    continue;
                }

                // Creo una riga col tag Library
                newLib = xDoc.CreateElement(DependenciesMapXML.Element.Library);
                newLib.SetAttribute(DependenciesMapXML.Attribute.Module, levOneLib.GetModuleAttributeValue());
                newLib.SetAttribute(DependenciesMapXML.Attribute.Name, levOneLib.Name);
                newLib.SetAttribute(DependenciesMapXML.Attribute.Policy, levOneLib.Policy);
                xDoc.DocumentElement.AppendChild(newLib);

                string[] libraryImports = GetAllDependencies(fileName, fullLibraryMap, stdPath); // search on MicroareaServer

                List<LibraryData> depLibs = new List<LibraryData>(libraryImports.Length);
                foreach (string dll in libraryImports)
                {
                    string dllName = Path.GetFileNameWithoutExtension(dll);

                    // we already filtered the dependencies outside the destination folder
                    Debug.Assert(fullLibraryMap.ContainsKey(dllName));

                    LibraryData lib = fullLibraryMap[dllName];
                    depLibs.Add(lib);
                }
                depLibs.Sort();
                // use sorted collection of LibraryData
                foreach (LibraryData lib in depLibs)
                {
                    newDep = xDoc.CreateElement(DependenciesMapXML.Element.Dependency);
                    newDep.SetAttribute(DependenciesMapXML.Attribute.Module, lib.GetModuleAttributeValue());
                    newDep.SetAttribute(DependenciesMapXML.Attribute.Name, lib.Name);
                    newDep.SetAttribute(DependenciesMapXML.Attribute.Policy, lib.Policy);
                    newLib.AppendChild(newDep);
                }
            } // foreach (LibraryData levOneLib in libs)
            // </Library>

            Save(xDoc);
        }


        private string[] GetAllDependencies(string fileName, Dictionary<string, LibraryData> fullLibraryMap, string stdPath)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            GetAllDependencies(fileName, result, fullLibraryMap, stdPath);
            List<string> resList = new List<string>(result.Count);
            resList.AddRange(result.Values);
            resList.Sort();
            return resList.ToArray();
        }
        private void GetAllDependencies(string file, Dictionary<string, string> result, Dictionary<string, LibraryData> fullLibraryMap, string stdPath)
        {
			string[] libraryImports = null;
            using (PEAnalyzer libraryAnalyzer = new PEAnalyzer(file))
				libraryImports = libraryAnalyzer.GetImports();
            if (libraryImports == null)
            {
                if (this.diagnostic != null)
                    this.diagnostic.SetError("Unable to read dependencies of file " + file);
                return;
            }
            foreach (string library in libraryImports)
            {
                string key = Path.GetFileNameWithoutExtension(library);
                if (!fullLibraryMap.ContainsKey(key))
                    continue; // will not find it in TbApps
                if (!result.ContainsKey(library))
                {
                    result[library] = library;
                    LibraryData levTwoLib = fullLibraryMap[key];
                    string libPath = Path.Combine(stdPath, levTwoLib.GetDllRelPath());
                    GetAllDependencies(libPath, result, fullLibraryMap, stdPath);
                }
            }
        }

        /// <summary>
        /// Salva le info nel file _Solution_.DependenciesMap.xml.
        /// </summary>
        //---------------------------------------------------------------------
        private void Save(XmlDocument xDoc)
        {
            string depMapFile = Path.Combine(path, BasePathFinder.GetDependenciesMapFileName(productName));

            // Se il file è ReadOnly lo rendo scrivibile prima di trasferire i dati.
            FileInfo fileInfo = new FileInfo(depMapFile);
            if (
                fileInfo.Exists &&
                ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                )
                fileInfo.Attributes -= FileAttributes.ReadOnly;

            // Scrivo il file xml in formato indentato.
            XmlTextWriter tr = new XmlTextWriter(depMapFile, null);
            tr.Formatting = Formatting.Indented;
            xDoc.WriteContentTo(tr);
            tr.Close();

            //imposto la data oggi a mezzanotte....
            fileInfo.LastWriteTimeUtc = new DateTime
                (
                DateTime.Today.Year,
                DateTime.Today.Month,
                DateTime.Today.Day,
                0,
                0,
                0);

        }
        #endregion
    }
}
