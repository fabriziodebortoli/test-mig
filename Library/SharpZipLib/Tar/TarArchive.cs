using System;
using System.IO;
using System.Text;

namespace ICSharpCode.SharpZipLib.Tar {
	
	public delegate void ProgressMessageHandler(TarArchive archive, string message);
	
	/// <summary>
	/// The TarArchive class implements the concept of a
	/// tar archive. A tar archive is a series of entries, each of
	/// which represents a file system object. Each entry in
	/// the archive consists of a header record. Directory entries
	/// consist only of the header record, and are followed by entries
	/// for the directory's contents. File entries consist of a
	/// header record followed by the number of records needed to
	/// contain the file's contents. All entries are written on
	/// record boundaries. Records are 512 bytes long.
	/// 
	/// TarArchives are instantiated in either read or write mode,
	/// based upon whether they are instantiated with an InputStream
	/// or an OutputStream. Once instantiated TarArchives read/write
	/// mode can not be changed.
	/// 
	/// There is currently no support for random access to tar archives.
	/// However, it seems that subclassing TarArchive, and using the
	/// TarBuffer.getCurrentRecordNum() and TarBuffer.getCurrentBlockNum()
	/// methods, this would be rather trvial.
	/// </summary>
	public class TarArchive
	{
		protected bool verbose;
		protected bool debug;
		protected bool keepOldFiles;
		protected bool asciiTranslate;
		
		protected int    userId;
		protected string userName;
		protected int    groupId;
		protected string groupName;
		
		protected string rootPath;
		protected string pathPrefix;
		
		protected int    recordSize;
		protected byte[] recordBuf;
		
		protected TarInputStream  tarIn;
		protected TarOutputStream tarOut;
		
		public event ProgressMessageHandler ProgressMessageEvent;
		
		protected virtual void OnProgressMessageEvent(string message)
		{
			if (ProgressMessageEvent != null) {
				ProgressMessageEvent(this, message);
			}
		}
		
		protected TarArchive()
		{
		}
		
		/// <summary>
		/// The InputStream based constructors create a TarArchive for the
		/// purposes of e'x'tracting or lis't'ing a tar archive. Thus, use
		/// these constructors when you wish to extract files from or list
		/// the contents of an existing tar archive.
		/// </summary>
		public static TarArchive CreateInputTarArchive(Stream inputStream)
		{
			return CreateInputTarArchive(inputStream, TarBuffer.DEFAULT_BLKSIZE);
		}
		
		public static TarArchive CreateInputTarArchive(Stream inputStream, int blockSize)
		{
			return CreateInputTarArchive(inputStream, blockSize, TarBuffer.DEFAULT_RCDSIZE);
		}
		
		public static TarArchive CreateInputTarArchive(Stream inputStream, int blockSize, int recordSize)
		{
			TarArchive archive = new TarArchive();
			archive.tarIn = new TarInputStream(inputStream, blockSize, recordSize);
			archive.Initialize(recordSize);
			return archive;
		}
		
		/// <summary>
		/// The OutputStream based constructors create a TarArchive for the
		/// purposes of 'c'reating a tar archive. Thus, use these constructors
		/// when you wish to create a new tar archive and write files into it.
		/// </summary>
		public static TarArchive CreateOutputTarArchive(Stream outputStream)
		{
			return CreateOutputTarArchive(outputStream, TarBuffer.DEFAULT_BLKSIZE);
		}
		
		public static TarArchive CreateOutputTarArchive(Stream outputStream, int blockSize)
		{
			return CreateOutputTarArchive(outputStream, blockSize, TarBuffer.DEFAULT_RCDSIZE);
		}
		
