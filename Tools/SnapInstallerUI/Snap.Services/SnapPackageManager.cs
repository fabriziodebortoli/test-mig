using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microarea.Snap.Core;
using Microarea.Snap.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microarea.Snap.Services
{
    internal class SnapPackageManager : IPackageManager
    {
        IRegistryService registryService;
        ISettings settings;

        public SnapPackageManager(ISettings settings, IRegistryService registryService)
        {
            this.settings = settings;
            this.registryService = registryService;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "ZipFile doesn't hold the stream so it doesn't dispose it, see ZipFile.IsStreamOwner for details")]
        internal static IPackage CreatePackage(IManifest manifest, IFile manifestFile, IFolder rootFolder, IFile outputFile)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException("manifest");
            }
            if (manifestFile == null)
            {
                throw new ArgumentNullException("manifestFile");
            }
            if (rootFolder == null)
            {
                throw new ArgumentNullException("rootFolder");
            }
            if (outputFile == null)
            {
                throw new ArgumentNullException("outputFile");
            }

            using (var outputStream = outputFile.OpenWrite())
            using (var zipOutputStream = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(outputStream))
            {
                zipOutputStream.SetLevel(9); //0-9, 9 being the highest level of compression

                using (var fileStream = manifestFile.OpenRead(rootFolder))
                {
                    //Aggiungo il manifest in modo che rimanga nella root dello zip
                    zipOutputStream.CompressFile(manifestFile.Name, fileStream);
                }

                //Imposto l'offset e poi aggiungo gli altri file
                zipOutputStream.SetFolderOffset(rootFolder.FullName);
                //Passo manifestFile.Name anziche` manifestFile.FullName affinche` nello zip il file manifest si trovi nella root
                foreach (var file in manifest.Files)
                {
                    using (var fileStream = file.OpenRead(rootFolder))
                    {
                        zipOutputStream.CompressFile(Path.Combine(rootFolder.FullName, file.FullName), fileStream);
                    }
                }
            }

            var package = new SnapPackage();
            package.Init(outputFile, manifest);

            return package;
        }

        public virtual IPackage CreatePackage(IFile manifestFile, IFolder rootFolder, IFile outputFile)
        {
            if (manifestFile == null)
            {
                throw new ArgumentNullException("manifestFile");
            }
            if (rootFolder == null)
            {
                throw new ArgumentNullException("rootFolder");
            }
            if (!rootFolder.Exists)
            {
                throw new ArgumentException("rootFolder does not exist");
            }
            if (outputFile == null)
            {
                throw new ArgumentNullException("outputFile");
            }

            var manifest = new SnapManifest();
            manifest.Init(manifestFile.OpenRead());

            return CreatePackage(manifest, manifestFile, rootFolder, outputFile);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "ZipFile doesn't hold the stream so it doesn't dispose it, see ZipFile.IsStreamOwner for details")]
        public virtual void Install(IPackage package, IFolder rootFolder)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }
            if (rootFolder == null)
            {
                throw new ArgumentNullException("rootFolder");
            }
            var packageWorkingFolder = IocFactory.Instance.GetInstance<IFolder, string>(Path.Combine(this.settings.WorkingFolder, package.Manifest.Id));
            //NB cancellazione working folder non transazionale perche` abbia effetto immediato
            if (Directory.Exists(packageWorkingFolder.FullName))
            {
                Directory.Delete(packageWorkingFolder.FullName, true);
            }
            using (var inputStream = package.PackageFile.OpenRead())
            using (var zipFile = new ZipFile(inputStream))
            {
                var installedDictionaries = this.registryService.RetrieveInstalledDictionaries();
                //Se non riesco a reperire info circa i dizionari installati procedo senza curarmene
                if (installedDictionaries == null || installedDictionaries.Length == 0)
                {
                    zipFile.ExtractAll(packageWorkingFolder.FullName);
                }
                else
                {
                    //Altrimenti devo installare solo i dizionari del pacchetto che corrispondono a quelli installati dall'MSI
                    string dictionary = null;
                    foreach (ZipEntry zipEntry in zipFile)
                    {
                        dictionary = GetDictionary(zipEntry.Name);
                        //se il file e` nella cartella di un dizionario...
                        if (dictionary != null)
                        {
                            //...ed e` contenuto nella lista dei dizionari da installare...
                            if (installedDictionaries.Where(id => string.Compare(id, dictionary, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault() != null)
                            {
                                //...allora lo installo
                                zipEntry.Extract(zipFile.GetInputStream(zipEntry), packageWorkingFolder.FullName);
                            }
                            //...altrimenti non lo installo.
                        }
                        else
                        {
                            //Se invece non e` un dizionario allora installo senza farmi domande
                            zipEntry.Extract(zipFile.GetInputStream(zipEntry), packageWorkingFolder.FullName);
                        }
                    }
                }
            }
            var manifestFile = packageWorkingFolder.GetFiles("*." + Constants.ManifestExtension)
                .FirstOrDefault()
                ;
            if (manifestFile != null)
            {
                manifestFile.Delete();
            }

            packageWorkingFolder.CopyTo(rootFolder);
        }

        readonly static Regex dictionaryFolderNameRegEx = new Regex("[a-zA-z]{2}-[a-zA-z]+");

        internal static string GetDictionary(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }
            foreach (var pathToken in fileName.Split('/'))
            {
                var match = dictionaryFolderNameRegEx.Match(pathToken);
                if (match.Success && string.Compare(match.Value, pathToken, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return pathToken;
                }
            }

            return null;
        }

        public virtual void Uninstall(IPackage package, IFolder rootFolder)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }
            if (rootFolder == null)
            {
                throw new ArgumentNullException("rootFolder");
            }
            if (package.Manifest == null)
            {
                throw new PackageException("Package without manifest");
            }

            IFolder parentFolder = null;
            //Ordino i file per quantita` di token di path in modo da partire a cancellare dai file piu` annidati verso
            //i file meno annidati in modo da poter cancellare anche le folder se sono vuote.
            foreach (var manifestFile in package.Manifest.Files.OrderByDescending(f => f.FullName.Split('\\').Count()))
            {
                if (!manifestFile.IsPathRooted)
                {
                    manifestFile.Delete(rootFolder);
                    parentFolder = manifestFile.ParentFolder;
                    if (parentFolder.IsEmpty(rootFolder))
                    {
                        parentFolder.Delete(rootFolder);
                    }
                }
                else
                {
                    manifestFile.Delete();
                    parentFolder = manifestFile.ParentFolder;
                    if (parentFolder.IsEmpty())
                    {
                        parentFolder.Delete();
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "ZipFile doesn't hold the stream so it doesn't dispose it, see ZipFile.IsStreamOwner for details")]
        public virtual void Update(IPackage oldPackage, IPackage newPackage, IFolder rootFolder)
        {
            if (oldPackage == null)
            {
                throw new ArgumentNullException("oldPackage");
            }
            if (newPackage == null)
            {
                throw new ArgumentNullException("newPackage");
            }
            if (rootFolder == null)
            {
                throw new ArgumentNullException("rootFolder");
            }
            var packageWorkingFolder = IocFactory.Instance.GetInstance<IFolder, string>(Path.Combine(this.settings.WorkingFolder, newPackage.Manifest.Id));
            //NB cancellazione working folder non transazionale perche` abbia effetto immediato
            if (Directory.Exists(packageWorkingFolder.FullName))
            {
                Directory.Delete(packageWorkingFolder.FullName, true);
            }
            using (var inputStream = newPackage.PackageFile.OpenRead())
            using (var zipFile = new ZipFile(inputStream))
            {
                var installedDictionaries = this.registryService.RetrieveInstalledDictionaries();
                //Se non riesco a reperire info circa i dizionari installati procedo senza curarmene
                if (installedDictionaries == null || installedDictionaries.Length == 0)
                {
                    zipFile.ExtractAll(packageWorkingFolder.FullName);
                }
                else
                {
                    //Altrimenti devo installare solo i dizionari del pacchetto che corrispondono a quelli installati dall'MSI
                    string dictionary = null;
                    foreach (ZipEntry zipEntry in zipFile)
                    {
                        dictionary = GetDictionary(zipEntry.Name);
                        //se il file e` nella cartella di un dizionario...
                        if (dictionary != null)
                        {
                            //...ed e` contenuto nella lista dei dizionari da installare...
                            if (installedDictionaries.Where(id => string.Compare(id, dictionary, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault() != null)
                            {
                                //...allora lo installo
                                zipEntry.Extract(zipFile.GetInputStream(zipEntry), packageWorkingFolder.FullName);
                            }
                            //...altrimenti non lo installo.
                        }
                        else
                        {
                            //Se invece non e` un dizionario allora installo senza farmi domande
                            zipEntry.Extract(zipFile.GetInputStream(zipEntry), packageWorkingFolder.FullName);
                        }
                    }
                }
            }
            var manifestFile = packageWorkingFolder.GetFiles("*." + Constants.ManifestExtension)
                .FirstOrDefault()
                ;
            if (manifestFile != null)
            {
                manifestFile.Delete();
            }

            //Copio tutti i nuovi file nella cartella di destinazione (sovrascrivendo cosi` i file che sono comuni tra vecchio pacchetto e nuovo pacchetto)...
            packageWorkingFolder.CopyTo(rootFolder);

            //...solo dopo cancello tutti i file che sono presenti nel vecchio pacchetto ma che non sono piu` presenti nel nuovo (rimarrebbero come cadaveri).
            var oldFiles = oldPackage.Manifest.Files
                .Where(oldFile => newPackage.Manifest.Files.Where(newFile => string.CompareOrdinal(newFile.FullName, oldFile.FullName) == 0).Count() == 0);

            foreach (var oldFile in oldFiles)
            {
                oldFile.Delete(rootFolder);
            }

            //Agire cosi` e` piu` efficiente rispetto a cancellare prima tutti i file del vecchio pacchetto ed installare dopo tutti i file del nuovo pacchetto.
            //Logica mutuata da windows installer.
        }
    }

    internal static class ZipEntryExtensions
    {
        internal static void Extract(this ZipEntry @this, Stream zipStream, string where)
        {
            string fullZipToPath = Path.Combine(where, @this.Name);
            string directoryName = Path.GetDirectoryName(fullZipToPath);
            if (directoryName.Length > 0 && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
                Directory.SetLastWriteTime(directoryName, @this.DateTime);
            }
            if (!@this.IsFile)
            {
                return;
            }

            // 4K is optimum
            byte[] buffer = new byte[4096];

            // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
            // of the file, but does not waste memory.
            // The "using" will close the stream even if an exception occurs.
            using (FileStream streamWriter = System.IO.File.Create(fullZipToPath))
            {
                StreamUtils.Copy(zipStream, streamWriter, buffer);
            }
            System.IO.File.SetLastWriteTime(fullZipToPath, @this.DateTime);
        }
    }

    internal static class ZipFileExtensions
    {
        internal static void ExtractAll(this ZipFile @this, string where)
        {
            foreach (ZipEntry zipEntry in @this)
            {
                // Ignore directories
                if (!zipEntry.IsFile)
                {
                    continue;
                }

                zipEntry.Extract(@this.GetInputStream(zipEntry), where);
            }
        }
    }

    internal static class ZipOutputStreamExtensions
    {
        static readonly Dictionary<ZipOutputStream, int> cache = new Dictionary<ZipOutputStream, int>();

        internal static void SetFolderOffset(this ZipOutputStream @this, string rootFolderFullName)
        {
            int temp = 0;
            if (!cache.TryGetValue(@this, out temp))
            {
                // This setting will strip the leading part of the folder path in the entries, to
                // make the entries relative to the starting folder.
                // To include the full path for each entry up to the drive root, assign folderOffset = 0.
                int folderOffset = 0;
                if (!string.IsNullOrWhiteSpace(rootFolderFullName))
                {
                    folderOffset = rootFolderFullName.Length + (rootFolderFullName.EndsWith("\\", StringComparison.OrdinalIgnoreCase) ? 0 : 1);
                }

                cache.Add(@this, folderOffset);
            }
        }

        internal static void CompressFile(this ZipOutputStream @this, string fileName, Stream fileStream)
        {
            //FileInfo fi = new FileInfo(fileName);

            var folderOffset = 0;
            cache.TryGetValue(@this, out folderOffset);

            var entryName = fileName.Substring(folderOffset); // Makes the name in zip based on the folder

            entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
            var newEntry = new ZipEntry(entryName);
            //newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity

            // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
            // A password on the ZipOutputStream is required if using AES.
            //   newEntry.AESKeySize = 256;

            // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
            // you need to do one of the following: Specify UseZip64.Off, or set the Size.
            // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
            // but the zip will be in Zip64 format which not all utilities can understand.
            @this.UseZip64 = UseZip64.Off;
            //newEntry.Size = fi.Length;

            @this.PutNextEntry(newEntry);

            // Zip the file in buffered chunks
            byte[] buffer = new byte[4096];
            StreamUtils.Copy(fileStream, @this, buffer);
            @this.CloseEntry();
        }
    }
}
