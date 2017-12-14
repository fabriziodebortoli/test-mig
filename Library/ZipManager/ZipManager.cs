using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;

namespace Microarea.Library.ZipManager
{
	/// <summary>
	/// La classe realizza alcune funzioni ad alto livello
	/// appoggiandosi alla libreria open source #ZipLib
	/// http://www.icsharpcode.net/OpenSource/SharpZipLib/
	/// che ha una licenza che ne permette l'uso in pacchetti commerciali ed
	/// è il porting in C# di diverse librerie Java.
	/// </summary>
	public class ZipManager
	{
		#region Private members
		private ArrayList excludeFiles = new ArrayList();
		private static char[] pathSeparatorChars = new char[]{ Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
		private bool aborted = false;
		#endregion

		#region Properties
		//---------------------------------------------------------------------
		public ArrayList ExcludeFiles	{ get { return excludeFiles; } set { excludeFiles = value; } }

		//---------------------------------------------------------------------
		public bool Aborted	{ get { return this.aborted; }}
		#endregion

		//---------------------------------------------------------------------
		public void Abort()
		{
			this.aborted = true;
		}

		/// <summary>
		/// Esplode un file ZIP in una directory
		/// </summary>
		/// <param name="zipFile">Full name del file ZIP</param>
		/// <param name="baseDir">Directory dove esplodere il file</param>
		//---------------------------------------------------------------------
		public bool UnzipFile(string zipFile, string baseDir)
		{
			if (this.aborted)
				return false;

			DirectoryInfo baseDirInfo = new DirectoryInfo(baseDir);
			ZipInputStream s = new ZipInputStream(File.OpenRead(zipFile));

			int totFiles = CountZippedFiles(zipFile);
			int currentFile = 0;

			ZipEntry theEntry;
			while (!this.aborted && (theEntry = s.GetNextEntry()) != null)
				UnzipEntry(s, theEntry, baseDirInfo, totFiles, ref currentFile);

			s.Close();

			if (this.aborted)
				return false;

			return true;
		}

		//---------------------------------------------------------------------
		public int CountZippedFiles(string zipFile)
		{
			ZipInputStream s = new ZipInputStream(File.OpenRead(zipFile));
			ZipEntry theEntry;
			int totFiles = 0;
			while ((theEntry = s.GetNextEntry()) != null)
				if (Path.GetFileName(theEntry.Name).Length != 0)	// controllo non sia una dir (theEntry.Name non dovrebbe mai essere null)
					++totFiles;
			s.Close();
			return totFiles;
		}

		/// <summary>
		/// Esplode un file ZIP sottotforma di stream in una directory
		/// </summary>
		/// <param name="zipFile">stream</param>
		/// <param name="baseDir">Directory dove esplodere il file</param>
		//---------------------------------------------------------------------
		public bool UnzipFile(Stream zipStream, string baseDir)
		{
			if (this.aborted)
				return false;

			DirectoryInfo baseDirInfo = new DirectoryInfo(baseDir);
			ZipInputStream s = new ZipInputStream(zipStream);

			int totFiles = 0;//CountZippedFiles(zipStream, false);
			int currentFile = 0;

			ZipEntry theEntry;
			while (!this.aborted && (theEntry = s.GetNextEntry()) != null)
				UnzipEntry(s, theEntry, baseDirInfo, totFiles, ref currentFile);

			s.Close();

			if (this.aborted)
				return false;

			return true;
		}

		//---------------------------------------------------------------------
		public int CountZippedFiles(Stream zipStream)
		{
			ZipInputStream s = new ZipInputStream(zipStream);
			ZipEntry theEntry;
			int totFiles = 0;
			while ((theEntry = s.GetNextEntry()) != null)
				if (Path.GetFileName(theEntry.Name).Length != 0)	// controllo non sia una dir (theEntry.Name non dovrebbe mai essere null)
					++totFiles;
			s.Close();
			return totFiles;
		}

		/// <summary>
		/// Effettua una compressione ZIP su una cartella creando un nuovo
		/// file .ZIP.
		/// Il percorso della cartella non verrà incluso nei file compressi
		/// che riporteranno soltanto il proprio percorso relativo.
		/// </summary>
		/// <param name="zipFile">Full Name del file ZIP da creare.</param>
		/// <param name="baseDir">Cartella da comprimere.</param>
		//---------------------------------------------------------------------
		public bool ZipDirContent(string zipFileFullName, string baseDir)
		{
			if (this.aborted)
				return false;

			return ZipDirContent(zipFileFullName, baseDir, true);
		}

		/// <summary>
		/// Effettua una compressione ZIP su una cartella creando un nuovo
		/// file .ZIP.
		/// Il percorso della cartella non verrà incluso nei file compressi
		/// che riporteranno soltanto il proprio percorso relativo.
		/// </summary>
		/// <param name="zipFile">Full Name del file ZIP da creare.</param>
		/// <param name="baseDir">Cartella da comprimere.</param>
		/// <param name="recursive">true per comprimere anche le sottocartelle, false altrimenti.</param>
		//---------------------------------------------------------------------
		public bool ZipDirContent(string zipFileFullName, string baseDir, bool recursive)
		{
			if (this.aborted)
				return false;

			Crc32 crc = new Crc32();
			ZipOutputStream s = new ZipOutputStream(File.Create(zipFileFullName));
		
			s.SetLevel(6); // 0 - store only to 9 - means best compression
		
			DirectoryInfo di = new DirectoryInfo(baseDir);
			bool result = ZipDirContent(ref crc, ref s, di, zipFileFullName, di.FullName, recursive);
		
			s.Finish();
			s.Close();
			
			return result;
		}

		//---------------------------------------------------------------------
		private bool ZipDirContent
			(
			ref Crc32 crc, 
			ref ZipOutputStream s, 
			DirectoryInfo di, 
			string zipFile, 
			string baseDir,
			bool recursive
			)
		{
			if (this.aborted)
				return false;

			foreach (FileInfo fi in di.GetFiles())
				if (IsFileToZip(zipFile, fi))
				{
					string file = fi.FullName;
					string relFile = file.Substring(baseDir.Length + 1);

					OnZippingFile(new ZippingEventArgs(file, relFile));	// evento "compressione del file xxx"
					ZipFile(crc, s, file, relFile);
				}

			if (recursive)
				foreach (DirectoryInfo sdi in di.GetDirectories())
					if (sdi.GetDirectories().Length > 0 || sdi.GetFiles().Length > 0)	// non è vuota
					{
						string relDirectory = sdi.FullName.Substring(baseDir.Length + 1);
						ZipDirectory(s, sdi.FullName, relDirectory);
						ZipDirContent(ref crc, ref s, sdi, zipFile, baseDir, true);
					}

			if (this.aborted)
				return false;
			return true;
		}

		//---------------------------------------------------------------------
		protected bool IsFileToZip(string zipFile, FileInfo file)
		{
			if (string.Compare(file.FullName, zipFile, true, CultureInfo.InvariantCulture) == 0)
				return false;

			foreach (string fileName in excludeFiles)
				if (string.Compare(file.Name, fileName, true, CultureInfo.InvariantCulture)  == 0)
					return false;

			return true;
		}

		//---------------------------------------------------------------------
		private void UnzipEntry
			(
			Stream s, 
			ZipEntry theEntry, 
			DirectoryInfo outputDir,
			int totFiles, 
			ref int currentFile
			)
		{
			string fullFileName = Path.Combine(outputDir.FullName, theEntry.Name.TrimStart(pathSeparatorChars));
			UnzipEntry(s, theEntry, fullFileName, totFiles, ref currentFile);
		}

		//---------------------------------------------------------------------
		private void UnzipEntry
			(
			Stream s, 
			ZipEntry theEntry, 
			string fullFileName,
			int totFiles, 
			ref int currentFile
			)
		{
			Debug.WriteLine(theEntry.Name);
			
			string directoryName = Path.GetDirectoryName(fullFileName);
			Directory.CreateDirectory(directoryName);
			
			// gli archivi creati con pkzip aggiungono voci anche per le directories
			if (Path.GetFileName(theEntry.Name).Length == 0)	// theEntry.Name non dovrebbe mai essere null
				return;

			// evento "estrazione del file xxx"
			OnUnzippingFile(new UnzippingEventArgs(fullFileName, theEntry.Name, totFiles, ++currentFile));

			DateTime creationDate = theEntry.DateTime;

			FileStream streamWriter;
			if (File.Exists(fullFileName))
				File.SetAttributes(fullFileName, FileAttributes.Normal);
			streamWriter = File.Create(fullFileName);
			
			int size = 2048;
			byte[] data = new byte[2048];
			while (true) 
			{
				size = s.Read(data, 0, data.Length);
				if (size > 0) 
					streamWriter.Write(data, 0, size);
				else 
					break;
			}
			
			streamWriter.Close();
			File.SetCreationTimeUtc(fullFileName, creationDate);
			File.SetLastWriteTimeUtc(fullFileName, creationDate);
		}

		//---------------------------------------------------------------------
		protected void ZipFile
			(
				Crc32 crc, 
				ZipOutputStream s,
				string sourceFile,		
				string entryFileName
			)
		{
			FileStream fs = File.OpenRead(sourceFile);
	
			byte[] buffer = new byte[fs.Length];
			fs.Read(buffer, 0, buffer.Length);
			entryFileName = entryFileName.Replace('\\', '/');
			ZipEntry entry = new ZipEntry(entryFileName);

			entry.DateTime = File.GetLastWriteTimeUtc(sourceFile);

			// set Size and the crc, because the information
			// about the size and crc should be stored in the header
			// if it is not set it is automatically written in the footer.
			// (in this case size == crc == -1 in the header)
			// Some ZIP programs have problems with zip files that don't store
			// the size and crc in the header.
			entry.Size = fs.Length;
			fs.Close();
			crc.Reset();
			crc.Update(buffer);
			entry.Crc = crc.Value;
			s.PutNextEntry(entry);
			s.Write(buffer, 0, buffer.Length);
		}

		//---------------------------------------------------------------------
		public void ZipSingleFile(string zipFile, string sourceFile)
		{
			Crc32 crc = new Crc32();
			ZipOutputStream s = new ZipOutputStream(File.Create(zipFile));
		
			s.SetLevel(6); // 0 - store only to 9 - means best compression
		
			string path = Path.GetFileName(sourceFile);
			ZipFile(crc, s, sourceFile, path);
		
			s.Finish();
			s.Close();
		}

		//---------------------------------------------------------------------
		protected void ZipDirectory
			(
			//Crc32 crc, 
			ZipOutputStream s,
			string sourceDirectory,		
			string entryDirectoryName
			)
		{
			entryDirectoryName = entryDirectoryName.Replace('\\', '/');
			if (entryDirectoryName[entryDirectoryName.Length - 1] != '/')
				entryDirectoryName += "/";

			ZipEntry entry = new ZipEntry(entryDirectoryName);
			entry.DateTime = Directory.GetLastWriteTimeUtc(sourceDirectory);
			s.PutNextEntry(entry);
		}

		
		//---------------------------------------------------------------------
		public bool ExtractSingleFile(FileInfo zipFile, string zippedFile, DirectoryInfo outputDir)
		{
			ZipFile zip = new ZipFile(zipFile.FullName);	// TODO - try-catch
			zippedFile = zippedFile.Replace('\\', '/');
			ZipEntry theEntry = zip.GetEntry(zippedFile);
			if (theEntry == null)
				return false;

			using(Stream s = zip.GetInputStream(theEntry))
			{
				int currentFile = 0;
				UnzipEntry(s, theEntry, outputDir, zip.Size, ref currentFile);
			}
			return true;
		}
	
		//---------------------------------------------------------------------
		public void ZipFile(string zipFile, string sourceFile)
		{
			ZipOutputStream s = new ZipOutputStream(File.Create(zipFile));

			ZipFile(s, sourceFile);
		
			s.Finish();
			s.Close();
		}

		//---------------------------------------------------------------------
		public void ZipFile(ZipOutputStream outStream, string sourceFile)
		{
			Crc32 crc = new Crc32();

			outStream.SetLevel(6); // 0 - store only to 9 - means best compression

			ZipFile(crc, outStream, sourceFile, sourceFile);
		}

		#region Events
		//---------------------------- EVENTS ---------------------------------
		public event ZipEventHandler ZippingFile;
		public event ZipEventHandler UnzippingFile;

		//---------------------------------------------------------------------
		public void OnZippingFile(ZipLibraryEventArgs e)
		{
			if (ZippingFile != null)
				ZippingFile(this, e);
		}
		public void OnUnzippingFile(ZipLibraryEventArgs e)
		{
			if (UnzippingFile != null)
				UnzippingFile(this, e);
		}
		#endregion
	}

