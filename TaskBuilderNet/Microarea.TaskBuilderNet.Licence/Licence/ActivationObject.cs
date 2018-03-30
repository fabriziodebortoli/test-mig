using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Activation;
using Microarea.TaskBuilderNet.Licence.Licence.ConfigurationInfoProvider;
using Microarea.TaskBuilderNet.Licence.Licence.XmlSyntax;
using WceAttribute = Microarea.TaskBuilderNet.Licence.Activation.WceStrings.Attribute;
using WceElement = Microarea.TaskBuilderNet.Licence.Activation.WceStrings.Element;

namespace Microarea.TaskBuilderNet.Licence.Licence
{

    //=========================================================================
    /// <summary>
    /// </summary>
    [Serializable]
    public class ActivationObject : ICloneable
    {
        public ProductInfo[] Products = new ProductInfo[] { };
        public string DoubleLicensingErrorMessages = String.Empty;
        public string LicensedFileShadow = String.Empty;
        internal static bool MustVerifyLawInfo = true;
        private int activationVersion = 0;
        public string ActivationKey = null;
        [NonSerialized]
        private IConfigurationInfoProvider configurationInfoProvider;
        private string cachedMasterProdID = null;
        private SerialNumberType cachedMasterProdSerialType = SerialNumberType.UNDEFINED;
        //---------------------------------------------------------------------
        private UserInfo userInfo;
        public UserInfo User
        {
            get
            {
                if (configurationInfoProvider != null)
                    this.userInfo = configurationInfoProvider.GetUserInfo();
                return this.userInfo;
            }
        }

        //---------------------------------------------------------------------
        [NonSerialized]
        private Diagnostic diagnostic = new Diagnostic(NameSolverStrings.ActivationObject);
        public Diagnostic Diagnostic
        {
            get { return this.diagnostic; }
        }

        //---------------------------------------------------------------------
        public ActivationObject() { } // DO NOT REMOVE (needed for serialization)


        //---------------------------------------------------------------------
        public ActivationObject(IConfigurationInfoProvider configurationInfoProvider)
        {
            Hashtable articleDoms = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            this.configurationInfoProvider = configurationInfoProvider;
            ActivationObject.MustVerifyLawInfo = configurationInfoProvider.FilterByCountry;

            this.userInfo = configurationInfoProvider.GetUserInfo();

            ArrayList prodList = new ArrayList();
            //Costruisco la lista di prodotti
            string[] prodNames = configurationInfoProvider.GetProductNames();
            ArrayList pList = new ArrayList();
            int maxActivationVersion = 0;
            for (int i = 0; i < prodNames.Length; ++i)
            {

                configurationInfoProvider.GetArticles(prodNames[i], articleDoms);
            }
            for (int i = 0; i < prodNames.Length; ++i)
            {
                string prodName = prodNames[i];
                ProductInfo prodInfo = BuildProduct(prodName, articleDoms);
                if (prodInfo == null) continue;
                if (!string.IsNullOrEmpty(prodInfo.ActivationKey))
                {
                    if (!string.IsNullOrEmpty(ActivationKey))
                    {
                        ActivationKey = "ERROR";
                        diagnostic.SetError("Installed products have more activation keys.");
                    }
                    else
                        ActivationKey = prodInfo.ActivationKey;
                }

                //determino la maxActivationVersion
                if (prodInfo != null && prodInfo.ActivationVersion > maxActivationVersion)
                {
                    if (i > 0 && this.diagnostic != null)
                        diagnostic.SetWarning("Installed products have incompatible activation version."); // TODO LOCALIZE
                    maxActivationVersion = prodInfo.ActivationVersion;
                }
                pList.Add(prodInfo);
            }


            for (int i = 0; i < pList.Count; ++i)
            {
                ProductInfo prodInfo = pList[i] as ProductInfo;

                if (prodInfo == null) continue;
                prodList.Add(prodInfo);

            }

            this.activationVersion = maxActivationVersion;
            this.Products = (ProductInfo[])prodList.ToArray(typeof(ProductInfo));
            SistemaIBSuite(); SistemaFatturazioneElettronica();
            CheckDependenciesExpression();
            TryToGetMasterProdIDApproximatively();

        }

