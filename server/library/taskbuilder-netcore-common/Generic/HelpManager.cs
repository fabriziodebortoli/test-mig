using Microarea.Common.NameSolver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace Microarea.Common.Generic
{
    public class HelpManager
    {

        private static HelpManager theHelpManager = null;
        private Dictionary<string, string> namespaceAliases = new Dictionary<string, string>();

        private HelpManager()
        {
        }

        private static HelpManager helpManager { get { if (theHelpManager == null) theHelpManager = new HelpManager(); return theHelpManager; } }

        //----------------------------------------------------------------------------------------------
        private static string LoginManagerOnLineHelpUrl
        {
            get
            {
                return string.Format("{0}OnlineHelp.aspx", PathFinder.PathFinderInstance.LoginManagerBaseUrl);
            }
        }

        //----------------------------------------------------------------------------------------------
        private static string EasyLookOnLineHelpUrl
        {
            get
            {
                return string.Format("{0}OnlineHelp.aspx", PathFinder.PathFinderInstance.EasyLookServiceBaseUrl);
            }
        }

        //----------------------------------------------------------------------------------------------
        private static string LoginManagerPrivateAreaUrl
        {
            get
            {
                return string.Format("{0}SitePrivateArea.aspx", PathFinder.PathFinderInstance.LoginManagerBaseUrl);
            }
        }

        /// <summary>
        /// Provides a way to change the namespace that will be used to invoke the help
        /// The alias will be applied to all derived namespaces, i.e., aliasing "My.Old.Root" with "New.Name"
        /// will cause the namespace "My.Old.Root.Child.Grandchild" to be aliased as "New.Name.Child.Grandchild"
        /// </summary>
        /// <param name="nspace">Namespace to be aliased</param>
        /// <param name="alias">Alias to apply</param>
        public static void AddNamespaceAlias(string nspace, string alias)
        {
            if (!helpManager.namespaceAliases.ContainsKey(nspace))
                helpManager.namespaceAliases.Add(nspace, alias);
        }

        /// <summary>
        /// Invoca la pagina di help desiderata
        /// N.B il namespace nel formato document.ERP.CustomerSuppliers.document.Customer verrà
        /// trasformato in RefGuide.ERP.CustomerSuppliers.document.Customer
        /// Utilizzata in c# e c++ per chiamate dirette all'help (non Mago.web);
        /// </summary>
        //----------------------------------------------------------------------------
        public static bool CallOnlineHelp(string strNamespace, string strCulture)
        {
            strNamespace = Regex.Replace(strNamespace, "\\bdocument\\.\\b", "RefGuide.", RegexOptions.IgnoreCase);
            try
            {
                string url = GetOnlineHelpUrl(strNamespace, strCulture);
                Process.Start(url);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Formatta l'Url da chiamare per ottenere la guida in linea 
        /// N.B il namespace nel formato document.ERP.CustomerSuppliers.document.Customer verrà
        /// trasformato in RefGuide.ERP.CustomerSuppliers.document.Customer
        /// Utilizzata da Easylook per invocare l'help da Mago.web
        /// </summary>
        //----------------------------------------------------------------------------
        public static string GetOnlineHelpUrl(string nameSpace, string culture, bool fromEasyLook = false)
        {
            string url = fromEasyLook
                ? EasyLookOnLineHelpUrl
                : LoginManagerOnLineHelpUrl;
            string tempToken = CreateTempToken(url);

            nameSpace = Regex.Replace(nameSpace, "\\bdocument\\.\\b", "RefGuide.", RegexOptions.IgnoreCase);

            return string.Format(
                            "{0}?q={1}&n={2}&lang={3}",
                            url,
                            HttpUtility.UrlEncode(tempToken),
                            HttpUtility.UrlEncode(nameSpace),
                            HttpUtility.UrlEncode(culture)
                            );
        }

        ///// <summary>
        ///// Useful as delegate for the HelpRequested event of forms, control, etc.
        ///// i.e.: form.HelpRequested += new HelpEventHandler(HelpManager.HelpRequested);
        ///// To work, requires that SetOnLineHelpUrl was called to set the base URL of the help provider
        ///// </summary>
        ///// <param name="sender">active object when F1 was pressed</param>
        ///// <param name="hlpevent">additional information: mouse positions, etc.</param>
        ////----------------------------------------------------------------------------
        //public static void HelpRequested(object sender, HelpEventArgs hlpevent)
        //{
        //    string helpPageNamespace = ReplaceAlias(sender.GetType().FullName);
        //    helpPageNamespace = "RefGuide-" + helpPageNamespace.Replace("Microarea.", "");

        //    CallOnlineHelp(helpPageNamespace, Thread.CurrentThread.CurrentUICulture.Name);
        //}

        /// <summary>
        /// Useful to respond to the "help" button on dialogs, i.e., wizard pages
        /// To work, requires that SetOnLineHelpUrl was called to set the base URL of the help provider
        /// </summary>
        /// <param name="sender">active object when the "help" button was pressed</param>
        /// <param name="strNamespace">namespace to be used for calling help (optional, can be empty)</param>
        /// <param name="topicHtmlPage">namespace of the help page to call (optional, can be empty)</param>
        //----------------------------------------------------------------------------
        public static void OnHelpFromDialog(object sender, string strNamespace, string topicHtmlPage)
        {
            string helpPageNamespace;

            if (topicHtmlPage != string.Empty)
                helpPageNamespace = topicHtmlPage;
            else if (strNamespace != string.Empty)
                helpPageNamespace = strNamespace;
            else
                helpPageNamespace = ReplaceAlias(sender.GetType().FullName);

            helpPageNamespace = helpPageNamespace.Replace("Microarea.", "");
            if (!helpPageNamespace.StartsWith("RefGuide-"))
                helpPageNamespace = "RefGuide-" + helpPageNamespace;

            CallOnlineHelp(helpPageNamespace, Thread.CurrentThread.CurrentUICulture.Name);
        }

        //----------------------------------------------------------------------------
        private static string ReplaceAlias(string nspace)
        {
            foreach (KeyValuePair<string, string> de in helpManager.namespaceAliases)
                if (nspace.StartsWith(de.Key.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    nspace = nspace.ToLower().Replace(de.Key, de.Value);
                    break;
                }
            return nspace;
        }

        /// <summary>
		/// Crea un token temporaneo utilizzato in LoginManager per verificare che la chiamata 
		/// provenga da Mago e non da una chiamata web diretta (utilizzata per SingleSignOn)
		/// </summary>
        //----------------------------------------------------------------------------
        public static string CreateTempToken(string url)
        {
            string token = Guid.NewGuid().ToString();

            try
            {
                string firstUrl = string.Format("{0}?init={1}", url, token);

                //invia alla pagina di onlineHelp per prima cosa un token di "controllo", poi invia
                //la chiamata vera e propria che "consuma" il token e apre l'help vero e proprio
                //using (WebClient cli = new WebClient())
                //{
                //    using (Stream stream = cli.OpenRead(firstUrl)) { }
                //}
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("init",token)
                });

                using (HttpClient cli = new HttpClient())
                {
                    HttpResponseMessage response = cli.PostAsync(firstUrl, content).Result;
                }
            }
            catch
            {
                return string.Empty;
            }

            return token;
        }

        /// <summary>
        /// 
        /// </summary>
        //--------------------------------------------------------------------------------------------------------------------------------
        public static string GetProducerSiteURL()
        {
            return InstallationData.BrandLoader.GetBrandedStringBySourceString("ProducerSite");
        }

        /// <summary>
        /// 
        /// </summary>
        //--------------------------------------------------------------------------------------------------------------------------------
        public static void ConnectToProducerSite()
        {
            string producerSiteURL = GetProducerSiteURL();
            if (!string.IsNullOrWhiteSpace(producerSiteURL))
                Process.Start(producerSiteURL);
        }

        /// <summary>
        /// 
        /// </summary>
        //---------------------------------------------------------------------------
        public static string GetProducerSiteLoginPage()
        {
            return InstallationData.BrandLoader.GetBrandedStringBySourceString("ProducerSiteLoginPage");
        }

        ///// <summary>
        /////
        ///// </summary>
        ////--------------------------------------------------------------------------------------------------------------------------------
        //public static string GetProducerSitePrivateAreaLinkWithToken(string authToken)
        //{
        //    string baseAddress = GetProducerSiteLoginPage();
        //    string token = GenerateSSOToken(authToken);

        //    if (String.IsNullOrWhiteSpace(token))
        //    {
        //        return baseAddress;
        //    }

        //    return FormatSSOLink(baseAddress, token);
        //}


        ///// <summary>
        /////
        ///// </summary>
        ////--------------------------------------------------------------------------------------------------------------------------------
        //public static void ConnectToProducerSitePrivateArea(string authToken)
        //{
        //    try
        //    {
        //        ConnectToSSOLink("http://www.microarea.it/MyAccount/MyUserProfile.aspx", authToken);
        //    }
        //    catch { }
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ////--------------------------------------------------------------------------------------------------------------------------------
        //public static void ConnectToProducerSiteLoginPage(string authToken)
        //{
        //    try
        //    {
        //        ConnectToSSOLink(GetProducerSiteLoginPage(), authToken);
        //    }
        //    catch { }
        //}

        ////--------------------------------------------------------------------------------------------------------------------------------
        //public static void ConnectToSSOLink(string pageLink, string authToken)
        //{
        //    if (string.IsNullOrWhiteSpace(pageLink))
        //    {
        //        ConnectToProducerSite();
        //        return;
        //    }

        //    string token = GenerateSSOToken(authToken);

        //    if (String.IsNullOrWhiteSpace(token))
        //    {
        //        Process.Start(pageLink);
        //        return;
        //    }

        //    Process.Start(FormatSSOLink(pageLink, token));
        //}

        //--------------------------------------------------------------------------------------------------------------------------------
        private static string FormatSSOLink(string pageLink, string token)
        {
            string formatvalue = "{0}{1}token={2}";
            int i = pageLink.IndexOf("?");
            string toconcat = ((i == -1) ? "?" : "");
            return String.Format(formatvalue, pageLink, toconcat, token);
        }

        ////--------------------------------------------------------------------------------------------------------------------------------
        //private static string GenerateSSOToken(string authToken)
        //{
        //    string token = null;

        //    if (string.IsNullOrWhiteSpace(authToken))
        //        return token;


        //    bool b = false;
        //    string userinfoid = null;
        //    bool isAdmin = false;
        //    LoginManager loginMng = new LoginManager();
        //    try
        //    {
        //        b = loginMng.GetLoginInformation(authToken);
        //        userinfoid = loginMng.GetUserInfoID();
        //        isAdmin = loginMng.UserCanAccessWebSitePrivateArea();
        //    }
        //    catch
        //    {

        //        return token;
        //    }
        //    if (!b)
        //    {
        //        return token;
        //    }

        //    //Se nell'url è indicato un token, provo a valorizzarlo chiedendone la generazione ai server microarea.
        //    if (!loginMng.UserName.IsNullOrEmpty() && !userinfoid.IsNullOrEmpty())
        //    {

        //        try
        //        {
        //            string loginInfo = String.Format("{0}*{1}", userinfoid, loginMng.UserName);
        //            loginInfo = Crypto.Encrypt(loginInfo);

        //            using (MicroareaBackend.Registration microareaBackend = new MicroareaBackend.Registration())
        //            {
        //                token = microareaBackend.GenerateSSOToken(
        //                    loginInfo,
        //                    isAdmin
        //                    );
        //            }
        //        }
        //        catch { token = null; }

        //    }

        //    return token;
        //}

        ///// <summary>
        ///// Formatta l'Url da chiamare per ottenere tramite la pagina di loginManager SitePrivateArea.aspx 
        ///// la pagina di amministrazione dell'area riservata sul sito microarea
        ///// </summary>
        ////----------------------------------------------------------------------------
        //public static string GetUserProfileUrl()
        //{
        //    return string.Format(
        //                    "{0}?u={1}",
        //                    LoginManagerPrivateAreaUrl,
        //                    HttpUtility.UrlEncode(CreateTempToken(LoginManagerPrivateAreaUrl))
        //                    );
        //}
    }




}
