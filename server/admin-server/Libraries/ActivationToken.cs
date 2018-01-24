using System;
using System.Collections.Generic;
using System.Xml;
using System.Collections;

namespace Microarea.AdminServer.Libraries
{
    public class ActivationToken
    {
        List<ProductInfo> Products = new List<ProductInfo> { };
        // string MACAddress;
        string key;
        string /*DateTime*/ ExpiryDate;
        String Edition;
        String Database;
        String Country;
        String PurchaseId;
        string /*DateTime*/ MLUExpiryDate;
        SpecialMode ModeType = SpecialMode.NONE;
        Extension ExtensionType = Extension.NONE;
        enum SpecialMode { DVLP, RNFS, DEMO, NONE };
        enum Extension { TEST, BACKUP, STANDALONE, NONE };
        public List<string> ModulesNameSpace = new List<string> { };
        //chiave di attivazione contente tutte le informazioni di moduli attivati , cal e userinfo
        //ricevuta dl backend microarea e criptata con il macaddress.
        public ActivationToken(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;
            this.key= key;
            //MACAddress = LocalMachine.GetMacAddress();
            //string mykey = CrypterManager.Decrypt(key);
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(key);//lara
            ExpiryDate = xmldoc.DocumentElement.GetAttribute("validto");
            MLUExpiryDate = xmldoc.DocumentElement.GetAttribute("mluexpiry");
            Enum.TryParse(xmldoc.DocumentElement.GetAttribute("extension"), out ExtensionType);
            Enum.TryParse(xmldoc.DocumentElement.GetAttribute("mode"), out ModeType);
            PurchaseId = xmldoc.DocumentElement.GetAttribute("purchaseid");
            Country = xmldoc.DocumentElement.GetAttribute("country");
            Database = xmldoc.DocumentElement.GetAttribute("db");
            Edition = xmldoc.DocumentElement.GetAttribute("edition");

            XmlNodeList pList = xmldoc.GetElementsByTagName("Product");
            if (pList == null || pList.Count == 0) return;

            foreach (XmlElement prod in pList)
            {
                ProductInfo product = new ProductInfo(prod);
                Products.Add(product);
                ModulesNameSpace.AddRange(product.ModulesNameSpace);
            }


        }

       
        //---------------------------------------------------------------------
        public void Login()
        {
            int loginResult = authenticationSlots.Login(/*loginId, userName, macIp, companyId, companyName, askingProcess, webLogin, gdiLogin, concurrent, overWriteLogin, out authenticationToken*/);

        }

        AuthenticationSlots authenticationSlots = null;
        //private object v;

        //---------------------------------------------------------------------
        public void CreateSlots()
        {
            authenticationSlots = AuthenticationSlots.Create(this);
            int slotsRes = authenticationSlots.Init(this, null /*SysDBConnection*/);

        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            //per ora torna la roba vecchia; poi dovrà serializzare l oggetto in memoria
            return key;
        }
  


        //---------------------------------------------------------------------
        public bool IsEnterprise()
        {
            return String.Compare(Edition, "e", true) == 0;

        }

        //---------------------------------------------------------------------
        public bool IsActivated(string moduleNameSpace)
        {
            return ModulesNameSpace.Contains(moduleNameSpace.ToLower());

        }

        //---------------------------------------------------------------------
        public bool IsCalAvailable(string moduleNameSpace, string authenticationToken)
        {
            
            //se mi è stato passato un token di autenticazione, verifico che sia
            //associato ad utente loginato.
            int loginId = -1;
            int companyId = -1;
            bool unnamedCal = false;
            if (
                authenticationToken != string.Empty &&
                !GetAuthenticationInformations(authenticationToken, out loginId, out companyId, out unnamedCal)
                )
                return false;

            ArticleInfo ai = null;

            try
            {
                ai = GetArticleByFunctionality(moduleNameSpace);
            }
            catch (Exception /*err*/)
            {
                // diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsCalAvailable: " + err.Message);
                return false;
            }

            if (ai == null)
                return false;

            //se l'articolo non ha cal non devo associarlo all'utente, basta
            //la presenza per validare la richiesta
            if (!ai.HasCal())
                return true;


            if (authenticationToken == string.Empty || authenticationToken == null)
            {
                //diagnostic.Set
                //    (
                //    DiagnosticType.LogInfo | DiagnosticType.Error,
                //    "IsCalAvailable: " + string.Format(LoginManagerStrings.CalWithoutAuthentication, functionality)
                //    );
                return false;
            }

            //anomalia assegnazione cal ent
            authenticationSlots.licut = moduleNameSpace.EndsWith(".licenzautente");/* (functionality == Microarea.TaskBuilderNet.Licence.Licence.XmlSyntax.Consts.UserLicence);*/


            int res = authenticationSlots.AssignUserToArticle(loginId, authenticationToken, ai);



            if (res != (int)LoginReturnCodes.NoError)
            {
                //string descri = string.Empty;
                //diagnostic.Set
                //	(
                //	DiagnosticType.LogInfo | DiagnosticType.Error, 
                //	"IsCalAvailable: " + string.Format(LoginManagerStrings.ErrAssignUserToArticle, GetLoginName(loginId, out descri), ai.Name)
                //	);
                return false;
            }

            return true;

        }

