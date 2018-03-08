using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Specialized;
using System.IO;
using Newtonsoft.Json;
using TaskBuilderNetCore.EasyStudio.Serializers;
using System.Text;
using System.Linq;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.Common.CustomAttributes;
using Microarea.Common.NameSolver;
using System;
using Microsoft.AspNetCore.Mvc;

namespace TaskBuilderNetCore.EasyStudio.Services
{
	//====================================================================
	[Name("appSvc"), Description("This service manages application structure info and serialization.")]
	[DefaultSerializer(typeof(ApplicationSerializer))]
	public class ApplicationService : Service
	{	
        ApplicationSerializer AppSerializer { get => Serializer as ApplicationSerializer; }
		EasyStudioPreferences preferences;
   
		//---------------------------------------------------------------
		public ApplicationService()
		{
			preferences = new EasyStudioPreferences();
		}

		//---------------------------------------------------------------
		public string GetEasyStudioCustomizationsListFor(string docNS, string user, bool onlyDesignable = true)
		{
			return PathFinder.PathFinderInstance.GetEasyStudioCustomizationsListFor(docNS, user, onlyDesignable);
		}

		//---------------------------------------------------------------
		public string GetCurrentContext(string user, bool getDefault)
		{
			return preferences.GetCurrentContext(user, getDefault);
		}

		//---------------------------------------------------------------
		public bool SetCurrentContext(string appName, string modName, bool isPairDefault)
		{
			return preferences.SetPreferences(appName, modName, isPairDefault);
		}

		//---------------------------------------------------------------
		public string RefreshAll(ApplicationType type)
		{
			PathFinder.PathFinderInstance.ApplicationInfos.Clear();
			return GetAppsModsAsJson(type);
		}

		//---------------------------------------------------------------
		public bool CreateApplication(string applicationName, ApplicationType type)
		{
            if (string.IsNullOrEmpty(applicationName))
            {
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingApplicationName);
                return false;
            }
                
            try
            {
                return AppSerializer.CreateApplication(applicationName, type);
            }
            catch (Exception ex)
            {
                Diagnostic.NotifyMessage(ex);
                return false;
            }
		}

		//---------------------------------------------------------------
		public bool CreateModule(string applicationName, string moduleName)
		{
            if (string.IsNullOrEmpty(applicationName))
            {
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingApplicationName);
                return false;
            }

            if (string.IsNullOrEmpty(moduleName))
            {
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingModuleName);
                return false;
            }

            try
            {
                return AppSerializer.CreateModule(applicationName, moduleName);
            }
            catch (Exception ex)
            {
                Diagnostic.NotifyMessage(ex);
                return false;
            }
		}


		//---------------------------------------------------------------
		public bool DeleteApplication(string applicationName)
		{
            if (string.IsNullOrEmpty(applicationName))
            {
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingApplicationName);
                return false;
            }

            try
            {
                return AppSerializer.DeleteApplication(applicationName);
            }
            catch (Exception ex)
            {
                Diagnostic.NotifyMessage(ex);
                return false;
            }   
		}

        //---------------------------------------------------------------
        public bool DeleteModule(string applicationName, string moduleName)
        {
            if (string.IsNullOrEmpty(applicationName))
            {
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingApplicationName);
                return false;
            }

            if (string.IsNullOrEmpty(moduleName))
            {
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingModuleName);
                return false;
            }

            try
            {
                return AppSerializer.DeleteModule(applicationName, moduleName);
            }
            catch (Exception ex)
            {
                Diagnostic.NotifyMessage(ex);
                return false;
            }
        }

        //---------------------------------------------------------------
        public bool ExistsApplication(string applicationName)
		{
			return AppSerializer.ExistsApplication(applicationName);
		}

		//---------------------------------------------------------------
		public bool ExistsModule(string applicationName, string moduleName)
		{
			return AppSerializer.ExistsModule(applicationName, moduleName);
		}

		//---------------------------------------------------------------
		public bool RenameApplication(string oldName, string newName)
		{
            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
            {
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingApplicationName);
                return false;
            }

            try
            {
                return AppSerializer.RenameApplication(oldName, newName);
            }

            catch (Exception ex)
            {
                Diagnostic.NotifyMessage(ex);
                return false;
            }
		}

		//---------------------------------------------------------------
		public bool RenameModule(string applicationName, string oldName, string newName)
		{
            if (string.IsNullOrEmpty(applicationName))
            {
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingApplicationName);
                return false;
            }

            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
            {
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingModuleName);
                return false;
            }
            try
            {
                return AppSerializer.RenameModule(applicationName, oldName, newName);
            }

            catch (Exception ex)
            {
                Diagnostic.NotifyMessage(ex);
                return false;
            }
		}

		//---------------------------------------------------------------
		public IEnumerable<string> GetApplications(ApplicationType applicationType)
		{
			StringCollection applicationNames = null;
			AppSerializer.PathFinder.GetApplicationsList(applicationType, out applicationNames);

			return applicationNames.Cast<string>().ToList() as IEnumerable<string>;
		}

		//---------------------------------------------------------------
		public IEnumerable<string> GetModules(string applicationName)
		{
            if (string.IsNullOrEmpty(applicationName))
            {
                Diagnostic.Add(DiagnosticType.Error, Strings.MissingApplicationName);
                return null;
            }

            var modules = AppSerializer.PathFinder.GetModulesList(applicationName);

			var moduleNames = new List<string>();
			foreach (ModuleInfo moduleInfo in modules)
			{
				moduleNames.Add(moduleInfo.Name);
			}

			return moduleNames;
		}

		//---------------------------------------------------------------
		public string GetAppsModsAsJson(ApplicationType applicationType)
		{
			var apps = GetApplications(applicationType);

			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			JsonWriter jsonWriter = new JsonTextWriter(sw);
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("allApplications");
			jsonWriter.WriteStartArray();

			foreach (var esApp in apps)
			{
				if (esApp == null) continue;
				var mods = GetModules(esApp);
				foreach (var esMod in mods)
				{
					jsonWriter.WriteStartObject();

					jsonWriter.WritePropertyName("application");
					jsonWriter.WriteValue(esApp);

					jsonWriter.WritePropertyName("module");
					jsonWriter.WriteValue(esMod);
					jsonWriter.WriteEndObject();
				}
			}

			jsonWriter.WriteEndArray();

            LicenceService licenceService = Services.GetService(typeof(LicenceService)) as LicenceService;
            if (licenceService != null)
            {
                jsonWriter.WritePropertyName("DeveloperEd");
			    jsonWriter.WriteValue(licenceService.IsDeveloperEdition);
            }

            jsonWriter.WriteEndObject();
			jsonWriter.Close();
			sw.Close();

			return sw.ToString();
		}
	}

    //=========================================================================
    internal class Strings
    {
        internal static readonly string MissingApplicationName = "Missing parameter applicationName";
        internal static readonly string MissingModuleName = "Missing parameter moduleName";

        internal static readonly string ErrorCreatingObject = "Error Creating Object";
        internal static readonly string ErrorDeletingObject = "Error Deleting Object";

		internal static readonly string ObjectSuccessfullyCreated = "Successfully Created";
		internal static readonly string ObjectSuccessfullyDeleted = "Successfully Deleted";
	}
}

