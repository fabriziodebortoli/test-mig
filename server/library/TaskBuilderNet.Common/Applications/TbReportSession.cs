using System;
using System.Collections;
using System.Globalization;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Generic;
using Microarea.Common.CoreTypes;
using Microarea.Common.NameSolver;
using Microarea.Common.Hotlink;

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
        public ILocalizer Localizer = null;

        public Enums Enums;
        public ApplicationFontStyles ApplicationFontStyles;
        public ApplicationFormatStyles ApplicationFormatStyles;
        //public Functions				Functions;
        public ReferenceObjects Hotlinks;

        private Hashtable cache; // used to store application reportSession values

        private CultureInfo companyCulture = CultureInfo.InvariantCulture;
        public string UICulture;

        private string sNamespace;
        public string Namespace { get { return sNamespace; } set { sNamespace = value; } }

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
                string ext = name.Mid(idx + 1);
                if (ext.CompareNoCase("wrm"))
                {
                    name = name.Left(idx);
                    idx = name.LastIndexOf('.');
                    if (idx < 0) return string.Empty;
                }
                name = name.Mid(idx);
                return name;
            }
        }


        public IPathFinder PathFinder { get { return UserInfo == null ? null : UserInfo.PathFinder; } }
        public string LoggedUser { get { return UserInfo == null ? "" : UserInfo.User; } }
        public string CompanyDbConnection { get { return UserInfo == null ? "" : UserInfo.CompanyDbConnection; } }
        public string Provider { get { return UserInfo == null ? "" : UserInfo.Provider; } }
        public bool IsAuthenticated { get { return UserInfo != null; } }
        public CultureInfo CompanyCulture { get { return companyCulture; } set { companyCulture = value; } }

        public Hashtable Cache
        {
            get
            {
                if (cache == null)
                    cache = new Hashtable(StringComparer.OrdinalIgnoreCase);
                return cache;
            }
        }
        virtual public bool SkipTypeChecking { get { return string.IsNullOrEmpty(CompanyDbConnection); } }

        // Il caricamento di function per le funzioni interne pesa poco e quelle esterne sono caricate on demand
        // gli Hotlinks sono solo caricati on demand e quindi non pesano.
        // Enumerativi, Fonts, Formaters sono invece caricati solo allo start della applicazione
        //-----------------------------------------------------------------------------
        public TbSession(UserInfo UserInfo)
        {
            //solleva l'eccezione per far si che easylook reindirizzi sulla pagina di login
            if (UserInfo == null)
                throw new InvalidSessionException();

            this.UserInfo = UserInfo;
            this.CompanyCulture = UserInfo.CompanyCulture;

            //Functions = new Functions();
            Hotlinks = new ReferenceObjects(this);
        }

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

        //--------------------------------------------------------------------------------
        public virtual ITbLoaderClient GetTBClientInterface()
        {
            return UserInfo.GetTbLoaderInterface();
        }
    }

    /// <summary>
    /// Descrizione di riepilogo per TbSession.
    /// </summary>
    ///=============================================================================
    public class TbReportSession : TbSession
    {
        private string reportParameters;

        public int PageRendered = -1;
        public bool StoppedByUser = false;

        public bool XmlReport = false;
        public bool EInvoice = false;
        public bool WriteNotValidField = false;
        public string ReportParameters { get { return reportParameters; } set { reportParameters = value; } }
  
  
        public TbReportSession(UserInfo UserInfo)
            : base(UserInfo)
        {

        }
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

