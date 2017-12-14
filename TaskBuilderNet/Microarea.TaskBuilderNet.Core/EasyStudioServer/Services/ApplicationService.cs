using System.Collections.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.EasyStudioServer;
using System.ComponentModel;
using System.Collections.Specialized;
using System.IO;
using Newtonsoft.Json;
using Microarea.TaskBuilderNet.Core.EasyStudioServer.Serializers;
using Microarea.TaskBuilderNet.Core.NameSolver;
using System.Text;
using System.Linq;
using System;

namespace Microarea.TaskBuilderNet.Core.EasyStudioServer.Services
{
	//====================================================================
	[DisplayName("appSvc"), Description("This service manages application structure info and serialization.")]
    [DefaultSerializer(typeof(ApplicationSerializer))]
	public class ApplicationService : Component, IService
    {
        IApplicationSerializer serializer;
 
        //---------------------------------------------------------------
        public ISerializer Serializer
        {
            get
            {
                return AppSerializer;
            }

            set
            {
                if (value is ApplicationSerializer)
                    AppSerializer = (ApplicationSerializer) value;
                else
                    throw (new SerializerException(value, string.Format(Strings.WrongSerializerType, typeof(ApplicationSerializer).Name)));
              }
        }

        //---------------------------------------------------------------
        private IApplicationSerializer AppSerializer
        {
            get
            {
                if (serializer == null)
                    serializer = DefaultSerializer as ApplicationSerializer;

                return serializer;
            }

            set
            {
                serializer = value;
            }
        }

        //---------------------------------------------------------------
        public ApplicationService()
		{
        }

        //---------------------------------------------------------------
        public bool CreateApplication(string applicationName, Interfaces.ApplicationType type)
		{
			return AppSerializer.CreateApplication(applicationName, type);
		}

		//---------------------------------------------------------------
		public bool CreateModule(string applicationName, string moduleName)
		{
			return AppSerializer.CreateModule(applicationName, moduleName);
		}

		//---------------------------------------------------------------
		public bool DeleteApplication(string applicationName)
		{
			return AppSerializer.DeleteApplication(applicationName);
		}

		//---------------------------------------------------------------
		public bool ExistsApplication(string applicationName)
		{
			return AppSerializer.ExistsApplication(applicationName);
		}

		//---------------------------------------------------------------
		public bool DeleteModule(string applicationName, string moduleName)
		{
			return AppSerializer.DeleteModule(applicationName, moduleName);
		}

		//---------------------------------------------------------------
		public bool ExistsModule(string applicationName, string moduleName)
		{
			return AppSerializer.ExistsModule(applicationName, moduleName);
		}

        //---------------------------------------------------------------
        public bool RenameApplication(string oldName, string newName)
        {
            return AppSerializer.RenameApplication(oldName, newName);
        }

        //---------------------------------------------------------------
        public bool RenameModule(string applicationName, string oldName, string newName)
        {
            return AppSerializer.RenameModule(applicationName, oldName, newName);
        }

        //---------------------------------------------------------------
        public IEnumerable<string> GetApplications(Interfaces.ApplicationType applicationType)
		{
			StringCollection applicationNames = null;
            Serializer.PathFinder.GetApplicationsList((Interfaces.ApplicationType)applicationType, out applicationNames);

			return applicationNames.Cast<string>().ToList() as IEnumerable<string>;
		}

		//---------------------------------------------------------------
		public IEnumerable<string> GetModules(string applicationName)
		{
            var modules = Serializer.PathFinder.GetModulesList(applicationName);

			var moduleNames = new List<string>();
			foreach (IBaseModuleInfo moduleInfo in modules)
			{
				moduleNames.Add(moduleInfo.Name);
			}

			return moduleNames;
		}

		//---------------------------------------------------------------
		public string GetAppsModsAsJson(Interfaces.ApplicationType applicationType, bool isDeveloperEdition)
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

			jsonWriter.WritePropertyName("DeveloperEd");
			jsonWriter.WriteValue(isDeveloperEdition);

			jsonWriter.WriteEndObject();
			jsonWriter.Close();
			sw.Close();

			return sw.ToString();
		}
	}
}

