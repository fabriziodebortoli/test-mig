using System.CodeDom.Compiler;
using System.IO;
using ICSharpCode.NRefactory.TypeSystem;
using Microsoft.CSharp;

namespace Microarea.EasyBuilder
{
    //=========================================================================
    internal class InMemoryBuildStrategy : BuildStrategy
    {
        //---------------------------------------------------------------------
        public override EBCompilerResults Build(Sources sources)
        {
            CompilerParameters parameters = new CompilerParameters();
            parameters.IncludeDebugInformation = false;
            parameters.GenerateInMemory = true;
            parameters.OutputAssembly = string.Empty;

            CompilerResults buildResults = null;
            using (CodeDomProvider compiler = new CSharpCodeProvider())
            {
                foreach (IUnresolvedAssembly content in sources.ProjectContent.AssemblyReferences)
                    parameters.ReferencedAssemblies.Add(content.Location);

				AddLocalizationFiles(sources.Dictionaries, sources.CustomizationInfos, parameters, false);

                buildResults = compiler.CompileAssemblyFromSource(parameters, sources.GetAllCode());
            }

            //cancello i file di risorsa
            foreach (string resourceFile in parameters.EmbeddedResources)
            {
                try
                {
                    if (File.Exists(resourceFile))
                        File.Delete(resourceFile);
                }
                catch { }
            }

            return EBCompilerResultsFactory.FromCompilerResults(buildResults);
        }
    }
}