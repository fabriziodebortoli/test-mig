using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using System.Globalization;

namespace Microarea.TaskBuilderNet.TbSenderBL
{
	//===================================================================================
	public class PostaliteSettings
	{
		private readonly IPathFinder pathFinder;
		private readonly IDateTimeProvider timeProvider;
		
		//-------------------------------------------------------------------------------
		public PostaliteSettings(IPathFinder pathFinder, IDateTimeProvider timeProvider = null)
		{
			this.pathFinder = pathFinder;
			this.timeProvider = timeProvider;

			Company = pathFinder.Company;
			
			IsEnabled = ReadBool(PostaliteSettingsStrings.PostaliteParameters, PostaliteSettingsStrings.Enabled);

			MarginLeft = ReadInt(PostaliteSettingsStrings.PostaliteParameters, PostaliteSettingsStrings.MarginLeft, 11);
			MarginRight = ReadInt(PostaliteSettingsStrings.PostaliteParameters, PostaliteSettingsStrings.MarginRight, 11);
			MarginTop = ReadInt(PostaliteSettingsStrings.PostaliteParameters, PostaliteSettingsStrings.MarginTop, 14);
			MarginBottom = ReadInt(PostaliteSettingsStrings.PostaliteParameters, PostaliteSettingsStrings.MarginBottom, 14);
			
			//mancano gli enum print e delivery type
			
			DateTime defaultTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 19, 0, 0);
			SendTime = ReadDateTime(PostaliteSettingsStrings.PostaliteParameters, PostaliteSettingsStrings.SendTime, defaultTime);
			RecurHourInterval = ReadInt(PostaliteSettingsStrings.PostaliteParameters, PostaliteSettingsStrings.RecurHourInterval, 12);
			GroupingInterval = ReadInt(PostaliteSettingsStrings.PostaliteParameters, PostaliteSettingsStrings.GroupingInterval, 15);
			AdviceOfDeliveryEmail = ReadString(PostaliteSettingsStrings.PostaliteParameters, PostaliteSettingsStrings.AdviceOfDeliveryEmail);
			CreditLimit = ReadDouble(PostaliteSettingsStrings.PostaliteParameters, PostaliteSettingsStrings.CreditLimit, 10);
			DefaultCountry = ReadString(PostaliteSettingsStrings.PostaliteParameters, PostaliteSettingsStrings.DefaultCountry);

