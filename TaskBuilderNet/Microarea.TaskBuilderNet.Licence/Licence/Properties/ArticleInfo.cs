using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Activation;
using Microarea.TaskBuilderNet.Licence.Licence.ConfigurationInfoProvider;
using Microarea.TaskBuilderNet.Licence.Licence.XmlSyntax;

namespace Microarea.TaskBuilderNet.Licence.Licence
{

    /// <summary>
    /// Oggetto che contiene tutte le informazioni relative ad un articolo
    /// </summary>
    //=========================================================================
    [Serializable]
    public class ArticleInfo : IComparable
    {
        public bool ImplicitActivation = false;
        public string MaxCal;
        public string Producer;
        public string InternalCode;
        public string PrivateCode;
        public string ProdID;
        public string AcceptDEMO;
        public string Needless;
        public string MandatoryCountry;
        public string OptionalCountry;
        public string NeedlessText;
        public string DependencyExpression;
        public bool Licensed;
        public bool Modifiable = true;
        public bool Mandatory;
        public CalUseEnum CalUse = CalUseEnum.Unnamed;
        public IncludePathInfo[] IncludeModulesPaths;
        public string Name = string.Empty;
        public string Edition = string.Empty;
        private int namedCalNumber = -1;
        private int concurrentCalNumber = -1;
        private int webCalNumber = -1;
        private int manufacturinigMobileCalNumber = -1;
        private int wmsMobileCalNumber = -1;
        public IList<SerialNumberInfo> SerialList;
        public ArrayList PKs;
        public string Solution = string.Empty;
        public ArrayList ShortNames;
        public IncludedSMInfo[] IncludedSM;
        public ModuleInfo[] Modules;
        public CalTypeEnum CalType = CalTypeEnum.None;
        public ModuleModeEnum ModuleMode = ModuleModeEnum.Default;
        public bool HasSerial;
        private string localizedName;
        public string IncorrectSerialsMessage = null;
        public bool DefaultDemo = true;
        public bool Obsolete = false;
        public bool BasicServer = false;
        public Avaiability Available = Avaiability.Available;
        public bool NamedCal { get { return CalUse == CalUseEnum.Named || CalUse == CalUseEnum.Function || CalUse == CalUseEnum.tbf; } }

        public enum Avaiability { Available, Unavailable, AvailableNFS };

        //---------------------------------------------------------------------
        public ModuleInfo GetModuleByName(string application, string moduleName)
        {
            return GetModuleByName(this, application, moduleName);
        }

        //---------------------------------------------------------------------
        public static ModuleInfo GetModuleByName(ArticleInfo ai, string application, string moduleName)
        {
            foreach (ModuleInfo mi in ai.Modules)
            {
                if (
                    (string.Compare(mi.Name, moduleName, true, CultureInfo.InvariantCulture) == 0) &&
                    (string.Compare(mi.Application, application, true, CultureInfo.InvariantCulture) == 0)
                    )
                    return mi;
            }
            return null;
        }

        //---------------------------------------------------------------------
        public int NamedCalNumber
        {
            get
            {
                if (namedCalNumber == -1)
                {
                    string error;
                    namedCalNumber = SerialNumberInfo.GetCalNumberFromSerials(this.Name, SerialList, CalType, out error, false, false, NamedCal, false);
                    if (String.Compare(this.Edition, NameSolverStrings.StandardEdition, true, CultureInfo.InvariantCulture) == 0 && NamedCalNumber > 3)
                    {
                        namedCalNumber = 3;
                        error += String.Format(CultureInfo.InvariantCulture, LicenceStrings.TooManyCal, LocalizedName);
                    }
                    IncorrectSerialsMessage = error;
                }
                return namedCalNumber;
            }
            set { namedCalNumber = value; }
        }
        //---------------------------------------------------------------------
        public int ConcurrentCalNumber
        {
            get
            {
                if (concurrentCalNumber == -1)
                {
                    string error;
                    concurrentCalNumber = SerialNumberInfo.GetCalNumberFromSerials(this.Name, SerialList, CalType, out error, false, true, false, false);
                    if (String.Compare(this.Edition, NameSolverStrings.StandardEdition, true, CultureInfo.InvariantCulture) == 0 && NamedCalNumber > 3)
                    {
                        concurrentCalNumber = 3;
                        error += String.Format(CultureInfo.InvariantCulture, LicenceStrings.TooManyCal, LocalizedName);
                    }
                    IncorrectSerialsMessage = error;
                }
                return concurrentCalNumber;
            }
            set { concurrentCalNumber = value; }
        }

