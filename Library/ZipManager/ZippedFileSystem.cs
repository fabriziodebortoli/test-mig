using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;

namespace Microarea.Library.ZipManager
{

	//=========================================================================
	public class ZippedFileSystem
	{
		private readonly ZipFile zip;
		private Stream baseStream;
		protected readonly string zipFileFullName;

		//---------------------------------------------------------------------
		public ZippedFileSystem(string zipFileFullName)
		{
			baseStream = File.OpenRead(zipFileFullName);
			zip = new ZipFile(/*baseStream*/zipFileFullName);	// TODO - try-catch
			this.zipFileFullName = zipFileFullName;
		}

		//---------------------------------------------------------------------
		public string ZipFileFullName { get { return this.zipFileFullName; } }

		//---------------------------------------------------------------------
		public bool FileExists(string relativeFilePath)
		{
			ZipEntry theEntry = GetEntry(relativeFilePath);
			Debug.WriteLineIf(theEntry == null, string.Format(CultureInfo.CurrentCulture, "ZippedFileSystem.FileExists failed: {0}", relativeFilePath));
			return theEntry != null;
		}

		//---------------------------------------------------------------------
		public void Close()	// TODO - fare IDisposable
		{
			this.zip.Close();
		}

		//---------------------------------------------------------------------
		public bool DirectoryExists(string relativeDirectoryPath)
		{
			if (relativeDirectoryPath.Length == 0)
				return true;	// root
			if (relativeDirectoryPath.Length == 1)
			{
				char lastChar = relativeDirectoryPath[relativeDirectoryPath.Length - 1];
				if (lastChar == '\\' || lastChar == '/')
					return true;	// root
			}

			relativeDirectoryPath = relativeDirectoryPath.Replace('\\', '/');
			ZipEntry theEntry = null;

			// provo a vedere se la becco come entry singola (p.es. WinZip la mette)
			string entryToSearch = relativeDirectoryPath;
			if (entryToSearch.Length == 0 || entryToSearch[entryToSearch.Length - 1] != '/')
				entryToSearch += "/";
			theEntry = zip.GetEntry(entryToSearch);
			if (theEntry != null)
				return true;

			return false;
			/*
			// non esiste entry esplicita, cerco fra tutti i file se la hanno come prefisso
			ZipInputStream s = new ZipInputStream(File.OpenRead(zipFileFullName));

			bool found = false;
			entryToSearch = entryToSearch.ToLower(CultureInfo.InvariantCulture);
			while ((theEntry = s.GetNextEntry()) != null)
			{
				if (theEntry.Name.ToLower(CultureInfo.InvariantCulture).StartsWith(entryToSearch))
				{
					found = true;
					break;
				}
			}
			
			s.Close();
			Debug.WriteLineIf(!found, string.Format("ZippedFileSystem.DirectoryExists failed: {0}", relativeDirectoryPath));
			return found;
			*/
		}

		//---------------------------------------------------------------------
		public DateTime GetFileLastWriteTimeUtc(string relativeFilePath)
		{
			ZipEntry theEntry = GetEntry(relativeFilePath);
			if (theEntry != null)
				return theEntry.DateTime;
			else
			{
				// TODO - fare un'eccezione + specifica derivata da FileNotFoundException
				throw new FileNotFoundException("File not found in zipped fyle system", relativeFilePath);	// LOCALIZE ???
			}
		}
		
		//---------------------------------------------------------------------
		public bool FileCopy(string relativeFilePath, string destinationFileFullName)
		{
			ZipEntry theEntry = GetEntry(relativeFilePath);
			if (theEntry == null)
				return false;

			// TODO - evento di copia
			
			if (File.Exists(destinationFileFullName))
				File.SetAttributes(destinationFileFullName, FileAttributes.Normal);
			else
				Directory.CreateDirectory(Path.GetDirectoryName(destinationFileFullName));

			Stream zipInputStream = zip.GetInputStream(theEntry);
			UnzipEntry(zipInputStream, theEntry, destinationFileFullName);
			//zipInputStream.Close();

			return true;
		}
		
		//---------------------------------------------------------------------
		public long GetFileSize(string relativeFilePath)	// TEMP - sostituire con uno ZippedFileSystemInfo
		{
			ZipEntry theEntry = GetEntry(relativeFilePath);
			if (theEntry == null)
				return 0;
			return theEntry.Size;
		}
		
		//---------------------------------------------------------------------
		public Stream GetEntryAsStream(string relativeFilePath)
		{
			ZipEntry theEntry;
			return GetEntryAsStream(relativeFilePath, out theEntry);
		}
			
		//---------------------------------------------------------------------
		public Stream GetEntryAsStream(string relativeFilePath, out ZipEntry theEntry)
		{
			theEntry = GetEntry(relativeFilePath);
			if (theEntry == null)
				return null;

			Stream zipInputStream = zip.GetInputStream(theEntry);
			return zipInputStream;
		}
	