	//=========================================================================
	public delegate void ZipEventHandler(object sender, ZipLibraryEventArgs e);

	//=========================================================================
	public abstract class ZipLibraryEventArgs : EventArgs
	{
		public readonly string UncompressedFileName;
		public readonly string CompressedFileName;

		public ZipLibraryEventArgs(string uncompressedFileName, string compressedFileName)
		{
			this.UncompressedFileName = uncompressedFileName;
			this.CompressedFileName = compressedFileName;
		}
	}
	public class ZippingEventArgs : ZipLibraryEventArgs
	{
		public ZippingEventArgs(string uncompressedFileName, string compressedFileName)
			: base(uncompressedFileName, compressedFileName) {}
	}
	public class UnzippingEventArgs : ZipLibraryEventArgs
	{
		public readonly int TotalFiles	= -1;
		public readonly int CurrentFile	= -1;
		public UnzippingEventArgs(string uncompressedFileName, string compressedFileName)
			: base(uncompressedFileName, compressedFileName) {}
		public UnzippingEventArgs
			(
			string uncompressedFileName, 
			string compressedFileName,
			int totalFiles,
			int currentFiles
			)
			: base(uncompressedFileName, compressedFileName)
		{
			this.TotalFiles		= totalFiles;
			this.CurrentFile	= currentFiles;
		}
	}
}