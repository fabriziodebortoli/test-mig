using System;
using System.Configuration;


namespace Microarea.Library.SMHandlerDbInterface
{
	public class Queries
	{
		private static bool PAI_it()
		{
            return System.Configuration.ConfigurationManager.AppSettings["PAILang"] == "it";
		}
	#region Queries Italiano
		public static string SelectLoginID ()
		{
			return PAI_it() ?
				"SELECT * FROM PAI_InternetLogins WHERE LoginID=@LoginID":
				"SELECT * FROM PAI_InternetLogins WHERE LoginID=@LoginID";
		}
		public static  string SelectProductActive()
		{
			return PAI_it() ?
				"SELECT PAI_Prodotti.Attivo  FROM PAI_Prodotti WHERE PAI_Prodotti.Signature=@Sign and PAI_Prodotti.Rivend=@Riv":
				"SELECT PAI_ProductsMaster.Active  FROM PAI_ProductsMaster WHERE PAI_ProductsMaster.Signature=@Sign and PAI_ProductsMaster.Reseller=@Riv";
		}

		public static  string SelectProduct()
		{
			return PAI_it() ?
				"SELECT PAI_Prodotti.Attivo, PAI_Prodotti.Signature, PAI_Prodotti.CodProd, PAI_Prodotti.Descri, PAI_Prodotti.UrlWebService FROM PAI_Prodotti WHERE PAI_Prodotti.Rivend=@Riv":
				"SELECT PAI_ProductsMaster.Active, PAI_ProductsMaster.Signature, PAI_ProductsMaster.ProductCode, PAI_ProductsMaster.Description, PAI_ProductsMaster.WebServiceUrl FROM PAI_ProductsMaster WHERE PAI_ProductsMaster.Reseller=@Riv";
		}

		public static  string SelectProductCodes()
		{
			return PAI_it() ?
				"SELECT PAI_ArticoliAltriDati.ParteMag FROM PAI_ArticoliAltriDati WHERE PAI_ArticoliAltriDati.CodiceProdotto=@Code":
				"SELECT PAI_ItemsOtherData.ItemCode FROM PAI_ItemsOtherData WHERE PAI_ItemsOtherData.ProductCode=@Code";
		}

		public static  string SelectSalesRoleType()
		{
			return PAI_it() ?
				"SELECT TipologiaMPP, RagSoc FROM PAI_Rivenditori WHERE rivend=@Riv":
				"SELECT SalesRoleType, CompanyName FROM PAI_Resellers WHERE Reseller=@Riv";
		}

		public static  string SelectReseller()
		{
			return PAI_it() ?
				"SELECT PAI_Assist.Rivend FROM PAI_Assist WHERE (PAI_Assist.CodFam = 'TBNETNFS' or PAI_Assist.CodFam = 'MDP_EMB') and PAI_Assist.Disattivo = '0' and PAI_Assist.Rivend=@CodAz":
				"SELECT PAI_SupportSubscriptions.CompanyCode FROM PAI_SupportSubscriptions WHERE (PAI_SupportSubscriptions.SupportContractCode = 'TBNETNFS' or PAI_SupportSubscriptions.SupportContractCode = 'MDP_EMB') and PAI_SupportSubscriptions.Disabled = '0' and PAI_SupportSubscriptions.CompanyCode=@CodAz";
		}

		public static  string SelectModuleCode()
		{
			return PAI_it() ?
				"SELECT PAI_ProdottiModuliFunzionali.Modulo FROM PAI_ProdottiModuliFunzionali, PAI_Prodotti WHERE PAI_ProdottiModuliFunzionali.CodProd = PAI_Prodotti.CodProd and PAI_Prodotti.Signature=@Sign and PAI_Prodotti.Rivend=@Riv and PAI_ProdottiModuliFunzionali.ServerShortName=@Servershortname":
				"SELECT PAI_ProductsFuncModules.ModuleCode FROM PAI_ProductsFuncModules, PAI_ProductsMaster WHERE PAI_ProductsFuncModules.ProductCode = PAI_ProductsMaster.ProductCode and PAI_ProductsMaster.Signature=@Sign and PAI_ProductsMaster.Reseller=@Riv and PAI_ProductsFuncModules.ServerShortName=@Servershortname";
		}

		public static  string SelectSupportContractCode()
		{
			return PAI_it() ?
				"SELECT * FROM PAI_Assist WHERE PAI_Assist.CodFam = 'TB.NET' and PAI_Assist.Disattivo = '0' and PAI_Assist.Rivend=@CodAz":
				"SELECT * FROM PAI_SupportSubscriptions WHERE PAI_SupportSubscriptions.SupportContractCode = 'TB.NET' and PAI_SupportSubscriptions.Disabled = '0' and PAI_SupportSubscriptions.CompanyCode=@CodAz";
		}

