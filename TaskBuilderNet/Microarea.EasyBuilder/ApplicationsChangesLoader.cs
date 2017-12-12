using System;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.Core.EasyBuilder.Refactor;
using System.Xml.Schema;
using System.Xml;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.EasyBuilder;

namespace Microarea.EasyBuilder
{
    //==========================================================================================
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationsChangesLoader
    {
        static bool loaded;
        static ApplicationsChangesLoader instance;
        /// <summary>
        /// 
        /// </summary>
        public static bool Loaded
        {
            get
            {
                return loaded;
            }
        }
        /// <summary>
         /// 
         /// </summary>
        public static ApplicationsChangesLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    BasePathFinder pf = BasePathFinder.BasePathFinderInstance;
                    instance = new ApplicationsChangesLoader(pf, pf.Diagnostic);
                    instance.Load();
   
                    loaded = true;
                }

                return instance;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<VersionChanges> Changes
        {
            get
            {
                return changes;
            }
        }

        readonly static Type versionChangesType = typeof(VersionChanges);
        BasePathFinder pathFinder;
        IDiagnostic diagnostic;
        Changes changes;

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        internal ApplicationsChangesLoader(BasePathFinder pathFinder, IDiagnostic diagnostic)
        {
            changes = new Changes();
            this.pathFinder = pathFinder;
            this.diagnostic = diagnostic;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void Load()
        {
            changes.Clear();

            string changesXmlFilePath = null;
            VersionChanges vc = null;
            foreach (BaseApplicationInfo appInfo in pathFinder.ApplicationInfos)
            {
                changesXmlFilePath = Path.Combine(appInfo.Path, "Changes.xml");
                vc = this.Load(changesXmlFilePath);

                if (vc != null)
                {
                    changes.Add(vc);
                }
            }

            if (vc != null) 
                changes.TargetVersion = pathFinder.InstallationVer.Version;

        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private VersionChanges Load(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            using (FileStream inputFile = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer serializer = new XmlSerializer(versionChangesType);

                //XmlSchema setupSchema = XmlSchema.Read(
                //    this.GetType().Assembly.GetManifestResourceStream("Microarea.Setup.Schemas.SetupDefinition.xsd"),
                //    null
                //    );

                XmlReaderSettings readerSettings = new XmlReaderSettings();
                //readerSettings.Schemas.Add(setupSchema);
                //readerSettings.ValidationType = ValidationType.Schema;
                VersionChanges versionChanges = null;
                try
                {

                    XmlReader xmlReader = XmlReader.Create(inputFile, readerSettings);

                    versionChanges = (VersionChanges)serializer.Deserialize(xmlReader);
                }
                catch (Exception exc)
                {

                    throw exc;
                }


                return versionChanges;
            }
        }
    }
}
