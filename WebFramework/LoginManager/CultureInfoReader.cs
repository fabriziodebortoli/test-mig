using System;
using System.Data;
using System.Diagnostics;

using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.WebServices.LoginManager
{
	//=========================================================================
	public class CultureInfoReader
	{
		private IDbConnection sqlConnection;
		private IBasePathFinder	pathFinder;
		
		public const string PreferredLanguage	= "PreferredLanguage";
		public const string ApplicationLanguage	= "ApplicationLanguage";
		
		//-----------------------------------------------------------------------
		public CultureInfoReader(IBasePathFinder pathFinder, IDbConnection sqlConnection )
		{
			this.pathFinder = pathFinder;
			this.sqlConnection = sqlConnection;

			if (sqlConnection.State != ConnectionState.Open)
				sqlConnection.Open();
		}

		/// <summary>
		/// Setta le lingue che erano null a string.Empty
		/// </summary>
		//-----------------------------------------------------------------------
		private void AdjustLanguages(ref string preferredLanguage, ref string applicationLanguage)
		{
			if (preferredLanguage == null)
				preferredLanguage = string.Empty;

			if (applicationLanguage == null)
				applicationLanguage = string.Empty;
		}

		//-----------------------------------------------------------------------
		private bool CheckLanguages(string preferredLanguage, string applicationLanguage)
		{
			return	preferredLanguage	!= null && preferredLanguage	!= string.Empty &&
					applicationLanguage != null && applicationLanguage	!= string.Empty;
		}

		//-----------------------------------------------------------------------
		public bool GetUserLanguages(int loginID, int companyID, bool recursive, out string preferredLanguage, out string applicationLanguage)
		{
			preferredLanguage	= string.Empty;
			applicationLanguage = string.Empty;
			
			string query = string.Format("SELECT {0}, {1} FROM MSD_Logins WHERE LoginId = {2}", PreferredLanguage, ApplicationLanguage, loginID );

			IDataReader aSqlDataReader = null;
			try
			{
				IDbCommand aSqlCommand = sqlConnection.CreateCommand();
				aSqlCommand.CommandText = query;
				aSqlDataReader = aSqlCommand.ExecuteReader(CommandBehavior.SingleRow);

				if (aSqlDataReader.Read())
				{
					preferredLanguage = (string)aSqlDataReader[PreferredLanguage];
					applicationLanguage = (string)aSqlDataReader[ApplicationLanguage];
				}		
			}
			catch(Exception err)
			{
				Debug.Fail(err.Message);
				Exception e = new Exception
					(
					string.Format("Error executing query {0} against database", query),
					err
					);
				throw e;
			}
			finally
			{
				if (aSqlDataReader != null)
					aSqlDataReader.Dispose();
			}

			if (recursive && !CheckLanguages(preferredLanguage, applicationLanguage))
				return GetCompanyLanguages(companyID, recursive, ref preferredLanguage, ref applicationLanguage);

			return true;
		}

		//-----------------------------------------------------------------------
		public bool GetCompanyLanguages(int companyID, bool recursive, ref string preferredLanguage, ref string applicationLanguage)
		{
			string compPreferredLanguage	= string.Empty;
			string compApplicationLanguage	= string.Empty;

			string query = string.Format("SELECT {0}, {1} FROM MSD_Companies WHERE CompanyId = {2}", PreferredLanguage, ApplicationLanguage, companyID);
			
			IDataReader aSqlDataReader = null;
			try
			{
				IDbCommand aSqlCommand = sqlConnection.CreateCommand();
				aSqlCommand.CommandText = query;
				aSqlDataReader = aSqlCommand.ExecuteReader(CommandBehavior.SingleRow);

				if (aSqlDataReader.Read())
				{
					compPreferredLanguage = (string)aSqlDataReader[PreferredLanguage];
					compApplicationLanguage = (string)aSqlDataReader[ApplicationLanguage];
				}
			}
			catch(Exception err)
			{
				Debug.Fail(err.Message);
				Exception e = new Exception
					(
					string.Format("Error executing query {0} against database", query),
					err
					);
				throw e;
			}
			finally
			{
				if (aSqlDataReader != null)
					aSqlDataReader.Close();
			}

			AdjustLanguages(ref compPreferredLanguage, ref compApplicationLanguage);
			if (preferredLanguage == string.Empty || preferredLanguage == null)
				preferredLanguage = compPreferredLanguage;

			if (applicationLanguage == string.Empty || applicationLanguage == null)
				applicationLanguage = compApplicationLanguage;
			
			if (recursive && !CheckLanguages(preferredLanguage, applicationLanguage))
				return GetGlobalLanguages(ref preferredLanguage, ref applicationLanguage);

			return true;
		}

		//-----------------------------------------------------------------------
		private bool GetGlobalLanguages(ref string preferredLanguage, ref string applicationLanguage)
		{
			string globPreferredLanguage = InstallationData.ServerConnectionInfo.PreferredLanguage;
			string globApplicationLanguage = InstallationData.ServerConnectionInfo.ApplicationLanguage;

			AdjustLanguages(ref globPreferredLanguage, ref globApplicationLanguage);

			if (preferredLanguage == string.Empty || preferredLanguage == null)
				preferredLanguage = globPreferredLanguage;

			if (applicationLanguage == string.Empty || applicationLanguage == null)
				applicationLanguage = globApplicationLanguage;

			if (preferredLanguage == string.Empty)
				preferredLanguage = NameSolverStrings.DefaultLanguage;

			if (applicationLanguage == string.Empty)
				applicationLanguage = NameSolverStrings.DefaultLanguage;

			return true;
		}
	}
}