        //3.13 orrore, sono stata costretta /2 
        /*
         gli utenti che aggiornano alle versioni Mago.net 3.13 o Mago4 1.4 e che avevano fatturazione elettronica devono spostarsi su mdc senza farlo a mano
         */
        //---------------------------------------------------------------------
        public void SistemaFatturazioneElettronica()
        {
            ArticleInfo electronicInvoicing = GetArticleByFunctionality("ERP", "ElectronicInvoicing", false);
            ArticleInfo fepaconnector = GetArticleByFunctionality("ERP", "FEPAConnector", false);

            if ((electronicInvoicing != null && electronicInvoicing.Licensed) || (fepaconnector != null && fepaconnector.Licensed) || GetCountry().ToUpperInvariant() == "BR")
            {
                ArticleInfo a = GetArticleInfo("MDC", "ElectronicInvoicing");
                if (a == null || a.Licensed) return;

                a.Licensed = true;
                a.ImplicitActivation = true;
                bool mdc = false;
                foreach (ProductInfo p in Products)
                {
                    if (mdc) break;
                    foreach (ArticleInfo art in p.Articles)
                    {
                        if (art.Name == a.Name)
                        {
                            XmlDocument doc = ProductInfo.WriteToLicensed(p);
                            if (doc != null)
                            {
                                ActivationObject.SaveLicensed(doc.OuterXml, p.CompleteName, diagnostic);
                                mdc = true;
                            }
                            break;
                        }
                    }
                }
                if (mdc)
                    foreach (ProductInfo p in Products)
                    {
                        //lo levo dall'altro licensed se no anche in caso l utente lo deselezionasse poi torna attivo ogni riavvio.
                        //anomalia 24874: in caso di emedded questi moduli saranno il server quindi non devo disattivarli 
                        //Li lascio in caso il modulo sia server, dovrebbe bastare, vuol dire che ripasserà di qui ad ogni avvio di login manager. 
                        if (electronicInvoicing != null && electronicInvoicing.Licensed && !electronicInvoicing.IsAServer())
                            electronicInvoicing.Licensed = false;
                        if (fepaconnector != null && fepaconnector.Licensed && !fepaconnector.IsAServer())
                            fepaconnector.Licensed = false;
                        foreach (ArticleInfo art in p.Articles)
                        {
                            if (
                               (electronicInvoicing != null && !electronicInvoicing.IsAServer() && String.Compare(art.Name, electronicInvoicing.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                                ||
                               (fepaconnector != null && !fepaconnector.IsAServer() && String.Compare(art.Name, fepaconnector.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                                )
                            {
                                XmlDocument doc = ProductInfo.WriteToLicensed(p);
                                if (doc != null)
                                    ActivationObject.SaveLicensed(doc.OuterXml, p.CompleteName, diagnostic);
                                break;
                            }
                        }
                    }
            }

        }



        //3.11 orrore, sono stata costretta
        //---------------------------------------------------------------------
        public void SistemaIBSuite()
        {
            ArticleInfo ibsuite = GetArticleByFunctionality("IBSuite", "IBConnector");
            if (ibsuite != null && ibsuite.Licensed)
            {
                ArticleInfo a = GetArticleInfo("ERP", "IBConnectorSuite");
                if (a == null || a.Licensed) return;

                a.Licensed = true;

                foreach (ProductInfo p in Products)
                {
                    foreach (ArticleInfo art in p.Articles)
                    {
                        if (art.Name == a.Name)
                        {
                            XmlDocument doc = ProductInfo.WriteToLicensed(p);
                            if (doc != null)
                                ActivationObject.SaveLicensed(doc.OuterXml, p.CompleteName, diagnostic);
                            return;
                        }
                    }
                }
            }
        }

        //
        //-----------------------------------------------------------------------
        public static bool SaveLicensed(string xml, string name, Diagnostic diagnostic)
        {

            if (string.IsNullOrEmpty(xml))
                return false;
            XmlDocument xmldocL = new XmlDocument();
            try
            {
                xmldocL.LoadXml(xml);
                string filename = String.Format("{0}.Licensed.config", name);
                string path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetLogManAppDataPath(), filename);
                xmldocL.Save(path);
                return true;
            }
            catch (Exception exc)
            {
                diagnostic.Set(
                        DiagnosticType.LogInfo | DiagnosticType.Error,
                        "Error saving Licensed" + exc.Message
                        );
                return false;
            }
        }


        //private Edition editionCached = Edition.Undefined;
        //---------------------------------------------------------------------
        private bool DummyFunction()
        {
            if (Products.Length == 0)
                return true;
            else
                return false;
        }

        //---------------------------------------------------------------------
        private void CheckDependenciesExpression()
        {
            foreach (ProductInfo pi in this.Products)
                pi.EvaluateDepExp(this.Products);
        }

        //---------------------------------------------------------------------
        public int ActivationVersion { get { return this.activationVersion; } }

        //---------------------------------------------------------------------
        private ProductInfo BuildProduct(string prodName, Hashtable articleDoms)
        {
            XmlElement prodEl = configurationInfoProvider.GetProductLicensed(prodName);
            XmlElement sol = configurationInfoProvider.GetProductSolution(prodName);

            if (prodEl == null && sol == null)
            {
                SetDiagnostic(DiagnosticType.Warning, String.Format("Product {0} is not loadable.", prodName));
                return null;
            }
            if (sol == null)
            {
                SetDiagnostic(DiagnosticType.Warning, String.Format("Product {0} is not loadable, but its Licensed exists.", prodName));
                return null;
            }

            bool articlesLicensedByDefault = configurationInfoProvider.ArticlesLicensedByDefault;
            bool lic = !(prodEl == null);
            if (!lic) // now sol != null
            {
                prodEl = sol;
                articlesLicensedByDefault = false;
            }

            ProductInfo prodInfo = new ProductInfo(prodEl, BasePathFinder.BasePathFinderInstance.GetInstallationVersionFromInstallationVer(prodName));
            //se il prodid è vuoto metto il nome prodotto, dovrebbe capitare solo nel caso di verticali.
            prodInfo.ProductId = sol.GetAttribute(WceAttribute.ProductId);

            if (string.IsNullOrEmpty(prodInfo.ProductId)) prodInfo.ProductId = prodName;
            prodInfo.EditionId = sol.GetAttribute(WceAttribute.EditionId);
            prodInfo.Family = sol.GetAttribute(WceAttribute.Family);
            prodInfo.ProductName = sol.GetAttribute(WceAttribute.CompleteName);
            prodInfo.ViewName = sol.GetAttribute("localize");

            prodInfo.CompleteName = prodName;
            prodInfo.Application = configurationInfoProvider.GetApplication(prodName);
            prodInfo.HasLicensed = lic;
            string priorityVal = sol.GetAttribute("priority");
            int priorityNum = -1;
            prodInfo.Priority = (Int32.TryParse(priorityVal, out priorityNum)) ? priorityNum : -1;

            if (sol == null) // no solution file available
            {
                prodInfo.Articles = new ArticleInfo[] { };
                return prodInfo;
            }

            ArrayList prodArts = new ArrayList(articleDoms.Count);

            string country = configurationInfoProvider.GetCountry();

            string artXPathMask = WceElement.SalesModule + "[@name='{0}']";

            XmlNodeList salesModuleNodeList = sol.GetElementsByTagName(WceStrings.Element.SalesModule);
            prodInfo.IsComplacent = (salesModuleNodeList.Count == articleDoms.Count);

            foreach (XmlElement solArt in salesModuleNodeList)
            {
                string artName = solArt.GetAttribute(WceStrings.Attribute.Name);
                string obsolete = solArt.GetAttribute(WceStrings.Attribute.Obsolete);
                CountryLawInfo lawInfo = new CountryLawInfo();
                if (MustVerifyLawInfo)
                    lawInfo.Create(solArt);
                if (lawInfo != null && !lawInfo.Verify(country))
                    continue;

                string producer = null;
                IList<SerialNumberInfo> serials = new List<SerialNumberInfo>();
                ArrayList pks = new ArrayList();
                string xPath = string.Format(CultureInfo.InvariantCulture, artXPathMask, artName);
                XmlElement art = PseudoXPath.SelectSingleElement(prodEl, xPath);

                if (art != null)
                {
                    // Backward compatibility with 1.2 activation
                    producer = art.GetAttribute(WceAttribute.InternalCode);
                    if (producer == null || producer.Length <= 0)
                        producer = art.GetAttribute(WceAttribute.Producer);
                    XmlNodeList serialsNodes = art.SelectNodes(WceElement.Serial);
                    foreach (XmlNode n in serialsNodes)
                    {
                        string inner = n.InnerText;
                        if (inner == null || inner.Length == 0) continue;
                        SerialNumberInfo sni = new SerialNumberInfo(n.InnerText);
                        string pk = ((XmlElement)n).GetAttribute(WceAttribute.ProducerKey).Trim();
                        if (pk.Length > 0)
                            sni.PK = pk;

                        serials.Add(sni);
                    }

                    //pks
                    XmlNodeList pKsNodes = art.SelectNodes(WceElement.ProducerKey);
                    if (pKsNodes != null)
                    {
                        foreach (XmlNode n in pKsNodes)
                        {
                            string pk = n.InnerText.Trim();
                            if (pk == null || pk.Length == 0) continue;
                            pks.Add(pk);
                        }
                    }
                }

                if (!articleDoms.Contains(artName))
                    continue;

                bool licensed = art == null ? false : articlesLicensedByDefault;
                /*if (!licensed && false) // TODO booleano ad hoc - configurationInfoProvider.LoadNotLicensedArticles()
                    continue;*/

                ArticleInfo artInfo = new ArticleInfo(artName, country, this.diagnostic, articleDoms);

                artInfo.Name = artName;
                artInfo.Obsolete = String.Compare(bool.TrueString, obsolete, true, CultureInfo.InvariantCulture) == 0;
                //artInfo.Producer	= producer;
                artInfo.SerialList = serials;
                artInfo.Licensed = licensed;
                artInfo.PKs = pks;
                artInfo.Solution = prodName;

                SerialNumberType snType;
                if (!VerifyShortNames(artInfo, out snType))
                {
                    //se i serial number non corrispondono allo short name del modulo 
                    //ripulisco la lista di SN, ma lascio il modulo non licenziato.
                    artInfo.SerialList.Clear();
                    artInfo.Licensed = false;
                }
                if (!VerifyShortNamesDeeply(artInfo, producer, snType, prodInfo.ProductId))
                {
                    //se i serial number non corrispondono all'ID del prodotto 
                    //ripulisco la lista di SN, ma lascio il modulo non licenziato.
                    artInfo.SerialList.Clear();
                    artInfo.Licensed = false;
                }

                if (!VerifyProductId(artInfo))
                {
                    //se i serial number non corrispondono all'ID del prodotto 
                    //ripulisco la lista di SN, ma lascio il modulo non licenziato.
                    artInfo.SerialList.Clear();
                    artInfo.Licensed = false;
                }

                if (ActivationObjectHelper.IsPowerProducer(artInfo.InternalCode) && !VerifyEdition(artInfo))//solo per microarea
                {
                    //se i serial number non corrispondono all'EDITION del prodotto 
                    //ripulisco la lista di SN, ma lascio il modulo non licenziato.
                    artInfo.SerialList.Clear();
                    artInfo.Licensed = false;
                }


                if (snType == SerialNumberType.DevelopmentPlusK || snType == SerialNumberType.PersonalPlusK ||
                        snType == SerialNumberType.DevelopmentPlusUser || snType == SerialNumberType.PersonalPlusUser)
                    prodInfo.DevelopPlusVersion = true;

                if (!prodInfo.DevelopVersion &&
                    (
                    snType == SerialNumberType.DevelopmentIU ||
                    snType == SerialNumberType.DevelopmentPlusK || snType == SerialNumberType.PersonalPlusK ||
                    snType == SerialNumberType.DevelopmentPlusUser || snType == SerialNumberType.PersonalPlusUser))
                    prodInfo.DevelopVersion = true;

                if (snType == SerialNumberType.DevelopmentIU)
                    prodInfo.DevelopVersionIU = true;

                if (snType == SerialNumberType.DevelopmentPlusK)
                    prodInfo.DevelopmentPlusK = true;

                if (snType == SerialNumberType.DevelopmentPlusUser)
                    prodInfo.DevelopmentPlusUser = true;

                if (snType == SerialNumberType.PersonalPlusUser)
                    prodInfo.PersonalPlusUser = true;

                if (snType == SerialNumberType.PersonalPlusK)
                    prodInfo.PersonalPlusK = true;

                if (!prodInfo.ResellerVersion && snType == SerialNumberType.Reseller)
                    prodInfo.ResellerVersion = true;

                if (!prodInfo.DistributorVersion && snType == SerialNumberType.Distributor)
                    prodInfo.DistributorVersion = true;

                if (!prodInfo.DistributorVersion && snType == SerialNumberType.Demo)
                    prodInfo.DemoVersion = true;

                if (snType == SerialNumberType.Multi)
                    prodInfo.MultiVersion = true;

                if (snType == SerialNumberType.StandAlone)
                    prodInfo.StandaloneVersion = true;

                if (snType == SerialNumberType.Backup)
                    prodInfo.BackupVersion = true;

                if (snType == SerialNumberType.Test)
                    prodInfo.TestVersion = true;

                if (!artInfo.Obsolete)
                    prodArts.Add(artInfo);
            }


            prodInfo.Articles = (ArticleInfo[])prodArts.ToArray(typeof(ArticleInfo));
            return prodInfo;
        }


        //---------------------------------------------------------------------
        private bool VerifyEdition(ArticleInfo art)
        {

            if (art == null || art.SerialList == null || art.SerialList.Count == 0)
                return true;
            //per compatibilità col passato 
            if (art.Edition == Edition.Undefined.ToString())
                return true;

            IList<SerialNumberInfo> serials = art.SerialList;
            if (serials != null && serials.Count > 0)
            {
                foreach (SerialNumberInfo sni in serials)
                {

                    try
                    {
                        if (!sni.IsComplete)
                            continue;
                        SerialNumber sn = new SerialNumber(sni.GetSerialWOSeparator(), art.CalType);
                        if (String.Compare(art.Edition, sn.Edition.ToString(), true, CultureInfo.InvariantCulture) != 0)
                        {
                            if (art.Edition != "ALL")
                            {
                                SetDiagnostic(DiagnosticType.Error | DiagnosticType.LogInfo, String.Format("Serial number {0} is in wrong edition. Please delete it from module {1}.", sn.Value, art.LocalizedName));
                                return false;
                            }
                        }
                    }
                    catch { }
                }
            }
            return true;
        }



        //---------------------------------------------------------------------
        private bool VerifyProductId(ArticleInfo art)
        {
            bool found = true;

            if (art == null || art.SerialList == null || art.SerialList.Count == 0)
                return found;
            //per compatibilità col passato se un salesmodule criptato non ha il codice non pretendo niente.
            if (art.ProdID == null || art.ProdID.Length == 0)
                return found;

            IList<SerialNumberInfo> serials = art.SerialList;
            if (serials != null && serials.Count > 0)
            {
                foreach (SerialNumberInfo sni in serials)
                {
                    string module = null;
                    try
                    {
                        if (!sni.IsComplete)
                            continue;
                        SerialNumber sn = new SerialNumber(sni.GetSerialWOSeparator(), art.CalType);
                        module = sn.Module;
                        if (!sn.HasCorrectCrc)
                        {
                            SetDiagnostic(DiagnosticType.Error, String.Format("Serial number not correct for module: {0}. Serial: {1}.", art.Name, sni.GetSerialWSeparator()));
                            return false;
                        }

                        found = found && (
                            String.Compare(sn.Product, art.ProdID, true, CultureInfo.InvariantCulture) == 0 ||  // prodid del modulo e serialnumber uguali
                           (String.Compare(sn.Product, art.AcceptDEMO, true, CultureInfo.InvariantCulture) == 0 && String.Compare(sn.Module, Consts.DemoID, true, CultureInfo.InvariantCulture) == 0) ||//se demo mn e acceptdemo mn

                          //opure acceptdemo è uguale al serial che è  un dviu o un rnfs e che è stato messu su un tbf ( caso tbf senza mago)
                          (
                          String.Compare(sn.Product, art.AcceptDEMO, true, CultureInfo.InvariantCulture) == 0 &&
                          (String.Compare(sn.Module, Consts.DevIDIU, true, CultureInfo.InvariantCulture) == 0 || String.Compare(sn.Module, Consts.ResellerID, true, CultureInfo.InvariantCulture) == 0) &&
                          art.CalType == CalTypeEnum.tbf
                          )
                            );
                    }
                    catch (SerialNumberFormatException exc)
                    {
                        SetDiagnostic(DiagnosticType.Error, String.Format("Serial number format exception for module: {0}. Serial: {1}. Message: {2}", art.Name, sni.GetSerialWSeparator(), exc.Message));
                        return false;
                    }

                }
            }
            if (!found)
            {
                Debug.WriteLine(String.Format(CultureInfo.InvariantCulture, LicenceStrings.NoProductIDCorresponding, art.Name));
                SetDiagnostic(DiagnosticType.Error | DiagnosticType.LogInfo, String.Format(LicenceStrings.NoProductIDCorresponding, art.Name));
            }
            return found;
        }


        //---------------------------------------------------------------------
        private bool VerifyShortNamesDeeply(ArticleInfo art, string producer, SerialNumberType sntype, string prodID)
        {
            //foreach (SerialNumberInfo s in art.SerialList)
            for (int i = art.SerialList.Count - 1; i >= 0; i--)
            {
                SerialNumberInfo s = art.SerialList[i];
                string shortName = SerialNumberInfo.GetModuleShortNameFromSerial(s, art.CalType);

                if (SerialNumber.IsMeaningLess(shortName, ActivationObjectHelper.IsPowerProducer(producer)) || SerialNumber.IsIntegrativeSerial(shortName, ActivationObjectHelper.IsPowerProducer(producer)))
                {

                    if (prodID != SerialNumberInfo.GetProdId(s))
                    {
                        SetDiagnostic(DiagnosticType.Error | DiagnosticType.LogInfo, String.Format("Serial number {0} doesn't belong to module {1}. Please delete it.", s.GetSerialWSeparator(), art.LocalizedName));
                    }
                    else continue;
                }




                //se è cal wms mobile va bene
                if (SerialNumber.IsWMSMobile(shortName, ActivationObjectHelper.IsPowerProducer(producer)))
                    continue;
                if (sntype != SerialNumberType.Normal)
                    continue;
                bool hasCalNumber = SerialNumberInfo.GetCalNumberFromSerial(art.Name, s, art.CalType, art.NamedCal) > 0;


                if (shortName != null && shortName != String.Empty && hasCalNumber)
                {
                    bool trovato = false;
                    foreach (string sn in art.ShortNames)
                        trovato = trovato | (String.Compare(sn, shortName, true, CultureInfo.InvariantCulture) == 0);

                    if (!trovato)
                    {
                        SetDiagnostic(DiagnosticType.Error | DiagnosticType.LogInfo, String.Format("Serial number {0} doesn't belong to module {1}. Please delete it.", s.GetSerialWSeparator(), art.LocalizedName));

                        //  art.SerialList.RemoveAt(i); //sarebbe più elegante eliminarlo, 
                        //ma poi avrei difficoltà sia con la form di attivazione che non si accorge del cambio 
                        //sia con la chiave di attivazione che non corrisponderebbe , 
                        //quindi per evitare errori peggiori preferisco eliminare il modulo intero invaccando tutto, 
                        //il seriale sbagliato deve essere eliminato coscientemente dall'utente,
                        return false;
                    }
                }
            }
            return true;
        }


        //---------------------------------------------------------------------
        private bool VerifyShortNames(ArticleInfo art, out SerialNumberType sntype)
        {
            sntype = SerialNumberType.Normal;
            if (art == null || art.SerialList == null || art.SerialList.Count == 0 || art.ShortNames == null || art.ShortNames.Count == 0)
                return true;
            bool found = false;
            foreach (SerialNumberInfo sni in art.SerialList)
            {

                string module = null;
                try
                {
                    if (!sni.IsComplete)
                        continue;
                    SerialNumber sn = new SerialNumber(sni.GetSerialWOSeparator(), art.CalType);
                    module = sn.Module;
                    if (!sn.HasCorrectCrc)
                    {
                        SetDiagnostic(DiagnosticType.Error, String.Format("Serial number not correct for module: {0}. Serial: {1}.", art.Name, sni.GetSerialWSeparator()));
                        return false;
                    }

                }
                catch (SerialNumberFormatException exc)
                {
                    SetDiagnostic(DiagnosticType.Error, String.Format("Serial number format exception for module: {0}. Serial: {1}. Message: {2}", art.Name, sni.GetSerialWSeparator(), exc.Message));
                    return false;
                }

                foreach (String shortName in art.ShortNames)
                {
                    if (String.Compare(Consts.DevIdUser, module, true, CultureInfo.InvariantCulture) == 0)
                    { sntype = SerialNumberType.DevelopmentPlusUser; return true; }
                    if (String.Compare(Consts.PersIdUSer, module, true, CultureInfo.InvariantCulture) == 0)
                    { sntype = SerialNumberType.PersonalPlusUser; return true; }
                    if (String.Compare(Consts.PersIdRiv, module, true, CultureInfo.InvariantCulture) == 0)
                    { sntype = SerialNumberType.PersonalPlusK; return true; }
                    if (String.Compare(Consts.DevIdRiv, module, true, CultureInfo.InvariantCulture) == 0)
                    { sntype = SerialNumberType.DevelopmentPlusK; return true; }
                    //eb nn più esistenti
                    //if (String.Compare(Consts.DevPLUSs1, module, true, CultureInfo.InvariantCulture) == 0)
                    //{ sntype = SerialNumberType.DevelopmentPlusS1; return true; }
                    //if (String.Compare(Consts.DevPLUSs3, module, true, CultureInfo.InvariantCulture) == 0)
                    //{ sntype = SerialNumberType.DevelopmentPlusS3; return true; }
                    if (String.Compare(Consts.DevIDIU, module, true, CultureInfo.InvariantCulture) == 0)
                    { sntype = SerialNumberType.DevelopmentIU; return true; }
                    if (String.Compare(Consts.ResellerID, module, true, CultureInfo.InvariantCulture) == 0)
                    { sntype = SerialNumberType.Reseller; return true; }
                    if (String.Compare(Consts.DistributorID, module, true, CultureInfo.InvariantCulture) == 0)
                    { sntype = SerialNumberType.Distributor; return true; }
                    if (String.Compare(Consts.DemoID, module, true, CultureInfo.InvariantCulture) == 0)
                    { sntype = SerialNumberType.Demo; return true; }


                    if (!found) found = (String.Compare(shortName, module, true, CultureInfo.InvariantCulture) == 0);
                    if (found) break;
                }

                if (String.Compare(Consts.TestID, module, true, CultureInfo.InvariantCulture) == 0)
                    sntype = SerialNumberType.Test;

                if (String.Compare(Consts.BackUpID, module, true, CultureInfo.InvariantCulture) == 0)
                    sntype = SerialNumberType.Backup;

                if (String.Compare(Consts.StandAloneID, module, true, CultureInfo.InvariantCulture) == 0)
                    sntype = SerialNumberType.StandAlone;
            }
            if (!found)
            {
                Debug.WriteLine(String.Format(CultureInfo.InvariantCulture, LicenceStrings.NoShortNameCorresponding, art.Name));
                SetDiagnostic(DiagnosticType.Error | DiagnosticType.LogInfo, String.Format(LicenceStrings.NoShortNameCorresponding, art.Name));
            }
            return found;
        }

        //---------------------------------------------------------------------
        public ProductInfo GetProductByName(string productName)
        {
            foreach (ProductInfo productInfo in this.Products)
                if (string.Compare(productName, productInfo.CompleteName, true, CultureInfo.InvariantCulture) == 0)
                    return productInfo;
            return null;
        }

        //---------------------------------------------------------------------
        public bool IsVirgin()
        {
            if (Products != null && Products.Length > 0)
                foreach (ProductInfo productInfo in this.Products)
                    if (productInfo != null && productInfo.HasLicensed)
                        return false;
            return true;
        }

        //---------------------------------------------------------------------
        public bool IsUserInfoComplete()
        {
            if (User == null)
                return false;
            return User.IsComplete();
        }

        //---------------------------------------------------------------------
        public bool HasSerials(string[] products)
        {
            if (products == null)
                return false;
            bool hasall = true;
            foreach (string name in products)
            {
                ProductInfo p = GetProductByName(name);
                if (p == null)
                    continue;
                hasall = hasall && p.HasSerials();
            }
            return hasall;
        }

        //---------------------------------------------------------------------
        public bool HasSerials()
        {
            if (this.Products == null || this.Products.Length == 0)
                return true;
            bool hasall = true;
            foreach (ProductInfo p in Products)
            {
                if (p == null)
                    continue;
                hasall = hasall && p.HasSerials();
            }
            return hasall;
        }

        //---------------------------------------------------------------------
        public string GetCountry()
        {
            if (User != null && User.Country != null)
                return User.Country;
            return String.Empty;
        }

        //---------------------------------------------------------------------
        public StringCollection GetIncludePaths(string product)
        {
            StringCollection list = new StringCollection();
            if (this.Products == null)
                return list;
            ProductInfo prod = GetProductByName(product);
            if (prod == null)
                return list;
            foreach (ArticleInfo art in prod.Articles)
            {
                if (art.IncludeModulesPaths == null) continue;
                foreach (IncludePathInfo x in art.IncludeModulesPaths)
                {
                    string includePath = x.Path;
                    if (includePath != null && includePath != String.Empty && !list.Contains(includePath))
                        list.Add(includePath);
                }
            }
            return list;
        }

        //---------------------------------------------------------------------
        private DatabaseVersion GetDatabaseVersion1()
        {
            DatabaseVersion db = DatabaseVersion.Undefined;

            if (this.Products == null || this.Products.Length <= 0)
                return DatabaseVersion.Undefined;
            for (int i = 0; i < this.Products.Length; i++)
            {
                DatabaseVersion thisDb = this.Products[i].GetDatabaseVersion();
                if (i == 0)
                    db = thisDb;

                else if (db != thisDb)
                    return DatabaseVersion.Undefined;
            }
            return db;
        }

        //---------------------------------------------------------------------
        public DBNetworkType GetDBNetworkType()
        {
            //if (this.IsDemo())
            //	return DBNetworkType.Large; // this is a requirement

            DBNetworkType netType = DBNetworkType.Undefined;

            //Recupero la versione del database andando a leggere tra i serial 
            //del modulo che fa da server, 
            //in modo da essere sicuri che il serial sia di microarea e che sia valido.

            ArticleInfo art = GetAValidServerCALArticle();

            if (art == null || art.SerialList == null || art.SerialList.Count == 0)
                return DBNetworkType.Undefined;

            foreach (SerialNumberInfo sni in art.SerialList)
            {
                netType = SerialNumberInfo.GetDBNetworkType(sni);
                if (netType != DBNetworkType.Undefined)
                    return netType;
            }

            return DBNetworkType.Undefined;
        }

        //---------------------------------------------------------------------
        public Edition GetEdition()
        {
            Edition edition = Edition.Undefined;
            //Recupero la versione del database andando a leggere tra i serial 
            //del modulo che fa da server, 
            //in modo da essere sicuri che il serial sia di microarea e che sia valido.

            ArticleInfo art = GetAValidServerCALArticle();
            if (art != null && art.SerialList != null && art.SerialList.Count > 0)
                foreach (SerialNumberInfo sni in art.SerialList)
                {
                    edition = SerialNumberInfo.GetEdition(sni);
                    if (edition != Edition.Undefined)
                        return edition;
                }
            //se non ho trovato un db interessante nel modulo di licenza utente
            //cerco di leggere l'attributo edition
            if (art != null)
            {
                try
                {
                    edition = (Edition)Enum.Parse(typeof(Edition), art.Edition, true);
                    if (edition == Edition.Undefined)
                        edition = GetEdition1();
                    if (edition == Edition.Undefined)
                        SetDiagnostic(DiagnosticType.LogInfo | DiagnosticType.Warning, LicenceStrings.EditionError);
                }
                catch (Exception exc)
                {
                    SetDiagnostic(DiagnosticType.LogInfo | DiagnosticType.Error, "GetEdition error : " + exc.ToString());
                }
            }
            return edition;
        }

        //---------------------------------------------------------------------
        private Edition GetEdition1()
        {
            Edition edition = Edition.Undefined;
            if (this.Products == null || this.Products.Length <= 0)
                return Edition.Undefined;
            for (int i = 0; i < this.Products.Length; i++)
            {
                if (i == 0)
                    edition = this.Products[i].GetEdition();
                else if (edition != this.Products[i].GetEdition())
                    return Edition.Undefined;
            }
            return edition;
        }

        //---------------------------------------------------------------------
        public string GetStandardUserId()
        {
            string id = String.Empty;
            if (this.User != null)
                id = this.User.GetUserId();
            return id;
        }

        #region ICloneable Members

        /// <summary>
        /// Performs a non-deep cloning
        /// </summary>
        /// <returns></returns>
        //---------------------------------------------------------------------
        public object Clone()
        {
            return ActivationObject.Clone(this);
        }

        #endregion

        /// <summary>
        /// Performs a non-deep cloning
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        //---------------------------------------------------------------------
        public static ActivationObject Clone(ActivationObject original)
        {
            ActivationObject copy = new ActivationObject();
            copy.Products = original.Products;
            copy.userInfo = original.userInfo;
            copy.DoubleLicensingErrorMessages = original.DoubleLicensingErrorMessages;
            copy.LicensedFileShadow = original.LicensedFileShadow;
            copy.activationVersion = original.activationVersion;
            copy.configurationInfoProvider = original.configurationInfoProvider;
            copy.diagnostic = original.diagnostic;
            copy.activationVersion = original.activationVersion;
            return copy;
        }

        //---------------------------------------------------------------------
        public void InvalidateCaches()
        {
            if (configurationInfoProvider != null)
                configurationInfoProvider.InvalidateCaches();
        }

        //---------------------------------------------------------------------
        public ArticleInfo GetArticleByName(string articleName)
        {
            foreach (ProductInfo p in this.Products)
            {
                ArticleInfo artInfo = p.GetArticleByName(articleName);
                if (artInfo != null)
                    return artInfo;
            }
            return null;
        }

        //---------------------------------------------------------------------
        public ArticleInfo GetWmsArticle()
        {
            return GetArticleByFunctionality("ERP", "WMSMobileLicence");
        }

        //---------------------------------------------------------------------
        public ArticleInfo GetManufacturingMobileArticle()
        {
            return GetArticleByFunctionality("ERP", "ManufacturingMobileLicence");
        }

        //---------------------------------------------------------------------
        public ArticleInfo GetOfficeLicenceArticle()
        {
            return GetArticleByFunctionality(Consts.ClientNet, Consts.UserOfficeLicence);
        }

        //---------------------------------------------------------------------
        public ArticleInfo GetUserLicenceArticle()
        {
            return GetArticleByFunctionality(Consts.ClientNet, Consts.UserLicence);
        }


        //qui si pone la questione di valore studio che ha codice vs e che include il tbf
        //---------------------------------------------------------------------
        public string GetMasterProductID()
        {
            if (string.IsNullOrWhiteSpace(cachedMasterProdID) || cachedMasterProdID == "F4")
            {
                ArticleInfo art = GetUserLicenceArticle();
                if (art != null)
                    cachedMasterProdID = art.ProdID;

                else SetDiagnostic(DiagnosticType.LogInfo | DiagnosticType.Error,
                       "Missing Master Product, please verify your installation " + InstallationData.InstallationName);
            }
            return cachedMasterProdID;
        }

        //---------------------------------------------------------------------
        public string GetMasterSolutionName()
        {
            ArticleInfo art = GetUserLicenceArticle();
            if (art == null) return null;
            return art.Solution;
        }

        //---------------------------------------------------------------------
        public string GetMainSerialNumber()
        {
            string serialValue = string.Empty;
            try
            {
                ArrayList list = GetServerCALArticles();
                if (list == null || list.Count == 0)
                    return null;
                foreach (ArticleInfo article in list)
                {
                    //ordino i serial number in ordine alfabetico per fare in modo di tornare sempre lo stesso.
                    //in realtà se aggiungi una cal financial ad una configurazione ent solo cal full potrebbe cambiare perchè 
                    //in moduli vengono presi in ordine alfabetico
                    List<SerialNumberInfo> f = article.SerialList as List<SerialNumberInfo>;
                    f.Sort(new MyListSorter());
                    foreach (SerialNumberInfo serial in f)
                    {
                        serialValue = serial.GetSerialWOSeparator();
                        SerialNumber n = new SerialNumber(serialValue);
                        if (serial.IsSpecial(article.CalType))
                            return serialValue;
                        foreach (string sn in article.ShortNames)
                            if (String.Compare(n.ProductCode.Module, sn, true, CultureInfo.InvariantCulture) == 0)
                                return serialValue;
                    }
                }
            }
            catch { }
            return serialValue;
        }

        //---------------------------------------------------------------------
        public ArrayList GetServerCALArticles()
        {
            return GetServerCALArticles(Edition.Undefined);
        }

        //Ritorna tutti gli articoli soggetti a cal di tipo server gdi, 
        //come il server per le versioni standard e professional
        //e financials, full e manufacturing per la versione enterprise
        //ho bisogno di edition per filtrare via gli autofunctional abusivi (verticali autofunctional messi in una pro regalano cal floating)
        //---------------------------------------------------------------------
        public ArrayList GetServerCALArticles(Edition ed)
        {
            ArrayList list = new ArrayList();
            foreach (ArticleInfo art in GetAllArticles())
                //  if (art.Licensed && (art.CalType == CalTypeEnum.Master || art.CalType == CalTypeEnum.AutoFunctional || art.CalType == CalTypeEnum.MasterNew))


                if (art.Licensed &&
                        (art.CalType == CalTypeEnum.Master
                        || art.CalType == CalTypeEnum.MasterNew
                        || ((art.CalType == CalTypeEnum.AutoFunctional && ed == Edition.Undefined) || (art.CalType == CalTypeEnum.AutoFunctional && ed == Edition.Enterprise) || (art.CalType == CalTypeEnum.AutoTbs && ed == Edition.Enterprise))))

                    list.Add(art);


            if (list.Count == 0)
                foreach (ArticleInfo art in GetAllArticles())
                    if (art.Licensed && (art.CalType == CalTypeEnum.tbf))
                        list.Add(art);

            return list;
        }



        //---------------------------------------------------------------------
        public ArticleInfo GetAValidServerCALArticle()
        {
            foreach (ArticleInfo art in GetServerCALArticles())
            {
                if (art.SerialList.Count == 0 || String.Compare(GetMasterProductID(), art.ProdID, StringComparison.InvariantCultureIgnoreCase) != 0)
                    continue;
                return art;
            }
            return null;

        }

        //---------------------------------------------------------------------
        public ArrayList GetAllArticles()
        {
            ArrayList list = new ArrayList();
            foreach (ProductInfo prod in Products)
                foreach (ArticleInfo art in prod.Articles)
                    list.Add(art);
            return list;
        }


        /*//---------------------------------------------------------------------
        /// <summary>
        /// ritorna la lista di nomi dei salesModule elencati nella solution
        /// </summary>
        public ArrayList GetAllModulesNames(string prodName)
        {
            ArrayList list = new ArrayList();
            XmlElement sol = configurationInfoProvider.GetProductSolution(prodName);
            XmlNodeList salesModuleNodeList = sol.GetElementsByTagName(WceStrings.Element.SalesModule);
            foreach (XmlElement solArt in salesModuleNodeList)
            {
                string name = solArt.GetAttribute(WceStrings.Attribute.Name);
                if (!string.IsNullOrEmpty(name))
                    list.Add(name);
            }
            return list;
        }
        */
        //---------------------------------------------------------------------
        public ArticleInfo GetWebArticles()
        {
            return GetArticleByFunctionality(Consts.WebFramework, Consts.EasyLookLicence);
        }

        //---------------------------------------------------------------------
        public ArticleInfo GetMDocPlatformArticle()
        {
            return GetArticleByFunctionality(Consts.Extensions, Consts.MDocPlatform);
        }

        //---------------------------------------------------------------------
        public bool CheckMissingSerial(ArticleInfo info)
        {
            //se non siamo in sviluppo e il modulo non ha serial 
            //sebbena debba averli il modulo lo considero non attivato

            if (info.HasSerial && (info.SerialList == null || info.SerialList.Count == 0))
            {
                info.Licensed = false;
                if (String.Compare(info.NeedlessText, "tbfincluded", StringComparison.InvariantCultureIgnoreCase) != 0)
                    SetDiagnostic(DiagnosticType.LogInfo | DiagnosticType.Error, "Missing serial numbers for module: " + info.LocalizedName);
                return false;
            }
            return true;
        }


        //---------------------------------------------------------------------
        public void TryToGetMasterProdIDApproximatively()
        {
            ArticleInfo[] infos = GetArticlesByFunctionality(Consts.ClientNet, Consts.UserLicence);
            if (infos == null) return;

            List<string> list = new List<string>();
            foreach (ArticleInfo info in infos)
            {
                if (list.Contains(info.ProdID)) continue;
                list.Add(info.ProdID);

                if (list.Count == 1)
                    cachedMasterProdID = list[0];
            }

        }

        //---------------------------------------------------------------------
        public ArticleInfo GetArticleByFunctionality(string application, string functionality, bool purgeback = true)
        {
            ArticleInfo[] infos = GetArticlesByFunctionality(application, functionality);
            if (infos == null) return null;
            int calCount = 0;
            ArticleInfo temp = null;
            bool check = !IsSpecialPlus();


            foreach (ArticleInfo info in infos)
            {
                if (!info.Licensed)
                    continue;
                if (check && !CheckMissingSerial(info))
                    continue;
                if (purgeback && info.ModuleMode == ModuleModeEnum.Back)
                    continue;
                //verificato che il modulo contiene la funzionalità ricercata, 
                //devo adesso verificare se tale funzionalità è inclusa in un modulo incluso 
                //o se è proprio del modulo trovato.
                // se la funzionalità ricercata fosse apportata da un modulo incluso 
                //allora si dovrebbe verificare se questo modulo è incluso come exlusive
                //Se fosse incluso come exlusive 
                //non si deve aggiungere alla lista il modulo attuale 
                //se quello incluso a sua volta è attivato.
                bool ignoreThis = false;
                foreach (IncludedSMInfo ismi in info.IncludedSM)
                {
                    if (ismi.includedSMMode == IncludedSMInfo.IncludedSMModeEnum.Exclusive)
                    {
                        ArticleInfo found = FindModule(ismi.Name, infos);
                        if (found == null)
                            continue;
                        if (!found.Licensed)
                        {
                            temp = found;
                            //temp.CalNumber = found.CalNumber;
                            return temp;
                        }

                        //se il modulo che trovo è attivato bene altrimenti devo attivarglielo e assegnarli il numero di cal di questo....
                        ignoreThis = true;
                        break;

                    }
                }
                if (ignoreThis)
                    continue;

                if (!info.HasNamedCal())
                    return info;

                //ritorna l'article con maggior numero di cal tra quelli disponibili
                if (calCount == 0 || calCount < info.NamedCalNumber)
                {
                    if (info.Name.CompareNoCase("tbf.server") && temp != null && ActivationObjectHelper.GetEditionFromString(temp.Edition) == Edition.Enterprise) continue;
                    calCount = info.NamedCalNumber;
                    temp = info;
                }
            }
            //se siamo in demo al primo che trovo gli dò 3 cal e lo ritorno.
            if (IsDemo() && temp != null)
                temp.NamedCalNumber = 3;
            return temp;
        }

        //---------------------------------------------------------------------
        public ArticleInfo FindModule(string name, ArticleInfo[] list)
        {
            foreach (ArticleInfo info in list)
                if (String.Compare(info.Name, name, true, CultureInfo.InvariantCulture) == 0)
                    return info;

            return null;
        }

        /// <summary>
        /// Non effettua controlli su serial o su cal, 
        /// dice solo se il modulo\funzionalità è presente in uno dei salesModule 
        /// elencati nel licensed.config
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsInLicensed(string application, string functionality)
        {
            bool ok = false;
            ArticleInfo[] infos = GetArticlesByFunctionality(application, functionality);
            if (infos == null) return ok;

            foreach (ArticleInfo info in infos)
            {
                if (!info.Licensed)
                    continue;
                ok = true;
                break;
            }
            return ok;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// verifica se la coppia applicazione-modulo esiste nella configurazione senza considerarne l'attivazione
        /// </summary>
        public bool IsInConfiguration(string application, string functionality)
        {
            ArticleInfo[] infos = GetArticlesByFunctionality(application, functionality);
            return (infos != null && infos.Length != 0);
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// verifica se la coppia applicazione-modulo esiste nella configurazione senza considerarne l'attivazione
        /// </summary>
        public ArticleInfo GetArticleInfo(string application, string functionality)
        {
            ArticleInfo[] infos = GetArticlesByFunctionality(application, functionality);
            return (infos != null && infos.Length != 0) ? infos[0] : null;
        }
        //---------------------------------------------------------------------
        public IModule[] GetAllFunctionalModules()
        {
            List<IModule> list = new List<IModule>();

            foreach (ProductInfo p in Products)
                foreach (ArticleInfo art in p.Articles)
                    foreach (IModule m in art.Modules)
                        list.Add(m);
            return list.ToArray();
        }

        //---------------------------------------------------------------------
        public string GetConfigurationHash()
        {
            if (Products == null && userInfo == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            if (Products != null)
            {
                foreach (ProductInfo p in Products)
                {
                    if (p == null)
                        continue;
                    foreach (ArticleInfo art in p.Articles)
                    {
                        if (art == null || !art.Licensed)
                            continue;
                        sb.Append(art.Name.ToLower().GetHashCode());
                    }
                }
            }

            //aggiungo anche le userinfo, così che i cambiamenti di country possano modificare l'hash
            if (userInfo != null && !string.IsNullOrWhiteSpace(userInfo.Country))
                sb.Append(userInfo.Country.ToLower().GetHashCode());

            return sb.ToString();
        }

        //---------------------------------------------------------------------
        public ActivationState GetState()
        {
            if (!CheckActivationKey())
                return ActivationState.NoActivated;
            bool existserial = false;
            if (GetSerialNumberType() == SerialNumberType.Demo) return ActivationState.Demo;
            foreach (ProductInfo p in Products)
                if (p.HasSerials())
                {

                    //if (p.DemoVersion)
                    //    return ActivationState.Demo;
                    existserial = true;
                }
            return existserial ? ActivationState.Activated : ActivationState.NoActivated;
        }

        /// <summary>
        /// esegue un controllo tra configurazione e chiave di attivazione
        /// </summary>
        /// <returns>true se il controllo ha avuto successo</returns>
        //---------------------------------------------------------------------------
        internal bool CheckActivationKey()
        {
            try
            {
                FacadeBuilder fb = new FacadeBuilder();
                int val = -1;
                bool ok = fb.Build(GetKey_CompileTimeBrand(),
                    GetInstallationWce(),
                    ActivationKey,
                    ActivationVersion,
                    out val
                    );

                DummyFunction(ok);

                return (val % 2 == 0);
            }

            catch (ArgumentNullException exc2)
            {
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, exc2.ToString());
                return false;
            }

            catch (Exception exc)
            {
                string pathAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string pathMachineKeys = "Microsoft\\Crypto\\RSA\\MachineKeys";
                string path = Path.Combine(pathAppData, pathMachineKeys);
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, (String.Format("Error 230. Please verify the permission on folder {0} (may be you have to set Everyone - FullControl on it). Details: {1}", path, exc.Message)), String.Empty);
                return false;
            }

        }

        //---------------------------------------------------------------------------
        private string GetKey_CompileTimeBrand()
        {
            string id = GetMasterProductID();
            if (id == null)
                throw new ArgumentNullException("Missing Master Product, please verify your installation " + InstallationData.InstallationName);
            if (id.ToLower(CultureInfo.InvariantCulture) == "vs")
                return "<RSAKeyValue><Modulus>vNzSp3Q6BpjPymlFQTWEB/YGXJa+O7TYAUiBaHd7P5p0vkftrYsvTKRHf7abnWO+nnXYf4an71Etqn5E2/3PPVAvj4oV9q02Bvf1cHtOJyhVRp0R08suK9yo5sFknsWV1bV4VjY/S3nElJvPPUx3UV1YJAunVjp20xLunfr8K/8=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            //default corrispondente a mago
            return "<RSAKeyValue><Modulus>q/LW94WzyeW4WfHZYCsoZKHnjWbHE9SGc1LHtcPykvxWpgBaDFRpm5s00lDfr8RYX5cOAeXIV2kvu4vKnNCFj51myje+ZbGQCMkyQPgbp6aXKeipwbMWNq1ryB8AWh82e4JWKgg90uxMXqWArkaeVEnEE3yu8vyGVmN58uKorI0=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        }

        //---------------------------------------------------------------------------
        private int DummyFunction(bool ok)
        {
            //Aggiunto per simulare una chiamata utilizzando un valore di ritorno fittizio
            //Non rimuovere
            if (ok)
                return 1;
            return 0;
        }

        //---------------------------------------------------------------------
        public Hashtable GetActivatedList()
        {
            Hashtable applications = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            bool check = !IsSpecialPlus();

            foreach (ProductInfo p in Products)
            {
                foreach (ArticleInfo art in p.Articles)
                {
                    if (!art.Licensed)
                        continue;

                    if (//se non sono validi disabilito proprio il modulo in modo che non ci siano buchi nelle procedure successive.
                        (art.DVLPPlus() && IsDevelopmentSuite()) || //dvlp plus va solo coi seriali dvlpplus e se demo e rnfs va con limitazioni funzinali eventualmente descritte nei sales module
                        (art.IsNoDemoModule() && IsDemo()) ||
                        (check && !CheckMissingSerial(art))
                        )
                    {
                        art.Licensed = false;
                        continue;
                    }

                    //mail di mario del 17/11/2016 
                    //rendiamo i moduli non disponibili attivabili ed utilizzabili in parte, 
                    //quindi ho deciso che il modulo marchiato non disponibile se attivato da il warning 
                    //ma mantiene il contenuto attivato, quindi se dentro ci sonodelle cose che non si vuole rendere disponibili vanno commentate!

                    //if (//se l'articolo è UNAVAILABLE  oppure se availbaleNFS ma  l'attivazione non  è NFS nè dviu allora ne cancello  il contenuto, lasciandolo attivato ( vuoto)
                    //    art.Available == ArticleInfo.Avaiability.Unavailable || 
                    //    ((art.Available == ArticleInfo.Avaiability.AvailableNFS) && (!IsReseller() || IsDevelopmentIU())))
                    //    art.ClearNotAvailable();

                    foreach (ModuleInfo m in art.Modules)
                    {
                        //se il modulo è soggetto a controllo di dipendenza 
                        //e non è soddisfatta non lo includo nella lista.
                        if (m.DepEvalStatus != DependencyEvaluationStatus.Satisfied)
                            continue;
                        if (m.LawInfoSN != null && !m.LawInfoSN.Verify(GetSerialNumberType().ToString()))
                            continue;
                        string app = m.Application;
                        if (!applications.Contains(app))
                            applications.Add(app, new StringCollection());
                        StringCollection modules = applications[app] as StringCollection;
                        string name = m.Name.ToLower(CultureInfo.InvariantCulture);
                        if (!modules.Contains(name))
                            modules.Add(name);
                    }
                }
                foreach (IncludePathInfo ipi in p.IncludeModulesPathsFiltered)
                {
                    if (!ipi.OnActivated) continue; //non aggiungo quelli non relativi ad addon
                    string app = string.Empty;
                    if (!applications.Contains(app))
                        applications.Add(app, new StringCollection());
                    StringCollection modules = applications[app] as StringCollection;
                    //string name = ipi.Path.ToLower(CultureInfo.InvariantCulture);
                    // devo aggiungere gli include path con indicazione di 
                    // Applicazione.Modulo verificando per ogni 
                    // combinazione possibile per il prodotto inquestione
                    //l'esistenza su fileSystem 
                    ArrayList list = VerifyAddon(p, ipi);
                    foreach (string name in list)
                    {
                        if (!modules.Contains(name.ToLower(CultureInfo.InvariantCulture)))
                            modules.Add(name.ToLower(CultureInfo.InvariantCulture));
                    }
                }
            }
            return applications;
        }

        //---------------------------------------------------------------------
        private ArrayList VerifyAddon(ProductInfo p, IncludePathInfo ipi)
        {
            ArrayList l = new ArrayList();
            if (this.configurationInfoProvider == null ||
                this.configurationInfoProvider.GetPathFinder() == null ||
                p == null ||
                ipi == null ||
                !ipi.OnActivated)

                return l;

            string pattern = "{0}.{1}.{2}";
            foreach (ArticleInfo a in p.Articles)
            {
                if (!a.Licensed)
                    continue;
                foreach (ModuleInfo m in a.Modules)
                {
                    string app = m.Application;
                    string mod = m.Name;

                    ILibraryInfo li = null;
                    IBaseModuleInfo bmi = this.configurationInfoProvider.GetPathFinder().GetModuleInfoByName(app, mod);
                    if (bmi == null)
                        continue;
                    li = bmi.GetLibraryInfoByPath(ipi.Path);
                    if (li == null)
                        continue;
                    l.Add(String.Format(pattern, app, mod, ipi.Path));
                }
            }
            return l;
        }


        ///<summary>
        ///Comprende i serialnumbers speciali, cioè tutti tranne i serial number normali
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsSpecial()
        {
            //return GetSerialNumberType() != SerialNumberType.Normal;
            return IsDevelopmentIU() || IsDemo() || IsReseller() || IsDistributor();
        }

        ///<summary>
        ///Comprende i serialnumbers speciali, cioè tutti tranne i serial number normali
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsSpecialPlus()
        {
            //return GetSerialNumberType() != SerialNumberType.Normal;
            return IsDevelopmentSuite() || IsDemo() || IsReseller() || IsDistributor() || IsDevelopmentPlus();
        }
        ///<summary>
        ///Comprende i serialnumbers dviu,come dvlp ma 10 cal
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsDevelopmentIU()
        {
            return GetSerialNumberType() == SerialNumberType.DevelopmentIU;
        }

        ///<summary>
        ///Comprende tutti i serialnumbers di sviluppo
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsDevelopmentSuite()
        {
            return IsDevelopmentIU() || IsDevelopmentPlus();
        }


        ///<summary>
        ///Comprende il serialnumber di sviluppo che permette l'utilizzo dei moduli dvlpPlus (dveb)
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsDevelopmentPlus()
        {
            return GetSerialNumberType() == SerialNumberType.DevelopmentPlusK/*|| GetSerialNumberType() == SerialNumberType.DevelopmentPlusUser */||
                GetSerialNumberType() == SerialNumberType.PersonalPlusK/*||
                GetSerialNumberType() == SerialNumberType.PersonalPlusUser*/;
        }

        ///<summary>
        ///Comprende il serialnumber demo (demo)
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsDemo()
        {
            return GetSerialNumberType() == SerialNumberType.Demo;
        }

        ///<summary>
        ///Comprende il serialnumber per reseller, 3 cal (rnfs)
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsReseller()
        {
            return GetSerialNumberType() == SerialNumberType.Reseller;
        }

        ///<summary>
        ///Comprende il serialnumber per distributor da 10 cal (dnfs), ma non esiste più 
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsDistributor()
        {
            return GetSerialNumberType() == SerialNumberType.Distributor;
        }

        ///<summary>
        ///indica se il serial number è di tipo extension multicompany
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsMulti()
        {
            return GetSerialNumberType() == SerialNumberType.Multi;
        }

        ///<summary>
        ///indica se il serial number è di tipo extension Test
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsTest()
        {
            return GetSerialNumberType() == SerialNumberType.Test;
        }

        ///<summary>
        ///indica se il serial number è di tipo extension Backup
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsBackup()
        {
            return GetSerialNumberType() == SerialNumberType.Backup;
        }

        ///<summary>
        ///indica se il serial number è di tipo extension StandAlone
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsStandAlone()
        {
            return GetSerialNumberType() == SerialNumberType.StandAlone;
        }

        ///<summary>
        ///se viene trovato un serial di sql vecchio(en, ms, st) o se viene trovato un serial di tipo PLxxSQL senza relativo upg al 2012 allora non è possibile usare  sql 2012
        ///se invece non ci sono seriali di db e non ci sono seriali PLxxSQL o se  ci sono i seriali del 2012 allora ok
        /// </summary>
        //---------------------------------------------------------------------
        public bool Sql2012Allowed()
        {
            ArticleInfo art = GetAValidServerCALArticle();
            bool plSqlFound = false;
            bool sql2012Found = false;
            bool plSQL = false;
            bool sql2012 = false;
            bool sqlOLDFound = false;
            bool sqlOLD = false;
            //per ogni serial di ogni articolo verifico la presenza di serial number  cal embedded di sql, poi faccio le mie valutazioni
            if (art != null && art.SerialList != null && art.SerialList.Count > 0)
                foreach (SerialNumberInfo sni in art.SerialList)
                {
                    sni.Analize2012Status(art.CalType, ActivationObjectHelper.IsPowerProducer(art.InternalCode), out sqlOLD, out sql2012, out plSQL);

                    if (sqlOLD) sqlOLDFound = true;
                    if (sql2012) sql2012Found = true;
                    if (plSQL) plSqlFound = true;
                }

            /*29/04/2014 modifica gestione sql2012 per 3.9.9
             *cal solo old: connetto solo old
             *nessuna cal, cal mescolate, cal solo 2012: connetto old e 2012
             *
             *quindi torno true se sono tutti false (non ho trovato nessun serial di db) o se trovo un 2012
             *qualsiasi altro tipo di mescolanza viene gestito dal backend con sostituzioni al volo,
             *(ricordare memoria storica: le cal plsql, che contenevano licenza sql, devono essere tamponate da un serial di upgrade)
             * */
            return sql2012Found || (!sqlOLDFound && !plSqlFound && !sql2012Found);


        }

        //---------------------------------------------------------------------
        public SerialNumberType GetSerialNumberType()
        {
            if (cachedMasterProdSerialType == SerialNumberType.UNDEFINED)
                cachedMasterProdSerialType = GetSerialNumberType2();
            return cachedMasterProdSerialType;
        }

        //---------------------------------------------------------------------
        public SerialNumberType GetSerialNumberType2()
        {

            SerialNumberType sntALLProd = SerialNumberType.Normal;
            foreach (ProductInfo pi in Products)
            {
                SerialNumberType sntThisProd = SerialNumberType.UNDEFINED;
                if (!String.IsNullOrWhiteSpace(cachedMasterProdID) && pi.ProductId != cachedMasterProdID)
                    continue;

                if (pi.DevelopVersionIU)
                    return SerialNumberType.DevelopmentIU;

                else if (pi.DevelopmentPlusK)
                    return SerialNumberType.DevelopmentPlusK;

                else if (pi.DevelopmentPlusUser)
                    return SerialNumberType.DevelopmentPlusUser;

                else if (pi.PersonalPlusUser)
                    return SerialNumberType.PersonalPlusUser;

                else if (pi.PersonalPlusK)
                    return SerialNumberType.PersonalPlusK;

                else if (pi.ResellerVersion)
                    sntThisProd = SerialNumberType.Reseller;
                else if (pi.DistributorVersion)
                    sntThisProd = SerialNumberType.Distributor;
                else if (pi.DemoVersion)
                    sntThisProd = SerialNumberType.Demo;

                else if (pi.MultiVersion)
                    sntThisProd = SerialNumberType.Multi;
                else if (pi.TestVersion)
                    sntThisProd = SerialNumberType.Test;
                else if (pi.BackupVersion)
                    sntThisProd = SerialNumberType.Backup;
                else if (pi.StandaloneVersion)
                    sntThisProd = SerialNumberType.StandAlone;

                else sntThisProd = SerialNumberType.Normal;

                if (sntThisProd != SerialNumberType.Normal)
                    sntALLProd = sntThisProd;
            }

            return sntALLProd;

        }

        //---------------------------------------------------------------------
        public DatabaseVersion GetDatabaseVersion()
        {
            //if (IsDemo())
            //	return DatabaseVersion.SqlServer2000;

            DatabaseVersion db = DatabaseVersion.Undefined;
            //Recupero la versione del database andando a leggere tra i serial 
            //del modulo che espone la funzionalità LICENZAUTENTE, 
            //in modo da essere sicuri che il serial sia di microarea e che sia valido.

            ArticleInfo art = GetAValidServerCALArticle();
            if (art != null && art.SerialList != null && art.SerialList.Count > 0)
                foreach (SerialNumberInfo sni in art.SerialList)
                {
                    db = SerialNumberInfo.GetDatabaseVersion(sni);
                    if (db != DatabaseVersion.Undefined && db != DatabaseVersion.All)
                        return db;
                }

            //se non ho trovato un db interessante nel modulo di licenza utente
            //continuo la ricerca in tutti gli altri
            db = DatabaseVersion.Undefined;
            db = GetDatabaseVersion1();
            if (db == DatabaseVersion.Undefined)
                SetDiagnostic(DiagnosticType.Warning, LicenceStrings.DbError);
            return db;
        }

        //---------------------------------------------------------------------
        public bool GetCalNumber(out int gdiCalNamed, out int gdiCalConcurrent, out int easyCal, out int mdCal, out int tpCal, out int wmsCal, out int manufacturingCal, out Hashtable tpProducerCal, out Hashtable CALNumberForArticle)
        {
            tpCal = 0;
            gdiCalNamed = 0;
            gdiCalConcurrent = 0;//demo e dvlp e nfs sempre 0
            easyCal = 0;
            mdCal = 0;
            wmsCal = 0;
            manufacturingCal = 0;
            Edition ed = GetEdition();

            tpProducerCal = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            CALNumberForArticle = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            bool isEnt = false;

            //SERVER CAL
            ArrayList arts = null;
            try
            {
                arts = GetServerCALArticles(ed);
            }
            catch (Exception exc)
            {
                SetDiagnostic(DiagnosticType.LogInfo | DiagnosticType.Error, "error getting cal number: " + exc.Message);
                return false;
            }
            if (arts == null || arts.Count == 0)
            {
                SetDiagnostic(DiagnosticType.LogInfo | DiagnosticType.Error, LicenceStrings.NoLicence);
                return false;
            }
            int c = GetSpecialCalNumber();
            gdiCalNamed = c;
            //Ottenuti tutti gli articoli soggetti a cal gdi popolo un hashtable con nome modulo e numeor cal associate.
            //Per i moduli composti da più Include, anch'ssi soggetti a Cal  non aggiungo il modulo padre 
            //ma aggiungo i moduli figli, aggiungendo eventualemnte il numero di cal al modulo già presente in lista
            foreach (ArticleInfo art in arts)
            {
                if (art.NamedCalNumber <= 0 && art.ConcurrentCalNumber <= 0 && (art.IncludedSM == null || art.IncludedSM.Length == 0))
                {
                    if (!IsSpecial())
                        //nel caso di moduli con cal che in sviluppo sono senza serial number e conterebbero zero, 
                        //quindi non verrebbe aggiunto alla calused, impedendone l'utilizzo
                        //per gli altri casi invece skippo
                        continue;
                }
                if (art.IncorrectSerialsMessage != null)
                    SetDiagnostic(DiagnosticType.LogInfo | DiagnosticType.Error, art.IncorrectSerialsMessage);

                if (ActivationObjectHelper.GetEditionFromString(art.Edition) == Edition.Enterprise)
                    isEnt = true;

                if (!IsSpecialPlus())
                {

                    gdiCalNamed += art.NamedCalNumber;
                    gdiCalConcurrent += art.ConcurrentCalNumber;
                    if (isEnt)
                    {

                        int wmsartval = art.WmsMobileCalNumber;//le cal wmsmobile sono solo per ent sul server
                        int manartval = art.ManufacturingMobileCalNumber;//le cal manufacturing mobile sono solo per ent sul server
                                                                         //mobile

                        if (wmsartval > 0)
                        {
                            wmsCal += wmsartval;
                            if (!CALNumberForArticle.Contains("WMS"))
                                CALNumberForArticle.Add("WMS", wmsCal);
                            else
                            {
                                int i = (int)CALNumberForArticle["WMS"];
                                i += wmsCal;
                                CALNumberForArticle["WMS"] = i;
                            }

                        }
                        if (manartval > 0)
                        {
                            manufacturingCal += manartval;
                            if (!CALNumberForArticle.Contains("Manufacturing"))
                                CALNumberForArticle.Add("Manufacturing", manufacturingCal);
                            else
                            {
                                int i = (int)CALNumberForArticle["Manufacturing"];
                                i += manufacturingCal;
                                CALNumberForArticle["Manufacturing"] = i;
                            }
                        }
                    }
                }

                bool addedIncluded = false;
                bool addedIncluded2 = true;
                if (art.IncludedSM != null || art.IncludedSM.Length > 0)
                    foreach (IncludedSMInfo ismi in art.IncludedSM)
                        foreach (ArticleInfo a in GetAllArticles())
                        {
                            if (string.Compare(ismi.Name, a.Name, true, CultureInfo.InvariantCulture) == 0 && a.HasCal())
                            {

                                addedIncluded = true;

                                if (a.Name.CompareNoCase("tbf.server") && isEnt)

                                { addedIncluded = false; addedIncluded2 = false; }
                                if (!CALNumberForArticle.Contains(a.Name))
                                    CALNumberForArticle.Add(a.Name, (c == 0 ? art.NamedCalNumber + art.ConcurrentCalNumber : c));
                                else
                                {
                                    int i = (int)CALNumberForArticle[a.Name];
                                    i += (c == 0 ? art.NamedCalNumber + art.ConcurrentCalNumber : 0);
                                    CALNumberForArticle[a.Name] = i;
                                }
                            }
                        }
                if (!addedIncluded || !addedIncluded2)
                {
                    if (!CALNumberForArticle.Contains(art.Name))
                        CALNumberForArticle.Add(art.Name, (c == 0 ? art.NamedCalNumber + art.ConcurrentCalNumber : c));
                    else
                    {
                        int i = (int)CALNumberForArticle[art.Name];
                        i += (c == 0 ? art.NamedCalNumber + art.ConcurrentCalNumber : 0);
                        CALNumberForArticle[art.Name] = i;
                    }
                }
            }

            if (IsSpecialPlus())//develop, demo, rnfs o dnfs
            {
                gdiCalNamed = gdiCalConcurrent = easyCal = mdCal = tpCal = c;

                //mobile 
                ArticleInfo aiman = null;
                ArticleInfo aiwms = null;
                try
                {
                    aiman = GetManufacturingMobileArticle();
                    aiwms = GetWmsArticle();
                }
                catch
                {
                    aiman = null;
                    aiwms = null;
                }
                if (aiwms != null)
                {
                    wmsCal = c;
                    if (!CALNumberForArticle.Contains("WMS"))
                        CALNumberForArticle.Add("WMS", wmsCal);
                    else
                    {
                        int i = (int)CALNumberForArticle["WMS"];
                        i += wmsCal;
                        CALNumberForArticle["WMS"] = i;
                    }

                }

                if (aiman != null)
                {
                    manufacturingCal = c;
                    if (!CALNumberForArticle.Contains("Manufacturing"))
                        CALNumberForArticle.Add("Manufacturing", manufacturingCal);
                    else
                    {
                        int i = (int)CALNumberForArticle["Manufacturing"];
                        i += manufacturingCal;
                        CALNumberForArticle["Manufacturing"] = i;
                    }
                }

                return true;
            }

            if (IsStandAlone())
            {
                gdiCalNamed = 0;
                gdiCalConcurrent = 1;
                wmsCal = 0;
                manufacturingCal = 0;
            }
            if (IsTest())
            {
                gdiCalNamed = 0; wmsCal = 1; manufacturingCal = 1;
                gdiCalConcurrent = 2;
            }
            //Verifica dei 10 cal per ENT
            if (gdiCalConcurrent < 10 && isEnt && !IsTest() && !IsStandAlone())
            {
                gdiCalNamed = gdiCalConcurrent = easyCal = mdCal = tpCal = 0;
                SetDiagnostic(DiagnosticType.LogInfo | DiagnosticType.Error, LicenceStrings.TenCalNeeded);
                return true;
            }

            ArticleInfo officeart = null;
            //OFFICE CAL
            try
            {
                officeart = GetOfficeLicenceArticle();
            }
            catch (Exception exc)
            {
                SetDiagnostic(DiagnosticType.LogInfo | DiagnosticType.Error, "Error getting magicdocs module: " + exc.Message);
                return false;
            }
            if (officeart != null)
            {
                if (officeart.IncorrectSerialsMessage != null)
                    SetDiagnostic(DiagnosticType.LogInfo | DiagnosticType.Information, officeart.IncorrectSerialsMessage);

                if (officeart.WebCalNumber > 0)
                    mdCal = officeart.WebCalNumber;
                if (officeart.CalType == CalTypeEnum.AutoFunctional || officeart.CalType == CalTypeEnum.MasterNew || officeart.CalType == CalTypeEnum.TPGateNew || officeart.CalType == CalTypeEnum.ServerNew)
                    mdCal = SerialNumber.UnlimitedCalNumber;
                if (IsStandAlone())
                    mdCal = 1;
                if (IsTest())
                    mdCal = 2;
            }

            //WMS CAL
            if (!IsSpecial() && !IsStandAlone() && !IsTest() && !isEnt)//ho già sistemato questi casi qui sopra.
            {
                ArticleInfo wmsArt = null;
                try
                {
                    wmsArt = GetWmsArticle();
                }
                catch (Exception exc)
                {
                    SetDiagnostic(DiagnosticType.LogInfo | DiagnosticType.Error, "Error getting wms module: " + exc.Message);
                    return true;
                }

                if (wmsArt != null)
                    wmsCal = wmsArt.WmsMobileCalNumber;

                //manufmob cal
                ArticleInfo manufmobArt = null;
                try
                {
                    manufmobArt = GetManufacturingMobileArticle();
                }
                catch (Exception exc)
                {
                    SetDiagnostic(DiagnosticType.LogInfo | DiagnosticType.Error, "Error getting wms module: " + exc.Message);
                    return true;
                }

                if (manufmobArt != null)
                    manufacturingCal = manufmobArt.ManufacturingMobileCalNumber;


                //mobile
                if (!CALNumberForArticle.Contains("WMS"))
                    CALNumberForArticle.Add("WMS", wmsCal);
                else
                {
                    int i = (int)CALNumberForArticle["WMS"];
                    i += wmsCal;
                    CALNumberForArticle["WMS"] = i;
                }

                if (!CALNumberForArticle.Contains("Manufacturing"))
                    CALNumberForArticle.Add("Manufacturing", manufacturingCal);
                else
                {
                    int i = (int)CALNumberForArticle["Manufacturing"];
                    i += manufacturingCal;
                    CALNumberForArticle["Manufacturing"] = i;
                }
            }

            //WEB CAL
            ArticleInfo webArt = null;
            try
            {
                webArt = GetWebArticles();
            }
            catch (Exception exc)
            {
                SetDiagnostic(DiagnosticType.LogInfo | DiagnosticType.Error, "Error getting easylook module: " + exc.Message);
                return true;
            }

            if (webArt != null)
            {

                //foreach (ArticleInfo webArt in webArts)
                {
                    if (webArt.CalType == CalTypeEnum.AutoFunctional)
                    {
                        easyCal = SerialNumber.UnlimitedCalNumber;
                        //	break;
                    }
                    easyCal += webArt.WebCalNumber;
                }
                if (IsStandAlone())
                    easyCal = 1;
                if (IsTest())
                    easyCal = 2;
            }

            //third part CAL
            ArticleInfo tpArt = null;
            try
            {
                tpArt = GetMDocPlatformArticle();
            }
            catch (Exception exc)
            {
                SetDiagnostic(DiagnosticType.LogInfo | DiagnosticType.Error, "Error getting magiclink module: " + exc.Message);
                return true;
            }

            if (tpArt != null /*&& tpArts.Length > 0*/)
            {

                //foreach (ArticleInfo tpArt in tpArts)
                {
                    if (
                        (tpArt.Licensed && (tpArt.CalType == CalTypeEnum.MasterNew || tpArt.CalType == CalTypeEnum.TPGateNew)) ||
                         tpArt.CalType == CalTypeEnum.AutoFunctional
                        )
                        tpCal = SerialNumber.UnlimitedCalNumber;
                    else
                        tpCal += tpArt.WebCalNumber;

                    if (IsStandAlone())
                        tpCal = 1;
                    if (IsTest())
                        tpCal = 2;

                    foreach (SerialNumberInfo sni in tpArt.SerialList)
                    {
                        if (sni == null || sni.PK == null)
                            continue;
                        int calFromSerial = SerialNumberInfo.GetCalNumberFromSerial(tpArt.Name, sni, tpArt.CalType, true, false, false, false);
                        if (tpProducerCal[sni.PK] == null)
                            tpProducerCal.Add(sni.PK, calFromSerial);
                        else
                            tpProducerCal[sni.PK] = (int)tpProducerCal[sni.PK] + calFromSerial;
                    }

                    if (tpArt.PKs != null && tpArt.PKs.Count > 0)
                    {
                        int n = SerialNumber.UnlimitedCalNumber / tpArt.PKs.Count;
                        foreach (string pk in tpArt.PKs)
                        {
                            if (pk == null) continue;
                            if (tpProducerCal[pk] == null)
                                tpProducerCal.Add(pk, n);
                            //se cé già è già al massimo non aggiungo.
                        }
                    }

                }

            }

            return true;
        }

        //---------------------------------------------------------------------
        private int GetSpecialCalNumber()
        {
            if (IsDevelopmentIU())
                return 999;
            if (IsDevelopmentPlus())
                return 3;
            if (IsDemo() || IsDevelopmentSuite())
                return 3;
            if (IsReseller())
            {
                if (GetEdition() == Edition.Standard)
                    return 1;
                else
                    return 3;
            }

            if (IsDistributor())
            {
                if (GetEdition() == Edition.Standard)
                    return 1;
                else
                    return 10;
            }
            return 0;
        }

        //---------------------------------------------------------------------
        private ArticleInfo[] GetArticlesByFunctionality(string application, string functionality)
        {
            if (this.Products == null)
            {
                Debug.Fail("ActivationObject.GetArticlesByFunctionality: ProductsInfo null");
                // SetDiagnostic(DiagnosticType.Error | DiagnosticType.LogInfo, "2");
                return null;
            }
            //SetDiagnostic(DiagnosticType.Error | DiagnosticType.LogInfo, "3");
            ArrayList articlesList = new ArrayList();
            foreach (ProductInfo pi in this.Products)
            {
                //SetDiagnostic(DiagnosticType.Error | DiagnosticType.LogInfo, "4");
                if (pi.Articles == null)
                {
                    //  SetDiagnostic(DiagnosticType.Error | DiagnosticType.LogInfo, "5");
                    Debug.Fail("ActivationObject.GetArticlesByFunctionality: Articles non disponibili per il prodotto: " + pi.CompleteName);
                    continue;
                }
                foreach (ArticleInfo ai in pi.Articles)
                {
                    // SetDiagnostic(DiagnosticType.Error | DiagnosticType.LogInfo, "6");
                    if (ai.Modules == null)
                        continue;
                    //SetDiagnostic(DiagnosticType.Error | DiagnosticType.LogInfo, "7");
                    ModuleInfo mi = ArticleInfo.GetModuleByName(ai, application, functionality);
                    if (mi != null)
                    {

                        //SetDiagnostic(DiagnosticType.Error | DiagnosticType.LogInfo, "8");

                        articlesList.Add(ai);
                    }
                }
            }

            return (ArticleInfo[])articlesList.ToArray(typeof(ArticleInfo));
        }

        //---------------------------------------------------------------------
        public void SetDiagnostic(DiagnosticType type, string message)
        {
            if (diagnostic == null || message == null || message.Length == 0)
                return;
            diagnostic.Set(DiagnosticType.LogInfo | type, message);
        }


        //---------------------------------------------------------------------
        public string GetInstallationWce()
        {
            IBasePathFinder pathFinder = configurationInfoProvider.GetPathFinder();
            if (pathFinder == null)
                return null;


            ExportManager em = new ExportManager(diagnostic, pathFinder);
            string key, exceptionMessage;
            //se è attivabile lo carico e lo aggiungo alla lista, 
            string wce = em.GetInstallationWce(String.Empty, out key, out exceptionMessage);
            if (exceptionMessage != null && exceptionMessage.Length > 0)
            {
                string message = String.Concat(LicenceStrings.ReadingConfigurationError, " ", LicenceStrings.ExceptionMessage, exceptionMessage);
                SetDiagnostic(DiagnosticType.Error, message);
            }

            return wce;
        }

        //---------------------------------------------------------------------
        public XmlNode GetInstallationWceNode()
        {
            IBasePathFinder pathFinder = configurationInfoProvider.GetPathFinder();
            if (pathFinder == null)
                return null;

            ExportManager em = new ExportManager(diagnostic, pathFinder);
            string key, exceptionMessage;
            //se è attivabile lo carico e lo aggiungo alla lista, 
            XmlNode wce = em.GetInstallationWceNode(String.Empty, out key, out exceptionMessage);
            if (exceptionMessage != null && exceptionMessage.Length > 0)
            {
                string message = String.Concat(LicenceStrings.ReadingConfigurationError, " ", LicenceStrings.ExceptionMessage, exceptionMessage);
                SetDiagnostic(DiagnosticType.Error, message);
            }

            return wce;
        }

    }
}
