using System;
using System.Collections;
using System.Globalization;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Generic;
using Microarea.Common.CoreTypes;
using Microarea.Common.NameSolver;
using Microarea.Common.Hotlink;
using static Microarea.Common.Generic.InstallationInfo;
using Microarea.Common.StringLoader;
using System.Xml;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.WebSockets;
using System.ServiceModel.Channels;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using Microarea.Common.Applications;

namespace Microarea.Common.Applications
{
    /// <summary>
    /// Eccezione sollevata quando la session non e' piu valida
    /// </summary>
    ///=============================================================================
    public class InvalidSessionException : Exception
    {

    }


    /// <summary>
    /// Descrizione di riepilogo per TbSession.
    /// </summary>
    ///=============================================================================
    public class TbSession
    {
        public UserInfo UserInfo = null;

        public string TbBaseAddress = "http://localhost:5000/";

        public const string TbBaseRoute = "tbloader/api/";
        public const string TbLoginRoute = "tb/document/login/";
        public const string TbRunFunctionRoute = "tb/document/runFunction/";

        public const string TbInstanceKey = "tbloader-name";
        public string TbInstanceID = string.Empty;
        public bool LoggedToTb = false;

        public WebSocket WebSocket = null;

        public IPathFinder PathFinder = null;

        private string sNamespace;
        public string Namespace { get { return sNamespace; } set { sNamespace = value; } }

        public ILocalizer Localizer = null;

        public Enums Enums = null;
        public ApplicationFontStyles ApplicationFontStyles = null;
        public ApplicationFormatStyles ApplicationFormatStyles = null;
        public ReferenceObjectsList Hotlinks = null;

        private string filePath;
        public string FilePath { get { return filePath; } set { filePath = value; } }

        //sono ustate da expression per le funzioni interne
        public string ReportPath { get { return FilePath; } set { FilePath = value; } }
        public string ReportNamespace { get { return Namespace; } set { Namespace = value; } }

        public string ReportName
        {
            get
            {
                string name = Namespace;
                int idx = name.LastIndexOf('.');
                if (idx < 0) return string.Empty;

                string ext = name.Mid(idx);
                if (ext.CompareNoCase(".wrm"))
                {
                    name = name.Left(idx);
                    idx = name.LastIndexOf('.');
                    if (idx < 0) return string.Empty;
                }
                name = name.Mid(idx + 1);
                return name;
            }
        }

        //---------------------------------------------------------------------
        private DateTime applicationDate = DateTime.Today;
        public DateTime ApplicationDate
        {
            get { return applicationDate; }
            set { applicationDate = value; }
        }

        public string CompanyDbConnection { get { return UserInfo == null ? "" : UserInfo.CompanyDbConnection; } }

        public TbSession(UserInfo ui, string ns)
        {
            //solleva l'eccezione per far si che easylook reindirizzi sulla pagina di login
            if (ui == null)
                throw new InvalidSessionException();

            this.UserInfo = ui;
            this.Namespace = ns;
            this.PathFinder = new PathFinder(ui.Company, ui.ImpersonatedUser);

            if (!LoadSessionInfo(null, false))
                throw new InvalidSessionException();
            ;
        }


        //private Hashtable cache; // used to store application reportSession values
        //public Hashtable Cache
        //{
        //    get
        //    {
        //        if (cache == null)
        //            cache = new Hashtable(StringComparer.OrdinalIgnoreCase);
        //        return cache;
        //    }
        //}

        virtual public bool SkipTypeChecking { get { return string.IsNullOrEmpty(CompanyDbConnection); } }

        // Il caricamento di function per le funzioni interne pesa poco e quelle esterne sono caricate on demand
        // gli Hotlinks sono solo caricati on demand e quindi non pesano.
        // Enumerativi, Fonts, Formaters sono invece caricati solo allo start della applicazione
        //-----------------------------------------------------------------------------
        public FunctionsList Functions
        {
            get
            {
                //TODO RSWEB
                return BasePathFinder.BasePathFinderInstance.WebMethods;

            }
        }

        //-----------------------------------------------------------------------------
        public bool LoadSessionInfo()
        {
            return LoadSessionInfo(TBWebContext.Current);
        }

        //-----------------------------------------------------------------------------
        public bool LoadSessionInfo(IApplicationBag applicationBag)
        {
            return LoadSessionInfo(applicationBag, true);
        }

