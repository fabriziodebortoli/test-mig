using System.ComponentModel;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using Microarea.Common.NameSolver;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.Common.CustomAttributes;
using System.IO;
using Nustache.Core;

namespace TaskBuilderNetCore.EasyStudio.Services
{
    //====================================================================
    [Name("templateCodeSvc"), Description("This service manages template code and serialization.")]
    //====================================================================
    internal class TemplateCodeService : Service
    {
        //---------------------------------------------------------------
        internal string TemplatePath
        {
            get
            {
                return Path.Combine(PathFinder.GetEasyStudioHomePath(), NameSolverStrings.Templates, NameSolverStrings.Sourcecode);
            }
        }

        //--------------------------------------------------------------------------------
        internal string SubPathTBApplication
        {
            get
            {
                return NameSolverStrings.TbApplication;
            }
        }

        //------------------------------------------------------------------------------------
        internal string SubPathCustomization
        {
            get
            {
                return NameSolverStrings.Customization;
            }
        }

        //---------------------------------------------------------------------------------------
        public string GetTemplateCode(ApplicationType appType, string fileName)
        {
            string code = string.Empty;
            string templateFullName = string.Empty;
            switch (appType)
            {
                case ApplicationType.TaskBuilderApplication:
                    templateFullName = Path.Combine(TemplatePath, SubPathTBApplication, fileName);
                    break;
                case ApplicationType.Customization:
                    templateFullName = Path.Combine(TemplatePath, SubPathCustomization, fileName);
                    break;
                default:
                    return code;
            }

            if (!this.PathFinder.ExistFile(templateFullName))
                return code;

            using (StreamReader templateReader = new StreamReader(templateFullName))
            {
                code = templateReader.ReadToEnd();
            }

            return code;
        }

        //----------------------------------------------------------------------------
        public string GetTemplateCode(IDocument doc, string subPath, string fileName)
        {
            string templateFullName = System.IO.Path.Combine(TemplatePath, fileName);
            string code = string.Empty;
            
            if (!this.PathFinder.ExistPath(templateFullName))
                return code;

            using (StreamReader templateReader = new StreamReader(templateFullName))
            {
                // ReadLine returns the next line from the input stream, or a null reference if the end of the input stream is reached.
                code = templateReader.ReadToEnd();
                if (!string.IsNullOrEmpty(code))
                {
                   //code = Render.StringToString(code, doc);
                }
            }
            return code;
        }

        //-------------------------------------------------------------------------------------
        public bool ManageSerialization(ApplicationType appType, string fileTemplate, object dataToSerialize, string fullPathFileDestination)
        {
            string codeTemplate = GetTemplateCode(appType, fileTemplate);

            if (string.IsNullOrEmpty(codeTemplate))
                return false;

            Render.StringToFile(codeTemplate, dataToSerialize, fullPathFileDestination, RenderContextBehaviour.GetDefaultRenderContextBehaviour());

            return true;
        }
    }
}

