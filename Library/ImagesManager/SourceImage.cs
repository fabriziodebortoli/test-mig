using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;

using Microarea.Library.Licence;
using Microarea.Library.ZipManager;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;

using PolicyType = Microarea.Library.Interfaces.PolicyType;

namespace Microarea.Library.ImagesManagement
{
	//=========================================================================
	public interface ISourceImage : IDisposable
	{
		string Product { get; }
		string Release { get; }
		bool Compressed { get; }
		string	ImagePath { get; }
		string GetManifestRelease(string serverManifestRelPath);
		ManifestObject GetManifestObject(string manifestRelPath);
		string GetManifestBorn(string manifestRelPath);
		void AppendBinaryFile(string sourceFileRelPath, XmlElement el);
		string GetImageReleaseExtended();
		XmlDocument GetFileDom(string relPath);
		ZippedFileSystem ZipPackage { get; }
		//bool FileExists(string relativeFileName);
		//DateTime GetFileLastWriteTimeUtc(string relativeFileName);
	}
	public class SourceImage : IDisposable, ISourceImage
	{
		private readonly bool	compressed = false;
		private readonly string imagePath = string.Empty;
		private ZippedFileSystem zipPackage	= null;
		
		private string product = string.Empty;
		private string release = string.Empty;
		private string releaseExtended = string.Empty;
		
		//---------------------------------------------------------------------
		public SourceImage(ImagesManager imagesManager, string storageName, string storageRelease)
			: this(imagesManager, storageName, storageRelease, true)
		{
		}

		//---------------------------------------------------------------------
		public SourceImage(ImagesManager imagesManager, string storageName, string storageRelease, bool compressed)
		{
			this.imagePath	= imagesManager.GetImagePath(storageName, storageRelease);

			if (compressed)	// controlla però che esista lo zippone
			{
				string rel = new DirectoryInfo(this.imagePath).Name;
				string zipFileName = rel + ".zip";	// TEMP - valore cablato
				string zipFileFullName = Path.Combine(this.imagePath, zipFileName);
				if (File.Exists(zipFileFullName))
				{
					this.compressed	= true;
					zipPackage = new ZippedFileSystem(zipFileFullName);
				}
			}

			this.product = storageName;
			this.release = storageRelease;
		}
		
		// NOTE - solo per il controllo dei services
		//---------------------------------------------------------------------
		public SourceImage(string microareaServerPath)
		{
			this.compressed = false;
			this.imagePath	= microareaServerPath;	// TODO - ???
		}

		public string Product	{ get { return this.product; } }
		public string Release	{ get { return this.release; } }
		public bool Compressed  { get { return this.compressed; } }
		public string ImagePath { get { return this.imagePath; } }
		public ZippedFileSystem ZipPackage { get { return this.zipPackage; } }

		//---------------------------------------------------------------------
		public void Close()
		{
			Dispose();
		}
		public void Dispose()
		{
			CleanUp();
			GC.SuppressFinalize(this);
		}
		~SourceImage()
		{
			CleanUp();
		}
		private void CleanUp()
		{
			if (zipPackage != null)
			{
				zipPackage.Close();
				zipPackage = null;
			}
		}

