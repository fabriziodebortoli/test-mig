using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

using Microarea.Common.Generic;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.NameSolver
{
    /// <summary>
    /// Dati di configurazione del server
    /// </summary>
    //=========================================================================
    public sealed class ServerConnectionInfo : IServerConnectionInfo
    {
        public const int DefaultWebServicesTimeOut = 10000000;  //2 ore e 40 circa

        private string serverConnectionFile = string.Empty;
        private int passwordDuration = 90;
        private int minPasswordLength = 0;
        private int webServicesPort = 80;
        private int webServicesTimeOut = DefaultWebServicesTimeOut;
        private int maxTBLoader = 30;
        private int maxLoginPerTBLoader = 20;
        private int maxLoginFailed = 5;
        private int wMSCalPurgeMinutes = 6;
        private bool useStrongPwd = false;
        private bool useAutologin = true;
        private decimal minDBSizeToWarn = 2044723; // 1.95GB espressi in KB
        private int tbLoaderTimeOut = -1;
        private int tbWCFDefaultTimeout = 1;
        private int tbWCFDataTransferTimeout = 20;
        private bool enableLMVerboseLog = false;    // log verboso LoginManager
        private bool enableEAVerboseLog = false;    // log verboso EasyAttachmentSync
        private string masterSolutionName = string.Empty;
        private bool sendPingMail = false;
        private bool checkVATNr = true;
        private string pingMailRecipient = string.Empty;

        private string preferredLanguage = string.Empty;
        private string applicationLanguage = string.Empty;

        private string smtpRelayServerName = string.Empty;
        private bool smtpUseDefaultCredentials = false;
        private bool smtpUseSSL = false;
        private Int32 smtpPort = 25; // the port number on the SMTP host. The default value is 25
        private string smtpUserName = string.Empty;
        private string smtpPassword = string.Empty;
        private string smtpDomain = string.Empty;
        private string smtpFromAddress = "Scheduler@TaskBuilder.Net";

        private string sysDBConnectionString = string.Empty;

        //---------------------------------------------------------------------------
        public string ServerConnectionFile { get { return serverConnectionFile; } set { serverConnectionFile = value; } }
        public int PasswordDuration { get { return passwordDuration; } set { passwordDuration = value; } }
        public int MinPasswordLength { get { return minPasswordLength; } set { minPasswordLength = value; } }
        public int WebServicesPort { get { return webServicesPort; } set { webServicesPort = value; } }
        public int WebServicesTimeOut { get { return webServicesTimeOut; } set { webServicesTimeOut = value; } }
        public int MaxTBLoader { get { return maxTBLoader; } set { maxTBLoader = value; } }
        public int MaxLoginFailed { get { return maxLoginFailed; } set { maxLoginFailed = value; } }
        public bool UseStrongPwd { get { return useStrongPwd; } set { useStrongPwd = value; } }
        public bool UseAutoLogin { get { return useAutologin; } set { useAutologin = value; } }
        public string PreferredLanguage { get { return preferredLanguage; } set { preferredLanguage = value; } }
        public string ApplicationLanguage { get { return applicationLanguage; } set { applicationLanguage = value; } }
        public string SMTPRelayServerName { get { return smtpRelayServerName; } set { smtpRelayServerName = value; } }
        public bool SMTPUseDefaultCredentials { get { return smtpUseDefaultCredentials; } set { smtpUseDefaultCredentials = value; } }
        public bool SMTPUseSSL { get { return smtpUseSSL; } set { smtpUseSSL = value; } }
        public Int32 SMTPPort { get { return smtpPort; } set { smtpPort = value; } }
        public string SMTPUserName { get { return smtpUserName; } set { smtpUserName = value; } }
        public string SMTPPassword { get { return smtpPassword; } set { smtpPassword = value; } }
        public string SMTPDomain { get { return smtpDomain; } set { smtpDomain = value; } }
        public string SMTPFromAddress { get { return smtpFromAddress; } set { smtpFromAddress = value; } }
        public int MaxLoginPerTBLoader { get { return maxLoginPerTBLoader; } set { maxLoginPerTBLoader = value; } }
        public int TBLoaderTimeOut { get { return tbLoaderTimeOut; } set { tbLoaderTimeOut = value; } }
        public int TbWCFDefaultTimeout { get { return tbWCFDefaultTimeout; } set { tbWCFDefaultTimeout = value; } }
        public int TbWCFDataTransferTimeout { get { return tbWCFDataTransferTimeout; } set { tbWCFDataTransferTimeout = value; } }
        public decimal MinDBSizeToWarn { get { return minDBSizeToWarn; } set { minDBSizeToWarn = value; } }
        public bool EnableLMVerboseLog { get { return enableLMVerboseLog; } set { enableLMVerboseLog = value; } }
        public bool EnableEAVerboseLog { get { return enableEAVerboseLog; } set { enableEAVerboseLog = value; } }
        public Int32 WMSCalPurgeMinutes { get { return wMSCalPurgeMinutes; } set { wMSCalPurgeMinutes = value; } }
        public bool SendPingMail { get { return sendPingMail; } set { sendPingMail = value; } }
        public bool CheckVATNr { get { return checkVATNr; } set { checkVATNr = value; } }
        public string PingMailRecipient { get { return pingMailRecipient; } set { pingMailRecipient = value; } }
        public string MasterSolutionName { get { return masterSolutionName; } set { masterSolutionName = value; } }
        //---------------------------------------------------------------------------
        public string SysDBConnectionString
        {
            get
            {
                if (sysDBConnectionString == string.Empty)
                    return string.Empty;

                return AesCrypto.DecryptString(sysDBConnectionString);
            }
            set
            {
                sysDBConnectionString = AesCrypto.EncryptString(value);
            }
        }

        /// <summary>
        /// Costruttore
        /// </summary>
        //-----------------------------------------------------------------------
        public ServerConnectionInfo()
        {
        }

        /// <summary>
        /// Legge il file ServerConnection.config
        /// </summary>
        /// <returns>successo della lettura</returns>
        //-----------------------------------------------------------------------
        public bool Parse(string aServerConnectionFile)
        {
            ServerConnectionFile = aServerConnectionFile;
            if (!PathFinder.PathFinderInstance.ExistFile(aServerConnectionFile))
            {
                Debug.WriteLine(string.Format("File {0} does not exist!", aServerConnectionFile));
                return false;
            }

            XmlDocument ServerConnectionDocument = null;

            try
            {
                ServerConnectionDocument = PathFinder.PathFinderInstance.LoadXmlDocument(ServerConnectionDocument, ServerConnectionFile);

                XmlElement root = ServerConnectionDocument.DocumentElement;
                if (root == null)
                {
                    Debug.Fail("Error in ServerConnectionInfo.Parse");
                    return false;
                }

                XmlNodeList sysDBConnElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.SysDBConnectionString);
                if (sysDBConnElements != null && sysDBConnElements.Count == 1)
                    sysDBConnectionString = ((XmlElement)sysDBConnElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value);

                XmlNodeList PreferredLanguageElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.PreferredLanguage);
                if (PreferredLanguageElements != null && PreferredLanguageElements.Count == 1)
                    PreferredLanguage = ((XmlElement)PreferredLanguageElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value);

                if (string.Compare(PreferredLanguage, "Native", StringComparison.OrdinalIgnoreCase) == 0)
                    PreferredLanguage = NameSolverStrings.DefaultLanguage;

                XmlNodeList ApplicationLanguageElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.ApplicationLanguage);
                if (ApplicationLanguageElements != null && ApplicationLanguageElements.Count == 1)
                    ApplicationLanguage = ((XmlElement)ApplicationLanguageElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value);

                if (string.Compare(ApplicationLanguage, "Native", StringComparison.OrdinalIgnoreCase) == 0)
                    ApplicationLanguage = NameSolverStrings.DefaultLanguage;

                XmlNodeList MinPasswordLengthElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.MinPasswordLength);
                if (MinPasswordLengthElements != null && MinPasswordLengthElements.Count == 1)
                {
                    try
                    {
                        MinPasswordLength = Int32.Parse(((XmlElement)MinPasswordLengthElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value));
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }

                XmlNodeList PasswordDurationElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.PasswordDuration);
                if (PasswordDurationElements != null && PasswordDurationElements.Count == 1)
                {
                    try
                    {
                        PasswordDuration = Int32.Parse(((XmlElement)PasswordDurationElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value));
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }

                XmlNodeList WebServicesPortElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.WebServicesPort);
                if (WebServicesPortElements != null && WebServicesPortElements.Count == 1)
                {
                    try
                    {
                        WebServicesPort = Int32.Parse(((XmlElement)WebServicesPortElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value));
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }

                XmlNodeList WebServicesTimeoutElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.WebServicesTimeOut);
                if (WebServicesTimeoutElements != null && WebServicesTimeoutElements.Count == 1)
                {
                    try
                    {
                        WebServicesTimeOut = Int32.Parse(((XmlElement)WebServicesTimeoutElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value));
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }

                //MaxTBLoader
                XmlNodeList MaxTBLoaderElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.MaxTBLoader);
                if (MaxTBLoaderElements != null && MaxTBLoaderElements.Count == 1)
                {
                    try
                    {
                        MaxTBLoader = Int32.Parse(((XmlElement)MaxTBLoaderElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value));
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }
                //MaxLoginPerTBLoader
                XmlNodeList MaxLoginPerTBLoaderElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.MaxLoginPerTBLoader);
                if (MaxLoginPerTBLoaderElements != null && MaxLoginPerTBLoaderElements.Count == 1)
                {
                    try
                    {
                        MaxLoginPerTBLoader = Int32.Parse(((XmlElement)MaxLoginPerTBLoaderElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value));
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }
                //TBLoaderTimeout
                XmlNodeList TBLoaderTimeoutElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.TBLoaderTimeOut);
                if (TBLoaderTimeoutElements != null && TBLoaderTimeoutElements.Count == 1)
                {
                    try
                    {
                        Int32.TryParse(((XmlElement)TBLoaderTimeoutElements[0]).InnerText, out tbLoaderTimeOut);
                        if (tbLoaderTimeOut < -1)
                            tbLoaderTimeOut = -1;

                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }

                //TbWCFDefaultTimeout
                XmlNodeList TbWCFDefaultTimeoutElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.TbWCFDefaultTimeout);
                if (TbWCFDefaultTimeoutElements != null && TbWCFDefaultTimeoutElements.Count == 1)
                {
                    try
                    {
                        Int32.TryParse(((XmlElement)TbWCFDefaultTimeoutElements[0]).InnerText, out tbWCFDefaultTimeout);
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }

                //TbWCFDefaultTimeout
                XmlNodeList TbWCFDataTransferTimeoutElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.TbWCFDataTransferTimeout);
                if (TbWCFDataTransferTimeoutElements != null && TbWCFDataTransferTimeoutElements.Count == 1)
                {
                    try
                    {
                        Int32.TryParse(((XmlElement)TbWCFDataTransferTimeoutElements[0]).InnerText, out tbWCFDataTransferTimeout);
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }

                //MaxLoginFailed
                XmlNodeList MaxLoginFailedElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.MaxLoginFailed);
                if (MaxLoginFailedElements != null && MaxLoginFailedElements.Count == 1)
                {
                    try
                    {
                        MaxLoginFailed = Int32.Parse(((XmlElement)MaxLoginFailedElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value));
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }
                //UseStrongPwd
                XmlNodeList UseStrongPwdElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.UseStrongPwd);
                if (UseStrongPwdElements != null && UseStrongPwdElements.Count == 1)
                {
                    try
                    {
                        UseStrongPwd = bool.Parse(((XmlElement)UseStrongPwdElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value));
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }

                //UseAutoLogin
                XmlNodeList UseAutologinElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.UseAutologin);
                if (UseAutologinElements != null && UseAutologinElements.Count == 1)
                {
                    try
                    {
                        UseAutoLogin = bool.Parse(((XmlElement)UseAutologinElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value));
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }

                //SendPingMail
                XmlNodeList SendPingMailElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.SendPingMail);
                if (SendPingMailElements != null && SendPingMailElements.Count == 1)
                {
                    try
                    {
                        SendPingMail = bool.Parse(((XmlElement)SendPingMailElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value));
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }

                //CheckVATNr
                XmlNodeList CheckVATNrElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.CheckVATNr);
                if (CheckVATNrElements != null && CheckVATNrElements.Count == 1)
                {
                    try
                    {
                        CheckVATNr = bool.Parse(((XmlElement)CheckVATNrElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value));
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }


                //PingMailRecipient
                XmlNodeList PingMailRecipientElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.PingMailRecipient);
                if (PingMailRecipientElements != null && PingMailRecipientElements.Count == 1)
                {
                    try
                    {
                        PingMailRecipient =
                            ((XmlElement)PingMailRecipientElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value);
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }


                //masterSolutionName
                XmlNodeList MasterSolutionNameElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.MasterSolutionName);
                if (MasterSolutionNameElements != null && MasterSolutionNameElements.Count == 1)
                {
                    try
                    {
                        MasterSolutionName =
                            ((XmlElement)MasterSolutionNameElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value);
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }

                // DBSizeToWarn
                XmlNodeList dbSizeToWarnElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.MinDBSizeToWarn);
                if (dbSizeToWarnElements != null && dbSizeToWarnElements.Count == 1)
                {
                    try
                    {
                        decimal dbSizeInKB = Convert.ToDecimal(((XmlElement)dbSizeToWarnElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.Value));
                        MinDBSizeToWarn = dbSizeInKB;
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }

                // EnableVerboseLog
                XmlNodeList verboseLogElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.EnableVerboseLog);
                if (verboseLogElements != null && verboseLogElements.Count == 1)
                {
                    try
                    {
                        EnableLMVerboseLog = String.Compare(
                            ((XmlElement)verboseLogElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.LMAttr),
                            Boolean.TrueString, StringComparison.OrdinalIgnoreCase) == 0 ? true : false;
                        EnableEAVerboseLog = String.Compare(
                            ((XmlElement)verboseLogElements[0]).GetAttribute(ServerConnectionInfoXML.Attribute.EAAttr),
                            Boolean.TrueString, StringComparison.OrdinalIgnoreCase) == 0 ? true : false;
                    }
                    catch (Exception err)
                    {
                        Debug.Fail(err.Message.ToString());
                    }
                }

                XmlNodeList SMTPServerElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.SMTPServer);
                if (SMTPServerElements != null && SMTPServerElements.Count == 1)
                    SMTPRelayServerName = ((XmlElement)SMTPServerElements[0]).InnerText;

                XmlNodeList smtpUseDefaultCredentialsElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.SMTPUseDefaultCredentials);
                if (
                    smtpUseDefaultCredentialsElements != null &&
                    smtpUseDefaultCredentialsElements.Count == 1 &&
                    !String.IsNullOrEmpty(((XmlElement)smtpUseDefaultCredentialsElements[0]).InnerText)
                    )
                    this.SMTPUseDefaultCredentials = Convert.ToBoolean(((XmlElement)smtpUseDefaultCredentialsElements[0]).InnerText, System.Globalization.CultureInfo.InvariantCulture.GetFormat(typeof(bool)) as IFormatProvider);

                XmlNodeList smtpUseSSLElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.SMTPUseSSL);
                if (
                    smtpUseSSLElements != null &&
                    smtpUseSSLElements.Count == 1 &&
                    !String.IsNullOrEmpty(((XmlElement)smtpUseSSLElements[0]).InnerText)
                    )
                    this.SMTPUseSSL = Convert.ToBoolean(((XmlElement)smtpUseSSLElements[0]).InnerText, System.Globalization.CultureInfo.InvariantCulture.GetFormat(typeof(bool)) as IFormatProvider);

                XmlNodeList smtpPortElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.SMTPPort);
                if (
                    smtpPortElements != null &&
                    smtpPortElements.Count == 1 &&
                    !String.IsNullOrEmpty(((XmlElement)smtpPortElements[0]).InnerText)
                    )
                    this.SMTPPort = Convert.ToInt32(((XmlElement)smtpPortElements[0]).InnerText, System.Globalization.CultureInfo.InvariantCulture.GetFormat(typeof(Int32)) as IFormatProvider);

                XmlNodeList smtpUserNameElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.SMTPUserName);
                if (smtpUserNameElements != null && smtpUserNameElements.Count == 1)
                    this.SMTPUserName = ((XmlElement)smtpUserNameElements[0]).InnerText;

                XmlNodeList smtpPasswordElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.SMTPPassword);
                if (smtpPasswordElements != null && smtpPasswordElements.Count == 1)
                {
                    char[] revPwd = Generic.Storer.Unstore(((XmlElement)smtpPasswordElements[0]).InnerText).ToCharArray();
                    Array.Reverse(revPwd);
                    this.SMTPPassword = new String(revPwd);
                    //this.SMTPPassword = Generic.Storer.Unstore(((XmlElement)smtpPasswordElements[0]).InnerText);
                }

                XmlNodeList smtpDomainElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.SMTPDomain);
                if (smtpDomainElements != null && smtpDomainElements.Count == 1)
                    this.SMTPDomain = ((XmlElement)smtpDomainElements[0]).InnerText;

                XmlNodeList smtpFromAddressElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.SMTPFromAddress);
                if (smtpFromAddressElements != null && smtpFromAddressElements.Count == 1)
                    this.SMTPFromAddress = ((XmlElement)smtpFromAddressElements[0]).InnerText;


                XmlNodeList wmscalElements = root.GetElementsByTagName(ServerConnectionInfoXML.Element.WmsCalPurgeable);
                if (
                    wmscalElements != null &&
                    wmscalElements.Count == 1 &&
                    !String.IsNullOrEmpty(((XmlElement)wmscalElements[0]).InnerText)
                    )
                    this.wMSCalPurgeMinutes = Convert.ToInt32(((XmlElement)wmscalElements[0]).InnerText, System.Globalization.CultureInfo.InvariantCulture.GetFormat(typeof(Int32)) as IFormatProvider);

            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Scrive (ed eventualmente crea) il file ServerConnection.config
        /// </summary>
        /// <returns>successo dell'operazione</returns>
        //----------------------------------------------------------------------------
        public bool UnParse(string aServerConnectionFile)
        {
            ServerConnectionFile = aServerConnectionFile;

            XmlDocument serverConnectionDocument = new XmlDocument();

            //root del documento
            XmlNode root = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.Connection, "");

            serverConnectionDocument.AppendChild(root);

            //stringa di connessione al db di sistema
            XmlNode sysDBConnectionStringNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.SysDBConnectionString, "");
            XmlAttribute nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = sysDBConnectionString;
            sysDBConnectionStringNode.Attributes.Append(nodeValue);
            root.AppendChild(sysDBConnectionStringNode);

            //masterSolutionName
            XmlNode masterSolutionNameNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.MasterSolutionName, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = masterSolutionName;
            masterSolutionNameNode.Attributes.Append(nodeValue);
            root.AppendChild(masterSolutionNameNode);
            //UserUICulture
            XmlNode preferredLanguageNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.PreferredLanguage, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = preferredLanguage;
            preferredLanguageNode.Attributes.Append(nodeValue);
            root.AppendChild(preferredLanguageNode);

            //UserCulture
            XmlNode applicationLanguageNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.ApplicationLanguage, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = applicationLanguage;
            applicationLanguageNode.Attributes.Append(nodeValue);
            root.AppendChild(applicationLanguageNode);

            //PasswordDuration
            XmlNode passwordDurationNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.PasswordDuration, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = passwordDuration.ToString();
            passwordDurationNode.Attributes.Append(nodeValue);
            root.AppendChild(passwordDurationNode);

            //MinPasswordLength
            XmlNode MinPasswordLengthNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.MinPasswordLength, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = MinPasswordLength.ToString();
            MinPasswordLengthNode.Attributes.Append(nodeValue);
            root.AppendChild(MinPasswordLengthNode);

            //WebServicesPort
            XmlNode WebServicesPortNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.WebServicesPort, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = WebServicesPort.ToString();
            WebServicesPortNode.Attributes.Append(nodeValue);
            root.AppendChild(WebServicesPortNode);

            //WebServicesTimeOut
            XmlNode WebServicesTimeOutNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.WebServicesTimeOut, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = WebServicesTimeOut.ToString();
            WebServicesTimeOutNode.Attributes.Append(nodeValue);
            root.AppendChild(WebServicesTimeOutNode);

            //MaxTbLoader
            XmlNode MaxTbLoaderNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.MaxTBLoader, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = MaxTBLoader.ToString();
            MaxTbLoaderNode.Attributes.Append(nodeValue);
            root.AppendChild(MaxTbLoaderNode);

            //MaxLoginPerTBLoader
            XmlNode MaxLoginPerTBLoaderNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.MaxLoginPerTBLoader, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = MaxLoginPerTBLoader.ToString();
            MaxLoginPerTBLoaderNode.Attributes.Append(nodeValue);
            root.AppendChild(MaxLoginPerTBLoaderNode);

            //TBLoaderTimeout
            XmlNode TBLoaderTimeoutNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.TBLoaderTimeOut, "");
            TBLoaderTimeoutNode.InnerText = TBLoaderTimeOut.ToString();
            root.AppendChild(TBLoaderTimeoutNode);

            //TbWCFDefaultTimeout
            XmlNode TbWCFDefaultTimeoutNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.TbWCFDefaultTimeout, "");
            TbWCFDefaultTimeoutNode.InnerText = TbWCFDefaultTimeout.ToString();
            root.AppendChild(TbWCFDefaultTimeoutNode);

            //TbWCFDataTransferTimeout
            XmlNode TbWCFDataTransferTimeoutNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.TbWCFDataTransferTimeout, "");
            TbWCFDataTransferTimeoutNode.InnerText = TbWCFDataTransferTimeout.ToString();
            root.AppendChild(TbWCFDataTransferTimeoutNode);

            //MaxLoginFailed
            XmlNode MaxLoginFailedNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.MaxLoginFailed, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = MaxLoginFailed.ToString();
            MaxLoginFailedNode.Attributes.Append(nodeValue);
            root.AppendChild(MaxLoginFailedNode);

            //UseStrongPwd
            XmlNode UseStrongPwdNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.UseStrongPwd, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = UseStrongPwd.ToString();
            UseStrongPwdNode.Attributes.Append(nodeValue);
            root.AppendChild(UseStrongPwdNode);

            //UseAutoLogin
            XmlNode UseAutologinNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.UseAutologin, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = UseAutoLogin.ToString();
            UseAutologinNode.Attributes.Append(nodeValue);
            root.AppendChild(UseAutologinNode);

            // MinDBSizeToWarn: dimensioni minime size database da controllare
            XmlNode minDBSizeToWarnNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.MinDBSizeToWarn, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = (MinDBSizeToWarn != 0) ? MinDBSizeToWarn.ToString() : "2044723";
            minDBSizeToWarnNode.Attributes.Append(nodeValue);
            root.AppendChild(minDBSizeToWarnNode);

            //CheckVATNr
            XmlNode CheckVATNrNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.CheckVATNr, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = (CheckVATNr) ? bool.TrueString : bool.FalseString;
            CheckVATNrNode.Attributes.Append(nodeValue);
            root.AppendChild(CheckVATNrNode);

            //SendpingMail
            XmlNode sendpingMailNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.SendPingMail, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = (SendPingMail) ? bool.TrueString : bool.FalseString;
            sendpingMailNode.Attributes.Append(nodeValue);
            root.AppendChild(sendpingMailNode);
            //pingMailRecipient
            XmlNode pingMailRecipientNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.PingMailRecipient, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.Value);
            nodeValue.Value = PingMailRecipient;
            pingMailRecipientNode.Attributes.Append(nodeValue);
            root.AppendChild(pingMailRecipientNode);

            // EnableVerboseLog
            XmlNode enableVerboseLogNode = serverConnectionDocument.CreateNode(XmlNodeType.Element, ServerConnectionInfoXML.Element.EnableVerboseLog, "");
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.LMAttr);
            nodeValue.Value = EnableLMVerboseLog.ToString();
            enableVerboseLogNode.Attributes.Append(nodeValue);
            nodeValue = serverConnectionDocument.CreateAttribute(ServerConnectionInfoXML.Attribute.EAAttr);
            nodeValue.Value = EnableEAVerboseLog.ToString();
            enableVerboseLogNode.Attributes.Append(nodeValue);
            root.AppendChild(enableVerboseLogNode);

            if (!String.IsNullOrEmpty(SMTPRelayServerName))
            {
                XmlElement SMTPServerNode = serverConnectionDocument.CreateElement(ServerConnectionInfoXML.Element.SMTPServer);
                SMTPServerNode.InnerText = SMTPRelayServerName;
                root.AppendChild(SMTPServerNode);
            }

            XmlElement smtpUseDefaultCredentialsNode = serverConnectionDocument.CreateElement(ServerConnectionInfoXML.Element.SMTPUseDefaultCredentials);
            smtpUseDefaultCredentialsNode.InnerText = this.SMTPUseDefaultCredentials.ToString();
            root.AppendChild(smtpUseDefaultCredentialsNode);

            if (this.SMTPUseSSL)
            {
                XmlElement smtpUseSSLNode = serverConnectionDocument.CreateElement(ServerConnectionInfoXML.Element.SMTPUseSSL);
                smtpUseSSLNode.InnerText = this.SMTPUseSSL.ToString();
                root.AppendChild(smtpUseSSLNode);
            }

            XmlElement smtpPortNode = serverConnectionDocument.CreateElement(ServerConnectionInfoXML.Element.SMTPPort);
            smtpPortNode.InnerText = this.SMTPPort.ToString(System.Globalization.CultureInfo.InvariantCulture.GetFormat(typeof(Int32)) as IFormatProvider);
            root.AppendChild(smtpPortNode);

            if (!String.IsNullOrEmpty(this.SMTPUserName))
            {
                XmlElement smtpUserNameNode = serverConnectionDocument.CreateElement(ServerConnectionInfoXML.Element.SMTPUserName);
                smtpUserNameNode.InnerText = this.SMTPUserName;
                root.AppendChild(smtpUserNameNode);
            }

            if (!String.IsNullOrEmpty(this.SMTPPassword))
            {
                XmlElement smtpPasswordNode = serverConnectionDocument.CreateElement(ServerConnectionInfoXML.Element.SMTPPassword);
                char[] revPwd = this.SMTPPassword.ToCharArray();
                Array.Reverse(revPwd);
                smtpPasswordNode.InnerText = Generic.Storer.Store(new String(revPwd));
                // smtpPasswordNode.InnerText = Generic.Storer.Store(this.SMTPPassword);
                root.AppendChild(smtpPasswordNode);
            }

            if (!String.IsNullOrEmpty(this.SMTPDomain))
            {
                XmlElement smtpDomainNode = serverConnectionDocument.CreateElement(ServerConnectionInfoXML.Element.SMTPDomain);
                smtpDomainNode.InnerText = this.SMTPDomain;
                root.AppendChild(smtpDomainNode);
            }

            if (!String.IsNullOrEmpty(this.SMTPFromAddress))
            {
                XmlElement smtpDomainNode = serverConnectionDocument.CreateElement(ServerConnectionInfoXML.Element.SMTPFromAddress);
                smtpDomainNode.InnerText = this.SMTPFromAddress;
                root.AppendChild(smtpDomainNode);
            }

            XmlElement wmscalNode = serverConnectionDocument.CreateElement(ServerConnectionInfoXML.Element.WmsCalPurgeable);
            wmscalNode.InnerText = this.WMSCalPurgeMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture.GetFormat(typeof(Int32)) as IFormatProvider);
            root.AppendChild(wmscalNode);



            try
            {
                FileInfo connectionFileInfo = new FileInfo(ServerConnectionFile);

                if (connectionFileInfo.Exists && ((connectionFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly))
                    connectionFileInfo.Attributes -= FileAttributes.ReadOnly;

                FileInfo fi = new FileInfo(ServerConnectionFile);
                fi.Directory.Create();
                serverConnectionDocument.Save(File.OpenWrite(ServerConnectionFile));
            }
            catch (Exception err)
            {
                Debug.Fail(err.Message + "\nVerificare che il file non sia protetto da scrittura");
                return false;
            }

            //devo reinizializzare l'oggetto statico per rileggere le modifiche
            InstallationData.Clear();

            return true;
        }
    }
}