        //---------------------------------------------------------------------
        public int WebCalNumber
        {
            get
            {
                if (webCalNumber == -1)
                {
                    string error;
                    webCalNumber = SerialNumberInfo.GetCalNumberFromSerials(this.Name, SerialList, CalType, out error, true, false, NamedCal, false);
                    IncorrectSerialsMessage = error;
                }
                return webCalNumber;
            }
            set { webCalNumber = value; }
        }

        //---------------------------------------------------------------------
        public int WmsMobileCalNumber
        {
            get
            {
                if (wmsMobileCalNumber == -1)
                {
                    string error;
                    wmsMobileCalNumber = SerialNumberInfo.GetCalNumberFromSerials(this.Name, SerialList, CalType, out error, false, false, false, true);

                    IncorrectSerialsMessage = error;
                }
                return wmsMobileCalNumber;
            }
            set { wmsMobileCalNumber = value; }
        }

        //---------------------------------------------------------------------
        public int ManufacturingMobileCalNumber
        {
            get
            {
                if (manufacturinigMobileCalNumber == -1)
                {
                    string error;
                    manufacturinigMobileCalNumber = SerialNumberInfo.GetCalNumberFromSerials(this.Name, SerialList, CalType, out error, false, false, false, false, true);

                    IncorrectSerialsMessage = error;
                }
                return manufacturinigMobileCalNumber;
            }
            set { manufacturinigMobileCalNumber = value; }
        }


        //---------------------------------------------------------------------
        public string LocalizedName
        {
            get
            {
                return localizedName == String.Empty ? Name : localizedName;
            }
            set
            {
                if (value == null)
                    localizedName = String.Empty;
                else
                    localizedName = value;
            }
        }

        //---------------------------------------------------------------------
        public ArticleInfo() { } // DO NOT REMOVE, used by serialization

