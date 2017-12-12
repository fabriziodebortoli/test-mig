using Microarea.TaskBuilderNet.TbHermesBL.Config;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    // FABIO: gli oggetti 'account' per ora provengono solo dalla tabella dei worker ma potrebbero in futuro essere generati anche dai client
    // per cui generalizzo
	public interface IOM_MailAccount
	{
		int MailAccountID { get; set; }
        int MailAccountSubID { get; set; }
        bool IsWorkerAccount{ get; }
        MailServerType EnumAccountType { get; set; }
		string ReceiveServerName { get; set; }
		int ReceiveServerPort { get; set; }
		string ReceiveServerPassword { get; set; }
		string ReceiveServerUserName { get; set; }
        string SendServerName { get; set; }
		int SendServerPort { get; set; }
		string SendServerPassword { get; set; }
		string SendServerUserName { get; set; }
		bool BoolReceiveSsl { get; set; }
		bool BoolSmtpSsl { get; set; }
        int intMaxMailRecipients { get; set; }
        bool BoolIsImportOutbox { get; set; }
        bool BoolIsPrivate { get; set; }
        bool BoolIsDefault { get; set; }
        bool BoolIsOwner { get; set; }
        bool BoolIsResponseConfirm { get; set; }
        bool BoolIsAutomaticMasterAssociation { get; set; }
        int IntDayBeforeFromToday { get; set; }
	    int IntDayBeforeSearch { get; set; }
	    string StrOutboxName { get; set; }
    }
}
