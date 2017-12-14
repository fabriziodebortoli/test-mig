using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;

namespace Microarea.TaskBuilderNet.Core.Generic
{
    //=========================================================================
    public class CompressEventArgs : EventArgs
    {
        #region Data Members

        //---------------------------------------------------------------------
        public enum CompressionAction { Compressing, Uncompressing };
        public enum ActionResult { Success, Failed, UserAborted };

        //---------------------------------------------------------------------
        public CompressionAction Action;
        public string PackageFileName;
        public ActionResult Result = ActionResult.Success;
        public readonly string CurrentProcessingFileName;
        public readonly int NrOfEntries;

        #endregion

        #region Construction, Destruction And Initialization

        //---------------------------------------------------------------------
        public CompressEventArgs(CompressionAction action, string currentProcessingFileName)
        {
            this.Action = action;
            this.CurrentProcessingFileName = currentProcessingFileName;
        }

        //---------------------------------------------------------------------
        public CompressEventArgs(string packageFileName, string currentProcessingFileName)
        {
            this.PackageFileName = packageFileName;
            this.CurrentProcessingFileName = currentProcessingFileName;
            this.NrOfEntries = 0;
        }
        
        //---------------------------------------------------------------------
        public CompressEventArgs(string packageFileName, string currentProcessingFileName, ActionResult result)
        {
            this.PackageFileName = packageFileName;
            this.CurrentProcessingFileName = currentProcessingFileName;
            this.Result = result;
        }
        #endregion
    }

    /// Exception to be thrown and managed into the code
    //================================================================================
    public class CompressionException : Exception
    {
        #region Construction, Destruction And Initialization

        //---------------------------------------------------------------------
        public CompressionException(string message)
            : 
            base (message)
         {
         }
        
        #endregion
    }

    /// Single entry into the compressed package
    //================================================================================
    public class CompressedEntry
    {
        #region Public Enums

        public enum EntryType { Files, Folders, Both };

        #endregion

        #region Data Members

        private Uri     uri = null;
        private bool    isDirectory = false;
        private Stream  stream = null;

        #endregion

        #region Properties

        public Uri      AllUri          { get { return uri; } }
        public string   Name            { get { return uri.ToString(); } }
        public virtual long     Size            { get { return stream.Length; } }
        public virtual bool     IsDirectory     { get { int len = Name.Length; return len > 0 && Name[len - 1] == '/'; } }
        public virtual Stream   CurrentStream   { get { return stream; } }

        #endregion

        #region Construction, Destruction And Initialization

        //---------------------------------------------------------------------
        public CompressedEntry (Uri uri, bool isDirectory, Stream stream)
        {
            string relativeUri = uri.ToString(); 
            
            if (relativeUri.StartsWith("/"))
                relativeUri = relativeUri.Substring(1);
            
            this.uri = new Uri(relativeUri, UriKind.Relative);
            this.stream = stream;
            this.isDirectory = isDirectory;
        }
        
        #endregion
    }

    /// Single entry into the compressed package
    //================================================================================
    public class SharpZipLibCompressedEntry : CompressedEntry
    {
        #region Data Members

        ZipEntry part;
        ZipFile package;

        #endregion

        #region Properties

        public override long Size { get { return part.Size; } }
        public override bool IsDirectory { get { return part.IsDirectory; } }
        public override Stream CurrentStream { get { return package.GetInputStream(part); } }

        #endregion

        #region Construction, Destruction And Initialization

        //---------------------------------------------------------------------
        public SharpZipLibCompressedEntry(Uri uri, bool isDirectory, ZipFile package, ZipEntry part)
            : base (uri, isDirectory, null)
        {
            this.package = package;
            this.part = part;
        }

        #endregion
    }

    internal class ZipFilesManager
    {
        private ArrayList excludeFiles = new ArrayList();

        public ArrayList ExcludeFiles { get { return excludeFiles; } set { excludeFiles = value; } }

        //---------------------------------------------------------------------
        protected bool IsFileToCompress(string file)
        {
            foreach (string fileName in excludeFiles)
                if (string.Compare(file, fileName, true, CultureInfo.InvariantCulture) == 0)
                    return false;

            return true;
        }
    }

    /// Driver fir ZIP compression
    //================================================================================
    internal class ZipDriver : ZipFilesManager, IZipDriver
    {
        #region Data Members

        //---------------------------------------------------------------------
        private static string forwardSlash = "/";

        //---------------------------------------------------------------------
        private string          fileName        = string.Empty;
        private Uri             fileAbsoluteUri = null;
        private Package         package         = null;
        private CompressedFile  compressedFile  = null;
        
        #endregion

        #region Properties

        public string       FileName { get { return fileName; } }
        public bool         IsMemoryPackage { get { return fileName == string.Empty && fileAbsoluteUri == null && package != null; } }

        #endregion

        #region Construction, Destruction And Initialization

        //---------------------------------------------------------------------
        public ZipDriver(CompressedFile cf)
        {
            compressedFile = cf;
        }

        //---------------------------------------------------------------------
        public bool IsOpened ()
        { 
            return package != null; 
        } 
    
        //---------------------------------------------------------------------
        public bool Open(Stream stream, CompressedFile.OpenMode openMode)
        {
            // already opened
            if (IsOpened())
                throw (new CompressionException(string.Format("Cannot open file {0} because it is already opened!", fileName)));

            FileMode mode = OpenMode2FileMode(openMode);
            FileAccess access = OpenMode2FileAccess(openMode);

            try
            {
                package = ZipPackage.Open(stream, mode, access);
                this.fileName  = string.Empty;
                fileAbsoluteUri = null;
            }
            catch (Exception e)
            {
                if (package != null)
                    package.Close();
                throw new CompressionException(string.Format("Cannot open compressed file {0} due to the following error:\r\n {1} ", fileName, e.Message));
            }
            return true;
        }

