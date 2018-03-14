using System;
using System.Linq;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using System.IO;
using System.Diagnostics;
using Nustache.Core;

using Microarea.EasyBuilder.Properties;
using System.Text.RegularExpressions;
using Microarea.TaskBuilderNet.Core.NameSolver;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Microarea.EasyBuilder.Localization;
using ICSharpCode.NRefactory.CSharp;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using System.Resources;
using System.Text;

namespace Microarea.EasyBuilder
{
    //================================================================================
    internal class ControllerCsprojSources : ControllerSources
    {
        readonly string ebSourcesPattern = String.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "[\\t ]*?(?<source><Compile\\s+Include=\"{0}\\\\[A-Za-z0-9\\._-]+?\"\\s*?/>[\\s]*)",
                        NameSolverStrings.DesignerFolderName
                        );
        Regex ebSourcesRegex;

        readonly string ebResxPattern = "[\\t ]*?(?<source><EmbeddedResource\\s+.*?\"\\s*?/>[\\s]*)";
        Regex ebResxRegex;

        readonly string ebAfterBuildPattern = "<Target Name=\"AfterBuild\" Condition=\"'\\$\\(Repack\\)'.*\">\\s*<Exec Command.*\\s*</Target>";
        Regex ebAfterBuildRegex;

        readonly string endProjectPattern = "</Project>";
        Regex endProjectRegex;

        readonly string repackPattern = "<Repack>(true|false)</Repack>";
        Regex repackRegex;

        readonly string defaultConfigurationPattern = "<Configuration\\s*Condition=\"\\s*'\\$\\(Configuration\\)'\\s*==\\s*''\\s*\">\\s*Debug\\s*</Configuration>";
        Regex defaultConfigurationRegex;

        readonly string outputFolderPattern = "\\s*<OutputPath>\\s*.*\\s*</OutputPath>\\s*";
        Regex outputFolderRegex;

