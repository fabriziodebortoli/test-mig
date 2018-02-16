using System.ComponentModel;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using Microarea.Common.NameSolver;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.Common.CustomAttributes;
//using Nustache.Core;

namespace TaskBuilderNetCore.EasyStudio.Engine.Services
{
    //====================================================================
    [Name("templateCodeSvc"), Description("This service manages template code and serialization.")]
    //====================================================================
    internal class TemplateCodeService : Component, IService
    {
        //---------------------------------------------------------------
        internal string TemplatePath
        {
            get
            {
                ModuleInfo moduleInfo = PathFinder.PathFinderInstance.GetModuleInfoByName(NameSolverStrings.Extensions, NameSolverStrings.EasyStudio);
                if (moduleInfo == null)
                    return string.Empty;

                string templatePath = System.IO.Path.Combine(moduleInfo.Path, NameSolverStrings.Files);
                return System.IO.Path.Combine(templatePath, NameSolverStrings.Templates);
            }
        }

        //----------------------------------------------------------------------------
        public string GetTemplateCode(IDocument doc, string fileName)
        {
            string templateFullName = System.IO.Path.Combine(TemplatePath, fileName);
            string code = string.Empty;
            /*
            if (!this.PathFinder.FileSystemManager.ExistPath(templateFullName))
                return code;

            using (StreamReader templateReader = new StreamReader(templateFullName))
            {
                // ReadLine returns the next line from the input stream, or a null reference if the end of the input stream is reached.
                code = templateReader.ReadToEnd();
                if (!string.IsNullOrEmpty(code))
                {
                   // code = Render.StringToString(code, doc);
                }
            }*/
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

