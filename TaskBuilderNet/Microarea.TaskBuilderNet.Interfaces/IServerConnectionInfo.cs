using System;

namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface IServerConnectionInfo
	{
		string ApplicationLanguage { get; set; }
		int MaxLoginFailed { get; set; }
		int MaxTBLoader { get; set; }
		int MaxLoginPerTBLoader { get; set; }
		int MinPasswordLength { get; set; }
		bool Parse(string aServerConnectionFile);
		int PasswordDuration { get; set; }
		string PreferredLanguage { get; set; }
		string ServerConnectionFile { get; set; }
		string SMTPRelayServerName { get; set; }
		bool SMTPUseDefaultCredentials { get; set; }
		bool SMTPUseSSL { get; set; }
		Int32 SMTPPort { get; set; }
		string SMTPUserName { get; set; }
		string SMTPPassword { get; set; }
		string SMTPDomain { get; set; }
		string SMTPFromAddress { get; set; }
		string SysDBConnectionString { get; set; }
		bool UnParse(string aServerConnectionFile);
		bool UseStrongPwd { get; set; }
		bool UseAutoLogin { get; set; }
		int WebServicesPort { get; set; }
		int WebServicesTimeOut { get; set; }
		int TBLoaderTimeOut { get; set; }
		int TbWCFDefaultTimeout { get; set; }
		int TbWCFDataTransferTimeout { get; set; }
		decimal MinDBSizeToWarn { get; set; }
		bool EnableLMVerboseLog { get; set; }
		bool EnableEAVerboseLog { get; set; }
        bool SendPingMail { get; set; }
        bool CheckVATNr { get; set; }
        string PingMailRecipient { get; set; }
        string MasterSolutionName { get; set; }
        int WMSCalPurgeMinutes { get; set; }
    }
}