        /// <param name="articleDoms">Collection of DOMs of the sales modules definitions</param>
        //---------------------------------------------------------------------
        public ArticleInfo(string artName, string country, Diagnostic diagnostic, Hashtable articleDoms)
        {
            if (articleDoms == null || String.IsNullOrEmpty(artName))
            {
                if (diagnostic != null)
                    diagnostic.Set(DiagnosticType.Error, "error creating article info");
                throw new ArgumentException("error creating article info");
            }
            SolutionArticle solArt = articleDoms[artName] as SolutionArticle;

            XmlDocument artDoc = solArt.XmlDoc;
            XmlElement art = artDoc.DocumentElement;
            string includecode = art.GetAttribute("includecode");
            string needless = art.GetAttribute("needless");
            string needlesstext = art.GetAttribute("needlesstext");
            string optionalcountry = art.GetAttribute("optionalcountry");//se siamo in queste country elencate il modulo è selzionato di default ( modificabile)
            string mandatorycountry = art.GetAttribute("mandatorycountry");//se siamo in queste country elencate il modulo è selzionato di default ( non modificabile)
            string edition = art.GetAttribute(Consts.AttributeEdition);
            string masterCalString = art.GetAttribute(Consts.AttributeMasterCal);
            string hasCalString = art.GetAttribute(Consts.AttributeHasCal);
            string caltypeString = art.GetAttribute(Consts.AttributeCalType);
            string localizedName = art.GetAttribute(Consts.AttributeLocalize);
            string maxCal = art.GetAttribute(Consts.AttributeMaxCal);
            string internalCode = art.GetAttribute(Consts.AttributeInternalCode);
            string producer = art.GetAttribute(Consts.AttributeProducer);
            string prodID = art.GetAttribute(Consts.AttributeProdID); //tag added at encryption time
            string privateCode = art.GetAttribute(Consts.AttributePrivateCode);
            string hasSerialString = art.GetAttribute(Consts.AttributeHasSerial);
            string availableString = art.GetAttribute(Consts.AttributeAvailable);
            string mandatoryString = art.GetAttribute(Consts.AttributeMandatory);
            string namedcalString = art.GetAttribute(Consts.AttributeNamedCal);
            string calUseString = art.GetAttribute(Consts.AttributeCalUse);
            string moduleModeString = art.GetAttribute(Consts.AttributeMode);
            string acceptDemo = art.GetAttribute("acceptdemo");

            XmlNode fileNameNode = art.SelectSingleNode("//" + "File/@name"); // tag added at encryption time
            bool fileNameOK = VerifyFileName(internalCode, artName, fileNameNode);
            if (!fileNameOK)
            {
                string msg = String.Format(CultureInfo.InvariantCulture, "Il modulo {0} non è accettato: nome del file non è valido.", artName);
                if (diagnostic != null)
                    diagnostic.Set(DiagnosticType.Error, msg);
                throw new ArgumentException(msg);
            }
            //dipendenze funzionali
            XmlNode dependencyAtt = art.SelectSingleNode("./" + Consts.TagArticleDependency + "/@" + Consts.AttributeExpression);
            string dependency = (dependencyAtt != null) ? dependencyAtt.Value : String.Empty;

            //short names
            XmlNodeList shortNames = art.SelectNodes
                (
                "./" +
                Consts.TagShortNames +
                "/" +
                Consts.TagShortName +
                "/@" +
                Consts.AttributeName
                );
            ArrayList shortNamesList = new ArrayList();
            foreach (XmlNode node in shortNames)
                shortNamesList.Add(node.Value);

            //se mandatory non c'è, considero false
            bool mandatory = (String.Compare(bool.TrueString, mandatoryString, true, CultureInfo.InvariantCulture) == 0);
            //se hascal non c'è, considero false
            bool hasCal = (String.Compare(bool.TrueString, hasCalString, true, CultureInfo.InvariantCulture) == 0);
            //se mastercal non c'è, considero false
            bool masterCal = (String.Compare(bool.TrueString, masterCalString, true, CultureInfo.InvariantCulture) == 0);
            //se hasserial non c'è, considero true
            bool hasSerial = !(String.Compare(bool.FalseString, hasSerialString, true, CultureInfo.InvariantCulture) == 0);
            //se available non c'è, considero available
            Avaiability available = SetAvaiability(availableString);


            //per compatibilità retroattiva devo leggere namedcale e poi eventualemte rivalorizzarlo con caluse se c'`e
            //se namedcal non c'è, considero false
            bool namedcal = (String.Compare(bool.TrueString, namedcalString, true, CultureInfo.InvariantCulture) == 0);

            CalTypeEnum caltype = SetCalType(caltypeString, masterCal, hasCal);
            if (HasSerialNotAllowed(hasSerial, internalCode, producer))
            {
                if (diagnostic != null)
                    diagnostic.Set(DiagnosticType.Warning, String.Format(CultureInfo.InvariantCulture, LicenceStrings.InvalidHasserial, artName));
                hasSerial = true;
            }
            CalUseEnum caluse = SetCalUse(calUseString, namedcal);
            ModuleModeEnum moduleMode = SetModuleMode(moduleModeString);

            this.Name = artName;
            this.MaxCal = maxCal;
            //this.MasterCal= mastercal;
            this.Producer = producer;
            this.ProdID = prodID;
            this.AcceptDEMO = acceptDemo;
            this.Needless = needless;
            this.NeedlessText = needlesstext;
            this.OptionalCountry = optionalcountry;
            this.MandatoryCountry = mandatorycountry;
            this.DependencyExpression = dependency;
            this.Mandatory = mandatory;
            //this.HasCal	= hascal;
            this.CalType = caltype;
            //this.NamedCal	= namedcal;
            this.CalUse = caluse;
            this.ModuleMode = moduleMode;
            this.HasSerial = hasSerial;
            this.LocalizedName = localizedName;
            this.ShortNames = shortNamesList;
            this.DefaultDemo = solArt.Properties.DefaultDemo;
            this.Obsolete = solArt.Properties.Obsolete;
            this.BasicServer = solArt.Properties.BasicServer;
            this.Available = available;
            this.PrivateCode = privateCode;
            this.InternalCode = internalCode;
            if (edition != null && edition.Length != 0)
                this.Edition = edition;
            //if (addFunctional)//Aggiungo eventualmente la lista di moduli funzionali e funzionalità

            this.AddContents(art, country, artName, articleDoms, includecode);
        }

