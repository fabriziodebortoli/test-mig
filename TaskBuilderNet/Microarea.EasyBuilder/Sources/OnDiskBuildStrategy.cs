using System;
using System.Linq;
using System.Collections.Generic;

using Microarea.EasyBuilder.Packager;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using System.CodeDom.Compiler;
using System.IO;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microsoft.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace Microarea.EasyBuilder
{
    //=========================================================================
    internal class OnDiskBuildStrategy : BuildStrategy
    {
        private const string pdbFolderName = "Pdb";

        private List<string> temporaryFiles = new List<string>();

        string filePath;
        bool debug;

        //---------------------------------------------------------------------
        public OnDiskBuildStrategy(string filePath, bool debug)
        {
            this.filePath = filePath;
            this.debug = debug;
        }

        //---------------------------------------------------------------------
        public override EBCompilerResults Build(Sources sources)
        {
            string outputAssemblyFullPath = GetBuildTempFile(filePath);

            CompilerParameters parameters = new CompilerParameters();
            parameters.IncludeDebugInformation = debug;
            parameters.GenerateInMemory = false;
            parameters.OutputAssembly = outputAssemblyFullPath;

            CompilerResults buildResults = null;
            using (CodeDomProvider compiler = new CSharpCodeProvider())
            {
                string folder = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

				foreach (IUnresolvedAssembly content in sources.ProjectContent.AssemblyReferences)
					parameters.ReferencedAssemblies.Add(content.Location);

                AddLocalizationFiles(sources.Dictionaries, sources.CustomizationInfos, parameters, true);

                var files = sources.GenerateSources(filePath);
                buildResults = compiler.CompileAssemblyFromFile(parameters, files.ToArray());
            }

            ManageBuildFiles(filePath, outputAssemblyFullPath, parameters, buildResults.Errors.HasErrors);

            return EBCompilerResultsFactory.FromCompilerResults(buildResults);
        }

        //--------------------------------------------------------------------------------
        private void ManageBuildFiles(
            string filePath,
            string tempFilePath,
            CompilerParameters parameters,
            bool hasErrors
            )
        {
            var workingDirInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));

            //se non ho errori e non è una build temporanea,
            //copio la dll di build temporanea in quella definitiva
            //altrimenti la segno come temporanea da cancellare
            if (!hasErrors)
            {
                //se non ci sono errori cancello i .bak
                foreach (var bakSourceFile in workingDirInfo.GetFiles(String.Concat("*", NameSolverStrings.BakExtension)))
                {
                    bakSourceFile.Delete();
                }

                //prima controllo di avere un file buono, quindi lo sovrascrivo al vecchio
                if (File.Exists(tempFilePath))
                {
                    try
                    {
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                    }
                    catch { }

                    try
                    {
                        //faccio un copy e non un move altrimenti non eredita everyone fullcontrol
                        File.Copy(tempFilePath, filePath);
                        File.Delete(tempFilePath);
                    }
                    catch { }
                }
            }
            else
            {
                //se ci sono errori allora ripristino il bak della customizzazione corrente in dll.
                foreach (var sourceFile in workingDirInfo.GetFiles())
                {
					if(Path.GetExtension(sourceFile.FullName) == NameSolverStrings.BakExtension)
						sourceFile.MoveTo(filePath);
                }
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

            //se esiste, cancello il file che contiene il riferimento alla dll comune perché deve prevalere la mia dll
            //fino alla prossima pacchettizzazione
            string modulePath = Path.ChangeExtension(filePath, NameSolverStrings.EbLinkExtension);
            BaseCustomizationContext.CustomizationContextInstance.RemoveFromCustomListAndFromFileSystem(modulePath);

        }

        //--------------------------------------------------------------------------------
        private string GetBuildTempFile(string file)
        {
            //dll temporanea di build, che poi sovrascriverà la definitiva se tutto va a buon fine
            //e se non si tratta di una build temporanea
            string fileName = Path.GetFileName(file);
            string pdbDirectory = Path.Combine(BasePathFinder.BasePathFinderInstance.GetAppDataPath(true), pdbFolderName);
            string fileDirectory = Path.Combine(pdbDirectory, Guid.NewGuid().ToString());

            string tempDllPath = string.IsNullOrEmpty(file)
            ? Path.ChangeExtension(Path.GetTempFileName(), ".dll")
            : Path.Combine(fileDirectory, fileName);

            //cancello il file sorgente
            string csPath = Path.ChangeExtension(tempDllPath, NewCustomizationInfos.CSSourceFileExtension);
            if (File.Exists(csPath))
                File.Delete(csPath);

            string folder = Path.GetDirectoryName(tempDllPath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            //ATTENZIONE: il nome e percorso della dll deve corrispondere a quello del pdb
            //altrimenti non debugga
            return tempDllPath;
        }
    }
}