		//---------------------------------------------------------------------
		public ZipEntry GetEntry(string relativeFilePath)
		{
			if (relativeFilePath.IndexOf('\\') != -1)	// reduces pressure on managed heap replacing only if needed
				relativeFilePath = relativeFilePath.Replace('\\', '/');
			return zip.GetEntry(relativeFilePath);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="zipInputStream"></param>
		/// <param name="theEntry"></param>
		/// <param name="fullFileName"></param>
		//---------------------------------------------------------------------
		private void UnzipEntry(Stream zipInputStream, ZipEntry theEntry, string fullFileName)
		{
			string directoryName = Path.GetDirectoryName(fullFileName);
			Directory.CreateDirectory(directoryName);

			// TODO - evento
			//OnUnzippingFile(new UnzippingEventArgs(fullFileName, theEntry.Name));	// evento "estrazione del file xxx"
			
			// gli archivi creati con pkzip aggiungono voci anche per le directories
			string fileName = Path.GetFileName(theEntry.Name);
			if (fileName == null || fileName.Length == 0)
				return;

			DateTime creationDate = theEntry.DateTime;

			FileStream streamWriter;
			streamWriter = File.Create(fullFileName);
			
			int size = 2048;
			byte[] data = new byte[2048];
			while (true) 
			{
				size = zipInputStream.Read(data, 0, data.Length);
				if (size > 0) 
					streamWriter.Write(data, 0, size);
				else 
					break;
			}

			streamWriter.Close();

			// devo farlo dopo avere chiuso lo 
			File.SetCreationTimeUtc(fullFileName, creationDate);
			File.SetLastWriteTimeUtc(fullFileName, creationDate);
		}

		//---------------------------------------------------------------------
		public string[] GetNestedFiles(string relativeDirectoryPath)
		{
			if (!DirectoryExists(relativeDirectoryPath))
				return new string[]{};

			relativeDirectoryPath = relativeDirectoryPath.Replace('\\', '/');
			ZipEntry theEntry = null;

			// costruisco il nome della dir come entry singola
			string entryToSearch = relativeDirectoryPath.ToLower(CultureInfo.InvariantCulture);
			if (entryToSearch.Length == 0 || entryToSearch[entryToSearch.Length - 1] != '/')
				entryToSearch += "/";
			
			ZipInputStream s = new ZipInputStream(File.OpenRead(zipFileFullName));
			ArrayList list = new ArrayList();

			entryToSearch = entryToSearch.ToLower(CultureInfo.InvariantCulture);
			while ((theEntry = s.GetNextEntry()) != null)
			{
				string name = theEntry.Name;
				if (name.Length > entryToSearch.Length			&&	// tolgo l'entry della directory stessa (p.es. WinZip la mette)
					name.ToLower(CultureInfo.InvariantCulture).StartsWith(entryToSearch)	&&
					name[name.Length - 1] != '\\'				&&	// tolgo le subdirectories
					name[name.Length - 1] != '/')
					list.Add(theEntry.Name);
			}
			
			s.Close();
			return (string[])list.ToArray(typeof(string));
		}

		//---------------------------------------------------------------------
		public string[] GetFiles(string relativeDirectoryPath)
		{
			if (!DirectoryExists(relativeDirectoryPath))
				return new string[]{};

			if (relativeDirectoryPath.IndexOf('\\') != -1)	// reduces pressure on managed heap replacing only if needed
				relativeDirectoryPath = relativeDirectoryPath.Replace('\\', '/');

			// costruisco il nome della dir come entry singola
			string entryToSearch = relativeDirectoryPath;
			if (entryToSearch.Length == 1 &&
				entryToSearch[entryToSearch.Length - 1] == '/')
				entryToSearch = string.Empty;
			else if (entryToSearch.Length > 0 && entryToSearch[entryToSearch.Length - 1] != '/')
				entryToSearch += "/";
			
			ZipEntry theEntry = null;
			//baseStream.Close;
			ZipInputStream s = new ZipInputStream(/*baseStream*/File.OpenRead(zipFileFullName));
			ArrayList list = new ArrayList();

			while ((theEntry = s.GetNextEntry()) != null)
			{
				string name = theEntry.Name;
				char lastChar = name[name.Length - 1];
				if (lastChar == '/' || lastChar == '\\')	// take dirs off
					continue;
				if (name.Length > entryToSearch.Length			&&	// tolgo l'entry della directory stessa (p.es. WinZip la mette)
					string.Compare
						(name, 0, entryToSearch, 0, entryToSearch.Length, true, 
						System.Globalization.CultureInfo.InvariantCulture) == 0 &&
					name.IndexOfAny(new char[]{'\\', '/'}, entryToSearch.Length) == -1)	// tolgo i files nelle subdirectories
					list.Add(name);
			}
			
			s.Close();
			return (string[])list.ToArray(typeof(string));
		}

		//---------------------------------------------------------------------
		public string[] GetDirectories(string relativeDirectoryPath)
		{
			if (!DirectoryExists(relativeDirectoryPath))
				return new string[]{};

			relativeDirectoryPath = relativeDirectoryPath.Replace('\\', '/');
			ZipEntry theEntry = null;

			// costruisco il nome della dir come entry singola
			string entryToSearch = relativeDirectoryPath.ToLower(CultureInfo.InvariantCulture);
			if (entryToSearch.Length == 0 || entryToSearch[entryToSearch.Length - 1] != '/')
				entryToSearch += "/";
			
			ZipInputStream s = new ZipInputStream(File.OpenRead(zipFileFullName));
			ArrayList list = new ArrayList();

			entryToSearch = entryToSearch.ToLower(CultureInfo.InvariantCulture);
			while ((theEntry = s.GetNextEntry()) != null)
			{
				string name = theEntry.Name;
				if (name.Length > entryToSearch.Length			&&	// tolgo l'entry della directory stessa (p.es. WinZip la mette)
					name.ToLower(CultureInfo.InvariantCulture).StartsWith(entryToSearch)	&&
					(name[name.Length - 1] == '\\' || name[name.Length - 1] == '/')	&&	// deve essere una directory (la prox con basterebbe, ma così è più efficiente nello scartare i files)
					name.IndexOfAny(new char[]{'\\', '/'}, entryToSearch.Length) == name.Length - 1)	// tolgo i files nell subdirectories
					list.Add(theEntry.Name);
			}
			
			s.Close();
			return (string[])list.ToArray(typeof(string));
		}

		//---------------------------------------------------------------------
	}
}
