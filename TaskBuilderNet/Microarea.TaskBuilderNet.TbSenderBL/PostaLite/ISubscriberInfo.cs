
namespace Microarea.TaskBuilderNet.TbSenderBL.PostaLite
{
	public interface ISubscriberInfo
	{
		string Subscriber_LegalCode { get; set; }
		string Subscriber_ActivityCode { get; set; }
		string Subscriber_AreaCode { get; set; }
		//string Subscriber_DateBirth { get; set; }
		//string Subscriber_BirthLocation { get; set; }
		//string Subscriber_BirthLocationCode { get; set; }
		//string Subscriber_ResponsableContacts { get; set; }

		string Subscriber_SurNameCompanyName { get; set; }
		string Subscriber_Name { get; set; }
		string Subscriber_City { get; set; } // facoltativo??
		string Subscriber_Address { get; set; }
		string Subscriber_ZipCode { get; set; }
		string Subscriber_County { get; set; }
		string Subscriber_State { get; set; }
		string Subscriber_PrefixPhone { get; set; }
		string Subscriber_TelephoneNumber { get; set; }
		string Subscriber_PrefixFax { get; set; }
		string Subscriber_FaxNumber { get; set; }
		string Subscriber_FiscalCode { get; set; }
		string Subscriber_VatNumber { get; set; }
		//string Subscriber_Gender { get; set; }
		string Subscriber_EMail { get; set; }
		//string Subscriber_Note { get; set; }
		string Subscriber_CompanyID { get; set; }

		bool Subscriber_PrivateEntity { get; set; }
		string Sender_Option { get; set; }
		string Sender_CompanyName { get; set; }
		string Sender_LegalCode { get; set; }
		string Sender_ActivityCode { get; set; }
		string Sender_City { get; set; }
		string Sender_ZipCode { get; set; }
		string Sender_CountyCode { get; set; }
		string Sender_Address { get; set; }
		string Sender_TelephoneNumber { get; set; }
		//string Sender_FaxNumber { get; set; }
		string Sender_FiscalCode { get; set; }
		string Sender_VatNumber { get; set; }
		string Sender_Email { get; set; }
		string Sender_Country { get; set; }
	}
}
