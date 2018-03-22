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
using Microarea.Common.Generic;

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
			var listCustomizations = GetListAllCustsUserAndAllUser(docNS, user, onlyDesignable);
			return WriteJsonForListCustomizations(listCustomizations);		
		}
		//---------------------------------------------------------------
		public List<TBFile> GetListAllCustsUserAndAllUser(string docNS, string user, bool onlyDesignable)
		{
			var listAllDllsFound = PathFinder.GetEasyStudioCustomizationsListFor(docNS, onlyDesignable);
			var nsforDoc = new NameSpace(docNS);
			var listCustsPurged = new List<TBFile>();

			/*  listAllDllsFound ordinata per lunghezza decrescente, così da aggiungere prima tutte quelle per user specifico 
				e dopo, se non presenti già con stesso nome, quelle per AllUser
				ESEMPIO : 	\NewApplication1\\NewModule1\\ModuleObjects\\ContactOrigin\\sa\\ContactOrigin.dll"
							\NewApplication1\\NewModule1\\ModuleObjects\\ContactOrigin\\ContactOrigin.dll"			*/

			foreach (var aa in listAllDllsFound.OrderByDescending(x => x.completeFileName.Length))
			{
				var directoryFileName = Path.GetDirectoryName(aa.completeFileName);
				TBFile tBFile =			PathFinder.GetTBFile (aa.completeFileName);
				(string userPath, string allUserPath) = PathFinder.GetCustomizationPath(nsforDoc, user, tBFile);
				if (!string.IsNullOrEmpty(userPath) && directoryFileName == userPath)
					listCustsPurged.Add(tBFile);
				else if (!string.IsNullOrEmpty(allUserPath) && directoryFileName == allUserPath)
				{
					if (!listCustsPurged.Any(x => x.Name == tBFile.Name))
						listCustsPurged.Add(tBFile);
				}
			}
			//infine le ritorno per ordine alfabetico
			return listCustsPurged.OrderBy(x => x.Name).ToList();
		}
		//---------------------------------------------------------------
		private string WriteJsonForListCustomizations(List<TBFile> listCustomizations)
		{
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
		public bool StillExist(string defaultContext, ApplicationType? applicationType)
		{
			ApplicationType appType = CheckAppType(applicationType);
			var splitting = defaultContext.Split(Interfaces.Strings.Separator);
			(string, string) pairTest = (splitting[0], splitting[1]);
			List<(string, string)> json = GetPairsAppsMods(applicationType);
			return json.Contains(pairTest);
		}

		//---------------------------------------------------------------
		public string GetAppsModsAsJson(ApplicationType applicationType)
		{
			List<(string, string)> pairs = GetPairsAppsMods(applicationType);
			return WriteJsonForListContext(pairs);
		}

		//---------------------------------------------------------------
		public List<(string, string)> GetPairsAppsMods(ApplicationType? applicationType)
		{
			var listPairs = new List<(string, string)>();
			ApplicationType appType = CheckAppType(applicationType);

			var apps = GetApplications(appType);
			foreach (var esApp in apps)
			{
				if (esApp == null) continue;
				var mods = GetModules(esApp);
				foreach (var esMod in mods)
				{
					listPairs.Add((esApp, esMod));
				}
			}
			return listPairs;
		}
		//---------------------------------------------------------------
		public string WriteJsonForListContext(List<(string, string)> pairs)
		{
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			JsonWriter jsonWriter = new JsonTextWriter(sw);
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("allApplications");
			jsonWriter.WriteStartArray();

			foreach (var (esApp, esMod) in pairs)
			{
				jsonWriter.WriteStartObject();

				jsonWriter.WritePropertyName("application");
				jsonWriter.WriteValue(esApp);

				jsonWriter.WritePropertyName("module");
				jsonWriter.WriteValue(esMod);
				jsonWriter.WriteEndObject();
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
                Diagnostic.Add(DiagnosticType.Error, Interfaces.Strings.MissingApplicationName);
                return false;
            }             
            try
            {
                if (ExistsApplication(applicationName))
                {
                    Diagnostic.Add(DiagnosticType.Error, string.Concat(applicationName, " ", Interfaces.Strings.ApplicationAlreadyExists));
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
		public ApplicationType CheckAppType(ApplicationType? applicationType)
		{
			ApplicationType appType = ApplicationType.All;
			if (applicationType != null)
				appType = (ApplicationType)applicationType;
			return appType;
		}

		//---------------------------------------------------------------
		public bool CreateModule(string applicationName, string moduleName)
		{
            if (string.IsNullOrEmpty(applicationName))
            {
                Diagnostic.Add(DiagnosticType.Error, Interfaces.Strings.MissingApplicationName);
                return false;
            }

            if (string.IsNullOrEmpty(moduleName))
            {
                Diagnostic.Add(DiagnosticType.Error, Interfaces.Strings.MissingModuleName);
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
                Diagnostic.Add(DiagnosticType.Error, Interfaces.Strings.MissingApplicationName);
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
                Diagnostic.Add(DiagnosticType.Error, Interfaces.Strings.MissingApplicationName);
                return false;
            }

            if (string.IsNullOrEmpty(moduleName))
            {
                Diagnostic.Add(DiagnosticType.Error, Interfaces.Strings.MissingModuleName);
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
                Diagnostic.Add(DiagnosticType.Error, Interfaces.Strings.MissingApplicationName);
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
                Diagnostic.Add(DiagnosticType.Error, Interfaces.Strings.MissingApplicationName);
                return false;
            }

            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
            {
                Diagnostic.Add(DiagnosticType.Error, Interfaces.Strings.MissingModuleName);
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
                Diagnostic.Add(DiagnosticType.Error, Interfaces.Strings.MissingApplicationName);
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



	}

}