        //---------------------------------------------------------------------
        public bool Open(string fileName, CompressedFile.OpenMode openMode)
        {
            if (fileName == string.Empty)
                throw (new CompressionException("Cannot open file because fileName parameter is emtpy!"));

            // already opened
            if (IsOpened())
                throw (new CompressionException(string.Format("Cannot open file {0} because it is already opened!", fileName)));

            FileMode mode = OpenMode2FileMode(openMode);
            FileAccess access = OpenMode2FileAccess(openMode);

            if (mode != FileMode.Open)
            {
                FileInfo fi = new FileInfo(fileName);
                if (fi.Exists && ((fi.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly))
                    fi.Attributes -= FileAttributes.ReadOnly;
            }

            try
            {
                package = ZipPackage.Open(fileName, mode, access);
                this.fileName = fileName;
                fileAbsoluteUri = new Uri(fileName, UriKind.RelativeOrAbsolute);
            }
            catch (Exception e)
            {
                if (package != null)
                    package.Close();
                throw new CompressionException(string.Format("Cannot open compressed file {0} due to the following error:\r\n {1} ", fileName, e.Message));
            }
            return true;
        }

        //---------------------------------------------------------------------
        public void Close()
        {
            if (!IsOpened())
                return;

            if (package.FileOpenAccess != FileAccess.Read)
                package.Flush();
            package.Close();
            package = null;
        }

        #endregion

        #region Utility Methods

        //---------------------------------------------------------------------
        public Uri GetRelativeUri(string file, string relativePathFrom)
        {
            // file has an absolute uri and path is relative from the compressed file root
            if (file.Contains(Path.VolumeSeparatorChar) && relativePathFrom == string.Empty)
                return fileAbsoluteUri.MakeRelativeUri(new Uri(file, UriKind.Absolute));

            // I esclude alternative path
            string alternativePath = file;
			if (relativePathFrom != string.Empty)
				alternativePath = alternativePath.ReplaceNoCase(relativePathFrom, string.Empty);

            return new Uri(alternativePath, UriKind.Relative);
        }

        //---------------------------------------------------------------------
        private FileMode OpenMode2FileMode(CompressedFile.OpenMode openMode)
        {
            switch (openMode)
            {
                case CompressedFile.OpenMode.CreateAlways:
                    return FileMode.Create;
                case CompressedFile.OpenMode.CreateIfNotExist:
                    return FileMode.CreateNew;
                case CompressedFile.OpenMode.Write:
                    return FileMode.Truncate;
                case CompressedFile.OpenMode.Append:
                    return FileMode.Append;
                default:
                    return FileMode.Open;
            }
        }

        //---------------------------------------------------------------------
        private FileAccess OpenMode2FileAccess(CompressedFile.OpenMode openMode)
        {
            switch (openMode)
            {
                case CompressedFile.OpenMode.Read:
                    return FileAccess.Read;
                default:
                    return FileAccess.ReadWrite;
            }
        }

        //---------------------------------------------------------------------
        private void CopyStreamIntoPart(Stream source, ref PackagePart part)
        {
            const int bufSize = 4096;
            byte[] buf = new byte[bufSize];
            int bytesRead = 0;

            Stream destination = part.GetStream(FileMode.Create, FileAccess.Write);
            source.Seek(0, SeekOrigin.Begin);
            while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
                destination.Write(buf, 0, bytesRead);
            destination.Close();
            part.Package.Flush();
        }

        #endregion

        #region Compression Methods

        //---------------------------------------------------------------------
        public bool AddStream(string name, Stream stream)
        {
            if (name == string.Empty)
                throw (new CompressionException("Cannot add file because name parameter is empty!"));

            if (!IsOpened())
                throw (new CompressionException(string.Format("Cannot add stream {0} because compressed file {1} is not opened!", name, fileName)));

            Uri relativeUri = new Uri(name, UriKind.Relative);
            Uri uriPart = PackUriHelper.CreatePartUri(relativeUri);

            PackagePart part = package.CreatePart(uriPart, "", CompressionOption.Normal);
            if (part == null)
                throw (new CompressionException(string.Format("Cannot create part for the file {0} to be added!", name)));


            try
            {
                CopyStreamIntoPart(stream, ref part);
            }
            catch (Exception e)
            {
                throw (new CompressionException(string.Format("Cannot read content of the stream {0} due to the following error: {1} !", name, e.Message)));
            }

            return true;
        }

        //---------------------------------------------------------------------
        public bool AddFile(string file, Uri zippedUri, string fileTitle)
        {
            // file exclusions
            if (!IsFileToCompress(Path.GetFileName(file)))
                return true;

            compressedFile.FireBeginCompressFile(this, new CompressEventArgs(fileName, file));

            if (file == string.Empty)
            {
                compressedFile.FireEndCompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException("Cannot add file because file parameter is empty!"));
            }

            // file not opened
            if (!IsOpened())
            {
                compressedFile.FireEndCompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot add file {0} because compressed file {1} is not opened!", file, fileName)));
            }

            if (!File.Exists(file))
            {
                compressedFile.FireEndCompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot add file {0} because it does not exist on the file system!", file, fileName)));
            }

            // I create the Uri and the part "\\" + 
            Uri uriPart = fileTitle == string.Empty ?
                            PackUriHelper.CreatePartUri(zippedUri)
                            :
                            PackUriHelper.CreatePartUri(new Uri(fileTitle, UriKind.Relative))
                            ;

            PackagePart part = package.CreatePart(uriPart, "", CompressionOption.Normal);
            if (part == null)
            {
                compressedFile.FireEndCompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot create part for the file {0} to be added!", file)));
            }

            try
            {
                using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    CopyStreamIntoPart(fileStream, ref part);
                }
            }
            catch (Exception e)
            {
                compressedFile.FireEndCompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot read content of the file {0} due to the following error: {1} !", file, e.Message)));
            }

