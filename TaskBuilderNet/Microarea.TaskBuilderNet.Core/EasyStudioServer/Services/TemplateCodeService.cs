using Microarea.TaskBuilderNet.Interfaces.EasyStudioServer;
using System;
using System.Collections;
using System.IO;
using System.ComponentModel;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces.Model;
//using Nustache.Core;

namespace Microarea.TaskBuilderNet.Core.EasyStudioServer.Serializers
{
    //====================================================================
    [DisplayName("templateCodeSvc"), Description("This service manages template code and serialization.")]
    //====================================================================
    internal class TemplateCodeService : Component, IService
    {
        //---------------------------------------------------------------
        internal string TemplatePath
        {
            get
            {
                IBaseModuleInfo moduleInfo = BasePathFinder.BasePathFinderInstance.GetModuleInfoByName(NameSolverStrings.Extensions, "EasyBuilder");
                if (moduleInfo == null)
                    return string.Empty;

                string templatePath = Path.Combine(moduleInfo.Path, NameSolverStrings.Files);
                return Path.Combine(templatePath, NameSolverStrings.Templates);
            }
        }

        //----------------------------------------------------------------------------
        public string GetTemplateCode(IDocument doc, string fileName)
        {
            string templateFullName = Path.Combine(TemplatePath, fileName);
            string code = string.Empty;
            if (!File.Exists(templateFullName))
                return code;

            using (StreamReader templateReader = new StreamReader(templateFullName))
            {
                // ReadLine returns the next line from the input stream, or a null reference if the end of the input stream is reached.
                code = templateReader.ReadToEnd();
                if (!string.IsNullOrEmpty(code))
                {
                   // code = Render.StringToString(code, doc);
                }
            }
            return code;
        }



        //----------------------------------------------------------------------------
        public ISerializer Serializer
        {
            get
            {
                return null;
            }

            set
            {
            }
        }
    }
}

