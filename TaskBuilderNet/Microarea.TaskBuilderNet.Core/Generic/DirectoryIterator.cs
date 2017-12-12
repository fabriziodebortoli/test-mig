using System;
using System.IO;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	/// <summary>
	/// Iterates for each subfolder of 'root' 
	/// and applies the supplied 'FileProcessingFunction' 
	/// to all files matching the 'filter' criterium
	/// </summary>
	//================================================================================
	public class DirectoryIterator
	{
		private string[] filters;
		
		private bool stop = false;

		public delegate void FileProcessingFunction(string path);
		public delegate void FileProcess(DirectoryIterator sender, string path);
		public delegate void FileProcessError(DirectoryIterator sender, Exception exception);
		public delegate bool PathValid(DirectoryIterator sender, string path);
		
		public event FileProcess OnStartProcessingFile;
		public event FileProcess OnEndProcessingFile;
		public event FileProcess OnStartProcessingDirectory;
		public event FileProcess OnEndProcessingDirectory;
		public event FileProcessError OnError;
		
		public event FileProcessingFunction OnProcessFile;
		public event FileProcessingFunction OnProcessFolder;
		public event PathValid				OnCheckFilePath;		
		public event PathValid				OnCheckFolderPath;

		//--------------------------------------------------------------------------------
		private bool HasToStop { get { Application.DoEvents(); return stop; } }
		
		//--------------------------------------------------------------------------------
		public DirectoryIterator(string filter)
			: this (filter, null)
		{
		}
		
		//--------------------------------------------------------------------------------
		public DirectoryIterator(string[] filters)
			: this (filters, null)
		{
		}

		//--------------------------------------------------------------------------------
		public DirectoryIterator(string filter, FileProcessingFunction fileFunction)
			: this (new string[] { filter }, fileFunction)
		{
		}
		
		//--------------------------------------------------------------------------------
		public DirectoryIterator(string filter, FileProcessingFunction fileFunction, FileProcessingFunction folderFunction)
			: this (new string[] { filter }, fileFunction, folderFunction)
		{
		}

		//--------------------------------------------------------------------------------
		public DirectoryIterator(string[] filters, FileProcessingFunction fileFunction)
			: this(filters, fileFunction, null)
		{
			
		}

		//--------------------------------------------------------------------------------
		public DirectoryIterator(string[] filters, FileProcessingFunction fileFunction, FileProcessingFunction folderFunction)
		{
			this.OnProcessFile += fileFunction;
			this.OnProcessFolder += folderFunction;
			this.filters = filters;
		}

		//--------------------------------------------------------------------------------
		protected bool IsFolderValid(string path)
		{
			if (OnCheckFolderPath == null) return true;

			return OnCheckFolderPath(this, path);
		}

		//--------------------------------------------------------------------------------
		private bool IsFileValid(string path)
		{
			if (OnCheckFilePath == null) return true;

			return OnCheckFilePath(this, path);
		}

		//--------------------------------------------------------------------------------
		public void Start(string root)
		{
			ParseDirectory(root);
		}

		//--------------------------------------------------------------------------------
		public void Stop()
		{
			stop = true;
		}

		//--------------------------------------------------------------------------------
		private void FireEvent(FileProcess evt, string path)
		{
			if (evt != null) evt(this, path);
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseDirectory(string directoryPath)
		{
			if (stop || !IsFolderValid(directoryPath)) return;
			
			FireEvent(OnStartProcessingDirectory, directoryPath);

			try
			{
				if (OnProcessFolder != null) 
					OnProcessFolder(directoryPath);
			}
			catch(Exception ex)
			{
				if (OnError != null)
					OnError(this, ex);
			}

			foreach (string filter in filters)
			{
				string[] files = Directory.GetFiles(directoryPath, filter);
				foreach(string file in files) 
				{
					if (HasToStop) return;
					
					if (!IsFileValid(file)) continue;

					FireEvent(OnStartProcessingFile, file);
					try
					{
						if (OnProcessFile != null) 
							OnProcessFile(file);
					}
					catch(Exception ex)
					{
						if (OnError != null)
							OnError(this, ex);
					}
					finally
					{
						FireEvent(OnEndProcessingFile, file);
					}
				}
			}

			string[] dirs = Directory.GetDirectories(directoryPath, "*.*");
			
			foreach(string dir in dirs) 
			{
				if (HasToStop) return;
				
				ParseDirectory(dir);
			}

			FireEvent(OnEndProcessingDirectory, directoryPath);
		}

	}
}
