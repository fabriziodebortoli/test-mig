using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.TaskBuilderNet.TbHermesBL.Config
{
	public class HermesSettings
	{
		//---------------------------------------------------------------------
		public bool Enabled { get; set; }
        public bool LoggerEnabled { get; set; }  //fabio
        public bool LoggerRawData { get; set; }  //fabio
        public string Company { get; /*private*/ set; }
        public string LoggingPath { get; set; } //fabio
		public MailServerSettings MailServerSettings { get; /*private*/ set; }
        public Int32 TickRate { get; set; }

		//---------------------------------------------------------------------
		private const string fileName = "Hermes.config";

		//---------------------------------------------------------------------
		static public HermesSettings Load()
		{
			HermesSettings hs = new HermesSettings();
			hs.MailServerSettings = new MailServerSettings();

			PathFinder pathFinder = new PathFinder(null, null); // server per restituire ModuleInfo e non BaseModuleInfo
			BaseModuleInfo moduleInfo = (BaseModuleInfo)pathFinder.GetModuleInfoByName("OFM", "Mail");

			SettingsConfigInfo scInfo = new SettingsConfigInfo(fileName, moduleInfo);
			scInfo.Parse();
			if (scInfo.Sections != null)
			{
				List<SectionInfo> scList = new List<SectionInfo>();
				foreach (SectionInfo si in scInfo.Sections)
					scList.Add(si);
				SectionInfo siInst = scList.Find(x => x.Hidden == false && x.Name == SectionName.GeneralSettings.ToString());
				if (siInst != null)
				{
					SettingItem item = siInst.GetSettingItemByName(ItemName.Enabled.ToString());
					if (item != null)
						hs.Enabled = bool.Parse((string)item.Values[0]); // mi sa che non c'è nulla di tipizzato, me lo faccio io (o forse è fornito un modo???)
					item = siInst.GetSettingItemByName(ItemName.Company.ToString());
					if (item != null)
						hs.Company = (string)item.Values[0];
                    item = siInst.GetSettingItemByName(ItemName.TickRate.ToString());
                    if (item != null)
                        hs.TickRate = Convert.ToInt32((string)item.Values[0]);
                }
                SectionInfo siLog = scList.Find(x => x.Hidden == false && x.Name == SectionName.LogSettings.ToString());
                if (siLog != null)
                {
                    SettingItem item = siLog.GetSettingItemByName(ItemName.LoggingPath.ToString());
                    if (item != null)
                        hs.LoggingPath = (string)item.Values[0];
                    item = siLog.GetSettingItemByName(ItemName.LoggerEnabled.ToString());
                    if (item != null)
                        hs.LoggerEnabled = bool.Parse((string)item.Values[0]);
                    item = siLog.GetSettingItemByName(ItemName.LoggerRawData.ToString());
                    if (item != null)
                        hs.LoggerRawData = bool.Parse((string)item.Values[0]);
                }
                SectionInfo siMail = scList.Find(x => x.Hidden == false && x.Name == SectionName.MailServer.ToString());
				if (siMail != null)
				{
					SettingItem siSrvName = siMail.GetSettingItemByName(ItemName.ReceiveServerName.ToString());
					if (siSrvName != null)
						hs.MailServerSettings.ReceiveServerName = siSrvName.Values[0] as string;
					SettingItem siAccType = siMail.GetSettingItemByName(ItemName.AccountType.ToString());
					if (siAccType != null)
						hs.MailServerSettings.AccountType = (MailServerType)Enum.Parse(typeof(MailServerType), siAccType.Values[0] as string, true);
					SettingItem siUsrName = siMail.GetSettingItemByName(ItemName.ReceiveServerUserName.ToString());
					if (siUsrName != null)
						hs.MailServerSettings.ReceiveServerUserName = siUsrName.Values[0] as string;
					SettingItem siSrvPdw = siMail.GetSettingItemByName(ItemName.ReceiveServerPassword.ToString());
					if (siSrvPdw != null)
						hs.MailServerSettings.ReceiveServerPassword = siSrvPdw.Values[0] as string; // TODO decrypt
					SettingItem usrName = siMail.GetSettingItemByName(ItemName.SenderName.ToString());
					if (usrName != null)
						hs.MailServerSettings.UserName = usrName.Values[0] as string;
					SettingItem siMailAddr = siMail.GetSettingItemByName(ItemName.MailAddress.ToString());
					if (siMailAddr != null)
						hs.MailServerSettings.EMailAddress = siMailAddr.Values[0] as string;
					SettingItem siSrvSend = siMail.GetSettingItemByName(ItemName.SendServerName.ToString());
					if (siSrvSend != null)
						hs.MailServerSettings.SmtpServer = siSrvSend.Values[0] as string;
					SettingItem siSrvPort = siMail.GetSettingItemByName(ItemName.ReceiveServerPort.ToString());
					if (siSrvPort != null)
					{
						int port;
						if (Int32.TryParse(siSrvPort.Values[0] as string, out port))
							hs.MailServerSettings.Port = port;
						else
							hs.MailServerSettings.Port = MailServerSettings.DefaultPop3Port; // TODO distingui default Imap
					}
					else
						hs.MailServerSettings.Port = MailServerSettings.DefaultPop3Port; // TODO distingui default Imap
					SettingItem siSmtpPort = siMail.GetSettingItemByName(ItemName.SendServerPort.ToString());
					if (siSmtpPort != null)
					{
						int port;
						if (Int32.TryParse(siSmtpPort.Values[0] as string, out port))
							hs.MailServerSettings.SmtpPort = port;
						else
							hs.MailServerSettings.SmtpPort = MailServerSettings.DefaultSmtpPort;
					}
					else
						hs.MailServerSettings.SmtpPort = MailServerSettings.DefaultSmtpPort;
					SettingItem siInUseSsl = siMail.GetSettingItemByName(ItemName.ReceiveUseSSL.ToString());
					if (siInUseSsl != null)
					{
						bool inUseSS;
						if (bool.TryParse(siInUseSsl.Values[0] as string, out inUseSS))
							hs.MailServerSettings.ReceiveSsl = inUseSS;
					}
					SettingItem siOutUseSsl = siMail.GetSettingItemByName(ItemName.SendUseSSL.ToString());
					if (siOutUseSsl != null)
					{
						bool outUseSS;
						if (bool.TryParse(siOutUseSsl.Values[0] as string, out outUseSS))
							hs.MailServerSettings.SmtpSsl = outUseSS;
					}
				}
			}

			return hs;
		}

		//---------------------------------------------------------------------
		public void Save()
		{
			PathFinder pathFinder = new PathFinder(null, null); // server per restituire ModuleInfo e non BaseModuleInfo
			ModuleInfo moduleInfo = (ModuleInfo)pathFinder.GetModuleInfoByName("OFM", "Mail");

			SettingsConfigInfo configInfo = new SettingsConfigInfo(fileName, moduleInfo);

			SectionInfo sectionComp = new SectionInfo(SectionName.GeneralSettings.ToString(), "General settings");
			AddSettingItem(sectionComp, ItemName.Enabled, "Enabled", this.Enabled);
			if (false == string.IsNullOrEmpty(this.Company))
				AddSettingItem(sectionComp, ItemName.Company, "Enabled company", this.Company);
            AddSettingItem(sectionComp, ItemName.TickRate, "TickRate", this.TickRate);

            SectionInfo sectionLog = new SectionInfo(SectionName.LogSettings.ToString(), "Log settings");
            AddSettingItem(sectionLog, ItemName.LoggerEnabled, "LoggerEnabled", this.LoggerEnabled);
            AddSettingItem(sectionLog, ItemName.LoggerRawData, "LoggerRawData", this.LoggerRawData);
			if (false == string.IsNullOrEmpty(this.LoggingPath))
                AddSettingItem(sectionLog, ItemName.LoggingPath, "LoggingPath", this.LoggingPath);

			SectionInfo sectionMail = new SectionInfo(SectionName.MailServer.ToString(), "Mail Server");
			if (this.MailServerSettings != null)
			{
				AddSettingItem(sectionMail, ItemName.ReceiveServerName, "server name", this.MailServerSettings.ReceiveServerName);
				AddSettingItem(sectionMail, ItemName.AccountType, "server type", this.MailServerSettings.AccountType.ToString());
				AddSettingItem(sectionMail, ItemName.ReceiveServerUserName, "server login", this.MailServerSettings.ReceiveServerUserName);
				AddSettingItem(sectionMail, ItemName.ReceiveServerPassword, "server password", this.MailServerSettings.ReceiveServerPassword); // TODO encrypt
				AddSettingItem(sectionMail, ItemName.SenderName, "user name", this.MailServerSettings.UserName);
				AddSettingItem(sectionMail, ItemName.MailAddress, "e-mail address", this.MailServerSettings.EMailAddress);
				AddSettingItem(sectionMail, ItemName.SendServerName, "SMTP server", this.MailServerSettings.SmtpServer);
				AddSettingItem(sectionMail, ItemName.ReceiveServerPort, "Incoming port", this.MailServerSettings.Port);
				AddSettingItem(sectionMail, ItemName.ReceiveUseSSL, "Use SSL (in)", this.MailServerSettings.ReceiveSsl);
				AddSettingItem(sectionMail, ItemName.SendServerPort, "SMTP port", this.MailServerSettings.SmtpPort);
				AddSettingItem(sectionMail, ItemName.SendUseSSL, "Use SSL (out)", this.MailServerSettings.SmtpSsl);
			}

			string dirPath = moduleInfo.GetCustomAllCompaniesAllUsersSettingsPath();
			string filePath = Path.Combine(dirPath, fileName);

            SectionInfo[] sectionInfos = new SectionInfo[] { sectionComp, sectionMail, sectionLog };

			XmlDocument xmlDocument = configInfo.CreateDocument();
			foreach (SectionInfo sectionInfo in sectionInfos)
				configInfo.UnparseSection(xmlDocument, sectionInfo);

			if (!Directory.Exists(dirPath))
				Directory.CreateDirectory(dirPath);
			xmlDocument.Save(filePath);
		}

        private enum SectionName { GeneralSettings, MailServer, LogSettings }
		private enum ItemName
		{
			Enabled, Company, LoggerEnabled, LoggerRawData, LoggingPath, TickRate, 
			ReceiveServerName, AccountType, ReceiveServerUserName, ReceiveServerPassword, SenderName, MailAddress,
			SendServerName, SendServerPort, ReceiveServerPort, ReceiveUseSSL, SendUseSSL
		}

		//---------------------------------------------------------------------
		private void AddSettingItem(SectionInfo section, ItemName itemName, string itemDescription, object itemValue)
		{
			if (itemValue == null)
				return;
			string resType = "string";
			string strType = itemValue.GetType().ToString();

			switch (strType)
			{
				// TODO altri tipi di dato
				case "System.Int32":
					resType = "integer";
					break;
				case "System.Boolean":
					resType = "bool";
					break;
				case "System.String":
				default:
					resType = "string";
					break;
			}

			SettingItem setting = new SettingItem(itemName.ToString(), resType);
			//setting.Hidden = item.Hidden;
			setting.Localize = itemDescription;
			//setting.Release = item.Release;
			setting.SourceFileType = SourceOfSettingsConfig.AllCompaniesAllUsers;
			//setting.UserSetting = item.AllowAddNewParameter;
			//setting.Values.Add((item.IsPassword) ? item.ParameterValue : item.SubItems[1].Text);
			setting.Values.Add(itemValue);

			section.AddSetting(setting);
		}
	}

	public class MailServerSettings
	{
		// POP3 - port 110
		// IMAP - port 143
		// SMTP - port 25
		// HTTP - port 80
		// Secure SMTP (SSMTP) - port 465
		// Secure IMAP (IMAP4-SSL) - port 585
		// IMAP4 over SSL (IMAPS) - port 993
		// Secure POP3 (SSL-POP) - port 995

		//---------------------------------------------------------------------
		public string ReceiveServerName { get; set; }
		public int Port { get; set; }
		public MailServerType AccountType { get; set; }
		public string ReceiveServerUserName { get; set; }
		public string ReceiveServerPassword { get; set; }
		public bool ReceiveSsl { get; set; }
		public bool ReceiveStl { get; set; }

		public string UserName { get; set; }
		public string EMailAddress { get; set; }
		public string SmtpServer { get; set; }
		public int SmtpPort { get; set; }
		public bool SmtpSsl { get; set; }
		public bool SmtpStl { get; set; }

		//---------------------------------------------------------------------
		public const int DefaultPop3Port = 110;
		public const int DefaultPop3SecurePort = 995;
		public const int DefaultSmtpPort = 25;

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(obj, null))
				return false;
			if (Object.ReferenceEquals(obj, this))
				return true;
			MailServerSettings other = obj as MailServerSettings;
			if (Object.ReferenceEquals(other, null))
				return false;

			return string.Compare(this.ReceiveServerName, other.ReceiveServerName, StringComparison.InvariantCultureIgnoreCase) == 0
				&& string.Compare(this.ReceiveServerUserName, other.ReceiveServerUserName, StringComparison.InvariantCultureIgnoreCase) == 0
				&& string.Compare(this.ReceiveServerPassword, other.ReceiveServerPassword, StringComparison.InvariantCultureIgnoreCase) == 0
				&& this.Port == other.Port
				&& this.AccountType == other.AccountType
				&& string.Compare(this.UserName, other.UserName, StringComparison.InvariantCultureIgnoreCase) == 0
				&& string.Compare(this.EMailAddress, other.EMailAddress, StringComparison.InvariantCultureIgnoreCase) == 0
				&& string.Compare(this.SmtpServer, other.SmtpServer, StringComparison.InvariantCultureIgnoreCase) == 0
				&& this.SmtpPort == other.SmtpPort
				&& this.ReceiveSsl == other.ReceiveSsl
				&& this.ReceiveStl == other.ReceiveStl
				&& this.SmtpSsl == other.SmtpSsl
				&& this.SmtpStl == other.SmtpStl;
		}
		public override int GetHashCode()
		{
			unchecked // In an unchecked context, arithmetic overflow is ignored and the result is truncated.
			{
				return this.ReceiveServerName.GetHashCode()
					+ this.ReceiveServerUserName.GetHashCode()
					+ this.ReceiveServerPassword.GetHashCode()
					+ this.Port.GetHashCode()
					+ this.AccountType.GetHashCode()
					+ this.UserName.GetHashCode()
					+ this.EMailAddress.GetHashCode()
					+ this.SmtpServer.GetHashCode()
					+ this.SmtpPort.GetHashCode()
					+ this.ReceiveSsl.GetHashCode()
					+ this.ReceiveStl.GetHashCode()
					+ this.SmtpSsl.GetHashCode()
					+ this.SmtpStl.GetHashCode();
			}
		}
	}

}