        //Moduli di servizio non visualizzati nella griglia di wua
        //---------------------------------------------------------------------
        public bool IsBackModule()
        {
            return ModuleMode == ModuleModeEnum.Back;
        }

        //moduli non visualizzabili in demo
        //---------------------------------------------------------------------
        public bool IsNoDemoModule()
        {
            return ModuleMode == ModuleModeEnum.NoDemo || ModuleMode == ModuleModeEnum.PhasedOut || ModuleMode == ModuleModeEnum.NDB_PhasedOut;
        }

        //moduli non visualizzabili in dvlp normale  e iu
        //---------------------------------------------------------------------
        internal bool DVLPPlus()
        {
            return ModuleMode == ModuleModeEnum.DVLPPlus;
        }

        //---------------------------------------------------------------------
        private CalUseEnum SetCalUse(string caluseString, bool namedcal)
        {
            // In precedenza c'era solo namedcal (true  o false, con default false, 
            // adesso invece c'è caluse con i vari valori possibili, quindi se namedcal 
            // è false leggo caluse, altrimenti leggo caluse. In questo modo è protetto 
            // il pregresso)
            if (namedcal)
                return CalUseEnum.Named;

            if (caluseString.Length == 0)
                return CalUseEnum.Unnamed;

            try
            {
                return (CalUseEnum)Enum.Parse(typeof(CalUseEnum), caluseString, true);
            }
            catch { return CalUseEnum.Unnamed; }

        }
        //---------------------------------------------------------------------
        private ModuleModeEnum SetModuleMode(string moduleModeString)
        {
            if (moduleModeString.Length == 0)
                return ModuleModeEnum.Default;

            try
            {
                return (ModuleModeEnum)Enum.Parse(typeof(ModuleModeEnum), moduleModeString, true);
            }
            catch { return ModuleModeEnum.Default; }

        }
        //verifico che il nome del file impresso nel csm, se si tratta di un csm, 
        //sia lo stesso effettivo del file che si sta leggendo, unica eccezione per microarea
        //---------------------------------------------------------------------
        private bool VerifyFileName(string producer, string fileName, XmlNode fileNameNode)
        {
            return (
                !ActivationObjectHelper.IsCsm(Path.GetExtension(fileName)) ||
                (fileNameNode != null &&
                fileNameNode.Value != null &&
                fileNameNode.Value != String.Empty &&
                String.Compare(Path.GetFileNameWithoutExtension(fileName), fileNameNode.Value, true, CultureInfo.InvariantCulture) == 0) ||
                ActivationObjectHelper.IsPowerProducer(producer));
        }


        //---------------------------------------------------------------------
        private Avaiability SetAvaiability(string avaiabilityString)
        {
            if (string.IsNullOrWhiteSpace(avaiabilityString) ||
                string.Compare(avaiabilityString, "available", StringComparison.InvariantCultureIgnoreCase) == 0)
                return Avaiability.Available;
            if (string.Compare(avaiabilityString, "availableNFS", StringComparison.InvariantCultureIgnoreCase) == 0)
                return Avaiability.AvailableNFS;
            return Avaiability.Unavailable;

        }

        //Per gestire la compatibilità col passato quando non esisteva 
        //l'attributio caltype, ed erano però definiti hascal e mastercal
        //---------------------------------------------------------------------
        private CalTypeEnum SetCalType(string calTypeString, bool mastercal, bool hascal)
        {
            CalTypeEnum caltype = CalTypeEnum.None;

            try
            {
                if (calTypeString != String.Empty)
                    caltype = (CalTypeEnum)Enum.Parse(typeof(CalTypeEnum), calTypeString, true);

                else
                {
                    if (mastercal && hascal)
                        caltype = CalTypeEnum.Master;
                    if (!mastercal && hascal)
                        caltype = CalTypeEnum.Auto;
                    //precedentemente non era gestito mai il caso server
                }
            }
            catch (System.ArgumentException)
            {
                caltype = CalTypeEnum.None;
            }
            return caltype;
        }

