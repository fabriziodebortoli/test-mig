using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	//=========================================================================
	//Vedere per riferimenti:
	//http://msdn.microsoft.com/en-us/library/dd153782.aspx
	//http://msdn.microsoft.com/en-us/library/25y1ya39.aspx
	//http://blogs.msdn.com/b/suzcook/archive/2003/05/29/57143.aspx
	//http://blogs.msdn.com/b/suzcook/archive/2003/06/13/57180.aspx
	//=========================================================================
	public sealed class AssembliesLoader
	{
		//=====================================================================
		private class CacheKey
		{
			public string Path { get; set; }
			public DateTime LastWriteTime { get; set; }

			//-----------------------------------------------------------------
			public CacheKey(string path, DateTime lastWriteTime)
			{
				this.Path = path;
				this.LastWriteTime = lastWriteTime;
			}

			//-----------------------------------------------------------------
			public override int GetHashCode()
			{
				return Path.GetHashCode() ^ LastWriteTime.GetHashCode();
			}

			//-----------------------------------------------------------------
			public override bool Equals(object obj)
			{
				CacheKey aCacheItem = obj as CacheKey;
				if (aCacheItem == null)
					return false;

				return
					String.Compare(Path, aCacheItem.Path, StringComparison.InvariantCultureIgnoreCase) == 0 &&
					DateTime.Equals(LastWriteTime, aCacheItem.LastWriteTime);
			}
		}

		private static IDictionary<CacheKey, Assembly> assemblies = new Dictionary<CacheKey, Assembly>();

		/// <summary>
		/// Ritorna l'assembly specificata con il path senza lockarla
		/// </summary>
		/// <remarks>
		/// Legge il file in un array di byte e poi carica l'assembly dall'array di byte.
		/// Questo ci permette di non lockare la DLL ma ha come contraltare il fatto
		/// che se viene richiesto di caricare un assembly questo viene ricaricato senza badare al fatto 
		/// che fosse già in memoria, di fatto caricando diverse copie identiche dell'assembly.
		/// Per ovviare a quetso fatto gestiamo una cache interna che decide se ricaricare l'assembly
		/// o se ritornare quello già caricato in base al percorso di caricamento e alla data/ora
		/// di ultima modifica del file.
		/// </remarks>
		/// <param name="path"></param>
		//-------------------------------------------------------------------------------
		public static Assembly Load(string path)
		{
			if (String.IsNullOrWhiteSpace(path))
				throw new ArgumentException("Null or empty path, unable to load assembly");

			FileInfo assemblyFileInfo = new FileInfo(path);
			if (!assemblyFileInfo.Exists)
				throw new ArgumentException(String.Format("Assembly file does not exist in {0}", path));

			CacheKey aCacheKey = new CacheKey(path, assemblyFileInfo.LastWriteTime);

			Assembly aAssembly = null;
			lock (typeof(AssembliesLoader))
			{
				if (assemblies.TryGetValue(aCacheKey, out aAssembly))
					return aAssembly;

				aAssembly = Assembly.Load(File.ReadAllBytes(path));

				assemblies.Add(aCacheKey, aAssembly);

				return aAssembly;
			}
		}

		/// <remarks>
		/// Caricare un assembly attraverso il suo assembly name fa si che
		/// questo venga caricato nel 'Load context' con tutti i vantaggi che ne conseguono:
		/// corretto versionamento, l'assembly, se già in memoria, viene semplicemente ritornato,
		/// Le dipendenze sono risolte automaticamente ecc.
		/// Proprio per il fatto che l'assembly, se già in memoria, non viene ricaricato nuovamente ma viene ritornato
		/// non gestisco qui la cache assemblies.
		/// </remarks>
		//-------------------------------------------------------------------------------
		public static Assembly LoadFromFullAssemblyName(string fullAssemblyName)
		{
			if (String.IsNullOrWhiteSpace(fullAssemblyName))
				throw new ArgumentException("'fullAssemblyName' is null or empty");

			return Assembly.Load(fullAssemblyName);
		}

        /// <summary>
        /// Ritorna il percorso dell'assembly
        /// </summary>
        /// <remarks>
        /// Caricando l'assembly da un array di byte è disponibile l'informaizone circa
        /// il suo path nella proprietà CodeBase per cui risolvo il path cercandolo
        /// nella cache degli assembly caricati.
        /// </remarks>
        /// <param name="path"></param>
        //-------------------------------------------------------------------------------
        public static string GetAssemblyPath(Assembly asm)
        {
            if (asm == null)
            {
                return null;
            }
            foreach (KeyValuePair<CacheKey, Assembly> tuple in assemblies)
            {
                if (tuple.Value == asm)
                {
                    return tuple.Key.Path;
                }
            }

            return null;
        }

		/// <remarks>
		/// Caricare un assembly attraverso il suo AssemblyName fa si che
		/// valgano tutte le considerazioni espresse per il metodo LoadFromAssemblyName
		/// </remarks>
		//-------------------------------------------------------------------------------
		public static Assembly Load(AssemblyName assemblyName)
		{
			if (assemblyName == null)
				throw new ArgumentNullException("assemblyName");

			try
			{
				return Assembly.Load(assemblyName);
			}
			catch (FileNotFoundException)
			{
				return LoadCachedAssemblyByAssemblyName(assemblyName);
			}
		}

		//-------------------------------------------------------------------------------
		private static Assembly LoadCachedAssemblyByAssemblyName(AssemblyName assemblyName)
		{
			if (assemblyName == null)
			{
				return null;
			}
			foreach (KeyValuePair<CacheKey, Assembly> tuple in assemblies)
			{
				if (tuple.Value.GetName().Name == assemblyName.Name)
				{
					return Load(tuple.Key.Path);
				}
			}

			return null;
		}
	}
}