		public static TarArchive CreateOutputTarArchive(Stream outputStream, int blockSize, int recordSize)
		{
			TarArchive archive = new TarArchive();
			archive.tarOut = new TarOutputStream(outputStream, blockSize, recordSize);
			archive.Initialize(recordSize);
			return archive;
		}
		
		
		/// <summary>
		/// Common constructor initialization code.
		/// </summary>
		void Initialize(int recordSize)
		{
			this.rootPath   = null;
			this.pathPrefix = null;
			
//			this.tempPath   = System.getProperty( "user.dir" );
			
			this.userId    = 0;
			this.userName  = String.Empty;
			this.groupId   = 0;
			this.groupName = String.Empty;
			
			this.debug           = false;
			this.verbose         = false;
			this.keepOldFiles    = false;
			
			this.recordBuf = new byte[RecordSize];
		}
		
		/**
		* Set the debugging flag.
		*
		* @param debugF The new debug setting.
		*/
		public void SetDebug(bool debugF)
		{
			this.debug = debugF;
			if (this.tarIn != null) {
				this.tarIn.SetDebug(debugF);
			} 
			if (this.tarOut != null) {
				this.tarOut.SetDebug(debugF);
			}
		}
		
		/// <summary>
		/// Get/Set the verbosity setting.
		/// </summary>
		public bool IsVerbose {
			get {
				return verbose;
			}
			set {
				verbose = value;
			}
		}
		
		/// <summary>
		/// Set the flag that determines whether existing files are
		/// kept, or overwritten during extraction.
		/// </summary>
		/// <param name="keepOldFiles">
		/// If true, do not overwrite existing files.
		/// </param>
		public void SetKeepOldFiles(bool keepOldFiles)
		{
			this.keepOldFiles = keepOldFiles;
		}
		
		/// <summary>
		/// Set the ascii file translation flag. If ascii file translatio
		/// is true, then the MIME file type will be consulted to determine
		/// if the file is of type 'text/*'. If the MIME type is not found,
		/// then the TransFileTyper is consulted if it is not null. If
		/// either of these two checks indicates the file is an ascii text
		/// file, it will be translated. The translation converts the local
		/// operating system's concept of line ends into the UNIX line end,
		/// '\n', which is the defacto standard for a TAR archive. This makes
		/// text files compatible with UNIX, and since most tar implementations
		/// text files compatible with UNIX, and since most tar implementations
		/// </summary>
		/// <param name= "asciiTranslate">
		/// If true, translate ascii text files.
		/// </param>
		public void SetAsciiTranslation(bool asciiTranslate)
		{
			this.asciiTranslate = asciiTranslate;
		}
		
		/*
		/// <summary>
		/// Set the object that will determine if a file is of type
		/// ascii text for translation purposes.
		/// </summary>
		/// <param name="transTyper">
		/// The new TransFileTyper object.
		/// </param>
		public void SetTransFileTyper(TarTransFileTyper transTyper)
		{
			this.transTyper = transTyper;
		}*/
		
		/// <summary>
		/// Set user and group information that will be used to fill in the
		/// tar archive's entry headers. Since Java currently provides no means
		/// of determining a user name, user id, group name, or group id for
		/// a given File, TarArchive allows the programmer to specify values
		/// to be used in their place.
		/// </summary>
		/// <param name="userId">
		/// The user Id to use in the headers.
		/// </param>
		/// <param name="userName">
		/// The user name to use in the headers.
		/// </param>
		/// <param name="groupId">
		/// The group id to use in the headers.
		/// </param>
		/// <param name="groupName">
		/// The group name to use in the headers.
		/// </param>
		public void SetUserInfo(int userId, string userName, int groupId, string groupName)
		{
			this.userId    = userId;
			this.userName  = userName;
			this.groupId   = groupId;
			this.groupName = groupName;
		}
		
		/// <summary>
		/// Get the user id being used for archive entry headers.
		/// </summary>
		/// <returns>
		/// The current user id.
		/// </returns>
		public int UserId {
			get {
				return this.userId;
			}
		}
		
		/// <summary>
		/// Get the user name being used for archive entry headers.
		/// </summary>
		/// <returns>
		/// The current user name.
		/// </returns>
		public string UserName {
			get {
				return this.userName;
			}
		}
		
