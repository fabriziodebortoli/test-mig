using System;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyBuilder
{
    /// <summary>
    /// Supplies information for the current execution context
    /// </summary>
    //======================== NON RIMUOVERE QUESTA CLASSE ===========================
    //                                                                              //
    //    Questa classe non e` usata da nessuna parte in TaskBuilder ma             //
    //    non e` da rimuovere perche` e` usata nel codice delle personalizzazioni   //
    //    di EasyStudio per recuperare informazioni circa il contesto di esecuzione //
    //    dell'applicazione (ve lo scrive chi ci e` gia` passato:-))                //
    //                                                                              //
    //======================== NON RIMUOVERE QUESTA CLASSE ===========================
    public sealed class EasyBuilderContext
	{
		private EasyBuilderContext()
		{ }

		/// <summary>
		/// Returns the user name
		/// </summary>
		public static string User
		{
			get { return CUtility.GetUser(); }
		}

		/// <summary>
		/// Returns the company name
		/// </summary>
		public static string Company
		{
			get { return CUtility.GetCompany(); }
		}

		/// <summary>
		/// Returns true if the current user is administrator
		/// </summary>
		public static bool IsAdmin
		{
			get { return CUtility.IsAdmin(); }
		}
		/// <summary>
		/// Returns the application date
		/// </summary>
		public static DateTime ApplicationDate
		{
			get { return CUtility.GetApplicationDate(); }
		}

		/// <summary>
		/// Returns the WEB server name of the installation
		/// </summary>
		public static string WebServerName
		{
			get { return InstallationData.WebServerName; }
		}
		/// <summary>
		/// Returns the file server name of the installation
		/// </summary>
		public static string FileSystemServerName
		{
			get { return InstallationData.FileSystemServerName; }
		}
		/// <summary>
		/// Returns the installation name
		/// </summary>
		public static string InstallationName
		{
			get { return InstallationData.InstallationName; }
		}
		/// <summary>
		/// Returns the country code of the installation
		/// </summary>
		public static string Country
		{
			get { return InstallationData.Country; }
		}
		/// <summary>
		/// Returns true if the module or functionality is activated
		/// </summary>
		public static bool IsActivated(string application, string moduleOrFunctionality)
		{
			return CUtility.IsActivated(application, moduleOrFunctionality);
		}

	}
}