			PrivateEntity = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.PrivateEntity);
			PrivateEntityOption = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.PrivateEntityOption);
	  
			CompanyName = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.CompanyName);
			City = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.City);
			Address = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.Address);
			ZIPCode = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.ZIPCode);
			CadastralCode = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.CadastralCode);
			County = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.County);
			Country = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.Country);
			AreaCodeTelephone = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.AreaCodeTelephone);
			Telephone = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.Telephone);
			TaxIdNumber = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.TaxIdNumber);
			FiscalCode = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.FiscalCode);
			EMail = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.EMail);
			ActivityCode = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.ActivityCode);
			LegalStatusCode = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.LegalStatusCode);
			Notes = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.Notes);
			
			SenderCompanyName = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.SenderCompanyName);
			SenderCity = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.SenderCity);
			SenderAddress = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.SenderAddress);
			SenderZipCode = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.SenderZIPCode);
			SenderCounty = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.SenderCounty);
			SenderCountry = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.SenderCountry);
			SenderTaxIdNumber = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.SenderTaxIdNumber);
			SenderFiscalCode = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.SenderFiscalCode);
			SenderEMail = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.SenderEMail);
			SenderLegalStatusCode = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.SenderLegalStatusCode);
			SenderActivityCode = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.SenderActivityCode);

			AddresserCompanyName = ReadString(PostaliteSettingsStrings.PostaliteAddresser, PostaliteSettingsStrings.AddresserCompanyName);
			AddresserAddress = ReadString(PostaliteSettingsStrings.PostaliteAddresser, PostaliteSettingsStrings.AddresserAddress);
			AddresserZipCode = ReadString(PostaliteSettingsStrings.PostaliteAddresser, PostaliteSettingsStrings.AddresserZipCode);
			AddresserCity = ReadString(PostaliteSettingsStrings.PostaliteAddresser, PostaliteSettingsStrings.AddresserCity);
			AddresserCounty = ReadString(PostaliteSettingsStrings.PostaliteAddresser, PostaliteSettingsStrings.AddresserCounty);
			AddresserCountry = ReadString(PostaliteSettingsStrings.PostaliteAddresser, PostaliteSettingsStrings.AddresserCountry);

			LoginId = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.LoginId);
			string tempAuth = ReadString(PostaliteSettingsStrings.PostaliteSubscribe, PostaliteSettingsStrings.TokenAuth);
			TokenAuth = Crypto.Decrypt(tempAuth);
		}

		//-------------------------------------------------------------------------------
		private double ReadDouble(string sSection, string sEntry, int defaultValue)
		{
			return double.Parse(
				(string)ReadSetting.GetSettings(pathFinder, PostaliteSettingsStrings.PostaliteSettingsNamespace, sSection, sEntry, defaultValue),
				CultureInfo.InvariantCulture);
		}

		//-------------------------------------------------------------------------------
		private int ReadInt(string sSection, string sEntry, int defaultValue)
		{
			return int.Parse(
				(string)ReadSetting.GetSettings(pathFinder, PostaliteSettingsStrings.PostaliteSettingsNamespace, sSection, sEntry, defaultValue),
				CultureInfo.InvariantCulture);
		}

		//-------------------------------------------------------------------------------
		private string ReadString(string sSection, string sEntry)
		{
			return (string)ReadSetting.GetSettings(pathFinder, PostaliteSettingsStrings.PostaliteSettingsNamespace, sSection, sEntry, defaultSettingValue: string.Empty);
		}

		//-------------------------------------------------------------------------------
		private DateTime ReadDateTime(string sSection, string sEntry, DateTime defaultValue)
		{
			return DateTime.Parse(
				(string)ReadSetting.GetSettings(pathFinder, PostaliteSettingsStrings.PostaliteSettingsNamespace, sSection, sEntry, defaultValue),
				CultureInfo.InvariantCulture);
		}

		//-------------------------------------------------------------------------------
		private bool ReadBool(string sSection, string sEntry)
		{
			return bool.Parse(
				(string)ReadSetting.GetSettings(pathFinder, PostaliteSettingsStrings.PostaliteSettingsNamespace, sSection, sEntry, defaultSettingValue: false));
		}

		//-------------------------------------------------------------------------------
		internal string Company { get; private set; }
		internal bool IsEnabled { get; private set; }
		internal int MarginLeft { get; private set; }
		internal int MarginRight { get; private set; }
		internal int MarginTop { get; private set; }
		internal int MarginBottom { get; private set; }
		
		//Settings
		//internal string DeliveryType { get; private set; }	
		//internal string PrintType { get; private set; }	
		internal DateTime SendTime { get; private set; }										
		internal int RecurHourInterval { get; private set; }									
		internal int GroupingInterval { get; private set; }										
		internal double CreditLimit { get; private set; }										
		internal string AdviceOfDeliveryEmail { get; private set; }								
		internal string DefaultCountry { get; private set; }									
																								
		//Sottoscrizione																		

		/// <summary>
		/// Contiene un booleano in forma stringa "true" o "false" o string.empty
		/// </summary>
		/// 
		internal string	PrivateEntity { get; private set; }
		internal string PrivateEntityOption { get; private set; }
		internal string LoginId { get; private set; }
		internal string TokenAuth { get; private set; }
		internal string CompanyName { get; private set; }
		internal string City { get; private set; }
		internal string Address { get; private set; }
		internal string ZIPCode { get; private set; }
		internal string CadastralCode { get; private set; }
		internal string County { get; private set; }
		internal string Country { get; private set; }
		internal string AreaCodeTelephone { get; private set; }
		internal string Telephone { get; private set; }
		internal string TaxIdNumber { get; private set; }
		internal string FiscalCode { get; private set; }
		internal string EMail { get; private set; }
		internal string ActivityCode { get; private set; }
		internal string LegalStatusCode { get; private set; }
		internal string Notes { get; private set; }

		internal string SenderCompanyName { get; private set; }
		internal string SenderCity { get; private set; }
		internal string SenderAddress { get; private set; }
		internal string SenderZipCode { get; private set; }
		internal string SenderCounty { get; private set; }
		internal string SenderCountry { get; private set; }
		internal string SenderTaxIdNumber { get; private set; }
		internal string SenderFiscalCode { get; private set; }
		internal string SenderEMail { get; private set; }
		internal string SenderActivityCode { get; private set; }
		internal string SenderLegalStatusCode { get; private set; }

		internal string AddresserCompanyName { get; private set; }
		internal string AddresserAddress { get; private set; }
		internal string AddresserZipCode { get; private set; }
		internal string AddresserCity { get; private set; }
		internal string AddresserCounty { get; private set; }
		internal string AddresserCountry { get; private set; }

		
		//-------------------------------------------------------------------------------
		public DateTime GetSentAfterTime(postalite.api.Delivery delivery)
		{
			DateTime now = this.timeProvider != null ? this.timeProvider.Now : DateTime.Now;

			if (delivery == postalite.api.Delivery.Fax)
				// i Fax vanno spediti asap
				return now
					.AddMinutes(3); // aggiungo qualche minuto per risparmiare copertina accorpando (ottimizzazione)

			DateTime sendAfter = new DateTime(now.Year, now.Month, now.Day, this.SendTime.Hour, this.SendTime.Minute, this.SendTime.Second);

			if (this.RecurHourInterval <= 0)
				return sendAfter;

			while (now > sendAfter)
				sendAfter = sendAfter.AddHours(this.RecurHourInterval);

			return sendAfter;
		}
	}

	//===================================================================================
	internal class PostaliteSettingsStrings
	{
		//static const TCHAR szPostaliteSettingsFile[]	= _T("Postalite.config");
		internal const string PostaliteSettingsNamespace = "Extensions.TbMailer.Postalite";

		internal const string PostaliteSubscribe = "Subscribe";
		internal const string PostaliteParameters = "Parameters";
		internal const string PostaliteAddresser = "Addresser";


		internal const string Enabled = "Enabled";											
		internal const string MarginLeft = "MarginLeft";									
		internal const string MarginRight = "MarginRight";									
		internal const string MarginTop = "MarginTop";										
		internal const string MarginBottom = "MarginBottom";

		internal const string DeliveryType = "DeliveryType";
		internal const string PrintType = "PrintType";
		internal const string SendTime = "SendTime";										
		internal const string RecurHourInterval = "RecurHourInterval";						
		internal const string GroupingInterval = "GroupingInterval";						
		internal const string CreditLimit = "CreditLimit";
		internal const string AdviceOfDeliveryEmail = "AdviceOfDeliveryEmail";
		internal const string DefaultCountry = "DefaultCountry";

		internal const string PrivateEntity = "PrivateEntity";
		internal const string PrivateEntityOption = "PrivateEntityOption";
		internal const string LoginId = "LoginId";
		internal const string TokenAuth = "TokenAuth";
		internal const string CompanyName = "CompanyName";
		internal const string City = "City";
		internal const string Address = "Address";
		internal const string ZIPCode = "ZIPCode";
		internal const string County = "County";
		internal const string Country = "Country";
		internal const string AreaCodeTelephone = "AreaCodeTelephone";
		internal const string Telephone = "Telephone";
		internal const string TaxIdNumber = "TaxIdNumber";
		internal const string FiscalCode = "FiscalCode";
		internal const string EMail = "EMail";
		internal const string Notes = "Notes";
		internal const string LegalStatusCode = "LegalStatusCode";
		internal const string ActivityCode = "ActivityCode";
		internal const string CadastralCode = "CadastralCode";

		internal const string SenderCompanyName = "SenderCompanyName";
		internal const string SenderCity = "SenderCity";
		internal const string SenderAddress = "SenderAddress";
		internal const string SenderZIPCode = "SenderZIPCode";
		internal const string SenderCounty = "SenderCounty";
		internal const string SenderCountry = "SenderCountry";
		internal const string SenderTaxIdNumber = "SenderTaxIdNumber";
		internal const string SenderFiscalCode = "SenderFiscalCode";
		internal const string SenderEMail = "SenderEMail";
		internal const string SenderLegalStatusCode = "SenderLegalStatusCode";
		internal const string SenderActivityCode = "SenderActivityCode";
		internal const string AddresserCompanyName = "AddresserCompanyName";
		internal const string AddresserAddress = "AddresserAddress";
		internal const string AddresserZipCode = "AddresserZipCode";
		internal const string AddresserCity = "AddresserCity";
		internal const string AddresserCounty = "AddresserCounty";
		internal const string AddresserCountry = "AddresserCountry";
	}

	//===================================================================================
	[Serializable]
	public class PostaliteSettingsList : List<PostaliteSettings>
	{
		public PostaliteSettings this[string company]
		{
			get
			{
				foreach (PostaliteSettings current in this)
				{
					if (current.Company.CompareNoCase(company))
						return current;
				}

				return null;
			}
		}
	}

}