		/// <summary>
		/// Get the group id being used for archive entry headers.
		/// </summary>
		/// <returns>
		/// The current group id.
		/// </returns>
		public int GroupId {
			get {
				return this.groupId;
			}
		}
		
		/// <summary>
		/// Get the group name being used for archive entry headers.
		/// </summary>
		/// <returns>
		/// The current group name.
		/// </returns>
		public string GroupName {
			get {
				return this.groupName;
			}
		}
		
		/// <summary>
		/// Get the archive's record size. Because of its history, tar
		/// supports the concept of buffered IO consisting of BLOCKS of
		/// RECORDS. This allowed tar to match the IO characteristics of
		/// the physical device being used. Of course, in the Java world,
		/// this makes no sense, WITH ONE EXCEPTION - archives are expected
		/// to be propertly "blocked". Thus, all of the horrible TarBuffer
		/// support boils down to simply getting the "boundaries" correct.
		/// </summary>
		/// <returns>
		/// The record size this archive is using.
		/// </returns>
		public int RecordSize {
			get {
				if (this.tarIn != null) {
					return this.tarIn.GetRecordSize();
				} else if (this.tarOut != null) {
					return this.tarOut.GetRecordSize();
				}
				return TarBuffer.DEFAULT_RCDSIZE;
			}
		}
		
		/// <summary>
		/// Close the archive. This simply calls the underlying
		/// tar stream's close() method.
		/// </summary>
		public void CloseArchive()
		{
			if (this.tarIn != null) {
				this.tarIn.Close();
			} else if (this.tarOut != null) {
				this.tarOut.Flush();
				this.tarOut.Close();
			}
		}
		
		/// <summary>
		/// Perform the "list" command and list the contents of the archive.
		/// 
		/// NOTE That this method uses the progress display to actually list
		/// the conents. If the progress display is not set, nothing will be
		/// listed!
		/// </summary>
		public void ListContents()
		{
			while (true) {
				TarEntry entry = this.tarIn.GetNextEntry();
				
				if (entry == null) {
					if (this.debug) {
						Console.Error.WriteLine("READ EOF RECORD");
					}
					break;
				}
				OnProgressMessageEvent(entry.Name);
			}
		}
		
		/// <summary>
		/// Perform the "extract" command and extract the contents of the archive.
		/// </summary>
		/// <param name="destDir">
		/// The destination directory into which to extract.
		/// </param>
		public void ExtractContents(string destDir)
		{
			while (true) {
				TarEntry entry = this.tarIn.GetNextEntry();
				
				if (entry == null) {
					if (this.debug) {
						Console.Error.WriteLine("READ EOF RECORD");
					}
					break;
				}
				
				this.ExtractEntry(destDir, entry);
			}
		}
		
		void EnsureDirectoryExists(string directoryName)
		{
			if (!Directory.Exists(directoryName)) {
				try {
					Directory.CreateDirectory(directoryName);
				} catch (Exception e) {
					throw new IOException("error making directory path '" + directoryName + "', " + e.Message);
				}
			}
		}
		
		
		bool IsBinary(string filename)
		{
			FileStream fs = File.OpenRead(filename);
			
			byte[] content = new byte[fs.Length];
			
			fs.Read(content, 0, (int)fs.Length);
			fs.Close();
			
			// assume that ascii 0 or 
			// ascii 255 are only found in non text files.
			// and that all non text files contain 0 and 255
			foreach (byte b in content) {
				if (b == 0 || b == 255) {
					return true;
				}
			}
			
			return false;
		}		
		
