using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.EasyStudioServer;
using System;
using System.IO;
using System.Xml;

namespace Microarea.TaskBuilderNet.Core.EasyStudioServer.Serializers
{
    //====================================================================
    public class ApplicationSerializer : Serializer, IApplicationSerializer
    {
        //---------------------------------------------------------------
        public ApplicationSerializer()
        {
        }

        //---------------------------------------------------------------
        public bool CreateApplication(string applicationName, Interfaces.ApplicationType type)
        {
            EnsurePathFinder();

            string containerFolder = PathFinder.GetStandardApplicationContainerPath((Microarea.TaskBuilderNet.Interfaces.ApplicationType)type);
            string appFolder = Path.Combine(containerFolder, applicationName);

            //Creare la cartella di applicazione Standard\Applications\<newAppName>
            if (!Directory.Exists(appFolder))
                Directory.CreateDirectory(appFolder);

            //Verifico se è presente il file Application.config, nel caso non lo sia, lo creo

            string appConfigFile = Path.Combine(appFolder, NameSolverStrings.Application + NameSolverStrings.ConfigExtension);
            if (!File.Exists(appConfigFile))
            {
                CreateApplicationConfig(appConfigFile, applicationName, type);
                ((PathFinder)PathFinder).CreateApplicationInfo(applicationName, type, containerFolder);
            }

            return true;
        }

        //---------------------------------------------------------------
        public bool CreateModule(string applicationName, string moduleName)
        {
            EnsurePathFinder();

            ApplicationInfo applicationInfo = PathFinder.GetApplicationInfoByName(applicationName) as ApplicationInfo;
            if (applicationInfo == null)
                return false;

            string modulePath = Path.Combine(applicationInfo.Path, moduleName);
            //Verifico se è presente la cartella del modulo
            ModuleInfo moduleInfo = PathFinder.GetModuleInfoByName(applicationName, moduleName) as ModuleInfo;
            if (!Directory.Exists(modulePath))
            {
                if (moduleInfo == null)
                    moduleInfo = new ModuleInfo(moduleName, applicationInfo);
                Directory.CreateDirectory(modulePath);
            }

            //Verifico se è presente il file Module.config, nel caso non lo sia, lo creo
            string moduleConfigFile = Path.Combine(modulePath, NameSolverStrings.Module + NameSolverStrings.ConfigExtension);
            if (!File.Exists(moduleConfigFile))
            {
                CreateModuleConfig(moduleConfigFile, moduleName, moduleInfo);
            }
            return true;
        }

        //---------------------------------------------------------------
        public bool DeleteApplication(string applicationName)
        {
            EnsurePathFinder();

            IBaseApplicationInfo application = PathFinder.GetApplicationInfoByName(applicationName);
            if (application == null)
                return false;

            string appFolder = PathFinder.GetStandardApplicationContainerPath((Microarea.TaskBuilderNet.Interfaces.ApplicationType)application.ApplicationType);
            string standardApplicationFolder = Path.Combine(appFolder, applicationName);

            if (Directory.Exists(standardApplicationFolder))
            {
                Directory.Delete(standardApplicationFolder, true);
                return true;
            }
            return false;
        }

        //---------------------------------------------------------------
        public bool DeleteModule(string applicationName, string moduleName)
        {
            EnsurePathFinder();

            IBaseApplicationInfo application = PathFinder.GetApplicationInfoByName(applicationName);
            if (application == null)
                return false;

            string appFolder = PathFinder.GetStandardApplicationContainerPath((Microarea.TaskBuilderNet.Interfaces.ApplicationType)application.ApplicationType);
            string standardApplicationFolder = Path.Combine(appFolder, applicationName);
            string basePath = Path.Combine(standardApplicationFolder, moduleName);

            if (Directory.Exists(basePath))
            {
                Directory.Delete(basePath, true);
                return true;
            }

            return false;
        }

        //---------------------------------------------------------------
        private void CreateApplicationConfig(string filePath, string applicationName, Interfaces.ApplicationType type)
        {
            #region application config sample
            /*<?xml version="1.0" encoding="utf-8"?>
				<ApplicationInfo>
				  <Type>TbApplication</Type>
				  <DbSignature>ERP</DbSignature>
				  <Version>3.5.0</Version>
				  <HelpModule>Core</HelpModule>
				</ApplicationInfo> 
			 */
            #endregion

            ApplicationConfigInfo applicationInfo = new ApplicationConfigInfo(applicationName, filePath);
            applicationInfo.Type = type.ToString();
            applicationInfo.DbSignature = applicationName;
            applicationInfo.Save();
        }

        //-----------------------------------------------------------------------------
        private void CreateModuleConfig(string filePath, string moduleName, ModuleInfo moduleInfo)
        {
            #region module config sample
            /*
			<?xml version="1.0" encoding="utf-8"?>
			<ModuleInfo localize="Sales" optional="false" destinationfolder="TbApps" menuvieworder="70">
			  <Components>
				<Library name="SalesDbl" deploymentpolicy="base" sourcefolder="Dbl" aggregatename = "SalesDbl" />
			  </Components>
			</ModuleInfo>
			 */
            #endregion
            ModuleConfigInfo moduleConfigInfo = new ModuleConfigInfo(moduleName, moduleInfo, filePath);
            moduleConfigInfo.Signature = moduleName;
            moduleConfigInfo.Save();
        }

        //---------------------------------------------------------------
        public bool ExistsApplication(string applicationName)
        {
            EnsurePathFinder();

            IBaseApplicationInfo application = PathFinder.GetApplicationInfoByName(applicationName);
            return (application != null);
        }
        //---------------------------------------------------------------
        public bool ExistsModule(string applicationName, string moduleName)
        {
            EnsurePathFinder();
            IBaseModuleInfo module = PathFinder.GetModuleInfoByName(applicationName, moduleName);
            return (module != null);
        }

        //---------------------------------------------------------------
        public bool RenameApplication(string oldName, string newName)
        {
            EnsurePathFinder();

            if (string.Compare(oldName, newName, true) == 0)
                return false;

            BaseApplicationInfo info = PathFinder.GetApplicationInfoByName(oldName) as BaseApplicationInfo;
            string oldPath = info.Path;
            string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName);

            try
            {
                Directory.Move(oldPath, newPath);
                // aggiorno l'applicationConfig
                string configPath = Path.Combine(newPath, NameSolverStrings.Application + NameSolverStrings.ConfigExtension);
                CreateApplicationConfig(configPath, newName, info.ApplicationType);
            }
            catch (Exception ex)
            {
                throw (new SerializerException(this, ex.Message));
            }

            return true;
        }

        //---------------------------------------------------------------
        public bool RenameModule(string applicationName, string oldName, string newName)
        {
            EnsurePathFinder();

            if (string.Compare(oldName, newName, true) == 0)
                return false;

            BaseModuleInfo info = PathFinder.GetModuleInfoByName(applicationName, oldName) as BaseModuleInfo;
            string oldPath = info.Path;
            string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName);

            try
            {
                Directory.Move(oldPath, newPath);

                // aggiorno il module.Config
                ModuleConfigInfo mcInfo = info.ModuleConfigInfo as ModuleConfigInfo;
                mcInfo.ModuleName = newName;
                mcInfo.Signature = newName;
                mcInfo.Save();
            }
            catch (Exception ex)
            {
                throw (new SerializerException(this, ex.Message));
            }

            return true;
        }
    }
}