        //il serial è obbligatorio per tutti i moduli e solo microarea può averne senza serial
        //---------------------------------------------------------------------
        private bool HasSerialNotAllowed(bool hasserial, string internalCode, string producer)
        {
            return (!(ActivationObjectHelper.IsPowerProducer(internalCode) || ActivationObjectHelper.IsPowerProducer(producer)) && !hasserial);
        }

        //---------------------------------------------------------------------
        public static string GetArticleNameFromFilePath(string filePath)
        {
            if (filePath == null || filePath == String.Empty)
                return String.Empty;
            return Path.GetFileNameWithoutExtension(filePath);
        }

        //---------------------------------------------------------------------
        public bool HasCal()
        {
            return HasCal(CalType);
        }

        //---------------------------------------------------------------------
        public bool HasNamedCal()
        {
            return (HasCal(CalType) && NamedCal);
        }

        //---------------------------------------------------------------------
        public bool HasFloatingCal()
        {
            return (HasCal(CalType) && (CalType == CalTypeEnum.AutoFunctional || CalType == CalTypeEnum.AutoTbs));
        }

        //---------------------------------------------------------------------
        public bool IsAServer()
        {
            return ((CalType == CalTypeEnum.Master || CalType == CalTypeEnum.MasterNew) && NamedCal);
        }

        //---------------------------------------------------------------------
        public static bool HasCal(CalTypeEnum caltype)
        {
            return (caltype == CalTypeEnum.Auto || caltype == CalTypeEnum.AutoTbs || caltype == CalTypeEnum.AutoFunctional || caltype == CalTypeEnum.Server || caltype == CalTypeEnum.TPGate || caltype == CalTypeEnum.Master || caltype == CalTypeEnum.MasterNew || caltype == CalTypeEnum.TPGateNew || caltype == CalTypeEnum.WmsMobile || caltype == CalTypeEnum.tbf);
        }

        //---------------------------------------------------------------------
        public static bool HasSeparatedCal(CalTypeEnum caltype)
        {
            return (caltype == CalTypeEnum.Server || caltype == CalTypeEnum.TPGate || caltype == CalTypeEnum.Master || caltype == CalTypeEnum.MasterNew || caltype == CalTypeEnum.TPGateNew);
        }

