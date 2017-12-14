using Microarea.Snap.Core.Properties;
using Microarea.Snap.IO;
using Nustache.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microarea.Snap.Core
{
    internal class SnapManifestManager : IManifestManager
    {
        public IManifest CreateManifest(IFolder toExplore, IFile outputFile, IFile inputFile, string build, string productVersion)
        {
            if (toExplore == null)
            {
                throw new ArgumentNullException("toExplore");
            }
            if (!toExplore.Exists)
            {
                throw new ArgumentException("toExplore folder does not exist");
            }

            if (outputFile == null)
            {
                throw new ArgumentNullException("outputFile");
            }

            if (inputFile == null)
            {
                throw new ArgumentNullException("inputFile");
            }
            if (!inputFile.Exists)
            {
                throw new ArgumentException("inputFile does not exist");
            }

            if (build == null)
            {
                throw new ArgumentNullException("build");
            }
            if (build.Trim().Length == 0)
            {
                throw new ArgumentException("build is empty");
            }

            int buildNumber = 0;
            if (!int.TryParse(build, out buildNumber))
            {
                throw new ArgumentException("build is not a number");
            }

            var version = ParseVersion(productVersion);

            var rootFolderName = toExplore.FullName;
            if (!rootFolderName.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
            {
                rootFolderName += "\\";
            }

            var packageDefinition = LoadPackageDefinition(inputFile);

            var pdb = CreatePackageDataBag(packageDefinition, toExplore, rootFolderName, buildNumber, version);

            var manifestContent = Render.StringToString(Resources.ManifestTemplate, pdb);

            var manifest = new SnapManifest();
            manifest.Init(manifestContent);

            using (var outputStream = outputFile.OpenWrite())
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(manifestContent);
                xmlDocument.Save(outputStream);
            }

            return manifest;
        }

        internal static Version ParseVersion(string productVersion)
        {
            if (productVersion == null)
            {
                throw new ArgumentNullException("productVersion");
            }
            if (productVersion.Trim().Length == 0)
            {
                throw new ArgumentException("productVersion is empty");
            }
            if (productVersion.IndexOf("hf", StringComparison.OrdinalIgnoreCase) != -1)
            {
                productVersion = productVersion.Substring(0, productVersion.LastIndexOf('.'));
            }
            var version = new Version();
            if (!Version.TryParse(productVersion, out version))
            {
                if (productVersion.IndexOf("x", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    throw new ArgumentException("productVersion is not a version");
                }

                string firstToken = null;
                try
                {
                    firstToken = productVersion.Split('.')[0];
                }
                catch (Exception exc)
                {
                    throw new ArgumentException("productVersion is not a version", exc);
                }
                int major = 0;
                if (!int.TryParse(firstToken, out major))
                {
                    throw new ArgumentException("productVersion is not a version");
                }
                version = new Version(major, 0, 0, 0);
            }

            return version;
        }

        private static PackageDataBag CreatePackageDataBag(PackageDefinition packageDefinition, IFolder rootFolder, string rootFolderName, int buildNumber, Version productVersion)
        {
            var pdb = new PackageDataBag();

            pdb.Id = packageDefinition.Id;
            pdb.Version = buildNumber;
            pdb.Title = packageDefinition.Title;
            pdb.Producer = packageDefinition.Producer;
            pdb.Product = packageDefinition.Product;
            pdb.ProductVersion = productVersion.ToStringForManifest();
            pdb.Description = packageDefinition.Description;
            pdb.IconUrl = packageDefinition.IconUrl;
            pdb.ReleaseNotesUrl = packageDefinition.ReleaseNotesUrl;

            var fileSystemStructure = packageDefinition.FileSystemStructure;

            var subFolders = rootFolder.GetFolders("*");

            if (subFolders != null && subFolders.Length > 0)
            {
                AddFiles(
                    subFolders,
                    pdb,
                    rootFolderName,
                    fileSystemStructure.IncludeFolders,
                    fileSystemStructure.ExcludeFolders
                    );
            }

            AddFiles(rootFolder, pdb, rootFolderName, fileSystemStructure.IncludeFiles, fileSystemStructure.ExcludeFiles);

            return pdb;
        }

        private static void AddFiles(
            IFolder folder,
            PackageDataBag pdb,
            string rootFolderName,
            IncludeFile[] includeFiles,
            ExcludeFile[] excludeFiles
            )
        {
            var subFiles = folder.GetFiles("*");

            if (subFiles == null || subFiles.Length == 0)
            {
                return;
            }

            var filesCache = subFiles.ToLookup((f) => f.Name);

            var workingFiles = new List<string>();
            workingFiles.AddRange(subFiles.Select(f => f.Name));

            PatternResolver.ResolveFiles(
                    workingFiles,
                    includeFiles,
                    excludeFiles
                    );

            if (workingFiles.Count > 0)
            {
                foreach (string workingFile in workingFiles)
                {
                    pdb.Files.Add(new FileDataBag() { File = filesCache[workingFile].FirstOrDefault().FullName.Replace(rootFolderName, string.Empty) });
                }
            }
        }

        private static void AddFiles(
            IFolder[] workingFolders,
            PackageDataBag pdb,
            string rootFolderName,
            IncludeFolder[] includeFolders = null,
            ExcludeFolder[] excludeFolders = null
            )
        {
            var workingSet = new List<string>();
            workingSet.AddRange(workingFolders.Select(f => f.Name));

            var foldersCache = workingFolders.ToLookup(f => f.Name);

            var results = PatternResolver.ResolveFolders(
                        workingSet,
                        includeFolders,
                        excludeFolders
                        );

            if (results.Count == 0)
            {
                return;
            }

            IncludeFolder includeFolder = null;
            IFolder aFolder = null;
            foreach (string subFolderName in results.Keys)
            {
                includeFolder = results[subFolderName];
                aFolder = foldersCache[subFolderName].FirstOrDefault();

                if (includeFolder != null)
                {
                    if (includeFolder.RecursiveSpecified && includeFolder.Recursive == RecursiveType.yes)
                    {
                        AddFilesRecursive(
                            aFolder.GetFolders("*"),
                            pdb,
                            rootFolderName
                            );
                    }
                    else
                    {
                        AddFiles(
                            aFolder.GetFolders("*"),
                            pdb,
                            rootFolderName,
                            includeFolder.IncludeFolders,
                            includeFolder.ExcludeFolders
                            );
                    }
                }

                AddFiles(aFolder, pdb, rootFolderName, includeFolder.IncludeFiles, includeFolder.ExcludeFiles);
            }
        }

        private static void AddFilesRecursive(
            IFolder folder,
            PackageDataBag pdb,
            string rootFolderName
            )
        {
            var subFiles = folder.GetFiles("*");

            if (subFiles == null || subFiles.Length == 0)
            {
                return;
            }

            foreach (var subFile in subFiles)
            {
                pdb.Files.Add(new FileDataBag() { File = subFile.FullName.Replace(rootFolderName, string.Empty) });
            }
        }

        private static void AddFilesRecursive(IFolder[] folders, PackageDataBag pdb, string rootFolderName)
        {
            foreach (var folder in folders)
            {
                AddFilesRecursive(
                    folder.GetFolders("*"),
                    pdb,
                    rootFolderName
                    );

                AddFilesRecursive(folder, pdb, rootFolderName);
            }
        }

        private static PackageDefinition LoadPackageDefinition(IFile inputFile)
        {
            using (var inputStream = inputFile.OpenRead())
            {
                var serializer = new XmlSerializer(typeof(PackageDefinition));

                var type = typeof(SnapManifestManager);
                var setupSchema = XmlSchema.Read(
                    type.Assembly.GetManifestResourceStream(type.Namespace + ".Schemas.PackageDefinition.xsd"),
                    null
                    );

                var readerSettings = new XmlReaderSettings();
                readerSettings.Schemas.Add(setupSchema);
                readerSettings.ValidationType = ValidationType.Schema;

                XmlReader xmlReader = XmlReader.Create(inputStream, readerSettings);

                return (PackageDefinition)serializer.Deserialize(xmlReader);
            }
        }
    }

    internal class PackageDataBag
    {
        public string Id { get; set; } = string.Empty;
        public int Version { get; set; } = 1;
        public string Title { get; set; } = string.Empty;
        public string Producer { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string ProductVersion { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public string ReleaseNotesUrl { get; set; } = string.Empty;
        public List<FileDataBag> Files { get; set; } = new List<FileDataBag>();
    }

    internal class FileDataBag
    {
        public string File { get; set; } = string.Empty;
    }

    public static class VersionExtensions
    {
        public static string ToStringForManifest(this Version @this)
        {
            if (@this == null)
            {
                return string.Empty;
            }
            return (@this.Build > -1) ? @this.ToString(3) : @this.ToString(2);
        }
    }
}
