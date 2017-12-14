using System;
using System.Collections;
using System.IO;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
	/// <summary>
	/// Gestisce l'eliminazione automatica del file quando muore l'oggetto
	/// </summary>
	//================================================================================
	public class WoormFileSystemObject : IDisposable
	{
		private const string prefix = "WoormFileSystemObject:";

		private string path;
		private bool disposed;
		private WoormFileSystemObject parent;
		private ArrayList childs;
		
		//--------------------------------------------------------------------------------
		public string Name { get { return GetName(path); } }

		//--------------------------------------------------------------------------------
		public string Path { get { return path; } }

		//--------------------------------------------------------------------------------
		public static string GetName(string path)
		{
			return prefix + path.ToLower();
		}

		//--------------------------------------------------------------------------------
		public WoormFileSystemObject(string path)
			: this(path, null)
		{
		}
		
		//--------------------------------------------------------------------------------
		public WoormFileSystemObject(string path, WoormFileSystemObject parent)
		{
			this.path = path;
			this.disposed = false;
			this.parent = parent;
			this.childs = ArrayList.Synchronized(new ArrayList()); //array sincronizzato thread safe

			if (parent != null)
				parent.childs.Add(this);
		}

		//--------------------------------------------------------------------------------
		~WoormFileSystemObject()
		{
			DeleteFileSystemObject();
		}

		//--------------------------------------------------------------------------------
		public void Dispose()
		{
			if (!disposed && DeleteFileSystemObject())
				GC.SuppressFinalize(this);
		}
		
		//--------------------------------------------------------------------------------
		public void DisposeEventHanldler(object sender, EventArgs e)
		{
			Dispose();
		}

		//--------------------------------------------------------------------------------
		private bool DeleteFileSystemObject()
		{
			lock(this)
			{
				// se ho dei figli non posso proseguire
				// (mi cancellerà poi l'ultimo dei miei figli)
				if (childs.Count != 0) 
					return false;
				try
				{
					// rimuovo l'oggetto fisico associato
					if (File.Exists(path))
						File.Delete(path);
					else if (Directory.Exists(path) && Directory.GetFiles(path).Length == 0)
						Directory.Delete(path);
				}
				catch
				{
					// non propago l'errore perché potrei essere in fase di finalizzazione
					// (mal che vada, mi rimane un cadavere su file system)
					return false;
				}

				// cancello l'oggetto dalla lista dei figli del padre
				if (parent != null)
				{
					if (parent.childs.Contains(this))
						parent.childs.Remove(this);

					// il parent ha funzione di contenitore, se non ha più figli si 
					// può eliminare
					if (parent.childs.Count == 0)
						parent.Dispose();
				}

				disposed = true;
				
			}

			return true;
		}
	}
}