            compressedFile.FireEndCompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Success));
            return true;
        }

        //---------------------------------------------------------------------
        public bool AddFolder(string path, bool recursive, string relativePathFrom)
        {
            compressedFile.FireBeginCompressFolder(this, new CompressEventArgs(fileName, path));

            if (path == string.Empty)
            {
                compressedFile.FireEndCompressFolder(this, new CompressEventArgs(fileName, path, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException("Cannot add folder because path parameter is empty!"));
            }

            if (!IsOpened())
            {
                compressedFile.FireEndCompressFolder(this, new CompressEventArgs(fileName, path, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot add folder {0} because compressed file {1} is not opened!", path, fileName)));
            }

            if (!Directory.Exists(path))
            {
                compressedFile.FireEndCompressFolder(this, new CompressEventArgs(fileName, path, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot add folder {0} because it does not exist on the file system!", path, fileName)));
            }

            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
                AddFile(file, GetRelativeUri(file, relativePathFrom), string.Empty);

            if (recursive)
            {
                foreach (string folder in Directory.GetDirectories(path))
                    AddFolder(folder, recursive, relativePathFrom);
            }

            compressedFile.FireEndCompressFolder(this, new CompressEventArgs(fileName, path, CompressEventArgs.ActionResult.Success));
            return true;
        }

        //---------------------------------------------------------------------
        public bool Delete()
        {
            if (!File.Exists(fileName))
                return false;

            if (IsOpened())
                Close();

            try { File.Delete(fileName);  }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #endregion

        #region Uncompression Methods

        //---------------------------------------------------------------------
        public bool ExtractAll(string outputPath)
        {
            if (outputPath == string.Empty)
                throw (new CompressionException("Cannot extract all because outputPath parameter is empty!"));

            if (!IsOpened())
                throw (new CompressionException(string.Format("Cannot extract all because compressed file {0} is not opened!", fileName)));

            foreach (ZipPackagePart part in package.GetParts())
            {
                ExtractFile(part.Uri.ToString(), outputPath);
            }

            return true;
        }

        //---------------------------------------------------------------------
        public Stream ExtractFileAsStream(string file)
        {
            if (file == string.Empty)
                throw (new CompressionException("Cannot extract file because file or outputPath parameter is emtpy!"));

            if (!IsOpened())
                throw (new CompressionException(string.Format("Cannot extract file {0} because compressed file {1} is not opened!", file, fileName)));

            string relativeFile = file;
            if (!relativeFile.StartsWith(forwardSlash))
                relativeFile = forwardSlash + relativeFile;

            Uri relativeUri = new Uri(relativeFile, UriKind.Relative);
            // parth could not exist in the package
            PackagePart part = null;
            try
            {
                if (package.PartExists(relativeUri))
                {
                    part = package.GetPart(relativeUri);
                }
            }
            catch (Exception e)
            {
                throw (new CompressionException(string.Format("Cannot extract file due to the following exception: {0}!", e.Message)));
            }
            
            return part == null ? null : part.GetStream();
        }

        //---------------------------------------------------------------------
        public bool ExtractFile (string file, string outputPath)
        {
            compressedFile.FireBeginUncompressFile(this, new CompressEventArgs(fileName, file));

            if (file == string.Empty || outputPath == string.Empty)
            {
                compressedFile.FireEndUncompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException("Cannot extract file because file or outputPath parameter is emtpy!"));
            }

            if (!IsOpened())
            {
                compressedFile.FireEndUncompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot extract file {0} because compressed file {1} is not opened!", file, fileName)));
            }

            // path creation
            if (!Directory.Exists(outputPath))
            {
                DirectoryInfo folderInfo = Directory.CreateDirectory(outputPath);

                if (folderInfo == null)
                {
                    compressedFile.FireEndUncompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Failed));
                    throw (new CompressionException(string.Format("Cannot extract file {0} as cannot create folder {1} !", file, outputPath)));
                }
            }

            Stream fileStream = ExtractFileAsStream(file);

            if (fileStream == null)
            {
                compressedFile.FireEndUncompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Failed));
                return false;
            }

            string dosFile = file.Replace(forwardSlash, Path.DirectorySeparatorChar.ToString());
            while (dosFile.StartsWith(Path.DirectorySeparatorChar.ToString()))
                dosFile = dosFile.Substring(1);
            
            string sUnescapedDosFile = Uri.UnescapeDataString(dosFile);
            string fileToCreate = Path.Combine(outputPath, sUnescapedDosFile);
            string pathToCreate = Path.GetDirectoryName(fileToCreate);

            // path creation
            if (!Directory.Exists(pathToCreate))
            {
                DirectoryInfo folderInfo = Directory.CreateDirectory(pathToCreate);

                if (folderInfo == null)
                {
                    compressedFile.FireEndUncompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Failed));
                    throw (new CompressionException(string.Format("Cannot extract file {0} as cannot create folder {1} !", file, pathToCreate)));
                }
            }

            try
            {
                byte[] content = new byte[fileStream.Length];
                fileStream.Read(content, 0, content.Length);

                System.IO.File.WriteAllBytes(fileToCreate, content);
            }
            catch (Exception e)
            {
                compressedFile.FireEndUncompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot extract file {0} due to the following exception {1} !", file, e.Message)));
            }

            compressedFile.FireEndUncompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Success));
            return true;
        }

        //---------------------------------------------------------------------
        public bool ExtractFolder(string path, string outputPath, bool recursive)
        {
            compressedFile.FireBeginUncompressFolder(this, new CompressEventArgs(fileName, path));
            
            if (path == string.Empty || outputPath == string.Empty)
            {
                compressedFile.FireEndUncompressFolder(this, new CompressEventArgs(fileName, path, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException("Cannot extract folder because path or outputPath parameter is emtpy!"));
            }

            if (!IsOpened())
            {
                compressedFile.FireEndUncompressFolder(this, new CompressEventArgs(fileName, path, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot extract folder {0} because compressed file {1} is not opened!", path, fileName)));
            }

            string rootFolder = Path.GetDirectoryName(fileAbsoluteUri.AbsolutePath);
            string originalFolder = Path.Combine(rootFolder, path);
            Uri absoluteUri = new Uri(originalFolder);
            Uri relativeUri = fileAbsoluteUri.MakeRelativeUri(absoluteUri);

            string relativeFile = relativeUri.ToString();
            if (!relativeFile.StartsWith(forwardSlash))
                relativeFile = forwardSlash + relativeFile;

            // only sub-path of the given uri have to be extracted
            foreach (ZipPackagePart part in package.GetParts())
            {
                if (part.Uri.ToString().StartsWith(relativeFile))
                    ExtractFile(part.Uri.ToString(), outputPath);
            }

            compressedFile.FireEndUncompressFolder(this, new CompressEventArgs(fileName, path, CompressEventArgs.ActionResult.Success));
            return true;
        }

        #endregion

        #region Compressed Entries Enumeration

        //---------------------------------------------------------------------
        public CompressedEntry GetEntry(Uri uri)
        {
            if (uri.ToString() == string.Empty)
                throw (new CompressionException("Cannot get entry because entryName parameter is empty!"));

            if (!IsOpened())
                throw (new CompressionException(string.Format("Cannot get entry {0} because compressed file {1} is not opened!", uri.ToString(), fileName)));

            PackagePart part = package.GetPart(uri);
            if (part != null)
                return new CompressedEntry(part.Uri, false, part.GetStream());
            
            return null;
        }

        //---------------------------------------------------------------------
        public CompressedEntry[] GetAllEntries()
        {
            if (!IsOpened())
                throw (new CompressionException(string.Format("Cannot get all entries because compressed file {0} is not opened!", fileName)));

            PackagePartCollection parts = package.GetParts();
            if (parts == null)
                return null;

            int nrOfParts = parts.Count<PackagePart>();
            CompressedEntry[] entries = new CompressedEntry[nrOfParts];
            int nIndex = -1;
            foreach (PackagePart part in parts)
            {
                nIndex++;
                entries[nIndex] = new CompressedEntry(part.Uri, false, part.GetStream());
            }

            return entries;
        }

        //---------------------------------------------------------------------
        public int GetNrOfEntries()
        {
            if (!IsOpened())
                throw (new CompressionException(string.Format("Cannot get nr of entries because compressed file {0} is not opened!", fileName)));

            PackagePartCollection collection = package.GetParts();

            if (collection != null)
                return collection.Count<PackagePart>();
            
            return 0;
        }

        //---------------------------------------------------------------------
        public bool ExistsEntry(Uri uri)
        {
            if (uri.ToString() == string.Empty)
                throw (new CompressionException("Cannot check entry because uri parameter is empty!"));

            if (!IsOpened())
                throw (new CompressionException(string.Format("Cannot check entry {0} because compressed file {1} is not opened!", uri.ToString(), fileName)));

            return package.PartExists(uri);
        }

        //---------------------------------------------------------------------
        public CompressedEntry[] GetEntries(Uri relativeUri, CompressedEntry.EntryType type, bool recursive)
        {
            compressedFile.FireBeginUncompressFolder(this, new CompressEventArgs(fileName, relativeUri.ToString()));

            if (relativeUri.ToString() == string.Empty)
            {
                compressedFile.FireEndUncompressFolder(this, new CompressEventArgs(fileName, relativeUri.ToString(), CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException("Cannot extract entry because uri parameter is emtpy!"));
            }

            if (!IsOpened())
            {
                compressedFile.FireEndUncompressFolder(this, new CompressEventArgs(fileName, relativeUri.ToString(), CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot extract entry {0} because compressed file {1} is not opened!", relativeUri.ToString(), fileName)));
            }

            string relativeFile = relativeUri.ToString();
            if (!relativeFile.StartsWith(forwardSlash))
                relativeFile = forwardSlash + relativeFile;

            PackagePartCollection parts = package.GetParts();
            if (parts == null)
            {
                compressedFile.FireEndUncompressFolder(this, new CompressEventArgs(relativeFile, relativeUri.ToString(), CompressEventArgs.ActionResult.Failed));
               return null;
            }

            ArrayList allEntries = new ArrayList();
            // only sub-path of the given uri have to be extracted
            foreach (ZipPackagePart part in parts)
            {
                if (!part.Uri.ToString().StartsWith(relativeFile))
                    continue;

                // is a file
                if (!part.Uri.ToString().EndsWith(forwardSlash) && type != CompressedEntry.EntryType.Folders)
                    allEntries.Add(GetEntry(part.Uri));

                // is a folder
                if (part.Uri.ToString().EndsWith(forwardSlash) && type != CompressedEntry.EntryType.Files)
                    allEntries.Add(GetEntry(part.Uri));
            }

            // I resize correctly the array 
            CompressedEntry[] entries = new CompressedEntry[allEntries.Count];
            allEntries.CopyTo(entries);

            compressedFile.FireEndUncompressFolder(this, new CompressEventArgs(fileName, relativeUri.ToString(), CompressEventArgs.ActionResult.Success));
            return entries;
        }

        #endregion
    }

    //================================================================================
    internal class SharpZipLibDriver : ZipFilesManager, IZipDriver
    {
        const string forwardSlash = "/";

        string fileName = string.Empty;
        Uri fileAbsoluteUri;
        ZipFile package;
        ZipOutputStream zipOutput;
        CompressedFile compressedFile;

        //---------------------------------------------------------------------
        public string FileName { get { return fileName; } }

        //---------------------------------------------------------------------
        public SharpZipLibDriver(CompressedFile cf)
        {
            compressedFile = cf;
        }

        //---------------------------------------------------------------------
        public bool AddFile(string file, Uri zippedUri, string fileTitle)
        {
            // file exclusions
            if (!IsFileToCompress(Path.GetFileName(file)))
                return true;

            compressedFile.FireBeginCompressFile(this, new CompressEventArgs(fileName, file));

            if (string.IsNullOrWhiteSpace(file))
            {
                compressedFile.FireEndCompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException("Cannot add file because file parameter is empty!"));
            }

            // file not opened
            if (!IsOpened())
            {
                compressedFile.FireEndCompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot add file {0} because compressed file {1} is not opened!", file, fileName)));
            }

            if (!File.Exists(file))
            {
                compressedFile.FireEndCompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot add file {0} because it does not exist on the file system!", file, fileName)));
            }

            try
            {
                using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    var entryName = ZipEntry.CleanName(zippedUri.IsAbsoluteUri ? zippedUri.LocalPath : zippedUri.ToString()); // Removes drive from name and fixes slash direction
                    var newEntry = new ZipEntry(entryName);
                    var fi = new FileInfo(file);
                    newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity

                    // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                    // A password on the ZipOutputStream is required if using AES.
                    //   newEntry.AESKeySize = 256;

                    // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                    // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                    // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                    // but the zip will be in Zip64 format which not all utilities can understand.
                    //zipOutput.UseZip64 = UseZip64.Off;
                    newEntry.Size = fi.Length;

                    zipOutput.PutNextEntry(newEntry);

                    // Zip the file in buffered chunks
                    byte[] buffer = new byte[4096];
                    bool copying = true;

                    while (copying)
                    {
                        int bytesRead = fileStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            zipOutput.Write(buffer, 0, bytesRead);
                        }
                        else
                        {
                            zipOutput.Flush();
                            copying = false;
                        }
                    }
                    zipOutput.CloseEntry();
                }
            }
            catch (Exception e)
            {
                compressedFile.FireEndCompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot read content of the file {0} due to the following error: {1} !", file, e.Message)));
            }

            compressedFile.FireEndCompressFile(this, new CompressEventArgs(fileName, file, CompressEventArgs.ActionResult.Success));
            return true;
        }

        //---------------------------------------------------------------------
        public bool AddFolder(string path, bool recursive, string relativePathFrom)
        {
            compressedFile.FireBeginCompressFolder(this, new CompressEventArgs(fileName, path));

            if (path == string.Empty)
            {
                compressedFile.FireEndCompressFolder(this, new CompressEventArgs(fileName, path, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException("Cannot add folder because path parameter is empty!"));
            }

            if (!IsOpened())
            {
                compressedFile.FireEndCompressFolder(this, new CompressEventArgs(fileName, path, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot add folder {0} because compressed file {1} is not opened!", path, fileName)));
            }

            if (!Directory.Exists(path))
            {
                compressedFile.FireEndCompressFolder(this, new CompressEventArgs(fileName, path, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot add folder {0} because it does not exist on the file system!", path, fileName)));
            }

            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
                AddFile(file, GetRelativeUri(file, relativePathFrom), string.Empty);

            if (recursive)
            {
                foreach (string folder in Directory.GetDirectories(path))
                    AddFolder(folder, recursive, relativePathFrom);
            }

            compressedFile.FireEndCompressFolder(this, new CompressEventArgs(fileName, path, CompressEventArgs.ActionResult.Success));
            return true;
        }

        //---------------------------------------------------------------------
        public bool AddStream(string name, Stream stream)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw (new CompressionException("Cannot add file because name parameter is empty!"));

            if (!IsOpened())
                throw (new CompressionException(string.Format("Cannot add stream {0} because compressed file {1} is not opened!", name, fileName)));

            try
            {
                //var fi = new FileInfo(name);
                var entryName = name;

                entryName = ZipEntry.CleanName(entryName);
                var newEntry = new ZipEntry(entryName);
                //newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity

                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                //zipOutput.UseZip64 = UseZip64.Off;
                //newEntry.Size = fi.Length;

                zipOutput.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                byte[] buffer = new byte[4096];
                bool copying = true;
                stream.Seek(0, SeekOrigin.Begin);

                while (copying)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        zipOutput.Write(buffer, 0, bytesRead);
                    }
                    else
                    {
                        zipOutput.Flush();
                        copying = false;
                    }
                }
                zipOutput.CloseEntry();
            }
            catch (Exception e)
            {
                throw (new CompressionException(string.Format("Cannot read content of the stream {0} due to the following error: {1} !", name, e.Message)));
            }

            return true;
        }

        //---------------------------------------------------------------------
        public void Close()
        {
            if (!IsOpened())
                return;

            if (package != null)
            {
                package.Close();
                package = null;
            }
            if (zipOutput != null)
            {
                zipOutput.Dispose();
                package = null;
            }
        }

        //---------------------------------------------------------------------
        public bool ExistsEntry(Uri uri)
        {
            if (uri == null)
                throw (new CompressionException("Cannot check entry because uri parameter is empty!"));

            if (!IsOpened())
                throw (new CompressionException(string.Format("Cannot check entry {0} because compressed file {1} is not opened!", uri.ToString(), fileName)));

            return package.FindEntry(uri.ToString(), true) > 0;
        }

        //---------------------------------------------------------------------
        public bool ExtractAll(string outputPath)
        {
            if (outputPath == string.Empty)
                throw (new CompressionException("Cannot extract all because outputPath parameter is empty!"));

            if (!IsOpened())
                throw (new CompressionException(string.Format("Cannot extract all because compressed file {0} is not opened!", fileName)));

            foreach (ZipEntry zipEntry in package)
            {
                // Ignore directories
                if (zipEntry.IsDirectory)
                {
                    continue;
                }

                ExtractFile(zipEntry.Name, outputPath);
            }

            return true;
        }

        //---------------------------------------------------------------------
        public bool ExtractFile(string fileName, string outputPath)
        {
            compressedFile.FireBeginUncompressFile(this, new CompressEventArgs(this.fileName, fileName));

            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(outputPath))
            {
                compressedFile.FireEndUncompressFile(this, new CompressEventArgs(this.fileName, fileName, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException("Cannot extract file because file or outputPath parameter is emtpy!"));
            }

            if (!IsOpened())
            {
                compressedFile.FireEndUncompressFile(this, new CompressEventArgs(this.fileName, fileName, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot extract file {0} because compressed file {1} is not opened!", fileName, this.fileName)));
            }

            // path creation
            if (!Directory.Exists(outputPath))
            {
                DirectoryInfo folderInfo = Directory.CreateDirectory(outputPath);

                if (folderInfo == null)
                {
                    compressedFile.FireEndUncompressFile(this, new CompressEventArgs(this.fileName, fileName, CompressEventArgs.ActionResult.Failed));
                    throw (new CompressionException(string.Format("Cannot extract file {0} as cannot create folder {1} !", fileName, outputPath)));
                }
            }

            var fileBytes = ExtractFileAsByteArray(fileName);

            if (fileBytes == null)
            {
                compressedFile.FireEndUncompressFile(this, new CompressEventArgs(this.fileName, fileName, CompressEventArgs.ActionResult.Failed));
                return false;
            }

            string dosFile = fileName.Replace(forwardSlash, Path.DirectorySeparatorChar.ToString());
            while (dosFile.StartsWith(Path.DirectorySeparatorChar.ToString()))
                dosFile = dosFile.Substring(1);

            string sUnescapedDosFile = Uri.UnescapeDataString(dosFile);
            string fileToCreate = Path.Combine(outputPath, sUnescapedDosFile);
            string pathToCreate = Path.GetDirectoryName(fileToCreate);

            // path creation
            if (!Directory.Exists(pathToCreate))
            {
                DirectoryInfo folderInfo = Directory.CreateDirectory(pathToCreate);

                if (folderInfo == null)
                {
                    compressedFile.FireEndUncompressFile(this, new CompressEventArgs(this.fileName, fileName, CompressEventArgs.ActionResult.Failed));
                    throw (new CompressionException(string.Format("Cannot extract file {0} as cannot create folder {1} !", fileName, pathToCreate)));
                }
            }

            try
            {
                System.IO.File.WriteAllBytes(fileToCreate, fileBytes);
            }
            catch (Exception e)
            {
                compressedFile.FireEndUncompressFile(this, new CompressEventArgs(this.fileName, fileName, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot extract file {0} due to the following exception {1} !", fileName, e.Message)));
            }

            compressedFile.FireEndUncompressFile(this, new CompressEventArgs(this.fileName, fileName, CompressEventArgs.ActionResult.Success));
            return true;
        }

        //---------------------------------------------------------------------
        internal byte[] ExtractFileAsByteArray(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw (new CompressionException("Cannot extract file because fileName or outputPath parameter is emtpy!"));

            if (!IsOpened())
                throw (new CompressionException(string.Format("Cannot extract file {0} because compressed file {1} is not opened!", fileName, this.fileName)));

            string relativeFile = fileName;
            if (!relativeFile.StartsWith(forwardSlash))
                relativeFile = forwardSlash + relativeFile;

            // 4K is optimum
            byte[] buffer = new byte[4096];

            // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
            // of the file, but does not waste memory.
            // The "using" will close the stream even if an exception occurs.
            using (MemoryStream destination = new MemoryStream())
            {
                var source = package.GetInputStream(package.GetEntry(fileName));
                bool copying = true;

                while (copying)
                {
                    int bytesRead = source.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        destination.Write(buffer, 0, bytesRead);
                    }
                    else
                    {
                        destination.Flush();
                        copying = false;
                    }
                }

                return destination.ToArray();
            }
        }

        //---------------------------------------------------------------------
        public Stream ExtractFileAsStream(string file)
        {
            var outputStream = new MemoryStream(ExtractFileAsByteArray(file));
            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream;
        }

        //---------------------------------------------------------------------
        public bool ExtractFolder(string path, string outputPath, bool recursive)
        {
            compressedFile.FireBeginUncompressFolder(this, new CompressEventArgs(fileName, path));

            if (path == string.Empty || outputPath == string.Empty)
            {
                compressedFile.FireEndUncompressFolder(this, new CompressEventArgs(fileName, path, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException("Cannot extract folder because path or outputPath parameter is emtpy!"));
            }

            if (!IsOpened())
            {
                compressedFile.FireEndUncompressFolder(this, new CompressEventArgs(fileName, path, CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot extract folder {0} because compressed file {1} is not opened!", path, fileName)));
            }

            string rootFolder = Path.GetDirectoryName(fileAbsoluteUri.AbsolutePath);
            string originalFolder = Path.Combine(rootFolder, path);
            Uri absoluteUri = new Uri(originalFolder);
            Uri relativeUri = fileAbsoluteUri.MakeRelativeUri(absoluteUri);

            string relativeFile = relativeUri.ToString();
            if (!relativeFile.StartsWith(forwardSlash))
                relativeFile = forwardSlash + relativeFile;

            // only sub-path of the given uri have to be extracted
            foreach (ZipEntry part in package)
            {
                if (part.Name.StartsWith(relativeFile))
                    ExtractFile(part.Name, outputPath);
            }

            compressedFile.FireEndUncompressFolder(this, new CompressEventArgs(fileName, path, CompressEventArgs.ActionResult.Success));
            return true;
        }

        //---------------------------------------------------------------------
        public CompressedEntry[] GetAllEntries()
        {
            if (!IsOpened())
                throw (new CompressionException(string.Format("Cannot get all entries because compressed file {0} is not opened!", fileName)));

            var entries = new List<CompressedEntry>();
            foreach (ZipEntry part in package)
            {
                entries.Add(new SharpZipLibCompressedEntry(new Uri(part.Name, UriKind.RelativeOrAbsolute), part.IsDirectory, package, part));
            }

            return entries.ToArray();
        }

        //---------------------------------------------------------------------
        public CompressedEntry[] GetEntries(Uri relativeUri, CompressedEntry.EntryType type, bool recursive)
        {
            compressedFile.FireBeginUncompressFolder(this, new CompressEventArgs(fileName, relativeUri.ToString()));

            if (relativeUri.ToString() == string.Empty)
            {
                compressedFile.FireEndUncompressFolder(this, new CompressEventArgs(fileName, relativeUri.ToString(), CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException("Cannot extract entry because uri parameter is empty!"));
            }

            if (!IsOpened())
            {
                compressedFile.FireEndUncompressFolder(this, new CompressEventArgs(fileName, relativeUri.ToString(), CompressEventArgs.ActionResult.Failed));
                throw (new CompressionException(string.Format("Cannot extract entry {0} because compressed file {1} is not opened!", relativeUri.ToString(), fileName)));
            }

            string relativeFile = relativeUri.ToString();
            if (!relativeFile.StartsWith(forwardSlash))
                relativeFile = forwardSlash + relativeFile;

            ArrayList allEntries = new ArrayList();
            // only sub-path of the given uri have to be extracted
            foreach (ZipEntry part in package)
            {
                if (!part.Name.StartsWith(relativeFile))
                    continue;

                // is a file
                if (!part.IsDirectory)
                    allEntries.Add(GetEntry(new Uri(part.Name, UriKind.RelativeOrAbsolute)));

                // is a folder
                if (part.Name.EndsWith(forwardSlash) && type != CompressedEntry.EntryType.Files)
                    allEntries.Add(GetEntry(new Uri(part.Name, UriKind.RelativeOrAbsolute)));
            }

            // I resize correctly the array 
            CompressedEntry[] entries = new CompressedEntry[allEntries.Count];
            allEntries.CopyTo(entries);

            compressedFile.FireEndUncompressFolder(this, new CompressEventArgs(fileName, relativeUri.ToString(), CompressEventArgs.ActionResult.Success));
            return entries;
        }

        //---------------------------------------------------------------------
        public CompressedEntry GetEntry(Uri uri)
        {
            if (uri == null)
                throw (new CompressionException("Cannot get entry because uri parameter is empty!"));

            if (!IsOpened())
                throw (new CompressionException(string.Format("Cannot get entry {0} because compressed file {1} is not opened!", uri.ToString(), fileName)));

            var part = package.GetEntry(uri.ToString());
            if (part != null)
                return new SharpZipLibCompressedEntry(uri, false, package, part);

            return null;
        }

        //---------------------------------------------------------------------
        public int GetNrOfEntries()
        {
            if (!IsOpened())
                throw (new CompressionException(string.Format("Cannot get nr of entries because compressed file {0} is not opened!", fileName)));

            int nrEntries = 0;
            foreach (ZipEntry zipEntry in package)
            {
                nrEntries++;
            }

            return nrEntries;
        }

        //---------------------------------------------------------------------
        public Uri GetRelativeUri(string file, string relativePathFrom)
        {
            // file has an absolute uri and path is relative from the compressed file root
            if (file.Contains(Path.VolumeSeparatorChar) && relativePathFrom == string.Empty)
                return new Uri(file, UriKind.Absolute);

            // I esclude alternative path
            string alternativePath = file;
            if (relativePathFrom != string.Empty)
                alternativePath = alternativePath.ReplaceNoCase(relativePathFrom, string.Empty);

            return new Uri(alternativePath, UriKind.Relative);
        }

        //---------------------------------------------------------------------
        public bool IsOpened()
        {
            return package != null || zipOutput != null;
        }

        //---------------------------------------------------------------------
        public bool Open(Stream stream, CompressedFile.OpenMode openMode)
        {
            // already opened
            if (IsOpened())
                throw (new CompressionException(string.Format("Cannot open file {0} because it is already opened!", fileName)));

            try
            {
                if (openMode == CompressedFile.OpenMode.Read)
                {
                    package = new ZipFile(stream);
                    zipOutput = null;
                }
                else
                {
                    package = null;
                    zipOutput = new ZipOutputStream(stream);
                }

                this.fileName = string.Empty;
                fileAbsoluteUri = null;
            }
            catch (Exception e)
            {
                if (package != null)
                    package.Close();
                throw new CompressionException(string.Format("Cannot open compressed file {0} due to the following error:\r\n {1} ", fileName, e.Message));
            }
            return true;
        }

        //---------------------------------------------------------------------
        public bool Open(string fileName, CompressedFile.OpenMode openMode)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw (new CompressionException("Cannot open file because fileName parameter is empty!"));

            // already opened
            if (IsOpened())
                throw (new CompressionException(string.Format("Cannot open file {0} because it is already opened!", fileName)));

            if (openMode != CompressedFile.OpenMode.Read)
            {
                FileInfo fi = new FileInfo(fileName);
                if (fi.Exists && ((fi.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly))
                    fi.Attributes -= FileAttributes.ReadOnly;
            }

            try
            {
                if (openMode == CompressedFile.OpenMode.Read)
                {
                    package = new ZipFile(fileName);
                    zipOutput = null;
                }
                else
                {
                    package = null;
                    zipOutput = new ZipOutputStream(File.OpenWrite(fileName));
                }

                this.fileName = fileName;
                fileAbsoluteUri = new Uri(fileName, UriKind.RelativeOrAbsolute);
            }
            catch (Exception e)
            {
                if (package != null)
                    package.Close();
                throw new CompressionException(string.Format("Cannot open compressed file {0} due to the following error:\r\n {1} ", fileName, e.Message));
            }
            return true;
        }
    }

    //================================================================================
    public class CompressedFile : IDisposable
    {
        #region Public Enums

        public enum OpenMode    { Read, Write, Append, CreateIfNotExist, CreateAlways };
        public enum Version { None, V1, V2 };

        #endregion

        #region Private Data Members

        private bool        isAborted = false;
        private IZipDriver   driver = null;


        #endregion

        #region Properties

        public bool      IsOpened       { get { return driver != null && driver.IsOpened(); } }
        public bool      IsAborted      { get { return isAborted; } }
        //public ArrayList ExcludeFiles   { get { return driver == null ? null : driver.ExcludeFiles; ; } set { if (driver != null) driver.ExcludeFiles = value; } }

        #endregion

        #region Construction, Destruction And Initialization

        //--------------------------------------------------------------------------------
        public CompressedFile()
            : this (Version.V1)
        {}
        //--------------------------------------------------------------------------------
        public CompressedFile(Version version)
        {
            switch (version)
            {
                case Version.V2:
                    driver = new SharpZipLibDriver(this);
                    break;
                default:
                    driver = new ZipDriver(this);
                    break;
            }
        }

        //--------------------------------------------------------------------------------
        public CompressedFile(Stream stream, OpenMode openMode)
            : this (stream, openMode, Version.V1)
        {
        }
        //--------------------------------------------------------------------------------
        public CompressedFile(Stream stream, OpenMode openMode, Version version)
        {
            switch (version)
            {
                case Version.V2:
                    driver = new SharpZipLibDriver(this);
                    break;
                default:
                    driver = new ZipDriver(this);
                    break;
            }

            Open(stream, openMode);
        }

        //--------------------------------------------------------------------------------
        public CompressedFile(string fileName, OpenMode openMode)
            : this (fileName, openMode, Version.V1)
        {
        }
        //--------------------------------------------------------------------------------
        public CompressedFile(string fileName, OpenMode openMode, Version version)
        {
            switch (version)
            {
                case Version.V2:
                    driver = new SharpZipLibDriver(this);
                    break;
                default:
                    driver = new ZipDriver(this);
                    break;
            }

            Open(fileName, openMode);
        }

        //---------------------------------------------------------------------
        public bool Open(string fileName, OpenMode openMode)
        {
            return driver.Open(fileName, openMode);
        }

        //---------------------------------------------------------------------
        public bool Open(Stream stream, OpenMode openMode)
        {
            return driver.Open(stream, openMode);
        }

        //---------------------------------------------------------------------
        public void Close()
        {
            driver.Close();
			FireCompressedFileClose(EventArgs.Empty);
        }

        //---------------------------------------------------------------------
        public void Dispose()
        {
            Close();
        }

        #endregion

        #region Events, Delegates And Related Methods

        // event handlers
        //---------------------------------------------------------------------
        public delegate void CompressEventHandler(object sender, CompressEventArgs arg);

        // events
        //---------------------------------------------------------------------
        public event CompressEventHandler	BeginCompressFile;
        public event CompressEventHandler	EndCompressFile;
        public event CompressEventHandler	BeginCompressFolder;
        public event CompressEventHandler	EndCompressFolder;
        public event CompressEventHandler	BeginUncompressFile;
        public event CompressEventHandler	EndUncompressFile;
        public event CompressEventHandler	BeginUncompressFolder;
        public event CompressEventHandler	EndUncompressFolder;
        public event CompressEventHandler	Aborted;
		public event EventHandler			CompressedFileClose;

        // event public methods
		//---------------------------------------------------------------------
		public void FireBeginCompressFile(object sender, CompressEventArgs e)
		{
            e.Action = CompressEventArgs.CompressionAction.Compressing;

            if (BeginCompressFile != null)
                BeginCompressFile(sender, e);
		}

        //---------------------------------------------------------------------
        public void FireEndCompressFile(object sender, CompressEventArgs e)
        {
            e.Action = CompressEventArgs.CompressionAction.Compressing;
            if (EndCompressFile != null)
                EndCompressFile(sender, e);
        }

        //---------------------------------------------------------------------
        public void FireBeginCompressFolder(object sender, CompressEventArgs e)
        {
            e.Action = CompressEventArgs.CompressionAction.Compressing;
            if (BeginCompressFolder != null)
                BeginCompressFolder(sender, e);
        }

        //---------------------------------------------------------------------
        public void FireEndCompressFolder(object sender, CompressEventArgs e)
        {
            e.Action = CompressEventArgs.CompressionAction.Compressing;
            if (EndCompressFolder != null)
                EndCompressFolder(sender, e);
        }

        //---------------------------------------------------------------------
        public void FireBeginUncompressFile(object sender, CompressEventArgs e)
        {
            e.Action = CompressEventArgs.CompressionAction.Uncompressing;
            if (BeginUncompressFile != null)
                BeginUncompressFile(sender, e);
        }

        //---------------------------------------------------------------------
        public void FireEndUncompressFile(object sender, CompressEventArgs e)
        {
            e.Action = CompressEventArgs.CompressionAction.Uncompressing;
            if (EndUncompressFile != null)
                EndUncompressFile(sender, e);
        }

        //---------------------------------------------------------------------
        public void FireBeginUncompressFolder(object sender, CompressEventArgs e)
        {
            e.Action = CompressEventArgs.CompressionAction.Uncompressing;
            if (BeginUncompressFolder != null)
                BeginUncompressFolder(sender, e);
        }

        //---------------------------------------------------------------------
        public void FireEndUncompressFolder(object sender, CompressEventArgs e)
        {
            e.Action = CompressEventArgs.CompressionAction.Uncompressing;
            if (EndUncompressFolder != null)
                EndUncompressFolder(sender, e);
        }

        //---------------------------------------------------------------------
        public void FireAborted(CompressEventArgs e)
        {
            e.PackageFileName = driver.FileName;
            e.Result = CompressEventArgs.ActionResult.UserAborted;
            if (Aborted != null)
                Aborted(this, e);
        }

		//---------------------------------------------------------------------
        public void FireCompressedFileClose(EventArgs e)
        {
            if (CompressedFileClose != null)
                CompressedFileClose(this, e);
        }

        //---------------------------------------------------------------------
        public void Abort()
        {
            isAborted = true;
        }

        #endregion

        #region Compression Methods

        //---------------------------------------------------------------------
        public bool AddFile(string filePath)
        {
            return AddFile(Path.GetFullPath(filePath), Path.GetDirectoryName(driver.FileName), string.Empty);
        }

        //---------------------------------------------------------------------
        public bool AddFileWithTitle(string filePath, string fileTitle)
        {
            return AddFile(Path.GetFullPath(filePath), Path.GetDirectoryName(driver.FileName), fileTitle);
        }

        //---------------------------------------------------------------------
        public bool AddFile(string filePath, string relativePathFrom, string fileTitle)
        {
            if (isAborted)
            {
                FireAborted(new CompressEventArgs(CompressEventArgs.CompressionAction.Compressing, filePath));
                return false;
            }

            return driver.AddFile(filePath, driver.GetRelativeUri(filePath, relativePathFrom), fileTitle);
        }

		//---------------------------------------------------------------------
		public bool AddFile (string filePath, Uri zippedUri)
		{
			if (isAborted)
			{
				FireAborted(new CompressEventArgs(CompressEventArgs.CompressionAction.Compressing, filePath));
				return false;
			}

			return driver.AddFile(filePath, zippedUri, string.Empty);
		}
        //---------------------------------------------------------------------
        public bool AddFolder(string path, bool recursive)
        {
            return AddFolder(path, recursive, "");
        }

        //---------------------------------------------------------------------
        public bool AddFolder(string path, bool recursive, string relativePathFrom)
        {
            if (isAborted)
            {
                FireAborted(new CompressEventArgs(CompressEventArgs.CompressionAction.Compressing, path));
                return false;
            }

            return driver.AddFolder(path, recursive, relativePathFrom);
        }

        //---------------------------------------------------------------------
        public bool AddStream(string name, Stream stream)
        {
            if (isAborted)
            {
                FireAborted(new CompressEventArgs(CompressEventArgs.CompressionAction.Compressing, name));
                return false;
            }

            return driver.AddStream(name, stream);
        }

        #endregion

        #region UnCompression Methods

        //---------------------------------------------------------------------
        public bool ExtractAll(string outputPath)
        {
            if (isAborted)
            {
                FireAborted(new CompressEventArgs(CompressEventArgs.CompressionAction.Uncompressing, outputPath));
                return false;
            }

            return driver.ExtractAll(outputPath);
        }

        //---------------------------------------------------------------------
        public bool ExtractFile(string file, string outputPath)
        {
            if (isAborted)
            {
                FireAborted(new CompressEventArgs(CompressEventArgs.CompressionAction.Uncompressing, file));
                return false;
            }

            return driver.ExtractFile(file, outputPath);
        }

        //---------------------------------------------------------------------
        public bool ExtractFolder(string path, string outputPath, bool recursive)
        {
            if (isAborted)
            {
                FireAborted(new CompressEventArgs(CompressEventArgs.CompressionAction.Uncompressing, path));
                return false;
            }

            return driver.ExtractFolder(path, outputPath, recursive);
        }

        //---------------------------------------------------------------------
        public Stream ExtractFileAsStream(string file)
        {
            if (isAborted)
            {
                FireAborted(new CompressEventArgs(CompressEventArgs.CompressionAction.Uncompressing, file));
                return null;
            }

            return driver.ExtractFileAsStream(file);
        }

        #endregion

        #region CompressedEntries Enumeration

        //---------------------------------------------------------------------
        public CompressedEntry GetEntry(Uri uri)
        {
            if (isAborted)
            {
                FireAborted(new CompressEventArgs(CompressEventArgs.CompressionAction.Uncompressing, uri.ToString()));
                return null;
            }

            return driver.GetEntry(uri);
        }

        //---------------------------------------------------------------------
        public CompressedEntry GetEntry(string entryPath)
        {
            return GetEntry(new Uri(entryPath));
        }

        //---------------------------------------------------------------------
        public Stream GetEntryStream(string relativePath)
        {
            if (isAborted)
            {
                FireAborted(new CompressEventArgs(CompressEventArgs.CompressionAction.Uncompressing, relativePath));
                return null;
            }

            CompressedEntry entry = driver.GetEntry(new Uri(relativePath));
            return entry == null ? null : entry.CurrentStream;

            
        }

        //---------------------------------------------------------------------
        public CompressedEntry[] GetFolders(string path)
        {
            if (isAborted)
            {
                FireAborted(new CompressEventArgs(CompressEventArgs.CompressionAction.Uncompressing, path));
                return null;
            }

            return driver.GetEntries(new Uri(path), CompressedEntry.EntryType.Folders, false);
        }
        
        //---------------------------------------------------------------------
        public CompressedEntry[] GetFiles(string path)
        {
            if (isAborted)
            {
                FireAborted(new CompressEventArgs(CompressEventArgs.CompressionAction.Uncompressing, path));
                return null;
            }

            return driver.GetEntries(new Uri(path), CompressedEntry.EntryType.Files, false);
        }

        //---------------------------------------------------------------------
        public CompressedEntry[] GetAllEntries()
        {
            if (isAborted)
            {
                FireAborted(new CompressEventArgs(CompressEventArgs.CompressionAction.Uncompressing, "All Entries"));
                return null;
            }

            return driver.GetAllEntries();
        }

        //---------------------------------------------------------------------
        public int GetNrOfEntries()
        {
            if (isAborted)
            {
                FireAborted(new CompressEventArgs(CompressEventArgs.CompressionAction.Uncompressing, "Nr Of Entries"));
                return 0;
            }

            return driver.GetNrOfEntries();
        }

        //---------------------------------------------------------------------
        public bool ExistsEntry(Uri uri)
        {
            if (isAborted)
            {
                FireAborted(new CompressEventArgs(CompressEventArgs.CompressionAction.Uncompressing, "Exists Entry"));
                return false;
            }

            return driver.ExistsEntry(uri);
        }

        //---------------------------------------------------------------------
        public bool ExistsEntry(string relativeEntryPath)
        {
            return ExistsEntry(new Uri(relativeEntryPath));
        }

        #endregion
    }
}