        //-----------------------------------------------------------------------------
        // Carica Enumerativi, Fonts, Formaters, Functions and ReferencesObjects-Hotlinks
        // Il caricamento di function per le funzioni interne pesa poco e quelle esterne sono caricate on demand
        // gli Hotlinks sono solo caricati on demand e quindi non pesano.
        public bool LoadSessionInfo(bool checkActivation)
        {
            return LoadSessionInfo(null, checkActivation);
        }
        //-----------------------------------------------------------------------------
        public bool LoadSessionInfo(IApplicationBag applicationBag, bool checkActivation)
        {
            // se non sono autenticato non posso caricare nulla e segnalo errore
            if (UserInfo == null)
                return false;

            // per ottimizzare il caricamento da file uso la bag della applicazione.
            // devo sempre riassegnare la ReportSession perchè potrebbe appartenere ad autenticazioni 
            // diverse a ciascun Run mentre l'applicazione è sempre la stessa
            // ITRI mettere la dipendenza dai file caricati
            if (applicationBag == null)
            {
                //Tutto nuovo
                //Istanzio
                Enums = new Enums();
                ApplicationFontStyles = new ApplicationFontStyles(this);
                ApplicationFormatStyles = new ApplicationFormatStyles(this);

                //Leggo gli enumerativi
                Enums.LoadXml(checkActivation);
                //Load dei font
                ApplicationFontStyles.Load();
                //Load dei format
                ApplicationFormatStyles.Load();

                Hotlinks = new ReferenceObjectsList(this);
            }
            else
            {
                //Prendo gli enumerativi in Application
                Enums enums = (Enums)applicationBag[ApplicationKey.Enums];
                if (enums != null)
                {
                    //Me lo associo
                    Enums = enums;
                }
                else
                {
                    //Me li ricarico
                    Enums = new Enums();
                    Enums.LoadXml(checkActivation);
                    applicationBag[ApplicationKey.Enums] = Enums;
                }

                //Prendo i font in Application perr quell'Utente
                string fontName = ApplicationKey.Fonts + UserInfo.Company;
                ApplicationFontStyles fonts = (ApplicationFontStyles)applicationBag[fontName];
                if (fonts != null)
                {
                    //Me li associo
                    ApplicationFontStyles = fonts;
                    ApplicationFontStyles.ReportSession = this;
                }
                else
                {
                    //Me li ricarico
                    ApplicationFontStyles = new ApplicationFontStyles(this);
                    ApplicationFontStyles.Load();
                    applicationBag[fontName] = ApplicationFontStyles;
                }

                //Prendo i format in Application per quell'utente
                string formatName = ApplicationKey.Formats + UserInfo.Company;
                ApplicationFormatStyles formats = (ApplicationFormatStyles)applicationBag[formatName];
                if (formats != null)
                {
                    //Me li associo
                    formats.RestoreFromLocale();
                    ApplicationFormatStyles = formats;
                    ApplicationFormatStyles.ReportSession = this;
                    formats.SetToLocale();
                }
                else
                {
                    //Me li ricarico
                    ApplicationFormatStyles = new ApplicationFormatStyles(this);
                    ApplicationFormatStyles.Load();
                    applicationBag[formatName] = ApplicationFormatStyles;
                }
            }

            return Enums.Loaded && ApplicationFontStyles.Loaded && ApplicationFormatStyles.Loaded;
        }

        //---------------------------------------------------------------------
        public static async Task<bool> TbLogin(TbSession session)
        {
            if (session.LoggedToTb)
                return true;

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(session.TbBaseAddress);

                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>(UserInfo.AuthenticationTokenKey, session.UserInfo.AuthenticationToken)
                    });
                    var response = await client.PostAsync(TbSession.TbBaseRoute + TbSession.TbLoginRoute, content);
                    response.EnsureSuccessStatusCode(); // Throw in not success

                    var stringResponse = await response.Content.ReadAsStringAsync();