		//---------------------------------------------------------------------
		public ManifestObject GetManifestObject(string manifestFileRelPath)
		{
			ManifestObject manifestObj;
			XmlTextReader reader = null;
			
			if (this.compressed)
			{
				Stream s = zipPackage.GetEntryAsStream(manifestFileRelPath);
				if (s == null)
				{
					// può capitare nel caso di verticali che non distribuiscono parti su cui hanno dipendenza
					Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "manifest non trovato : {0}", manifestFileRelPath));
					return null;
				}
				string fileName = Path.GetFileName(manifestFileRelPath);
				reader = new XmlTextReader(s);
				manifestObj = ManifestManager.GetManifestObject(fileName, reader, true);
				return manifestObj;
			}
			else
			{
				string manifestFileFullName = Path.Combine(this.imagePath, manifestFileRelPath);
			
				if (!File.Exists(manifestFileFullName))
				{
					// può capitare nel caso di verticali che non distribuiscono parti su cui hanno dipendenza
					Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "manifest non trovato : {0}", manifestFileRelPath));	// TEMP
					return null;
				}

				manifestObj = ManifestManager.GetManifestObject(manifestFileFullName);
				return manifestObj;
			}
		}

		//---------------------------------------------------------------------
		public string GetManifestRelease(string manifestFileRelPath)
		{
			XmlTextReader reader = null;

			if (this.compressed)
			{
				Stream s = zipPackage.GetEntryAsStream(manifestFileRelPath);
				if (s == null)
					return string.Empty;
				reader = new XmlTextReader(s);
				return ManifestManager.GetManifestRelease(reader);
			}
			else
			{
				string manifestFileFullName = Path.Combine(this.imagePath, manifestFileRelPath);
			
				if (!File.Exists(manifestFileFullName))
					return string.Empty;

				reader = new XmlTextReader(manifestFileFullName);
				string rel = ManifestManager.GetManifestRelease(reader);
				reader.Close();
				return rel;
			}
		}
		public string GetManifestBorn(string manifestFileRelPath)
		{
			XmlTextReader reader = null;

			if (this.compressed)
			{
				Stream s = zipPackage.GetEntryAsStream(manifestFileRelPath);
				if (s == null)
					return string.Empty;
				reader = new XmlTextReader(s);
				return ManifestManager.GetManifestBorn(reader);
			}
			else
			{
				string manifestFileFullName = Path.Combine(this.imagePath, manifestFileRelPath);
			
				if (!File.Exists(manifestFileFullName))
					return string.Empty;

				reader = new XmlTextReader(manifestFileFullName);
				string rel = ManifestManager.GetManifestBorn(reader);
				reader.Close();
				return rel;
			}
		}

		//---------------------------------------------------------------------
		public string GetImageReleaseExtended()
		{
			// BACKWARDCOMPATIBILITY - dalla 1.1.0.20040430 il manifest di immagine è prefissato con
			//		il nome del prodotto.
			//		si garantisce compatibilità con la 1.0, non con le build successive e mai pubblicate,
			//		precedenti alla build indicata
			string manifestFileRelPath;
			if (string.Compare(product, "MagoNet-Pro", true, CultureInfo.InvariantCulture) == 0 &&
				string.Compare(release, "1.0", true, CultureInfo.InvariantCulture) == 0)
				manifestFileRelPath = ManifestManager.BuildDirectoryManifestFileName(PolicyType.Full);
			else
				manifestFileRelPath = ManifestManager.BuildDirectoryManifestFileName(product, PolicyType.Full);
			return GetManifestRelease(manifestFileRelPath);
		}

		//---------------------------------------------------------------------
		public XmlDocument GetFileDom(string fileRelPath)
		{
			Stream stream = null;
			try
			{
				if (this.compressed)
				{
					stream = zipPackage.GetEntryAsStream(fileRelPath);
					if (stream == null)
						return null;
				}
				else
				{
					string fileFullPath = Path.Combine(this.imagePath, fileRelPath);
					if (!File.Exists(fileFullPath))
						return null;
					stream = File.OpenRead(fileFullPath);
				}
				XmlDocument dom = new XmlDocument();
				dom.Load(stream);
				return dom;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}
		}

		//---------------------------------------------------------------------
		private byte[] GetFileAsBytes(string fileRelPath)
		{
			Stream stream = null;
			if (this.compressed)
			{
				stream = zipPackage.GetEntryAsStream(fileRelPath);
			}
			else
			{
				string fileFullPath = Path.Combine(this.imagePath, fileRelPath);
				if (!File.Exists(fileFullPath))
					return null;
				stream = File.OpenRead(fileFullPath);
			}
			long len = stream.Length;
			byte[] buffer = new byte[stream.Length];
			stream.Read(buffer, 0, (int)len);
			stream.Close();
			return buffer;
		}

		//---------------------------------------------------------------------
		public void AppendBinaryFile(string fileRelPath, XmlElement el)
		{
			byte[] buffer = GetFileAsBytes(fileRelPath);
			StringWriter sw = new StringWriter();
			XmlTextWriter w = new XmlTextWriter(sw);
			w.WriteStartElement(Consts.TagSalesModule);
			w.WriteAttributeString(Consts.AttributeSize, buffer.Length.ToString(CultureInfo.InvariantCulture));
			w.WriteBase64(buffer, 0, buffer.Length);
			w.WriteEndElement();
			XmlCDataSection cdata =  el.OwnerDocument.CreateCDataSection(sw.ToString());
			el.AppendChild(cdata);
		}
	}

	//=========================================================================
	public class RelativeFile
	{
		//---------------------------------------------------------------------
		public static bool Exists(string relativeFileName, ISourceImage sourceImage)
		{
			if (sourceImage.Compressed)
			{
				return sourceImage.ZipPackage.FileExists(relativeFileName);
			}
			else
			{
				string fullName = Path.Combine(sourceImage.ImagePath, relativeFileName);
				return File.Exists(fullName);
			}
		}

		//---------------------------------------------------------------------
		public static DateTime GetLastWriteTimeUtc(string relativeFileName, ISourceImage sourceImage)
		{
			if (sourceImage.Compressed)
			{
				return sourceImage.ZipPackage.GetFileLastWriteTimeUtc(relativeFileName);
			}
			else
			{
				string fullName = Path.Combine(sourceImage.ImagePath, relativeFileName);
				return File.GetLastWriteTimeUtc(fullName);
			}
		}

		//---------------------------------------------------------------------
		public static DateTime GetTruncatedLastWriteTimeUtc(string relativeFileName, ISourceImage sourceImage)
		{
			// NOTE - la lettura da manifest tronca i ms, devo fare lo stesso da file
			DateTime aDate = GetLastWriteTimeUtc(relativeFileName, sourceImage);
			aDate = DateTime.Parse(aDate.ToString("s"));
			return aDate;
		}

		//---------------------------------------------------------------------
		public static string GetTruncatedLastWriteTimeUtcAsString(string relativeFileName, ISourceImage sourceImage)
		{
			// NOTE - la lettura da manifest tronca i ms, devo fare lo stesso da file
			DateTime aDate = GetLastWriteTimeUtc(relativeFileName, sourceImage);
			if (sourceImage.Compressed)	// NOTE - il tapullo lo fa solo se compresso, dove è veramente necessario
				aDate = aDate.Subtract(new TimeSpan(0,0,1));	// TAPULLO - tolgo 1 secondo x compensare arrotondamenti del DosTime
			return aDate.ToString("s");
		}

		/*
		//---------------------------------------------------------------------
		public static void Copy(string relativeFileName, string destFileFullName, bool overwrite, SourceImage sourceImage)
		{
			if (sourceImage.Compressed)
			{
				// TODO
			}
			else
			{
				string fullName = Path.Combine(sourceImage.ImagePath, relativeFileName);
				if (File.Exists(destFileFullName))
					File.SetAttributes(destFileFullName, FileAttributes.Normal);
				File.Copy(fullName, destFileFullName, overwrite);
			}
		}
		*/

		//---------------------------------------------------------------------
		public static void CopyWithPath
			(
			string relativeFileName, 
			TargetImage targetImage,
			ISourceImage sourceImage, 
			ImagesManager imagesManager, 
			bool services
			)
		{
			string destinationBasePath = targetImage.BasePath;

			// TODO - anche per RaiseCopyEvents usare parametro services
			imagesManager.RaiseCopyEvents(relativeFileName, sourceImage.ImagePath, destinationBasePath);
			if (sourceImage.Compressed)
			{
				if (services)	// TEMP - forse va spostato prima x usarlo anche se non compressed?
					relativeFileName = ImagesManager.GetRelativeFilePathInServices(relativeFileName);

				if (targetImage.Compressed)
				{
					targetImage.PasteFile(sourceImage.ZipPackage, relativeFileName);
				}
				else
				{
					string destinationFileFullName = Path.Combine(destinationBasePath, relativeFileName);
					sourceImage.ZipPackage.FileCopy(relativeFileName, destinationFileFullName);
				}
			}
			else
			{
				if (targetImage.Compressed)
				{
					throw new NotImplementedException();
				}
				else
				{
					imagesManager.CopyFileWithPath
						(
						relativeFileName,
						sourceImage.ImagePath,
						destinationBasePath,
						services
						);
				}
			}
		}

		//---------------------------------------------------------------------
		public static long GetFileSize(string relativeFileName, ISourceImage sourceImage)
		{
			if (sourceImage.Compressed)
				return sourceImage.ZipPackage.GetFileSize(relativeFileName);
			else
			{
				string fullName = Path.Combine(sourceImage.ImagePath, relativeFileName);
				return new FileInfo(fullName).Length;
			}
		}
	}

	//=========================================================================
	public class RelativeDirectory
	{
		public static bool Exists(string relativeDirectoryName, ISourceImage sourceImage)
		{
			if (sourceImage.Compressed)
			{
				return sourceImage.ZipPackage.DirectoryExists(relativeDirectoryName);
			}
			else
			{
				string fullName = Path.Combine(sourceImage.ImagePath, relativeDirectoryName);
				return Directory.Exists(fullName);
			}
		}

		/*
		//---------------------------------------------------------------------
		public static void Copy(string relativeDirectoryName, string destDirectoryFullName, bool overwrite, SourceImage sourceImage)
		{
			if (sourceImage.Compressed)
			{
				// TODO
			}
			else
			{
				string fullName = Path.Combine(sourceImage.ImagePath, relativeDirectoryName);
				// TODO - usa ImagesManager.CopyDirectoryOptimized
			}
		}
		*/

		//---------------------------------------------------------------------
		public static string GetDirectoryName(string relativeDirectoryPath)
		{
			if (relativeDirectoryPath == null || relativeDirectoryPath.Length == 0)
				return string.Empty;	// NB: anche se non è compresso
			relativeDirectoryPath = relativeDirectoryPath.Replace('\\', '/');
			int lenght = relativeDirectoryPath.Length;
			if (relativeDirectoryPath[lenght - 1] == '/')
				relativeDirectoryPath = relativeDirectoryPath.Substring(0, lenght - 1);
			string[] splittedPath = relativeDirectoryPath.Split('/');
			return splittedPath[splittedPath.Length - 1];
		}

		//---------------------------------------------------------------------
		public static string[] GetRelativeDirectories(string relativeDirectoryPath, ISourceImage sourceImage)
		{
			if (sourceImage.Compressed)
			{
				return sourceImage.ZipPackage.GetDirectories(relativeDirectoryPath);
			}
			else
			{
				string absoluteDirectoryPath = Path.Combine(sourceImage.ImagePath, relativeDirectoryPath);
				if (!Directory.Exists(absoluteDirectoryPath))
					return new string[]{};
				string[] directories = Directory.GetDirectories(absoluteDirectoryPath);
				ArrayList list = new ArrayList(directories.Length);
				foreach (string directory in directories)
					list.Add(directory.Substring(sourceImage.ImagePath.Length + 1));
				return (string[])list.ToArray(typeof(string));
			}
		}

		//---------------------------------------------------------------------
		public static string[] GetRelativeFiles(string relativeDirectoryPath, ISourceImage sourceImage)
		{
			if (sourceImage.Compressed)
			{
				return sourceImage.ZipPackage.GetFiles(relativeDirectoryPath);
			}
			else
			{
				string absoluteDirectoryPath = Path.Combine(sourceImage.ImagePath, relativeDirectoryPath);
				if (!Directory.Exists(absoluteDirectoryPath))
					return new string[]{};
				string[] files = Directory.GetFiles(absoluteDirectoryPath);
				ArrayList list = new ArrayList(files.Length);
				foreach (string file in files)
					list.Add(file.Substring(sourceImage.ImagePath.Length + 1));
				return (string[])list.ToArray(typeof(string));
			}
		}

		//---------------------------------------------------------------------
	}

	//=========================================================================
	public class TargetImage : IDisposable
	{
		private readonly string	basePath;
		private readonly bool	compressed;

		private ZipOutputStream zipOutputStream = null;

		//---------------------------------------------------------------------
		public TargetImage(string basePath, bool compressed)
		{
			this.basePath	= basePath;
			this.compressed	= compressed;

			if (compressed)
			{
				Directory.CreateDirectory(basePath);
				string zipFile = Path.Combine(basePath, NameSolver.NameSolverStrings.FileZipUpdates);
				zipOutputStream = new ZipOutputStream(File.Create(zipFile));
				zipOutputStream.SetLevel(6); // 0 - store only to 9 - means best compression
			}
		}

		//---------------------------------------------------------------------
		public string	BasePath	{ get { return this.basePath;	}}
		public bool		Compressed	{ get { return this.compressed;	}}

		//---------------------------------------------------------------------
		public void PasteFile
			(
			ZippedFileSystem sourcePackage,
			string relativeFileName
			)
		{
			ZipEntry inputEntry;
			Stream zipInputStream = sourcePackage.GetEntryAsStream(relativeFileName, out inputEntry);
			if (zipInputStream == null)
				return;

			ZipEntry outputEntry = new ZipEntry(inputEntry);
			zipOutputStream.PutNextEntry(outputEntry);

			int size = 2048;
			byte[] data = new byte[2048];
			while (true) 
			{
				size = zipInputStream.Read(data, 0, data.Length);
				if (size > 0) 
					zipOutputStream.Write(data, 0, size);
				else 
					break;
			}
		}

		#region IDisposable Members

		//---------------------------------------------------------------------
		public void Close()
		{
			Dispose();
		}
		public void Dispose()
		{
			CleanUp();
			GC.SuppressFinalize(this);
		}
		~TargetImage()
		{
			CleanUp();
		}
		private void CleanUp()
		{
			if (zipOutputStream != null)
			{
				zipOutputStream.Finish();
				zipOutputStream.Close();
			}
		}

		#endregion
	}
}