		public static  string UpdateLoginToLock()
		{
			return PAI_it() ?
				"UPDATE PAI_InternetLogins SET PAI_InternetLogins.UltimoTentativo = @DataUltimoTentativo,  PAI_InternetLogins.NrTentativi = @NumeroTentativo,  PAI_InternetLogins.LoginBloccato = @Bloccato WHERE PAI_InternetLogins.LoginID = @Username":
				"UPDATE PAI_InternetLogins SET PAI_InternetLogins.LastAttemptDate = @DataUltimoTentativo,  PAI_InternetLogins.AttemptsNr = @NumeroTentativo,  PAI_InternetLogins.AccountLocked = @Bloccato WHERE PAI_InternetLogins.LoginID = @Username";
		}

		public static  string UpdateLoginOk	()
		{
			return PAI_it() ?
				"UPDATE PAI_InternetLogins SET PAI_InternetLogins.UltimaLogin = @DataUltimaLogin, PAI_InternetLogins.UltimoTentativo= @DataDefault,  PAI_InternetLogins.NrTentativi= @NumeroDiDefault WHERE PAI_InternetLogins.LoginID = @Username":
				"UPDATE PAI_InternetLogins SET PAI_InternetLogins.LastLoginDate = @DataUltimaLogin, PAI_InternetLogins.LastAttemptDate= @DataDefault,  PAI_InternetLogins.AttemptsNr= @NumeroDiDefault WHERE PAI_InternetLogins.LoginID = @Username";
		}

		public static  string SelectMailTo	()
		{
			return PAI_it() ?
				null:
				"SELECT Info FROM PAI_WebSiteInfo WHERE TagName=@TagName";
		}

		public static  string SelectProductCode	()
		{
			return PAI_it() ?
				"SELECT PAI_Prodotti.CodProd FROM PAI_Prodotti WHERE PAI_Prodotti.CodProd LIKE @RivendID ORDER BY PAI_Prodotti.CodProd DESC":
				"SELECT PAI_ProductsMaster.ProductCode FROM PAI_ProductsMaster WHERE PAI_ProductsMaster.ProductCode LIKE @RivendID ORDER BY PAI_ProductsMaster.ProductCode DESC";
		}

		public static string InsertNewSolution ()
		{
			return PAI_it() ?
				"INSERT INTO PAI_Prodotti ( CodProd, Descri, Attivo, Signature, Rivend, UrlWebService, DataAgg, Utente, NrSerie) VALUES   ( @CodProd, @Descri, @Attivo, @Signature, @Rivend, @UrlWebService, @DataAgg, @Utente, @NrSerie)":
				"INSERT INTO PAI_ProductsMaster ( ProductCode, Description, Active, Signature, Reseller, WebServiceURL, UpdatingDate, UserID, SerialNumber) VALUES   ( @CodProd, @Descri, @Attivo, @Signature, @Rivend, @UrlWebService, @DataAgg, @Utente, @NrSerie)";
		}
		public static string SelectSolution ()
		{
			return PAI_it() ?
				"SELECT COUNT (*) FROM PAI_Prodotti WHERE PAI_Prodotti.Signature=@Signature":
				"SELECT COUNT (*) FROM PAI_ProductsMaster WHERE PAI_ProductsMaster.Signature=@Signature";
		}

		public static  string SelectSupportContractCodeMagic()
		{
			return PAI_it() ?
				"SELECT * FROM PAI_Assist WHERE PAI_Assist.CodFam = 'MDP_EMB' and PAI_Assist.Disattivo = '0' and PAI_Assist.Rivend=@CodAz":
				"SELECT * FROM PAI_SupportSubscriptions WHERE PAI_SupportSubscriptions.SupportContractCode = 'MDP_EMB' and PAI_SupportSubscriptions.Disabled = '0' and PAI_SupportSubscriptions.CompanyCode=@CodAz";
		}

		public static string SelectProdID()
		{
			return PAI_it() ?
				"NOT SUPPORTED":
				"SELECT PAI_ItemsOtherData.ProductID FROM PAI_ItemsOtherData inner join PAI_ProductsMaster on PAI_ItemsOtherData.ProductCode = PAI_ProductsMaster.ProductCode WHERE PAI_ProductsMaster.Signature= @Sign GROUP BY PAI_ItemsOtherData.ProductID";
		}
	#endregion		
	
		#region Rimborso IVA Veicoli
		public static string RimborsoIVAVeicoli_SelectRegistrationCode()
		{
			return
				"SELECT SubscriptionID, Accepted, Disabled, RegistrationDate FROM ELC_SUBSCRIPTIONS " +
				"WHERE VATNr=@VATNr AND ProductCode=@ProductCode AND RegistrationCode=@RegistrationCode";
		}

		public static string RimborsoIVAVeicoli_UpdateRegistrationDate()
		{
			return
				"UPDATE ELC_SUBSCRIPTIONS SET ELC_SUBSCRIPTIONS.RegistrationDate=@RegistrationDate " +
				"WHERE ELC_SUBSCRIPTIONS.SubscriptionID=@SubscriptionID AND ELC_SUBSCRIPTIONS.VATNr=@VATNr AND ProductCode=@ProductCode AND ELC_SUBSCRIPTIONS.RegistrationCode=@RegistrationCode AND ELC_SUBSCRIPTIONS.Accepted='1' AND ELC_SUBSCRIPTIONS.Disabled ='0'";
		}

