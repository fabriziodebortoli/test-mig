using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
{
	public class DatabaseChecker : IDatabaseCkecker
	{
		Diagnostic diagnostic = new Diagnostic(Microarea.TaskBuilderNet.Core.DiagnosticManager.Diagnostic.EventLogName);
		private LoginManager currentLoginManager;

		public IDiagnostic Diagnostic
		{
			get
			{
				return diagnostic;
			}
		}

		public DatabaseChecker()
		{
			currentLoginManager = new LoginManager();
			
		}
		//-----------------------------------------------------------------------
		public bool Check(string token)
		{
			currentLoginManager.GetLoginInformation(token);

			// effettuo il check di versione release sul database
			string msg = String.Empty;
			bool valid = false;
			try
			{
				DatabaseCheckError checkDBRet = (DatabaseCheckError)CheckDatabase(out msg);

				switch (checkDBRet)
				{
					case DatabaseCheckError.NoError:
						valid = true; break;

					case DatabaseCheckError.NoDatabase:
						diagnostic.Set(DiagnosticType.Error, string.IsNullOrEmpty(msg) ? WebServicesWrapperStrings.CompanyDatabaseNotPresent : msg);
						valid = false; break;
					case DatabaseCheckError.NoTables:
						diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.CompanyDatabaseTablesNotPresent);
						valid = false; break;
					case DatabaseCheckError.NoActivatedDatabase:
						if (Functions.IsDebug() && currentLoginManager != null && currentLoginManager.IsDeveloperActivation())
							goto case DatabaseCheckError.NoError;
						diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.InvalidDatabaseForActivation);
						valid = false; break;
					case DatabaseCheckError.InvalidModule:
						diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.InvalidDatabaseError);
						valid = false; break;
					case DatabaseCheckError.DBSizeError:
						diagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.DBSizeError);
						valid = false; break;
					case DatabaseCheckError.Sql2012NotAllowedForCompany:
						diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.Sql2012NotAllowedForCompany);
						valid = false; break;
					case DatabaseCheckError.Sql2012NotAllowedForDMS:
						diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.Sql2012NotAllowedForDMS);
						valid = false; break;
					default:
						diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrLoginFailed);
						valid = false; break;
				}
			}
			catch (TBException e)
			{
				diagnostic.Set(DiagnosticType.Error, e.Message);
				valid = false;
			}

			return valid;
		}

		/// <summary>
		/// effettua il controllo sulla struttura del database aziendale. Non lo posso fare lato server a causa dell'utente
		/// ASP nel caso di connessione in winAuthentication. Controlla anche che non sia edition 2012 quando questa non è prevista dai serial number!
		/// </summary>
		//-----------------------------------------------------------------------
		private int CheckDatabase(out string msg)
		{
			int result = 0;
			msg = String.Empty;

			using (TBConnection companyConnection = new TBConnection(currentLoginManager.NonProviderCompanyConnectionString, TBDatabaseType.GetDBMSType(currentLoginManager.ProviderName)))
			{
				try
				{
					companyConnection.Open();

					if (InstallationData.CheckDBSize) VerifyDBSize(companyConnection);

					result = VerifySQL2012Licence(companyConnection);
					if (result != (int)DatabaseCheckError.NoError)
						return result;

					result = (int)TBCheckDatabase.CheckDatabase
					(
					companyConnection,
					currentLoginManager.GetDBNetworkType(),
					BasePathFinder.BasePathFinderInstance,
					currentLoginManager.IsDeveloperActivation()
					);
				}
				catch (TBException e)
				{
					msg = e.Message;
					return (int)DatabaseCheckError.NoDatabase;
				}
			}

			return result;
		}

		//-----------------------------------------------------------------------
		private int VerifySQL2012Licence(TBConnection companyConnection)
		{
			// esegue le verifiche di SQL2012 solo se la licenza non e' corretta
			if (!currentLoginManager.Sql2012Allowed("sbirulino"))
			{
				// prima controllo il database aziendale
				if (TBCheckDatabase.IsSql2012Edition(companyConnection))
					return (int)DatabaseCheckError.Sql2012NotAllowedForCompany;

				// se e' attivato EasyAttachment
				if (currentLoginManager.IsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
				{
					// mi faccio ritornare la stringa di connessione al DMS
					string dmsConnString = currentLoginManager.GetDMSConnectionString(currentLoginManager.AuthenticationToken);
					if (!string.IsNullOrWhiteSpace(dmsConnString))
					{
						using (TBConnection dmsConnection = new TBConnection(dmsConnString, DBMSType.SQLSERVER))
						{
							dmsConnection.Open();
							// controllo l'edizione del db del DMS
							if (TBCheckDatabase.IsSql2012Edition(dmsConnection))
								return (int)DatabaseCheckError.Sql2012NotAllowedForDMS;
						}
					}
				}
			}
			return (int)DatabaseCheckError.NoError;
		}

		//-----------------------------------------------------------------------
		/// <summary>
		/// Verifica se la size del db si sta avvicinando al massimo 
		/// confrontandola con una cifra che è indicata nel server connection config, in modo che sia parametrizzabile
		/// Se la dimensione è compresa tra le cifre limite allora viene rilasciata una messagebox di quelle non ripetibili 
		/// con apposito flag
		/// </summary>
		/// <param name="companyConnection"></param>
		private void VerifyDBSize(TBConnection companyConnection)
		{
			string freePercentage = "";
            if (Functions.IsDBSizeNearMaxLimit(companyConnection.SqlConnect, out freePercentage))
                diagnostic.SetWarning(string.Format(DatabaseLayerStrings.WarningOnDBSize, freePercentage));
		}
	}
}