        /// <param name="articleDoms">Collection of DOMs of the sales modules definitions</param>
        //---------------------------------------------------------------------
        private ArrayList GetContents
            (
            XmlElement article,
            out IncludePathInfo[] includePaths,
            string country,
            string moduleName,
            ArrayList readModule,
            out ArrayList IncludedSM,
            Hashtable articleDoms,
            string parentincludecode
            )
        {
            ArrayList modules = new ArrayList();
            IncludedSM = new ArrayList();
            includePaths = null;
            string includecode = article.GetAttribute("includecode");
            if (!String.IsNullOrEmpty(parentincludecode))
                if (String.Compare(includecode, parentincludecode, true) != 0)
                {
                    Debug.Fail("ArticleInfo INCLUDE FALLITO " + moduleName);

                    return modules;
                }

            if (readModule == null)
                readModule = new ArrayList();
            //se l'ho già letto esco, per evitare loop infiniti
            string key = moduleName.ToLower(CultureInfo.InvariantCulture);
            if (readModule.Contains(key))
                return modules;
            readModule.Add(key);

            if (article == null)
            {
                Debug.Fail("ArticleInfo.SetModules: Articolo non mappato?");
                return modules;
            }

            foreach (XmlElement applicationEl in article.GetElementsByTagName(Consts.TagApplication))
            {
                string container = applicationEl.GetAttribute(Consts.AttributeContainer);
                string application = applicationEl.GetAttribute(Consts.AttributeName);

                //moduli
                foreach (XmlElement functionalModule in applicationEl.GetElementsByTagName(Consts.TagModule))
                {
                    CountryLawInfo lawInfo = new CountryLawInfo();
                    if (ActivationObject.MustVerifyLawInfo)
                        lawInfo.Create(functionalModule);
                    if (lawInfo == null || lawInfo.Verify(country))
                    {

                        string depExp;
                        bool onactivated;
                        DepExpEvaluator.GetDepExp(functionalModule, out depExp, out onactivated);
                        string module = functionalModule.GetAttribute(Consts.AttributeName);

                        ModuleInfo moduleInfo = new ModuleInfo(module, application, container, ModuleInfo.ModuleType.Module, lawInfo, depExp);
                        SNTypeLawInfo sntli = new SNTypeLawInfo();
                        sntli.Create(functionalModule);
                        moduleInfo.LawInfoSN = sntli;
                        modules.Add(moduleInfo);
                    }
                }
                //funzionalità
                foreach (XmlElement functionality in applicationEl.GetElementsByTagName(Consts.TagFunctionality))
                {

                    CountryLawInfo lawInfo = new CountryLawInfo();
                    if (ActivationObject.MustVerifyLawInfo)
                        lawInfo.Create(functionality);
                    if (lawInfo == null || lawInfo.Verify(country))
                    {



                        string depExp;
                        bool onactivated;
                        DepExpEvaluator.GetDepExp(functionality, out depExp, out onactivated);
                        string module = functionality.GetAttribute(Consts.AttributeName);
                        ModuleInfo moduleInfo = new ModuleInfo(module, application, container, ModuleInfo.ModuleType.Functionality, lawInfo, depExp);
                        SNTypeLawInfo sntli = new SNTypeLawInfo();
                        sntli.Create(functionality);
                        moduleInfo.LawInfoSN = sntli;
                        modules.Add(moduleInfo);

                    }



                }
            }

            ArrayList includeModulesPathsList = new ArrayList();

            foreach (XmlElement applicationEl in article.GetElementsByTagName("Includes"))
            {
                //SalesModules Inclusi
                foreach (XmlElement include in applicationEl.GetElementsByTagName("Include"))
                {
                    string module = include.GetAttribute(Consts.AttributeName);

                    XmlElement articleEl = GetArticleElement(articleDoms, module);
                    if (articleEl == null)
                        continue;
                    string modeVal = include.GetAttribute(Consts.AttributeMode);

                    IncludedSMInfo.IncludedSMModeEnum mode = ParseModuleModeEnum(modeVal);
                    ArrayList temp;
                    CountryLawInfo lawInfo = new CountryLawInfo();
                    if (ActivationObject.MustVerifyLawInfo)
                        lawInfo.Create(include);
                    if (lawInfo == null || lawInfo.Verify(country))
                    {
                        ArrayList funcModIncluded = GetContents(articleEl, out includePaths, country, module, readModule, out temp, articleDoms, includecode);
                        IncludedSM.Add(new IncludedSMInfo(module, mode));
                        if (temp != null)
                            IncludedSM.AddRange(temp);
                        if (funcModIncluded != null)
                            modules.AddRange(funcModIncluded);
                        if (includePaths != null)
                            includeModulesPathsList.AddRange(includePaths);
                    }
                }
            }

            //include paths
            StringCollection scinc = new StringCollection();
            foreach (XmlElement include in article.GetElementsByTagName(Consts.TagIncludeModulesPath))
            {
                CountryLawInfo lawInfo = new CountryLawInfo();
                if (ActivationObject.MustVerifyLawInfo)
                    lawInfo.Create(include);
                if (lawInfo == null || lawInfo.Verify(country))
                {
                    string path = include.GetAttribute(Consts.AttributePath);
                    string pathKey = path.ToLower(CultureInfo.InvariantCulture);
                    if (!scinc.Contains(pathKey))
                    {
                        string depExp;
                        bool onactivated;
                        DepExpEvaluator.GetDepExp(include, out depExp, out onactivated);

                        scinc.Add(pathKey);
                        IncludePathInfo ipi = new IncludePathInfo(path, lawInfo, depExp, onactivated);
                        includeModulesPathsList.Add(ipi);
                    }
                }
            }
            includePaths = (IncludePathInfo[])includeModulesPathsList.ToArray(typeof(IncludePathInfo));

            return modules;
        }

