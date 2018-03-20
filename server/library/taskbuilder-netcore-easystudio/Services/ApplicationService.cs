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

namespace TaskBuilderNetCore.EasyStudio.Services
{
	//====================================================================
	[Name("appSvc"), Description("This service manages application structure info and serialization.")]
	[DefaultSerializer(typeof(ApplicationSerializer))]
	public class ApplicationService : Service
	{
		ApplicationSerializer AppSerializer { get => Serializer as ApplicationSerializer; }
   
		//---------------------------------------------------------------
		public ApplicationService()
		{
		}

		//---------------------------------------------------------------
		public string GetEasyStudioCustomizationsListFor(string docNS, string user, bool onlyDesignable = true)
		{
			var listCustomizations =  PathFinder.GetEasyStudioCustomizationsListFor(docNS, user, onlyDesignable);

			StringWriter sw = new StringWriter(new StringBuilder());
			JsonWriter jsonWriter = new JsonTextWriter(sw);
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("Customizations");

			jsonWriter.WriteStartArray();
			foreach (TBFile customiz in listCustomizations)
			{
				jsonWriter.WriteStartObject();

				jsonWriter.WritePropertyName("customizationName");
				jsonWriter.WriteValue(Path.GetFileNameWithoutExtension(customiz.Name));

				jsonWriter.WritePropertyName("applicationOwner");
				jsonWriter.WriteValue(customiz.ApplicationName);

				jsonWriter.WritePropertyName("moduleOwner");
				jsonWriter.WriteValue(customiz.ModuleName);

				jsonWriter.WritePropertyName("fileFullPath");
				jsonWriter.WriteValue(customiz.PathName);

				jsonWriter.WriteEndObject();
			}
		
			jsonWriter.WriteEndArray();
			jsonWriter.WriteEndObject();

			jsonWriter.Close();
			sw.Close();

			return sw.ToString();
		}

		//---------------------------------------------------------------
		public string RefreshAll(ApplicationType type)
		{
			PathFinder.ApplicationInfos.Clear();
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
                if (ExistsApplication(applicationName))
                {
                    Diagnostic.Add(DiagnosticType.Error, string.Concat(applicationName, " ", Strings.ApplicationAlreadyExists));
                    return false;
                }
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

        //-----------------------------------------------------------------------
        public bool Create(string user, string applicationName, ApplicationType applicationType, string moduleName = "")
        {
            bool success = true;
            if (!ExistsApplication(applicationName))
                success = CreateApplication(applicationName, applicationType);

            if (success && !string.IsNullOrEmpty(moduleName))
                success = CreateModule(applicationName, moduleName);

			return success;
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

        //-----------------------------------------------------------------------
        public bool Delete(string applicationName, string moduleName = "")
        {
            if (!string.IsNullOrEmpty(moduleName))
                return DeleteModule(applicationName, moduleName);

            return DeleteApplication(applicationName);
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

            LicenceService licenceService = Services.GetService<LicenceService>();
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
        internal static readonly string ApplicationAlreadyExists = "Application already exists!";

		internal static readonly string Separator = ";";
	}
}