        //---------------------------------------------------------------------
        private ArticleInfo GetArticleByFunctionality(string moduleNameSpace)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------
        internal bool GetAuthenticationInformations(string authenticationToken, out int loginId, out int companyId, out bool unNamed)
        {
            loginId = -1;
            companyId = -1;
            unNamed = false;

            if (authenticationSlots != null)
            {
                AuthenticationSlot slot = authenticationSlots.GetSlot(authenticationToken);
                if (slot != null)
                {
                    loginId = slot.LoginID;
                    companyId = slot.CompanyID;
                    unNamed = slot.UnNamed;
                    return true;
                }
            }

            return false;
        }



        //---------------------------------------------------------------------
        internal bool GetCalNumber(out int gdiCal, out int gdiConcurrent, out int easyCal, out int mdCal, out int tpCal, out int wmsCal, out int manufacturingCal, out Hashtable tpProducerCal, out Hashtable cALNumberForArticle)
        {
            throw new NotImplementedException();
        }

        //---------------------------------------------------------------------
        internal ArticleInfo GetWmsArticle()
        {
            throw new NotImplementedException();
        }

        //---------------------------------------------------------------------
        internal ArticleInfo GetManufacturingMobileArticle()
        {
            throw new NotImplementedException();
        }

        //---------------------------------------------------------------------
        internal ArticleInfo GetArticleByName(string articleName)
        {
            throw new NotImplementedException();
        }

        //---------------------------------------------------------------------
        internal bool IsDevelopmentIU()
        {
            throw new NotImplementedException();
        }

        //---------------------------------------------------------------------
        internal bool IsSpecial()
        {
            throw new NotImplementedException();
        }

        //---------------------------------------------------------------------
        internal ArticleInfo GetUserLicenceArticle()
        {
            throw new NotImplementedException();
        }
    }

    //---------------------------------------------------------------------
    public class ProductInfo
    {
        public string ProductID;
        public string Name;
        public List<ArticleInfo> Articles = new List<ArticleInfo> { };
        public List<string> ModulesNameSpace = new List<string> { };
        public ProductInfo(XmlElement productNode)
        {
            if (productNode == null) return;

            ProductID = productNode.GetAttribute("id");
            Name = productNode.GetAttribute("name");
            XmlNodeList articlesList = productNode.GetElementsByTagName("Articles");
            if (articlesList == null || articlesList.Count == 0) return;

            foreach (XmlElement art in articlesList)
            {
                ArticleInfo article = new ArticleInfo(art);
                Articles.Add(article);
                ModulesNameSpace.AddRange(article.ModulesNameSpace);
            }
        }
    }

    //---------------------------------------------------------------------
    public class ArticleInfo
    {
        public enum CalType { named, floating, mobile, tp, vertical, none };
        public string Name;
        public int Cal = 0;
        public CalType Caltype = CalType.none;
        public List<ModuleInfo> Modules = new List<ModuleInfo> { };
        public List<string> ModulesNameSpace = new List<string> { };

        public object NamedCalNumber { get; internal set; }

        public ArticleInfo(XmlElement art)
        {
            if (art == null) return;
            Name = art.GetAttribute("name");
            Int32.TryParse(art.GetAttribute("cal"), out Cal);
            string caltypevalue = art.GetAttribute("calType");
            Enum.TryParse(caltypevalue, out Caltype);

            XmlNodeList applist = art.GetElementsByTagName("App");

            foreach (XmlElement app in applist)
            {
                string appname = app.GetAttribute("app");
                XmlNodeList modulesList = app.GetElementsByTagName("mod");
                XmlNodeList funcList = app.GetElementsByTagName("func");
                foreach (XmlElement mod in modulesList)
                {

                    ModuleInfo module = new ModuleInfo(mod, appname);
                    Modules.Add(module);
                    ModulesNameSpace.Add(module.NameSpace);
                }
                foreach (XmlElement func in funcList)
                {

                    ModuleInfo module = new ModuleInfo(func, appname);
                    module.isMod = false;
                    ModulesNameSpace.Add(module.NameSpace);
                }
            }
        }

        internal bool HasCal()
        {
            throw new NotImplementedException();
        }
    }

    //---------------------------------------------------------------------
    public class ModuleInfo
    {
        public string Name;
        public string App;
        public bool isMod = true;
        public string NameSpace { get { return String.Concat(App.ToLower(), ".", Name.ToLower()); } }

        public ModuleInfo(XmlElement module, string app)
        {
            if (module == null) return;
            App = app;
            Name = module.GetAttribute("name");
        }
    }

    ////---------------------------------------------------------------------
    //public static class CrypterManager
    //{
    //    //algoritmo crypt da studiare, supponiamo che ora il macaddress sia scritto dentro la chiave in un punto a caso
    //    //---------------------------------------------------------------------
    //    public static string Crypt(string key)
    //    {
    //        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException("Key not found");
    //        string macadd = LocalMachine.GetMacAddress();
    //        return key.Insert(key.Length / 2, macadd);
    //    }

    //    //---------------------------------------------------------------------
    //    public static string Decrypt(string key)
    //    {
    //        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException("Key not found");
    //        string macadd = LocalMachine.GetMacAddress();
    //        return key.Replace(macadd, "");
    //    }
    //}
}

