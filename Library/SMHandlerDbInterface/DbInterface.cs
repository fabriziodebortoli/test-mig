using System;
using System.Data;
using System.Collections;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Web.Security;

using Microarea.Library.SMBaseHandler;
using Microarea.Library.WSLogger;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.Library.SMHandlerDbInterface
{
	/// <summary>
	/// Interfaccia al PAI. Scopiazzamento del sito per la parte di LOGIN.
	/// </summary>
	//=========================================================================
	public class SMHandlerDbInterface
	{
		private		static	string	connectionstring	= null;
		public		static	string	ConnectionString
		{
			set {connectionstring = value;}
			get {return connectionstring;}
		}
		private		static	BaseLogger	dbLogger		= null;

		//------------------------------------------------------------------------
		public static void SetPaiConnectionString()
		{
            connectionstring = System.Configuration.ConfigurationManager.AppSettings["PAIConnectionString"];
		}

		/// <summary>
		/// Effettua login, che può fallire se le credenziali non sono corrette
		/// "SELECT * FROM PAI_InternetLogins WHERE LoginID=@LoginID";
		/// </summary>
		//------------------------------------------------------------------------------
		public static bool Authenticate(string login, string password, MessagesInfoWriter msgInfos, BaseLogger logger, out string codiceAzienda, out bool isCDS, out string ragSoc)
		{
			ragSoc = String.Empty;
			dbLogger = logger;
			/*LOGIN*/
			isCDS = false;
			codiceAzienda = null;
			if (login == null || login == String.Empty)
			{
				dbLogger.WriteAndSendError( "SMHandlerDbInterface.Authenticate", MessagesCode.AuthenticationFailed, "The credential supplied are null.");
				msgInfos.AddMessage(new MessageInfo(MessagesCode.AuthenticationFailed));
				return false;
			}	
			codiceAzienda = String.Empty;

			string mySqlString = Queries.SelectLoginID();

			SqlConnection myConnection = new SqlConnection(connectionstring);
			SqlCommand myCommand = new SqlCommand(mySqlString, myConnection);
			myCommand.Parameters.AddWithValue("@LoginID", login);
			ParametriLogin myLoginParameters = new ParametriLogin();
			try
			{
				myConnection.Open();
				SqlDataReader dr = myCommand.ExecuteReader();

				if (dr.Read())
				{
					myLoginParameters.LoginID					= dr[Field.LoginID()].ToString();
					myLoginParameters.Password					= dr[Field.Password()].ToString();
					myLoginParameters.CodiceAzienda				= dr[Field.CustomerCode()].ToString();
					myLoginParameters.CodicePersona				= dr[Field.PIC()].ToString();
					myLoginParameters.CambiaPwd					= (dr[Field.EnableChangePwd()].ToString() == "1") ? true : false;
					myLoginParameters.ModificaPwdNonConsentita	= (dr[Field.PwdNotChange()].ToString() == "1") ? true : false;
					myLoginParameters.SitoInglese				= (dr[Field.EnglishOnly()].ToString() == "1") ? true : false;
					myLoginParameters.PasswordNonScade			= (dr[Field.PwdNotExpire()].ToString() == "1") ? true : false;
					myLoginParameters.LoginDisabilitata			= (dr[Field.AccountDisabled()].ToString() == "1") ? true : false;
					myLoginParameters.LoginBloccata				= (dr[Field.AccountLocked()].ToString() == "1") ? true : false;
					myLoginParameters.PasswordScaduta			= ((((DateTime) dr[Field.RecordingDate()]).AddDays(90) < DateTime.Today) && (dr[Field.PwdNotExpire()].ToString() == "0")) ? true : false;
					myLoginParameters.UltimaLogin				= (DateTime) dr[Field.LastLoginDate()];
					myLoginParameters.UltimoTentativo			= (DateTime) dr[Field.LastAttemptDate()];
					myLoginParameters.NumeroTentativo			= (int) dr[Field.AttemptsNr()];
					myLoginParameters.CambiataPasswordOggi		=( ((DateTime) dr[Field.RecordingDate()]).Date < (DateTime.Today.Date)) ? false : true;
				}
				else
				{
					dbLogger.WriteAndSendError( "SMHandlerDbInterface.Authenticate", MessagesCode.AuthenticationFailed, String.Format("The credential supplied are wrong, login: {0}.", login));
					msgInfos.AddMessage(new MessageInfo(MessagesCode.AuthenticationFailed));
					return false;
				}
			}
			catch (Exception exc)
			{
				dbLogger.WriteAndSendError( "SMHandlerDbInterface.Authenticate", MessagesCode.ErrorContactingDb, exc.Message);
				msgInfos.AddMessage(new MessageInfo(MessagesCode.ErrorContactingDb));
				return false;
			}
			finally
			{
				myConnection.Close();
			}
			
			bool ok = EvaluateLogin(myLoginParameters, password, msgInfos);
			if (!ok)
				return false;
			
			isCDS = AziendaIsCDS(myLoginParameters.CodiceAzienda, msgInfos, out ragSoc);
			codiceAzienda = myLoginParameters.CodiceAzienda;		
			logger.RagSociale = ragSoc;
			logger.CodAzienda = codiceAzienda;
			return true;	
		}

		/// <summary>
		/// Specifica se il prodotto è censito sul PAI
		/// "SELECT PAI_Prodotti.Attivo  FROM PAI_Prodotti WHERE PAI_Prodotti.Signature=@Sign and PAI_Prodotti.Rivend=@Riv";
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsRegisteredProduct(string codiceAzienda, string product, MessagesInfoWriter msgInfos, out bool active)
		{
			active = true;
			string mySqlString = Queries.SelectProductActive();
			SqlDataReader dr = null;
			SqlConnection myConnection = new SqlConnection(connectionstring);
			SqlCommand myCommand = new SqlCommand(mySqlString, myConnection);
			myCommand.Parameters.AddWithValue("@Sign", product);
			myCommand.Parameters.AddWithValue("@Riv", codiceAzienda);
			ParametriLogin myLoginParameters = new ParametriLogin();
			try
			{
				myConnection.Open();
				dr = myCommand.ExecuteReader();
				if (dr.Read())
				{
					active = String.Compare("1", dr[Field.Active()].ToString(), true) == 0;
					return true;
				}
				return false;
				
			}
			catch (Exception exc)
			{
				msgInfos.AddMessage(new MessageInfo(MessagesCode.ErrorContactingDb));
				dbLogger.WriteAndSendError("SMHandlerDbInterface.IsRegisteredProduct", MessagesCode.ErrorContactingDb, exc.Message);
				return false;
			}
			finally
			{
				dr.Close();
				myConnection.Close();
			}
		}

		//---------------------------------------------------------------------
		public static bool IntegratedSolutionExists(string solutionName)
		{
			try
			{
				if (connectionstring == null || connectionstring.Length == 0)
					SetPaiConnectionString();
				SqlConnection myConnection	= new SqlConnection(connectionstring);
				myConnection.Open();
				SqlCommand mySqlCommand	= new SqlCommand(Queries.SelectSolution(), myConnection);
				mySqlCommand.Parameters.AddWithValue("@Signature", solutionName);
				int recordsCount = (int)mySqlCommand.ExecuteScalar();
				return (recordsCount > 0);	
			}
			catch (Exception exc)
			{
				dbLogger.WriteAndSendError("SMHandlerDbInterface.RegisterIntegratedSolution", MessagesCode.ErrorContactingDb, exc.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------
		public static bool RegisterIntegratedSolution(string companyCode, string loginID, string solutionName, string description, string webServiceUrl, MessagesInfoWriter msgInfos)
		{
			try
			{
				string prodCode = GetProductCode(companyCode, msgInfos);
				if (prodCode != String.Empty)
				{
					SqlConnection myConnection	= new SqlConnection(connectionstring);
					myConnection.Open();
					SqlTransaction myTrans = null;
					myTrans	= myConnection.BeginTransaction();
					//query1
					SqlCommand mySqlCommand	= new SqlCommand(Queries.InsertNewSolution(), myConnection);
					mySqlCommand.Parameters.AddWithValue("@CodProd",			prodCode);
					mySqlCommand.Parameters.AddWithValue("@Descri",			description);
					mySqlCommand.Parameters.AddWithValue("@Attivo",			true);
					mySqlCommand.Parameters.AddWithValue("@Signature",		solutionName);
					mySqlCommand.Parameters.AddWithValue("@Rivend",			companyCode);
					mySqlCommand.Parameters.AddWithValue("@UrlWebService",	webServiceUrl);
					mySqlCommand.Parameters.AddWithValue("@DataAgg",			DateTime.Today);
					mySqlCommand.Parameters.AddWithValue("@Utente",			loginID);
					mySqlCommand.Parameters.AddWithValue("@NrSerie",			Convert.ToInt32(233281));
					mySqlCommand.Transaction = myTrans;
					//query2
					SqlCommand mySqlCommand2	= new SqlCommand(Queries.SelectSolution(), myConnection);
					mySqlCommand2.Parameters.AddWithValue("@Signature", solutionName);
					mySqlCommand2.Transaction = myTrans;
					try
					{
						int recordsCount = (int)mySqlCommand2.ExecuteScalar();
						if (recordsCount > 0)
							throw new Exception("This solution name is already registered!");

					}
					catch (Exception exc)
					{
						if (myTrans != null)
							myTrans.Rollback();
						msgInfos.AddMessage(new MessageInfo(MessagesCode.ErrorContactingDb));
						dbLogger.WriteAndSendError("SMHandlerDbInterface.RegisterIntegratedSolution", MessagesCode.ErrorContactingDb, exc.Message);
						return false;
					}

					try
					{
						mySqlCommand.ExecuteNonQuery();
					}
					catch (Exception exc)
					{
						if (myTrans != null)
							myTrans.Rollback();
						msgInfos.AddMessage(new MessageInfo(MessagesCode.ErrorContactingDb));
						dbLogger.WriteAndSendError("SMHandlerDbInterface.RegisterIntegratedSolution", MessagesCode.ErrorContactingDb, exc.Message);
						return false;
					}
					finally
					{
						myTrans.Commit();
						myConnection.Close();
						myConnection.Dispose();
					}
					return true;
				}
				else //superato il numero massimo di verticali registrati
				{
					msgInfos.AddMessage(new MessageInfo(MessagesCode.ErrorContactingDb));
					dbLogger.WriteAndSendError("SMHandlerDbInterface.RegisterIntegratedSolution", MessagesCode.ErrorContactingDb, string.Empty);
					return false;

				}
			}
			catch (Exception exc)
			{
				msgInfos.AddMessage(new MessageInfo(MessagesCode.ErrorContactingDb));
				dbLogger.WriteAndSendError("SMHandlerDbInterface.RegisterIntegratedSolution", MessagesCode.ErrorContactingDb, exc.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------
		public static string GetProdID(string codAz, string productSignature)
		{
			string prodid;
			string mySqlString = Queries.SelectProdID();
			SqlDataReader dr = null;
			SqlConnection myConnection = new SqlConnection(connectionstring);
			SqlCommand myCommand = new SqlCommand(mySqlString, myConnection);
			myCommand.Parameters.AddWithValue("@Sign", productSignature);

			try
			{
				myConnection.Open();
				dr = myCommand.ExecuteReader();

				if (dr.Read())
				{
					prodid = dr[Field.ProductID()].ToString();
					if (prodid != null && prodid.Length > 0)
						return prodid;
				}
				return null;
			}
			catch (Exception exc)
			{
				//msgInfos.AddMessage(new MessageInfo(MessagesCode.ErrorContactingDb));
				dbLogger.WriteAndSendError("SMHandlerDbInterface.GetProdID", MessagesCode.ErrorContactingDb, exc.Message);
				return null;
			}
			finally
			{
				dr.Close();

				myConnection.Close();
			}

		}

		//---------------------------------------------------------------------
		private static string GetProductCode(string companyCode, MessagesInfoWriter msgInfos)
		{
			string prodCode				= string.Empty;
			string exaDecProg			= string.Empty;
			string companyCodeID		= companyCode.Substring(companyCode.Length - 4,4);
			SqlConnection myConnection	= new SqlConnection(connectionstring);
			string mySqlString			= Queries.SelectProductCode();
			SqlCommand myCommand		= new SqlCommand(mySqlString, myConnection);
			myCommand.Parameters.AddWithValue("@RivendID", companyCodeID+"%");
			SqlDataAdapter myAdapter	= new SqlDataAdapter(myCommand);
			DataSet ds					= new DataSet();
			try
			{
				myAdapter.Fill(ds, "ProdCode");
				DataView myDataView = ds.Tables["ProdCode"].DefaultView;
				if (ds.Tables["ProdCode"].Rows.Count != 0)
				{
					string exaDecString = ((ds.Tables["ProdCode"].Rows[0][Field.ProductCode()]).ToString()).Substring(4);
					int exaDecInt = Int32.Parse(exaDecString,System.Globalization.NumberStyles.HexNumber);
					exaDecProg = (exaDecInt + 1).ToString("X");
				}
				else
					exaDecProg = "0001";
				if (exaDecProg.Length <= 4)
				{
					prodCode += companyCodeID;
					int i = 0;
					while (i < (4 - exaDecProg.Length))
					{
						prodCode += "0";
						i += 1;
					}	
					prodCode += exaDecProg;
				}
					
				return prodCode;
			}
			catch (Exception exc)
			{
				msgInfos.AddMessage(new MessageInfo(MessagesCode.ErrorContactingDb));
				dbLogger.WriteAndSendError("SMHandlerDbInterface.GetProductCode", MessagesCode.ErrorContactingDb, exc.Message);
				return null;
			}

			finally
			{
				ds.Dispose();
				myCommand.Dispose();
				myAdapter.Dispose();
				myConnection.Close();
			}
		}

		//---------------------------------------------------------------------
		public static IntegratedSolution[] GetIntegratedSolutions(string companyCode, string loginID, MessagesInfoWriter msgInfos)
		{
			bool active = true;
			string signature, prodCode, description, url;
			SqlDataReader dr = null;
			ArrayList list = new ArrayList();
			string mySqlString = Queries.SelectProduct();

			SqlConnection myConnection = new SqlConnection(connectionstring);
			SqlCommand myCommand = new SqlCommand(mySqlString, myConnection);
			myCommand.Parameters.AddWithValue("@Riv", companyCode);
			try
			{
				myConnection.Open();
				dr = myCommand.ExecuteReader();
				while (dr.Read())
				{
					active = String.Compare("1", dr[Field.Active()].ToString(), true) == 0;
					signature = dr[Field.Signature()].ToString();
					description = dr[Field.Description()].ToString();
					prodCode = dr[Field.ProductCode()].ToString();
					
					url = dr[Field.Url()].ToString();
					
					if (active)
					{
						string[] productCodes = GetProductCodes(prodCode, msgInfos);
						bool isEmbedded = productCodes.Length > 0;
						list.Add(
							new IntegratedSolution(
							loginID,
							companyCode,
							signature, 
							true, 
							false, 
							description, 
							url, 
							isEmbedded ? SolutionType.Embedded : SolutionType.VerticalIntegration, 
							productCodes, 
							string.Empty
							));
					}
				}
				return (IntegratedSolution[])list.ToArray(typeof(IntegratedSolution));
				
			}
			catch (Exception exc)
			{
				msgInfos.AddMessage(new MessageInfo(MessagesCode.ErrorContactingDb));
				dbLogger.WriteAndSendError("SMHandlerDbInterface.GetIntegratedSolutions", MessagesCode.ErrorContactingDb, exc.Message);
				return null;
			}
			finally
			{
				dr.Close();
				myConnection.Close();
			}
		}

		/// <summary>
		/// Specifica se il produttore ha licenza di Centro di Sviluppo Autorizzato
		///"SELECT TipologiaMPP, RagSoc FROM PAI_Rivenditori WHERE rivend=@Riv";
		///23/01/09--> non si controlla più
		/// </summary>
		//---------------------------------------------------------------------
		private static bool AziendaIsCDS(string codAz, MessagesInfoWriter msgInfos, out string ragSoc)
		{
			ragSoc = String.Empty;
			string mySqlString = Queries.SelectSalesRoleType();

			SqlConnection myConnection = new SqlConnection(connectionstring);
			SqlCommand myCommand = new SqlCommand(mySqlString, myConnection);
			myCommand.Parameters.AddWithValue("@Riv", codAz);
			//ParametriLogin myLoginParameters = new ParametriLogin();
//			string rivend = null;
			SqlDataReader dr = null;
			try
			{
				myConnection.Open();
				dr = myCommand.ExecuteReader();

				if (dr.Read())
				{
					ragSoc		= dr[Field.CompanyName()].ToString();
					/*
					rivend      = dr[Field.SalesRoleType()].ToString();
					if (rivend == "35717123")//Centro di Sviluppo
						return true;
					*/
				}
				return true;
				/*//verifico che non sia un non-CDS ma che può fare soluzioni magiclink
				myCommand = new SqlCommand(Queries.SelectSupportContractCodeMagic(), myConnection);
				myCommand.Parameters.AddWithValue("@CodAz", codAz);
				dr = myCommand.ExecuteReader();
				if (dr.Read())		
					return true;
				else
				{
					msgInfos.AddMessage(new MessageInfo(MessagesCode.NotAuthorizedCdS));
					return false;
				}
				*/
			}
			catch (Exception exc)
			{
				dbLogger.WriteAndSendError( "SMHandlerDbInterface.AziendaIsCDS", MessagesCode.ErrorContactingDb, exc.Message);
				msgInfos.AddMessage(new MessageInfo(MessagesCode.ErrorContactingDb));
				return false;
			}
			finally
			{
				if (dr != null)
					dr.Close();
				myConnection.Close();
			}
			
		}

		/// <summary>
		/// Specifica se il produttore ha licenza per fare verticali ed embedding
		/// "SELECT PAI_Assist.Rivend FROM PAI_Assist WHERE PAI_Assist.CodFam = 'TBNETNFS' and PAI_Assist.Disattivo = '0' and PAI_Assist.Rivend=@CodAz";
		/// 23/01/09--> non si controlla più
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsEmbedded(string codAz, out bool error)
		{
			error = false;
			return true;
			/*
			
			string mySqlString = Queries.SelectReseller();

			SqlConnection myConnection = new SqlConnection(connectionstring);
			SqlCommand myCommand = new SqlCommand(mySqlString, myConnection);
			myCommand.Parameters.AddWithValue("@CodAz", codAz);
			try
			{
				myConnection.Open();
				SqlDataReader dr = myCommand.ExecuteReader();
				return(dr.Read());
			}
			catch (Exception exc)
			{
				dbLogger.WriteAndSendError( "SMHandlerDbInterface.IsEmbedded", MessagesCode.ErrorContactingDb, exc.Message);
				error = true;
				return false;
			}
			finally
			{
				myConnection.Close();
			}*/
		}

		/// <summary>
		/// Legge dal PAI i namespace di mago permessi per il produttore e il salesModule in questione
		/// "SELECT PAI_ProdottiModuliFunzionali.Modulo FROM PAI_ProdottiModuliFunzionali, PAI_Prodotti WHERE PAI_ProdottiModuliFunzionali.CodProd = PAI_Prodotti.CodProd and PAI_Prodotti.Signature=@Sign and PAI_Prodotti.Rivend=@Riv";
		/// </summary>
		//---------------------------------------------------------------------
		public static ArrayList GetContainedPermitted(string productSignature, string codAz, string serverShortName)
		{
			string mySqlString = Queries.SelectModuleCode();
			SqlConnection myConnection = new SqlConnection(connectionstring);
			SqlCommand myCommand = new SqlCommand(mySqlString, myConnection);
			myCommand.Parameters.AddWithValue("@Sign", productSignature);
			myCommand.Parameters.AddWithValue("@Riv", codAz);
			myCommand.Parameters.AddWithValue("@Servershortname", serverShortName);
			ArrayList list = new ArrayList();
			try
			{
				myConnection.Open();
				SqlDataReader dr = myCommand.ExecuteReader();

				while(dr.Read())
				{
					list.Add(dr[0].ToString());
				}
				return list;
			}
			catch (Exception exc)
			{
				dbLogger.WriteAndSendError( "SMHandlerDbInterface.GetContainedPermitted", MessagesCode.ErrorContactingDb, exc.Message);
				return null;
			}
			finally
			{
				myConnection.Close();
			}
		}

		/// <summary>
		/// Specifica se il produtore ha licenza full per StandAlone
		/// "SELECT * FROM PAI_Assist WHERE PAI_Assist.CodFam = 'TB.NET' and PAI_Assist.Disattivo = '0' and PAI_Assist.Rivend=@CodAz";
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsFull(string codAz, out bool error)
		{
			error = false;
			string mySqlString = Queries.SelectSupportContractCode();

			SqlConnection myConnection = new SqlConnection(connectionstring);
			SqlCommand myCommand = new SqlCommand(mySqlString, myConnection);
			myCommand.Parameters.AddWithValue("@CodAz", codAz);
			try
			{
				myConnection.Open();
				SqlDataReader dr = myCommand.ExecuteReader();
				return dr.Read();
			}
			catch (Exception exc)
			{
				dbLogger.WriteAndSendError( "SMHandlerDbInterface.IsFull", MessagesCode.ErrorContactingDb, exc.Message);
				error = true;
				return false;
			}
			finally
			{
				myConnection.Close();
			}
		}

		/// <summary>
		/// Specifica se il produtore ha licenza full per StandAlone
		/// "SELECT * FROM PAI_Assist WHERE PAI_Assist.CodFam = 'TB.NET' and PAI_Assist.Disattivo = '0' and PAI_Assist.Rivend=@CodAz";
		/// </summary>
		//---------------------------------------------------------------------
		public static string GetMailTo()
		{
			string mySqlString = Queries.SelectMailTo();
			if (mySqlString == null || Field.Info() == null)
				return null;

			SqlConnection myConnection = new SqlConnection(connectionstring);
			SqlCommand myCommand = new SqlCommand(mySqlString, myConnection);
			myCommand.Parameters.AddWithValue("@TagName", "SMCRYPTING");
			try
			{
				myConnection.Open();
				SqlDataReader dr = myCommand.ExecuteReader();
				if (dr.Read())	
					return dr[Field.Info()].ToString();
			}
			catch (Exception exc)
			{
				dbLogger.WriteAndSendError( "SMHandlerDbInterface.GetMailTo", MessagesCode.ErrorContactingDb, exc.Message);
				return null;
			}
			finally
			{
				myConnection.Close();
			}
			return null;
		}


		/// <summary>
		/// Valuta la login se può essere ritenuta valida
		/// </summary>
		//------------------------------------------------------------------------------
		private static bool EvaluateLogin(ParametriLogin myLoginParameters, string password, MessagesInfoWriter msgInfos)
		{
			
			if (myLoginParameters == null)
			{
				dbLogger.WriteAndSendError("SMHandlerDbInterface.EvaluateLogin", MessagesCode.AuthenticationFailed, String.Format("The credential supplied are wrong, login: {0}.", myLoginParameters.LoginID));
				msgInfos.AddMessage(new MessageInfo(MessagesCode.AuthenticationFailed));
				return false;
			}
			
			if (myLoginParameters.LoginBloccata)
			{
				dbLogger.WriteAndSendError("SMHandlerDbInterface.EvaluateLogin", MessagesCode.LockedLogin, String.Format("Login: {0} blocked", myLoginParameters.LoginID));
				msgInfos.AddMessage(new MessageInfo(MessagesCode.LockedLogin));
				return false;
			}
			if (myLoginParameters.LoginDisabilitata)
			{
				dbLogger.WriteAndSendError("SMHandlerDbInterface.EvaluateLogin", MessagesCode.DisabledLogin, String.Format("Login: {0} disabled", myLoginParameters.LoginID));
				msgInfos.AddMessage(new MessageInfo(MessagesCode.DisabledLogin));
				return false;
			}
			//TOLGO CONTROLLO PERCHÈ CDS ESTERI HANNO LOGIN PER SITO INGLESE
//			if (myLoginParameters.SitoInglese)
//			{
//				dbLogger.WriteAndSendError("SMHandlerDbInterface.EvaluateLogin", MessagesCode.OnlyEnglishSite, String.Format("Login: {0} valida solo per sito inglese", myLoginParameters.LoginID));
//				msgInfos.AddMessage(new MessageInfo(MessagesCode.OnlyEnglishSite));
//				return false;
//			}

			//la password non corrisponde:
			if (myLoginParameters.Password != GetHashedPassword(password))
			{
				//CASO IN CUI LA PASSWORD INSERITA E CRIPTATA E' 
				//DIVERSA DA QUELLA MEMORIZZATA NEL DATABASE
				string errorIncrementaTentativi = string.Empty;
				bool updateTentativiErrati = false;
				bool loginBloccata = false;

				//Se bisogna cambiare la password:
				if (myLoginParameters.CambiaPwd)
				{
					//CASO IN CUI CAMBIAPWD = TRUE;
					if (myLoginParameters.Password != password)
					{
						//PASSWORD MEMORIZZATA NEL DATABASE DIVERSA 
						//DA PASSWORD INSERITA NON CRIPTATA
						msgInfos.AddMessage(new MessageInfo(MessagesCode.AuthenticationFailed));
						dbLogger.WriteAndSendError("SMHandlerDbInterface.EvaluateLogin", MessagesCode.AuthenticationFailed, String.Format("The credential supplied are wrong, login: {0}.", myLoginParameters.LoginID));
						updateTentativiErrati = IncrementaTentativiErrati(myLoginParameters.LoginID, myLoginParameters.UltimoTentativo, myLoginParameters.NumeroTentativo, out errorIncrementaTentativi, out loginBloccata);
						if(!updateTentativiErrati)
						{
							//CASO IN CUI L'INCREMENTO DEI TENTATIVI EFFETTUATI NON E' ANDATO A BUON FINE
							dbLogger.WriteAndSendError("SMHandlerDbInterface.EvaluateLogin", MessagesCode.ErrorContactingDb, errorIncrementaTentativi);
							msgInfos.AddMessage(new MessageInfo(MessagesCode.ErrorContactingDb));
							return false;
						}
						if (loginBloccata)
						{
							dbLogger.WriteAndSendError("SMHandlerDbInterface.EvaluateLogin", MessagesCode.TooMuchLoginErrors, String.Format("Login {0} blocked for excess of failed attempt.", myLoginParameters.LoginID));
							msgInfos.AddMessage(new MessageInfo(MessagesCode.TooMuchLoginErrors));
						}
						return false;

					}
					else //se la login è valida ma la psw è da cambiare perchè in chiaro, li obbligo prima ad andare sul sito a cambiarla
					{
						dbLogger.WriteAndSendError("SMHandlerDbInterface.EvaluateLogin", MessagesCode.PasswordToChange, String.Format("Login {0} is temporary, it must be changed.", myLoginParameters.LoginID));
						msgInfos.AddMessage(new MessageInfo(MessagesCode.PasswordToChange));
						return false;
					}
				}
				else
				{
					//CASO IN CUI CAMBIAPWD = FALSE;
					msgInfos.AddMessage(new MessageInfo(MessagesCode.AuthenticationFailed));
					dbLogger.WriteAndSendError("SMHandlerDbInterface.EvaluateLogin", MessagesCode.AuthenticationFailed, String.Format("The credential supplied are wrong, login: {0}.", myLoginParameters.LoginID));
					updateTentativiErrati = IncrementaTentativiErrati(myLoginParameters.LoginID, myLoginParameters.UltimoTentativo, myLoginParameters.NumeroTentativo, out errorIncrementaTentativi, out loginBloccata);
					if(!updateTentativiErrati)
					{
						//CASO IN CUI L'INCREMENTO DEI TENTATIVI EFFETTUATI NON E' ANDATO A BUON FINE
						dbLogger.WriteAndSendError("SMHandlerDbInterface.EvaluateLogin", MessagesCode.ErrorContactingDb, errorIncrementaTentativi);
						msgInfos.AddMessage(new MessageInfo(MessagesCode.ErrorContactingDb));
						return false;
					}
					if (loginBloccata)
					{
						dbLogger.WriteAndSendError("SMHandlerDbInterface.EvaluateLogin", MessagesCode.TooMuchLoginErrors, String.Format("Login {0} blocked for excess of failed attempt.", myLoginParameters.LoginID));
						msgInfos.AddMessage(new MessageInfo(MessagesCode.TooMuchLoginErrors));
					}
					return false;
				}
			}		

			if (myLoginParameters.PasswordScaduta)
			{
				dbLogger.WriteAndSendError("SMHandlerDbInterface.EvaluateLogin", MessagesCode.PasswordExpired, String.Format("Password expired for login: {0}", myLoginParameters.LoginID));
				msgInfos.AddMessage(new MessageInfo(MessagesCode.PasswordExpired));
				return false;
			}

			//sE LA PASSWORD PASSA TUTTI I CONTROLLI UPDATO IL DB
			string error = string.Empty;
			bool UpdateDataAccessoLogin = UpdateDataLogin(myLoginParameters.LoginID, out error);
			if(!UpdateDataAccessoLogin)
			{
				dbLogger.WriteAndSendError("SMHandlerDbInterface.EvaluateLogin", MessagesCode.ErrorContactingDb, error);
				msgInfos.AddMessage(new MessageInfo(MessagesCode.ErrorContactingDb));
				return false;
			}
			return true;
		}

		/// <summary>
		/// Aggiorna i campi in caso di login a buon fine
		/// "UPDATE PAI_InternetLogins SET PAI_InternetLogins.UltimaLogin = @DataUltimaLogin, PAI_InternetLogins.UltimoTentativo= @DataDefault,  PAI_InternetLogins.NrTentativi= @NumeroDiDefault WHERE PAI_InternetLogins.LoginID = @Username" 
		/// </summary>
		//------------------------------------------------------------------------------
		//Nel caso di Login andata a buon fine, fa l'update di DateTime.Now nel campo
		//DataUltimaLogin
		//------------------------------------------------------------------------------
		private static bool  UpdateDataLogin(string username, out string error)
		{
			error = string.Empty;
			string UpdateDataInternetLogins = Queries.UpdateLoginOk();	
			SqlConnection myConnectionUpdate = new SqlConnection(connectionstring);
			SqlCommand mySqlCommandUpdateDataInternetLogins	= new SqlCommand(UpdateDataInternetLogins, myConnectionUpdate);
			mySqlCommandUpdateDataInternetLogins.Parameters.AddWithValue("@DataUltimaLogin", DateTime.Now);
			mySqlCommandUpdateDataInternetLogins.Parameters.AddWithValue("@DataDefault", new DateTime(1799,12,31,0,0,0));
			mySqlCommandUpdateDataInternetLogins.Parameters.AddWithValue("@NumeroDiDefault",Convert.ToInt32(0));
			mySqlCommandUpdateDataInternetLogins.Parameters.AddWithValue("@Username", username);
			SqlTransaction myTransUpdate = null;
			try
			{
				myConnectionUpdate.Open();
				myTransUpdate	= myConnectionUpdate.BeginTransaction();
				mySqlCommandUpdateDataInternetLogins.Transaction = myTransUpdate;
				mySqlCommandUpdateDataInternetLogins.ExecuteNonQuery();
				myTransUpdate.Commit();
				return true;
			}
			catch (SqlException ex)
			{
				if (myTransUpdate != null)
					myTransUpdate.Rollback();
				error = ex.Message;
				return false;
			}
			finally
			{
				myConnectionUpdate.Close();
			}
		}

		/// <summary>
		/// Aggiorna il db in caso di login fallita
		///@"UPDATE PAI_InternetLogins SET PAI_InternetLogins.UltimoTentativo = @DataUltimoTentativo,  PAI_InternetLogins.NrTentativi = @NumeroTentativo,  PAI_InternetLogins.LoginBloccato = @Bloccato WHERE PAI_InternetLogins.LoginID = @Username"
		/// </summary>
		//------------------------------------------------------------------------------
		//Nel caso di Login non andata a buon fine, aggiorna i campi:
		//- UltimoTentativo (data ultimo tentativo)
		//- NrTentativi (numero di tentativi consecutivi errati)
		//- LoginBLoccato (campo che determina se la login è errata)
		//------------------------------------------------------------------------------
		private static bool IncrementaTentativiErrati(string username, DateTime DataUltimoTentativo, int NumeroTentativiErrati, out string error, out bool tentativiesauriti)
		{
			error = string.Empty;
			tentativiesauriti = false;
			string UpdateIncrementiTentativiInternetLogins = Queries.UpdateLoginToLock();
			SqlConnection myConnectionUpdateTentativi = new SqlConnection(connectionstring);
			SqlCommand mySqlCommandUpdateTentativi		= new SqlCommand(UpdateIncrementiTentativiInternetLogins, myConnectionUpdateTentativi);
			mySqlCommandUpdateTentativi.Parameters.AddWithValue("@Username", username);
			if (  ((DateTime) DataUltimoTentativo).AddMinutes(30) > DateTime.Now)
			{ 
				//CASO IN CUI L'ULTIMO FALLIMENTO DI LOGIN RISALE A MENO DI 30 MINUTI PRIMA
				mySqlCommandUpdateTentativi.Parameters.AddWithValue("@DataUltimoTentativo", DateTime.Now);
				mySqlCommandUpdateTentativi.Parameters.AddWithValue("@NumeroTentativo", NumeroTentativiErrati + 1);		
				if (NumeroTentativiErrati == 4)
				{
					//CASO IN CUI I TENTATIVI ERRATI ERANO GIA' IL MASSIMO CONSENTITO
					tentativiesauriti = true;
					mySqlCommandUpdateTentativi.Parameters.AddWithValue("@Bloccato", "1");
				}
				else
				{
					//CASO IN CUI I TENTATIVI ERRATI NON ERANO ANCORA IL MASSIMO CONSENTITO
					mySqlCommandUpdateTentativi.Parameters.AddWithValue("@Bloccato", "0");
				}
			}
			else
			{
				//CASO IN CUI L'ULTIMO FALLIMENTO DI LOGIN RISALE A PIU' DI 30 MINUTI PRIMA
				mySqlCommandUpdateTentativi.Parameters.AddWithValue("@DataUltimoTentativo", DateTime.Now);
				mySqlCommandUpdateTentativi.Parameters.AddWithValue("@NumeroTentativo", 1);		
				mySqlCommandUpdateTentativi.Parameters.AddWithValue("@Bloccato", "0");
			}
			SqlTransaction myTransUpdateTentativi			= null;
			try
			{
				myConnectionUpdateTentativi.Open();
				myTransUpdateTentativi	= myConnectionUpdateTentativi.BeginTransaction();
				mySqlCommandUpdateTentativi.Transaction = myTransUpdateTentativi;
				mySqlCommandUpdateTentativi.ExecuteNonQuery();
				myTransUpdateTentativi.Commit();
				return true;
			}
			catch (SqlException ex)
			{
				if (myTransUpdateTentativi != null)
					myTransUpdateTentativi.Rollback();
				tentativiesauriti = false;
				error = ex.Message;
				return false;
			}
			finally
			{
				myConnectionUpdateTentativi.Close();
			}
		}

		/// <summary>
		/// Ritorna la password criptata secondo la logica di business adottata
		/// nella tabella PAI_InternetLogins del PAI.
		/// </summary>
		/// <param name="password">La password in chiaro</param>
		/// <returns>La password criptata</returns>
		//------------------------------------------------------------------------------
		private static string GetHashedPassword(string password)
		{
			string hashedPassword;
			if (password == "" || password == null)
				hashedPassword = "";
			else
			{
				hashedPassword = PasswordCryptoServiceProvider.Encrypt(password);
			}
			return hashedPassword;
		}

		#region Rimborso IVA Veicoli
		/// <summary>
		/// Verifica il codice di attivazione.
		/// </summary>
		/// <param name="VATNr">Il numero di Partita IVA inserito dall'utente al momento del pagamento.</param>
		/// <param name="productCode">Il codice del prodotto acquistato.</param>
		/// <param name="registrationCode">Il codice di registrazione del programma.</param>
		/// <param name="returnCode">Codici restituiti: 0, -20, -21, -40, -50.</param>
		/// <returns>true se il codice di registrazione è buono (registrato, pagato e non disabilitato), false altrimenti.</returns>
		//---------------------------------------------------------------------
		public static bool RimborsoIVAVeicoli_CheckRegistrationCode(string VATNr, string productCode, string registrationCode, out int returnCode, out int subscriptionID, out DateTime registrationDate)
		{
			returnCode = 0;
			// Codice della tabella ELS_SUBSCRIPTIONS per l'utente anonimo
			subscriptionID = Field.AnonymousUserCode;
			// Standard di PAI per le date "vuote"
			registrationDate = Field.PAIDefaultDate;

			if (connectionstring == null || connectionstring.Length == 0)
				SetPaiConnectionString();

			SqlConnection myConnection = new SqlConnection(connectionstring);
			SqlCommand myCommand = new SqlCommand(Queries.RimborsoIVAVeicoli_SelectRegistrationCode(), myConnection);
			myCommand.Parameters.AddWithValue("@VATNr", VATNr);
			myCommand.Parameters.AddWithValue("@ProductCode", productCode);
			myCommand.Parameters.AddWithValue("@RegistrationCode", registrationCode);
			SqlDataReader dr = null;

			try
			{
				myConnection.Open();
				dr = myCommand.ExecuteReader();

				if (!dr.HasRows)
				{
					returnCode = -22;	// RIFIUTO: coppia VATNr/registrationCode non individuata nel database
					return false;
				}

				// Assumiamo che vi sia una sola riga, come deve essere
				dr.Read();
				subscriptionID = (int)dr["SubscriptionID"];
				object accepted = dr["Accepted"]; // pagamento accettato (0,1)
				object disabled = dr["Disabled"]; // rifiuto per disabilitazione (0,1)
				registrationDate = (DateTime)dr["RegistrationDate"];

				if (accepted == DBNull.Value || (string)accepted == "0")
				{
					returnCode = -21;	// RIFIUTO: pagamento rifiutato/annullato/non ancora effettuato
					return false;
				}

				if (disabled == DBNull.Value || (string)disabled == "1")
				{
					returnCode = -40;	// RIFIUTO: è stato disabilitato l'utente o il codice di attivazione o il pagamento
					return false;
				}
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				returnCode = -50;		// RIFIUTO: generico problema lato server
				return false;
			}
			finally
			{
				dr.Close();
			}

			return true;
		}

		//---------------------------------------------------------------------
		public static bool RimborsoIVAVeicoli_RegisterDate(int subscriptionID, string VATNr, string productCode, string registrationCode, out int returnCode)
		{
			returnCode = 0;

			if (connectionstring == null || connectionstring.Length == 0)
				SetPaiConnectionString();

			SqlConnection myConnection = new SqlConnection(connectionstring);
			SqlCommand myCommand = new SqlCommand(Queries.RimborsoIVAVeicoli_UpdateRegistrationDate(), myConnection);
			myCommand.Parameters.AddWithValue("@RegistrationDate", DateTime.Now);
			myCommand.Parameters.AddWithValue("@SubscriptionID", subscriptionID);
			myCommand.Parameters.AddWithValue("@VATNr", VATNr);
			myCommand.Parameters.AddWithValue("@ProductCode", productCode);
			myCommand.Parameters.AddWithValue("@RegistrationCode", registrationCode);

			try
			{
				myConnection.Open();
				int rows = myCommand.ExecuteNonQuery();

				if (rows < 1)
				{
					returnCode = -41;	// RIFIUTO: utente disabilitato "al volo" che però aveva già superato controlli preliminari
					return false;
				}

				// Non dovrebbe mai verificarsi... trovate più righe in ELC_SUBSCRIPTIONS
				// Codice positivo perchè l'errore è in Microarea, però probabilmente sono stati trovati più codici identici
				if (rows > 1)
				{
					returnCode = 11;	// ACCETTATO: con errore Microarea in database PAI (più righe presenti)
					return true;
				}
			}
			catch (Exception exc)
			{
				returnCode = -51;		// RIFIUTO: emailAddress e attivazione validi, ma problemi lato server
				dbLogger.WriteAndSendError("SMHandlerDbInterface.RimborsoIVAVeicoli_RegisterDate", MessagesCode.ErrorContactingDb, exc.Message);
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------
		public static bool RimborsoIVAVeicoli_UpdateRegistrationCode(int subscriptionID, string productCode, string registrationCode, DateTime registrationDate, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (connectionstring == null || connectionstring.Length == 0)
				SetPaiConnectionString();

			SqlConnection myConnection = new SqlConnection(connectionstring);
			SqlCommand myCommand = new SqlCommand(Queries.RimborsoIVAVeicoli_UpdateRegistrationCode(), myConnection);
			myCommand.Parameters.AddWithValue("@RegistrationCode", registrationCode);
			myCommand.Parameters.AddWithValue("@RegistrationDate", registrationDate);
			myCommand.Parameters.AddWithValue("@SubscriptionID", subscriptionID);
			myCommand.Parameters.AddWithValue("@ProductCode", productCode);

			try
			{
				myConnection.Open();
				int rows = myCommand.ExecuteNonQuery();

				if (rows != 1)
				{
					errorMessage = "Attenzione! E' successo un casino della madonna!!! Sono state aggiornate " + rows.ToString() + " righe in ELC_SUBSCRIPTIONS!";
					return false;
				}
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				errorMessage = exc.Message;
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------
		public static bool RimborsoIVAVeicoli_InsertUserInfo
			(
			int subscriptionID,
			string isActivated,
			int productVersion,
			string MACAddress,
			string databaseServer,
			string databaseLogin,
			string databaseCompany,
			string programRelease
			)
		{
			if (connectionstring == null || connectionstring.Length == 0)
				SetPaiConnectionString();

			SqlConnection myConnection = new SqlConnection(connectionstring);
			SqlCommand myCommand = new SqlCommand(Queries.RimborsoIVAVeicoli_InsertUserInfo(), myConnection);
			myCommand.Parameters.AddWithValue("@SubscriptionID", subscriptionID);
			myCommand.Parameters.AddWithValue("@IsActivated", isActivated);
			myCommand.Parameters.AddWithValue("@ProductVersion", productVersion);
			myCommand.Parameters.AddWithValue("@MACAddress", MACAddress);
			myCommand.Parameters.AddWithValue("@DatabaseServer", databaseServer);
			myCommand.Parameters.AddWithValue("@DatabaseLogin", databaseLogin);
			myCommand.Parameters.AddWithValue("@DatabaseCompany", databaseCompany);
			myCommand.Parameters.AddWithValue("@ProgramRelease", programRelease);
			myCommand.Parameters.AddWithValue("@LastUpdate", DateTime.Now);

			try
			{
				myConnection.Open();

				if (myCommand.ExecuteNonQuery() != 1)
					return false;
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------
		public static int RimborsoIVAVeicoli_CountRegistrationCode(string registrationCode, string productCode)
		{
			if (connectionstring == null || connectionstring.Length == 0)
				SetPaiConnectionString();

			SqlConnection myConnection = new SqlConnection(connectionstring);
			SqlCommand myCommand = new SqlCommand(Queries.RimborsoIVAVeicoli_CountRegistrationCode(), myConnection);
			myCommand.Parameters.AddWithValue("@RegistrationCode", registrationCode);
			myCommand.Parameters.AddWithValue("@ProductCode", productCode);

			int count = 0;

			try
			{
				myConnection.Open();
				count = (int)myCommand.ExecuteScalar();
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				count = -1;
			}

			return count;
		}
		#endregion

		#region Private methods

		//---------------------------------------------------------------------
		private static string[] GetProductCodes(string productCode, MessagesInfoWriter msgInfos)
		{
			SqlDataReader dr = null;
			ArrayList list = new ArrayList();
			string mySqlString = Queries.SelectProductCodes();
			string itemCode = null;
			SqlConnection myConnection = new SqlConnection(connectionstring);
			SqlCommand myCommand = new SqlCommand(mySqlString, myConnection);
			myCommand.Parameters.AddWithValue("@Code", productCode);

			try
			{
				myConnection.Open();
				dr = myCommand.ExecuteReader();
				while (dr.Read())
				{
					itemCode = dr[Field.ItemCode()].ToString();
					list.Add(itemCode);
				}
				return (string[])list.ToArray(typeof(string));
				
			}
			catch (Exception exc)
			{
				msgInfos.AddMessage(new MessageInfo(MessagesCode.ErrorContactingDb));
				dbLogger.WriteAndSendError("SMHandlerDbInterface.GetIntegratedSolutions", MessagesCode.ErrorContactingDb, exc.Message);
				return new string[]{};
			}
			finally
			{
				dr.Close();
				myConnection.Close();
			}
		}
		#endregion
	}

	/// <summary>
	/// contiene tutti i paramentri legati alla login
	/// </summary>
	//=========================================================================
	public class ParametriLogin
	{
		public string LoginID;
		public string Password;
		public string CodiceAzienda;
		public string CodicePersona;
		public string Email;
		public DateTime DataRegistrazione;
		public bool CambiaPwd;
		public bool ModificaPwdNonConsentita;
		public bool PasswordNonScade;
		public bool LoginDisabilitata;
		public bool LoginBloccata;
		public bool SitoInglese;
		public bool PasswordScaduta;
		public DateTime UltimaLogin;
		public DateTime UltimoTentativo;
		public int NumeroTentativo;
		public bool CambiataPasswordOggi;

	}
}