        //---------------------------------------------------------------------
        private IncludedSMInfo.IncludedSMModeEnum ParseModuleModeEnum(string modeValue)
        {
            if (modeValue == null || modeValue.Length == 0)
                return IncludedSMInfo.IncludedSMModeEnum.Default;
            try { return (IncludedSMInfo.IncludedSMModeEnum)Enum.Parse(typeof(IncludedSMInfo.IncludedSMModeEnum), modeValue, true); }
            catch { return IncludedSMInfo.IncludedSMModeEnum.Default; }
        }

        //---------------------------------------------------------------------
        private XmlElement GetArticleElement(Hashtable articleDoms, string moduleName)//string  modulePath)
        {
            SolutionArticle solArt = articleDoms[moduleName] as SolutionArticle;
            if (solArt == null) return null;
            XmlDocument doc = solArt.XmlDoc;
            return doc != null ? doc.DocumentElement : null;
        }
        //---------------------------------------------------------------------
        private XmlElement GetArticleElement(XmlDocument doc)
        {
            XmlNode articleNodes = doc.SelectSingleNode("//" + Consts.TagSalesModule);
            return articleNodes as XmlElement;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// pulisce il contenuto del modulo lasciandolo selezionato se lo è in modo che i moduli disattivi quando divengono attivi se sono selezionati vengano attivati in automatico.
        /// però non posso farlo subito perchè ci sono casi in cui il serial NFS abilita comunque i moduli not available
        /// </summary>
        //public void ClearNotAvailable()
        //{

        //    if (Available == Avaiability.Unavailable)
        //    {
        //        Modules = new ModuleInfo[] { };
        //        IncludedSM = new IncludedSMInfo[] { };
        //        IncludeModulesPaths = new IncludePathInfo[] { };
        //        return;
        //    }

        //}

        /// <summary>
        /// Setta i moduli funzionali, le funzionalità e gli eventuali includePaths dell'articolo in questione
        /// </summary>
        //---------------------------------------------------------------------
        private void AddContents(XmlElement article, string country, string filename, Hashtable articleDoms, string includecode)
        {

            ArrayList includedSM;
            IncludePathInfo[] includePaths = new IncludePathInfo[] { };

            ArrayList modules = GetContents(article, out includePaths, country, filename, null, out includedSM, articleDoms, includecode);
            Modules = (ModuleInfo[])modules.ToArray(typeof(ModuleInfo));
            IncludedSM = (IncludedSMInfo[])includedSM.ToArray(typeof(IncludedSMInfo));
            IncludeModulesPaths = includePaths;
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return this.Name;
        }

        #region overridden object methods
        //---------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            ArticleInfo comp = obj as ArticleInfo;
            if (comp == null)
                return false;

            return string.Compare(Name, comp.Name, true, CultureInfo.InvariantCulture) == 0;
        }

        //---------------------------------------------------------------------
        public override int GetHashCode()
        {
            return (Name + "," + Producer).ToLower(CultureInfo.InvariantCulture).GetHashCode();
        }
        #endregion

        #region IComparable Members

        //---------------------------------------------------------------------
        public int CompareTo(object obj)
        {
            ArticleInfo aInfo = obj as ArticleInfo;
            if (aInfo == null)
                return 1; // This instance is greater than obj
            return string.Compare(this.Name, aInfo.Name, true, CultureInfo.InvariantCulture);
        }

        #endregion



    }

    //=========================================================================
    [Serializable]
    public class IncludedSMInfo
    {
        [Serializable]
        public enum IncludedSMModeEnum { Default, Exclusive };
        public string Name;
        public IncludedSMModeEnum includedSMMode = IncludedSMModeEnum.Default;

        //---------------------------------------------------------------------
        public IncludedSMInfo(string name, IncludedSMModeEnum mode)
        {
            Name = name;
            includedSMMode = mode;
        }

        //---------------------------------------------------------------------
        public IncludedSMInfo()
        {
        }
    }
}