		public static string RimborsoIVAVeicoli_UpdateRegistrationCode()
		{
			return
				"UPDATE ELC_SUBSCRIPTIONS SET ELC_SUBSCRIPTIONS.RegistrationCode=@RegistrationCode, ELC_SUBSCRIPTIONS.RegistrationDate=@RegistrationDate " +
				"WHERE ELC_SUBSCRIPTIONS.SubscriptionID=@SubscriptionID AND ProductCode=@ProductCode AND ELC_SUBSCRIPTIONS.Accepted='1' AND ELC_SUBSCRIPTIONS.Disabled ='0'";
		}

		public static string RimborsoIVAVeicoli_InsertUserInfo()
		{
			return
				"INSERT INTO ELC_REGISTRATIONINFO(SubscriptionID, IsActivated, ProductVersion, MACAddress, DatabaseServer, DatabaseLogin, DatabaseCompany, ProgramRelease, LastUpdate) " +
				"VALUES (@SubscriptionID, @IsActivated, @ProductVersion, @MACAddress, @DatabaseServer, @DatabaseLogin, @DatabaseCompany, @ProgramRelease, @LastUpdate)";
		}

		public static string RimborsoIVAVeicoli_CountRegistrationCode()
		{
			return
				"SELECT COUNT(*) FROM ELC_SUBSCRIPTIONS " +
				"WHERE RegistrationCode = @RegistrationCode AND ProductCode=@ProductCode";
		}
		#endregion
	}

	//========================================================================
	public class Field
	{
		public static int AnonymousUserCode = 997; // Codice della tabella ELS_SUBSCRIPTIONS
		public static DateTime PAIDefaultDate = new DateTime(1799, 12, 31, 0, 0, 0); // Standard di PAI per le date "vuote" (31/12/1799)

		private static bool PAI_it()
		{
            return System.Configuration.ConfigurationManager.AppSettings["PAILang"] == "it";
		}

		#region Colonne Italiano
		//pai_internetlogins
		public static string LoginID()
		{
			return PAI_it() ?
				"LoginID":
				"LoginID";
		}

		public static string Password()
		{
			return PAI_it() ?
				"Password":
				"Password";
			}

		public static string CustomerCode()
		{
			return PAI_it() ?
				"CodiceAzienda":
				"CustomerCode";
		}

		public static string PIC()
		{
			return PAI_it() ?
				"CodicePersona":
				"PIC";
		}

		public static string EnableChangePwd()
		{
			return PAI_it() ?
				"CambiaPwd":
				"EnableChangePwd";
		}
		public static string PwdNotChange()
		{
			return PAI_it() ?
				"ModPwdNonCons":
				"PwdNotChange";
		}

		public static string EnglishOnly()
		{
			return PAI_it() ?
				"SitoInglese":
				"EnglishOnly";
		}
		public static string PwdNotExpire()
		{
			return PAI_it() ?
				"PwdNonScade":
				"PwdNotExpire";
		}
		public static string AccountDisabled()
		{
			return PAI_it() ?
				"LoginDisabilitato":
				"AccountDisabled";
		}
		public static string AccountLocked()
		{
			return PAI_it() ?
				"LoginBloccato":
				"AccountLocked";
		}
		public static string RecordingDate()
		{
			return PAI_it() ?
				"Data":
				"RecordingDate";
		}
		public static string LastLoginDate()
		{
			return PAI_it() ?
				"UltimaLogin":
				"LastLoginDate";
		}
		public static string LastAttemptDate	()
		{
			return PAI_it() ?
				"UltimoTentativo":
				"LastAttemptDate";
		}
		public static string AttemptsNr()
		{
			return PAI_it() ?
				"NrTentativi":
				"AttemptsNr";
		}
		//pai_prodotti
		public static string Active()
		{
			return PAI_it() ?
				"Attivo":
				"Active";
		}

		public static string Signature()
		{
			return "Signature";
		}

		public static string Url()
		{
			return PAI_it() ?
				"UrlWebService":
				"WebServiceURL";
		}
		public static string Description()
		{
			return PAI_it() ?
				"Descri":
				"Description";
		}
		public static string ProductCode()
		{
			return PAI_it() ?
				"CodProd":
				"ProductCode";
		}

		//pai_rivenditori
		public static string SalesRoleType()
		{
			return PAI_it() ?
				"TipologiaMPP":
				"SalesRoleType";
		}
		public static string CompanyName()
		{
			return PAI_it() ?
				"RagSoc":
				"CompanyName";
		}
		//PAI_WebSiteInfo SOLO INGLESE
		public static string Info()
		{
			return PAI_it() ?
				null:
				"Info";
		}
		public static string TagName()
		{
			return PAI_it() ?
				null:
				"TagName";
		}
		//pai_articolialtridati
		public static string ItemCode()
		{
			return PAI_it() ?
				"ParteMag":
				"ItemCode";
		}

		public static string ProductID()
		{
			return PAI_it() ?
				"NOT SUPPORTED":
				"ProductID";
		}

		#endregion


	
	
	}
}