                    if (stringResponse != null)
                    {
                        IEnumerable<string> list;
                        if (response.Headers.TryGetValues("Set-Cookie", out list))
                        {
                            if (list != null)
                            {
                                foreach (string s in list)
                                {
                                    if (s.Left(TbSession.TbInstanceKey.Length).CompareNoCase(TbSession.TbInstanceKey))
                                    {
                                        string tbinstance = s.Mid(TbSession.TbInstanceKey.Length + 1);
                                        int end = tbinstance.IndexOf(';');
                                        tbinstance = tbinstance.Left(end);

                                        session.TbInstanceID = tbinstance;
                                        session.LoggedToTb = true;
                                        break;
                                    }
                                }
                            }
                        }
                        // List<string> values; //= new List<string> ();
                        //IEnumerable <string> list;
                        //session.LoggedToTb = response.Headers.TryGetValues(TbSession.TbLoaderCookie, out list);

                        //IEnumerable<string> list2 = response.Headers.GetEnumerator();
                        //if (list != null)
                        //{
                        //    //session.TbName = s;
                        //}
                    }
                    return session.LoggedToTb;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request exception: {e.Message}");
                    return false;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Request exception: {e.Message}");
                    return false;
                }
            }
        }

        //-------------------------------------------------------------------------------------------------
        public static async Task<string> TbRunFunction(TbSession session, FunctionPrototype fun)
        {
            if (!session.LoggedToTb)
                return null;
            if (session.TbInstanceID.IsNullOrEmpty())
                return null;

            XmlDocument d = new XmlDocument();
            d.AppendChild(d.CreateElement(WebMethodsXML.Element.Arguments));
            fun.Parameters.Unparse(d.DocumentElement);
            string xargs = d.OuterXml;

            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler))
            {
                try
                {
                    client.BaseAddress = new Uri(session.TbBaseAddress);

                    cookieContainer.Add(client.BaseAddress, new Cookie(TbSession.TbInstanceKey, session.TbInstanceID));

                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>(UserInfo.AuthenticationTokenKey, session.UserInfo.AuthenticationToken),
                        new KeyValuePair<string, string>("ns", fun.NameSpace.ToString() ),
                        new KeyValuePair<string, string>("args", xargs)
                    });

                    var response = await client.PostAsync(TbSession.TbBaseRoute + TbSession.TbRunFunctionRoute, content);
                    response.EnsureSuccessStatusCode(); // Throw in not success

                    var stringResponse = await response.Content.ReadAsStringAsync();

                    return stringResponse;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request exception: {e.Message}");
                    return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Request exception: {e.Message}");
                    return null;
                }
            }
        }

        public static async Task<bool> IsActivated(TbSession session, string app, string fun)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(session.TbBaseAddress);

                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("application", app),
                       new KeyValuePair<string, string>("functionality", fun)
                    });

                    var response = await client.PostAsync("account-manager/isActivated/", content);
                    response.EnsureSuccessStatusCode(); // Throw in not success

                    var stringResponse = await response.Content.ReadAsStringAsync();

                    //TODO RSWEB decodificare meglio result value
                    return stringResponse.IndexOf("true") > 0; 
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request exception: {e.Message}");
                    return false;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Request exception: {e.Message}");
                    return false;
                }
            }
        }

        //-------------------------------------------------------------------------------------------------
        public class MyMessage
        {
            public int commandType = 7;     //commandtype.RUNREPORT
            public string message { get; set; }

            public string page = string.Empty;

        }

        public static async Task<bool> RunReport(TbSession session, FunctionPrototype fun)
        {
            if (session.WebSocket == null)
                return false;

            XmlDocument d = new XmlDocument();
            d.AppendChild(d.CreateElement(WebMethodsXML.Element.Arguments));
            fun.Parameters.Unparse(d.DocumentElement);
            string xargs = d.OuterXml;

            MyMessage msg = new MyMessage();
            msg.message = '{' + fun.NameSpace.ToString().ToJson("ns") + ',' + xargs.ToJson("args", false, false) + '}';

            string jmsg = JsonConvert.SerializeObject(msg);

            if (session.WebSocket.State == WebSocketState.Open)
            {
                await session.WebSocket.SendAsync(new ArraySegment<Byte>(Encoding.UTF8.GetBytes(jmsg)), WebSocketMessageType.Text, true, CancellationToken.None);
            }

            return true;
        }

    }

    /// <summary>
    /// Descrizione di riepilogo per TbReportSession.
    /// </summary>
    ///=============================================================================

    public enum EngineType { Paginated_Standard, FullXML_OfficeXML, PDFSharp_OfficePDF, FullExtraction }

    public class TbReportSession : TbSession
    {
        NameSpace ReportNameSpace = null;

        public int PageRendered = -1;
        public bool StoppedByUser = false;

        public bool XmlReport = false;
        public bool EInvoice = false;
        public bool WriteNotValidField = false;

        public EngineType EngineType = EngineType.Paginated_Standard;

        private string reportParameters;
        public string ReportParameters
        {
            get { return reportParameters; }
            set
            {
                reportParameters = value;
                if (!reportParameters.IsNullOrEmpty())
                {
                    XmlDomParameters = new XmlDocument();
                    XmlDomParameters.LoadXml(reportParameters);
                }
                if (XmlReport)
                    ReportNamespace = XmlDomParameters.DocumentElement.GetAttribute(XmlWriterTokens.Attribute.TbNamespace);
            }
        }
        public XmlDocument XmlDomParameters = null;

        public bool UseApproximation = true; // enable TaskBuilder Approximation for real
        public bool StripTrailingSpaces = true;

        public TbReportSession(UserInfo ui, string ns, string parameters = "")
            : base(ui, ns)
        {
            this.ReportNameSpace = new NameSpace(ns, NameSpaceObjectType.Report);
            this.ReportPath = PathFinder.GetCustomUserReportFile(ui.Company, ui.ImpersonatedUser, ReportNameSpace, true);
            this.ReportParameters = parameters;

            this.Localizer = new StringLoader.WoormLocalizer(this.ReportPath, PathFinder);

            //TODO RSWEB
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(StateMachine.ReportSession.UICulture);
        }


        //---------------------------------------------------------------------
        // private IBrandLoader BrandLoader = new BrandLoader();

    }

    //=========================================================================
    public class ApplicationKey
    {
        public static string Enums = "Enums";
        public static string Fonts = "Fonts";
        public static string Formats = "Formats";
    }

    public interface IApplicationBag
    {
        object this[string name] { get; set; }
    }

}