		/// <summary>
		/// Extract an entry from the archive. This method assumes that the
		/// tarIn stream has been properly set with a call to getNextEntry().
		/// </summary>
		/// <param name="destDir">
		/// The destination directory into which to extract.
		/// </param>
		/// <param name="entry">
		/// The TarEntry returned by tarIn.getNextEntry().
		/// </param>
		void ExtractEntry(string destDir, TarEntry entry)
		{
			if (this.verbose) {
				OnProgressMessageEvent(entry.Name);
			}
			
			string name = entry.Name;
			name = name.Replace('/', Path.DirectorySeparatorChar);
			
			if (!destDir.EndsWith(Path.DirectorySeparatorChar.ToString())) {
				destDir += Path.DirectorySeparatorChar;
			}
			
			string destFile = destDir + name;
			
			if (entry.IsDirectory) {
				EnsureDirectoryExists(destFile);
			} else {
				string parentDirectory = Path.GetDirectoryName(destFile);
				EnsureDirectoryExists(parentDirectory);
				
				if (this.keepOldFiles && File.Exists(destFile)) {
					if (this.verbose) {
						OnProgressMessageEvent("not overwriting " + entry.Name);
					}
				} else {
					bool asciiTrans = false;
					Stream outputStream = File.Create(destFile);
					if (this.asciiTranslate) {
						asciiTrans = !IsBinary(destFile);
// original java sourcecode : 
//						MimeType mime      = null;
//						string contentType = null;
//						try {
//							contentType = FileTypeMap.getDefaultFileTypeMap().getContentType( destFile );
//							
//							mime = new MimeType(contentType);
//							
//							if (mime.getPrimaryType().equalsIgnoreCase( "text" )) {
//							    	asciiTrans = true;
//							} else if ( this.transTyper != null ) {
//							    if ( this.transTyper.isAsciiFile( entry.getName() ) ) {
//							    	asciiTrans = true;
//							    }
//							}
//						} catch (MimeTypeParseException ex) {
//						}
//						
//						if (this.debug) {
//							Console.Error.WriteLine(("EXTRACT TRANS? '" + asciiTrans + "'  ContentType='" + contentType + "'  PrimaryType='" + mime.getPrimaryType() + "'" );
//						}
					}
					
					StreamWriter outw = null;
					if (asciiTrans) {
						outw = new StreamWriter(outputStream);
					}
					
					byte[] rdbuf = new byte[32 * 1024];
					
					while (true) {
						int numRead = this.tarIn.Read(rdbuf, 0, rdbuf.Length);
						
						if (numRead <= 0) {
							break;
						}
						
						if (asciiTrans) {
							for (int off = 0, b = 0; b < numRead; ++b) {
								if (rdbuf[b] == 10) {
									string s = Encoding.ASCII.GetString(rdbuf, off, (b - off));
									outw.WriteLine(s);
									off = b + 1;
								}
							}
						} else {
							outputStream.Write(rdbuf, 0, numRead);
						}
					}
					
					if (asciiTrans) {
						outw.Close();
					} else {
						outputStream.Close();
					}
				}
			}
		}
		
