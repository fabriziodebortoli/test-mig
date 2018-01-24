﻿using Microarea.Common.NameSolver;
using System;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.EasyStudio.Engine.Serializers
{
    //====================================================================
    public class ApplicationSerializer : Serializer, IApplicationSerializer
    {
        //---------------------------------------------------------------
        public ApplicationSerializer()
        {
        }

        //---------------------------------------------------------------
        public bool CreateApplication(string applicationName, ApplicationType type)
        {
            EnsurePathFinder();

            string containerFolder = PathFinder.PathFinderInstance.GetStandardApplicationContainerPath(type);
            string appFolder = System.IO.Path.Combine(containerFolder, applicationName);

            //Creare la cartella di applicazione Standard\Applications\<newAppName>
            if (!this.PathFinder.FileSystemManager.ExistPath(appFolder))
                this.PathFinder.FileSystemManager.CreateFolder(appFolder, true);

            //Verifico se è presente il file Application.config, nel caso non lo sia, lo creo
            string appConfigFile = System.IO.Path.Combine(appFolder, NameSolverStrings.Application + NameSolverStrings.ConfigExtension);
            if (!this.PathFinder.FileSystemManager.ExistFile(appConfigFile))
            {
                CreateApplicationConfig(appConfigFile, applicationName, type);
                this.PathFinder.CreateApplicationInfo(applicationName, type, containerFolder);
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

            string modulePath = System.IO.Path.Combine(applicationInfo.Path, moduleName);
            //Verifico se è presente la cartella del modulo
            ModuleInfo moduleInfo = this.PathFinder.GetModuleInfoByName(applicationName, moduleName) as ModuleInfo;
            if (!this.PathFinder.FileSystemManager.ExistPath(modulePath))
            {
                if (moduleInfo == null)
                    moduleInfo = new ModuleInfo(moduleName, applicationInfo);
                this.PathFinder.FileSystemManager.CreateFolder(modulePath, true);
            }

            //Verifico se è presente il file Module.config, nel caso non lo sia, lo creo
            string moduleConfigFile = System.IO.Path.Combine(modulePath, NameSolverStrings.Module + NameSolverStrings.ConfigExtension);
            if (!this.PathFinder.FileSystemManager.ExistFile(moduleConfigFile))
            {
                CreateModuleConfig(moduleConfigFile, moduleName, moduleInfo);
            }
            return true;
        }

        //---------------------------------------------------------------
        public bool DeleteApplication(string applicationName)
        {
            EnsurePathFinder();

            ApplicationInfo application = this.PathFinder.GetApplicationInfoByName(applicationName);
            if (application == null)
                return false;

            string appFolder = PathFinder.GetStandardApplicationContainerPath(application.ApplicationType);
            string standardApplicationFolder = System.IO.Path.Combine(appFolder, applicationName);

            if (this.PathFinder.FileSystemManager.ExistPath(standardApplicationFolder))
            {
                this.PathFinder.FileSystemManager.RemoveFolder(standardApplicationFolder, true, true, false);
                return true;
            }
            return false;
        }

        //---------------------------------------------------------------
        public bool DeleteModule(string applicationName, string moduleName)
        {
            EnsurePathFinder();

            ApplicationInfo application = this.PathFinder.GetApplicationInfoByName(applicationName);
            if (application == null)
                return false;

            string appFolder = PathFinder.GetStandardApplicationContainerPath(application.ApplicationType);
            string standardApplicationFolder = System.IO.Path.Combine(appFolder, applicationName);
            string basePath = System.IO.Path.Combine(standardApplicationFolder, moduleName);

            if (this.PathFinder.FileSystemManager.ExistPath(basePath))
            {
                this.PathFinder.FileSystemManager.RemoveFolder(basePath, true, true, false);
                return true;
            }

            return false;
        }

        //---------------------------------------------------------------
        private void CreateApplicationConfig(string filePath, string applicationName, ApplicationType type)
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

            ApplicationInfo application = this.PathFinder.GetApplicationInfoByName(applicationName);
            return (application != null);
        }
        //---------------------------------------------------------------
        public bool ExistsModule(string applicationName, string moduleName)
        {
            EnsurePathFinder();
            ModuleInfo module = this.PathFinder.GetModuleInfoByName(applicationName, moduleName);
            return (module != null);
        }

        //---------------------------------------------------------------
        public bool RenameApplication(string oldName, string newName)
        {
            EnsurePathFinder();

            if (string.Compare(oldName, newName, true) == 0)
                return false;

            ApplicationInfo info = PathFinder.GetApplicationInfoByName(oldName);
            string oldPath = info.Path;
            string newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(oldPath), newName);

            try
            {
                this.PathFinder.FileSystemManager.RenameFolder(oldPath, newPath);
                // aggiorno l'applicationConfig
                string configPath = System.IO.Path.Combine(newPath, NameSolverStrings.Application + NameSolverStrings.ConfigExtension);
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

            ModuleInfo info = this.PathFinder.GetModuleInfoByName(applicationName, oldName);
            string oldPath = info.Path;
            string newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(oldPath), newName);

            try
            {
                this.PathFinder.FileSystemManager.RenameFolder(oldPath, newPath);

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