        //--------------------------------------------------------------------------------
        public ControllerCsprojSources(
            SourcesSerializer sourcesSerializer,
            string applicationName,
            string moduleName,
            ApplicationType applicationType)
			: base(sourcesSerializer, applicationName, moduleName, applicationType)
        {
            ebSourcesRegex = new Regex(ebSourcesPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            ebResxRegex = new Regex(ebResxPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            ebAfterBuildRegex = new Regex(ebAfterBuildPattern);
            repackRegex = new Regex(repackPattern, RegexOptions.IgnoreCase);
            endProjectRegex = new Regex(endProjectPattern);
            defaultConfigurationRegex = new Regex(defaultConfigurationPattern);
            outputFolderRegex = new Regex(outputFolderPattern);
        }

        //--------------------------------------------------------------------------------
        public override IEnumerable<string> GenerateSources(string filePath)
        {
            var sources = base.GenerateSources(filePath);
            var resxFiles = GenerateResxFiles(filePath);

            string outputFolder = filePath;
            if (Path.HasExtension(filePath))
            {
                outputFolder = Path.GetDirectoryName(filePath);
            }

            var assemblyName = Path.GetFileNameWithoutExtension(filePath);
            var sourcesFolder = BasePathFinder.GetSourcesFolderPathFromDll(filePath);

            //Se non c'e` csproj allora bisogna crearlo, altrimenti aggiornarlo con i nuovi file
            //  l'output di progetto deve essere la cartella dell'utente
            var projectFileFullPath = Path.ChangeExtension(Path.Combine(sourcesFolder, assemblyName), NewCustomizationInfos.CSProjectExtension);
            if (!File.Exists(projectFileFullPath))
            {
                CreateFromTemplate(
                    projectFileFullPath,
                    this.Namespace.ToString(),
                    assemblyName,
                    outputFolder,
                    sources,
                    resxFiles
                    );
            }
            else
            {
                UpdateWithNewEBSources(
                    projectFileFullPath,
                    sources,
                    resxFiles, 
                    outputFolder
                    );
            }

            var utilsDirInfo = new DirectoryInfo(Path.Combine(sourcesFolder, "utils"));
            if (!utilsDirInfo.Exists)
            {
                utilsDirInfo.Create();
            }
            using (var bw = new BinaryWriter(File.Create(Path.Combine(utilsDirInfo.FullName, "ILRepack.exe"))))
            {
                using (var inputStream = this.GetType().Assembly.GetManifestResourceStream("Microarea.EasyBuilder.Resources.ILRepack.exe"))
                {
                    byte[] buffer = new byte[inputStream.Length];
                    inputStream.Read(buffer, 0, buffer.Length);
                    bw.Write(buffer);
                }
            }
            using (var bw = new BinaryWriter(File.Create(Path.Combine(utilsDirInfo.FullName, "ILRepackLicense.txt"))))
            {
                using (var inputStream = this.GetType().Assembly.GetManifestResourceStream("Microarea.EasyBuilder.Resources.ILRepackLicense.txt"))
                {
                    byte[] buffer = new byte[inputStream.Length];
                    inputStream.Read(buffer, 0, buffer.Length);
                    bw.Write(buffer);
                }
            }

            return sources;
        }

        //--------------------------------------------------------------------------------
        //private IEnumerable<string> GenerateResFiles(string filePath)
        //{
        //    NamespaceDeclaration ns = EasyBuilderSerializer.GetNamespaceDeclaration(this.CustomizationInfos.EbDesignerCompilationUnit);


        //    //.._src\
        //    string resFolderPath = BasePathFinder.GetResSourcesPathFromDll(filePath);
        //    var resDirInfo = new DirectoryInfo(resFolderPath);

        //    if (resDirInfo.Exists)
        //    {
        //        var resFiles = resDirInfo.GetFiles(string.Format("*{0}", LocalizationSources.ResourcesExtension));
        //        if (resFiles.Any())
        //        {
        //            var destDirInfo = new DirectoryInfo(Path.Combine(resDirInfo.FullName, "resources_bak"));
        //            if (destDirInfo.Exists)
        //            {
        //                destDirInfo.Delete(true);
        //            }
        //            destDirInfo.Create();
        //            foreach (var resxFile in resFiles)
        //            {
        //                resxFile.MoveTo(Path.Combine(destDirInfo.FullName, resxFile.Name));
        //            }
        //        }
        //    }

        //    foreach (string culture in this.Dictionaries.Keys)
        //    {
        //        string resourceFile = Path.Combine(
        //            resFolderPath,
        //            LocalizationSources.GetResourceFileFromCulture(culture)
        //            );

        //        Dictionary dictionary = this.Dictionaries[culture];

        //        using (ResourceWriter writer = new ResourceWriter(resourceFile))
        //        {
        //            foreach (KeyValuePair<string, string> pair in dictionary)
        //                writer.AddResource(pair.Key, pair.Value);
        //        }
        //        yield return resourceFile;
        //    }
        //}

        //--------------------------------------------------------------------------------
        private IEnumerable<string> GenerateResxFiles(string filePath)
        {
            NamespaceDeclaration ns = EasyBuilderSerializer.GetNamespaceDeclaration(this.CustomizationInfos.EbDesignerCompilationUnit);

            //.._src\
            string resFolderPath = BasePathFinder.GetResSourcesPathFromDll(filePath);

            var resDirInfo = new DirectoryInfo(resFolderPath);

            if (resDirInfo.Exists)
            {
                var resxFiles = resDirInfo.GetFiles("*.resx");
                if (resxFiles.Any())
                {
                    var destDirInfo = new DirectoryInfo(Path.Combine(resDirInfo.FullName, "resx_bak"));
                    if (destDirInfo.Exists)
                    {
                        destDirInfo.Delete(true);
                    }
                    destDirInfo.Create();
                    foreach (var resxFile in resxFiles)
                    {
                        resxFile.MoveTo(Path.Combine(destDirInfo.FullName, resxFile.Name));
                    }
                }
            }

            foreach (string culture in this.Dictionaries.Keys)
            {
                if (this.Dictionaries[culture].Count == 0)
                    continue;

                var resxFileName = LocalizationSources.GetResxFileFromCulture(culture);
                var resxFilePath = Path.Combine(resFolderPath, resxFileName);
                LocalizationSources.SaveLocalizationFiles(this.Dictionaries[culture], resxFilePath);

                yield return resxFilePath;
            }
        }

        //-------------------------------------------------------------------------------------
        private Tuple<List<string>, List<CsProjReference>> GetLibsPath(string projectFolderPath, Boolean alsoTBRef = true)
        {
            var libsPath = new List<string>();

            List<CsProjReference> taskBuilderReferences = null; 

            if (alsoTBRef)
                taskBuilderReferences = new List<CsProjReference>();

            foreach (IAssemblyReference content in ProjectContent.AssemblyReferences)
            {
                DefaultUnresolvedAssembly reference = content as DefaultUnresolvedAssembly;
                if (reference == null)
                    continue;

                if (
                    reference.AssemblyName.StartsWith("System") ||
                    String.Compare(reference.AssemblyName, "mscorlib", StringComparison.InvariantCultureIgnoreCase) == 0
                    )
                    continue;

                if (alsoTBRef && taskBuilderReferences != null)
                    taskBuilderReferences.Add(
                        new CsProjReference()
                        {
                            ReferenceName = reference.AssemblyName,
                            ReferenceDllPath = reference.Location.RelativeOn(projectFolderPath)
                        }
                        );
                var libPath = Path.GetDirectoryName(reference.Location);
                if (!libsPath.Contains(libPath))
                {
                    libsPath.Add(libPath);
                }
            }

            return new Tuple<List<string>, List<CsProjReference>>(libsPath, taskBuilderReferences);
        }

        //--------------------------------------------------------------------------------
        private void CreateFromTemplate(
            string projectFileFullPath,
            string rootNamespace,
            string assemblyName,
            string outputPath,
            IEnumerable<string> sourceFiles,
            IEnumerable<string> resxFiles
            )
        {
            string projectFolderPath = Path.GetDirectoryName(projectFileFullPath);
            var projectFolderDirInfo = new DirectoryInfo(projectFolderPath);
            if (!projectFolderDirInfo.Exists)
            {
                projectFolderDirInfo.Create();
            }

            //cs
            var projectSourceFiles = new List<CsProjSourceFile>();
            foreach (var sourceFile in sourceFiles)
            {
                projectSourceFiles.Add(new CsProjSourceFile() { RelativeFilePath = sourceFile.RelativeOn(projectFolderPath) });
            }
            //resx
            var projectResxFiles = new List<ResxSourceFile>();
            foreach (var resxFile in resxFiles)
            {
                projectResxFiles.Add(new ResxSourceFile() { RelativeFilePath = resxFile.RelativeOn(projectFolderPath) });
            }

            Tuple<List<string>, List<CsProjReference>> result = GetLibsPath(projectFolderPath);
            List<string> libsPath = result.Item1;
            List<CsProjReference> taskbuilderReferences = result.Item2;

            var relativeOutputPath = outputPath.RelativeOn(projectFolderPath);

            var cultureSb = GetCultures(resxFiles);
            var refSb = GetRefs(libsPath);

            var data = new
            {
                ProjectGuid = Guid.NewGuid().ToString("B"),
                RootNamespace = rootNamespace,
                AssemblyName = assemblyName,
                OutputPath = relativeOutputPath,
                SourceFiles = projectSourceFiles,
                ResxFiles = projectResxFiles,
                TaskBuilderReferences = taskbuilderReferences,
                SatelliteAssemblies = cultureSb.ToString(),
                LibPath = refSb.ToString(),
                Repack = resxFiles.Count() > 1 ? Boolean.TrueString : Boolean.FalseString
            };

            using (var stream = new MemoryStream(Resources.CsProj))
            using (var sr = new StreamReader(stream))
            {
                Render.StringToFile(sr.ReadToEnd(), data, projectFileFullPath);
            }

            var csprojUserData = new
            {
                InstanceFolderPath = BasePathFinder.BasePathFinderInstance.GetInstallationPath()
            };

            var userProjectFileFullPath = Path.ChangeExtension(projectFileFullPath, NewCustomizationInfos.CSProjectUserExtension);
            using (var stream = new MemoryStream(Resources.CsprojUser))
            using (var sr = new StreamReader(stream))
            {
                Render.StringToFile(sr.ReadToEnd(), csprojUserData, userProjectFileFullPath);
            }
        }

        private static StringBuilder GetRefs(List<string> libsPath, bool escapeDoubleQuotes = false)
        {
            var refSb = new StringBuilder();
            foreach (var libPath in libsPath)
            {
                refSb.Append("/lib:")
                    .Append(string.Format("\"{0}\"", libPath))
                    .Append(" ");
            }

            if (escapeDoubleQuotes)
            {
                refSb.Replace("\"", "&quot;");
            }

            return refSb;
        }

        private static StringBuilder GetCultures(IEnumerable<string> resxFiles, bool escapeDoubleQuotes = false)
        {
            var cultureSb = new StringBuilder(" ");
            foreach (var resxFile in resxFiles)
            {
                var filename = Path.GetFileName(resxFile);
                //Strings.it-IT.resx
                var tokens = filename.Split('.');
                if (tokens.Length > 2)
                {
                    cultureSb.Append(string.Format("\"..\\{0}\\$(AssemblyName).resources.dll\" ", tokens[1]));
                }
            }

            if (escapeDoubleQuotes)
            {
                cultureSb.Replace("\"", "&quot;");
            }
            return cultureSb;
        }

        //--------------------------------------------------------------------------------
        private void UpdateWithNewEBSources(
            string projectFileFullPath,
            IEnumerable<string> sourceFiles,
            IEnumerable<string> resxFiles,
            string outputFolder
            )
        {
            string csProjContent;
            using (var inputStream = File.OpenRead(projectFileFullPath))
            using (var sr = new StreamReader(inputStream))
            {
                csProjContent = sr.ReadToEnd();
            }


            var ebFilesStack = new Stack<Match>();
            var match = ebSourcesRegex.Match(csProjContent);
            while (match.Success)
            {
                ebFilesStack.Push(match);
                match = match.NextMatch();
            }

            match = ebResxRegex.Match(csProjContent);
            while (match.Success)
            {
                ebFilesStack.Push(match);
                match = match.NextMatch();
            }
            if (ebFilesStack.Count == 0)
            {
                Debug.Assert(false);
                return;
            }

            Match lastRow = ebFilesStack.Pop();
            Match firstRow = null;
            while (ebFilesStack.Count > 0)
            {
                firstRow = ebFilesStack.Pop();
            }
            csProjContent = csProjContent.Remove(firstRow.Index, lastRow.Index + lastRow.Length - firstRow.Index);

            var projectFolderPath = Path.GetDirectoryName(projectFileFullPath);
            var projectSourceFiles = new List<CsProjSourceFile>();
            foreach (var sourceFile in sourceFiles)
            {
                projectSourceFiles.Add(new CsProjSourceFile() { RelativeFilePath = sourceFile.RelativeOn(projectFolderPath) });
            }
            var projectResFiles = new List<ResxSourceFile>();
            foreach (var resxFile in resxFiles)
            {
                projectResFiles.Add(new ResxSourceFile() { RelativeFilePath = resxFile.RelativeOn(projectFolderPath) });
            }

            string newItemGroupForEB;
            using (var stream = new MemoryStream(Resources.ItemSource))
            using (var sr = new StreamReader(stream))
            {
                newItemGroupForEB = Render.StringToString(
                    sr.ReadToEnd(),
                    new
                    {
                        SourceFiles = projectSourceFiles,
                        ResxFiles = projectResFiles,
                    }
                    );
            }

            csProjContent = csProjContent.Insert(firstRow.Index, newItemGroupForEB);

            //Produzione riga di comando per ILRepack.exe corretta a seconda delle culture
            var cultureSb = GetCultures(resxFiles, escapeDoubleQuotes: true);

            //remove all OutputPath
            match = outputFolderRegex.Match(csProjContent);
            while (match.Success)
            {
                csProjContent = outputFolderRegex.Replace(csProjContent, "");
                match = match.NextMatch();
            }

            //manage Repack tag
            match = repackRegex.Match(csProjContent);
            if (!match.Success)
            {
                match = defaultConfigurationRegex.Match(csProjContent);
                if (match.Success)
                {
                    var insertionPtIdx = match.Index + match.Length;
                    csProjContent = csProjContent.Insert(insertionPtIdx, string.Format("\r\n<Repack>{0}</Repack>", (resxFiles.Count() > 1).ToString()));
                }
            }
            else   //Sostituiamo il valore di Repack per decidere se compattare le dll in caso di piu` dizionari.
                csProjContent = repackRegex.Replace(csProjContent, string.Format("<Repack>{0}</Repack>", (resxFiles.Count() > 1).ToString()));

            //match Repack again for insert OutputPath tag
            var relativeOutputPath = outputFolder.RelativeOn(projectFolderPath);
            match = repackRegex.Match(csProjContent);
            if (match.Success)
                csProjContent = csProjContent.Insert(match.Index + match.Length, string.Format("\r\n<OutputPath>{0}</OutputPath>\r\n", relativeOutputPath));
            
            match = ebAfterBuildRegex.Match(csProjContent);
            if (match.Success)
            {
                var startIdx = csProjContent.IndexOf(".dll", match.Index) + ".dll".Length;
                var endIdx = csProjContent.IndexOf("/lib:", startIdx);

                csProjContent = csProjContent.Remove(startIdx, endIdx - startIdx);
                csProjContent = csProjContent.Insert(startIdx, cultureSb.ToString());
            }
            else
            {
                match = endProjectRegex.Match(csProjContent);
                if (match.Success)
                {

                    var idx = match.Index;

                    //insert Exec command
                    StringBuilder sbAfterBuild = new StringBuilder("\r\n<Target Name=\"AfterBuild\" Condition=\"'$(Repack)' == 'true'\">\r\n<Exec Command=\".\\utils\\ILRepack.exe $(OutputPath)\\$(AssemblyName).dll ");
                    sbAfterBuild.Append(cultureSb.ToString());
                    //mange libs
                    Tuple<List<string>, List<CsProjReference>> result = GetLibsPath(projectFolderPath);
                    List<string> libsPath = result.Item1;
                    var libsToAppend = GetRefs(libsPath, true);

                    sbAfterBuild.Append(string.Format("{0}$(ndebug) ", libsToAppend));
                    sbAfterBuild.Append("/out:$(OutputPath)\\$(AssemblyName).dll\" />\r\n</Target>\r\n");
                    csProjContent = csProjContent.Insert(idx, sbAfterBuild.ToString());
                }
                else
                {
                    Debug.Assert(false, "Missing ILRepack Target in csproj file: " + projectFileFullPath);
                    return;
                }
            }

            using (var sw = new StreamWriter(projectFileFullPath))
            {
                sw.Write(csProjContent);
            }
        }
    }
}