		/// <summary>
		/// Write an entry to the archive. This method will call the putNextEntry
		/// and then write the contents of the entry, and finally call closeEntry()()
		/// for entries that are files. For directories, it will call putNextEntry(),
		/// and then, if the recurse flag is true, process each entry that is a
		/// child of the directory.
		/// </summary>
		/// <param name="entry">
		/// The TarEntry representing the entry to write to the archive.
		/// </param>
		/// <param name="recurse">
		/// If true, process the children of directory entries.
		/// </param>
		public void WriteEntry(TarEntry entry, bool recurse)
		{
			bool asciiTrans = false;
			
			string tempFileName = null;
			string eFile        = entry.File;
			
			// Work on a copy of the entry so we can manipulate it.
			// Note that we must distinguish how the entry was constructed.
			//
			if (eFile == null || eFile.Length == 0) {
				entry = TarEntry.CreateTarEntry(entry.Name);
			} else {
				//
				// The user may have explicitly set the entry's name to
				// something other than the file's path, so we must save
				// and restore it. This should work even when the name
				// was set from the File's name.
				//
				string saveName = entry.Name;
				entry = TarEntry.CreateEntryFromFile(eFile);
				entry.Name = saveName;
			}
			
			if (this.verbose) {
				OnProgressMessageEvent(entry.Name);
			}
			
			if (this.asciiTranslate && !entry.IsDirectory) {
				asciiTrans = !IsBinary(eFile);
// original java source :
//			    	MimeType mime = null;
//			    	string contentType = null;
//	
//			    	try {
//			    		contentType = FileTypeMap.getDefaultFileTypeMap(). getContentType( eFile );
//			    		
//			    		mime = new MimeType( contentType );
//			    		
//			    		if ( mime.getPrimaryType().
//			    		    equalsIgnoreCase( "text" ) )
//			    		    {
//			    		    	asciiTrans = true;
//			    		    }
//			    		    else if ( this.transTyper != null )
//			    		    {
//			    		    	if ( this.transTyper.isAsciiFile( eFile ) )
//			    		    	{
//			    		    		asciiTrans = true;
//			    		    	}
//			    		    }
//			    	} catch ( MimeTypeParseException ex )
//			    	{
//	//		    		 IGNORE THIS ERROR...
//			    	}
//		    	
//		    	if (this.debug) {
//		    		Console.Error.WriteLine("CREATE TRANS? '" + asciiTrans + "'  ContentType='" + contentType + "'  PrimaryType='" + mime.getPrimaryType()+ "'" );
//		    	}
		    	
		    	if (asciiTrans) {
		    		tempFileName = Path.GetTempFileName();
		    		
		    		StreamReader inStream  = File.OpenText(eFile);
		    		Stream       outStream = new BufferedStream(File.Create(tempFileName));
		    		
		    		while (true) {
		    			string line = inStream.ReadLine();
		    			if (line == null) {
		    				break;
		    			}
		    			byte[] data = Encoding.ASCII.GetBytes(line);
		    			outStream.Write(data, 0, data.Length);
		    			outStream.WriteByte((byte)'\n');
		    		}
		    		
		    		inStream.Close();
		    		outStream.Flush();
		    		outStream.Close();
		    		
		    		entry.Size = new FileInfo(tempFileName).Length;
		    		
		    		eFile = tempFileName;
		    	}
			}
		    
		    string newName = null;
		
			if (this.rootPath != null) {
				if (entry.Name.StartsWith(this.rootPath)) {
					newName = entry.Name.Substring(this.rootPath.Length + 1 );
				}
			}
			
			if (this.pathPrefix != null) {
				newName = (newName == null) ? this.pathPrefix + "/" + entry.Name : this.pathPrefix + "/" + newName;
			}
			
			if (newName != null) {
				entry.Name = newName;
			}
			
			this.tarOut.PutNextEntry(entry);
			
			if (entry.IsDirectory) {
				if (recurse) {
					TarEntry[] list = entry.GetDirectoryEntries();
					for (int i = 0; i < list.Length; ++i) {
						this.WriteEntry(list[i], recurse);
					}
				}
			} else {
				Stream inputStream = File.OpenRead(eFile);
				int numWritten = 0;
				byte[] eBuf = new byte[32 * 1024];
				while (true) {
					int numRead = inputStream.Read(eBuf, 0, eBuf.Length);
					
					if (numRead <=0) {
						break;
					}
					
					this.tarOut.Write(eBuf, 0, numRead);
					numWritten +=  numRead;
				}
				Console.WriteLine("written " + numWritten + " bytes");
				
				inputStream.Close();
				
				if (tempFileName != null && tempFileName.Length > 0) {
					File.Delete(tempFileName);
				}
				
				this.tarOut.CloseEntry();
			}
		}
	}
}
/* The original Java file had this header:
	** Authored by Timothy Gerard Endres
	** <mailto:time@gjt.org>  <http://www.trustice.com>
	**
	** This work has been placed into the public domain.
	** You may use this work in any way and for any purpose you wish.
	**
	** THIS SOFTWARE IS PROVIDED AS-IS WITHOUT WARRANTY OF ANY KIND,
	** NOT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY. THE AUTHOR
	** OF THIS SOFTWARE, ASSUMES _NO_ RESPONSIBILITY FOR ANY
	** CONSEQUENCE RESULTING FROM THE USE, MODIFICATION, OR
	** REDISTRIBUTION OF THIS SOFTWARE.
	**
	*/
