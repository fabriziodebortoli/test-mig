using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

using Microarea.Common.Generic;
using Microarea.Common.NameSolver;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Microarea.Common.StringLoader
{
    //=========================================================================
    public class StringLoader
	{
		private static DictionaryTableContainer container;
		internal static string cacheIdentifier = string.Empty;
		internal static bool cachingEnabled = false;

		//-----------------------------------------------------------------------------
		internal static DictionaryTableContainer Container
		{
			get
			{
				lock (typeof(DictionaryTableContainer))
				{
					if (container == null)
						container = new DictionaryTableContainer();
					return container;
				}
			}
		}

		//-----------------------------------------------------------------------------
		public static void EnableDictionaryCaching()
		{
			lock (typeof(DictionaryTableContainer))
			{
				cachingEnabled = true;
				cacheIdentifier = InstallationData.InstallationDate.ToString();
			}
		}

		//-----------------------------------------------------------------------------
		public static void DisableDictionaryCaching()
		{
			cachingEnabled = false;
		}

		//-----------------------------------------------------------------------------
		public static void ClearDictionaryCache()
		{
			Clear();
		}

		//-----------------------------------------------------------------------------
		internal static DictionaryStringBlock GetDictionary(string dictionaryPath, string type, string id, string name)
		{
			string key = string.Format("{0}-{1}-{2}-{3}", dictionaryPath, type, id, name);
			lock (Container)
			{
                DictionaryStringBlock dictionary = null;
				if (!Container.dictionaries.TryGetValue(key, out dictionary))
				{
					try
					{
                        bool found;

                        if (Container.failedDictionaries.TryGetValue(key, out found))
							return null;

						DictionaryBinaryFile d = DictionaryBinaryFileLRU.GetDictionary(dictionaryPath);

						if (d == null)
						{
							Container.failedDictionaries[key] = true;
							return null;
						}

						dictionary = d.GetBlock(type, id, name);

						if (dictionary == null)
						{
							Container.failedDictionaries[key] = true;
							return null;
						}
						Container.dictionaries[key] = dictionary;
					}
					catch (DictionarySerializerException e)
					{
						Debug.Fail(string.Format("Failed GetDictionary for path {0} error : {1}", dictionaryPath, e.Message));
						Container.failedDictionaries[key] = true;
						return null;
					}
				}

				return dictionary;
			}
		}

		//--------------------------------------------------------------------------------
		public static void Clear()
		{
			lock (Container)
			{
				Container.dictionaries.Clear();
				Container.failedDictionaries.Clear();
			}
		}
	}
	//=========================================================================
	[Serializable]
	public class DictionaryTable : Dictionary<string, DictionaryStringBlock> 
	{
		//-----------------------------------------------------------------------------
		public DictionaryTable()
            : base(StringComparer.OrdinalIgnoreCase) 
		{
		}

		//-----------------------------------------------------------------------------
		protected DictionaryTable(SerializationInfo info, StreamingContext context)
			//: base (info, context)   TODO rsweb
		{
		}

	}

	//=========================================================================
	internal class DictionaryBinaryFileLRU
	{
		private const int maxSize = 20;

		private static List<DictionaryBinaryFile> dictionaries = new List<DictionaryBinaryFile>();

		//-----------------------------------------------------------------------------
		public static DictionaryBinaryFile GetDictionary(string dictionaryPath)
		{
			if (!PathFinder.PathFinderInstance.ExistFile(dictionaryPath))
				return null;

			lock (dictionaries)
			{
				foreach (DictionaryBinaryFile d in dictionaries)
				{
					if (string.Compare(d.Path, dictionaryPath, StringComparison.OrdinalIgnoreCase) != 0)
						continue;

					dictionaries.Remove(d);
					dictionaries.Add(d);
					return d;
				}

				DictionaryBinaryFile newDict = new DictionaryBinaryFile(dictionaryPath);
				using (DictionaryBinaryParser p = new DictionaryBinaryParser(new FileStream(dictionaryPath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536)))
				{
					newDict.Parse(p);
				}
				if (dictionaries.Count == maxSize)
					dictionaries.RemoveAt(0);

				dictionaries.Add(newDict);

				return newDict;
			}
		}
	}

	//=========================================================================
	internal class DictionaryTableContainer
	{
		public	const int CACHE_VERSION = 3;

		public	DictionaryTable dictionaries = null;
		public Dictionary<string, bool> failedDictionaries = null;
		string	cachePath = null;

		//-----------------------------------------------------------------------------
		public DictionaryTableContainer()
		{
			InitCache();
		}

		//-----------------------------------------------------------------------------
		~DictionaryTableContainer()
		{
			SaveCache(null);
		}

		//-----------------------------------------------------------------------------
		private string CachePath 
		{
			get
			{
				if (cachePath == null)
				{
					cachePath = Path.Combine(PathFinder.PathFinderInstance.GetAppDataPath(true), "StringLoader.bin");
					if (cachePath.StartsWith(@"file:\"))
						cachePath = cachePath.Substring(6);
				}
				return cachePath;
			}
		}

		//-----------------------------------------------------------------------------
		private void SaveCache(object dummy)
		{
			if (!StringLoader.cachingEnabled) return;
 
			try
			{
				lock(this)
				{
					using (Stream s = File.Open(CachePath, FileMode.Create, FileAccess.Write))
					{
						//BinaryFormatter bf = new BinaryFormatter();                     TODO rsweb                 rimosso dalla piattaforma
						//bf.Serialize(s, CACHE_VERSION);
						//bf.Serialize(s, StringLoader.cacheIdentifier);
						//bf.Serialize(s, dictionaries);
						//bf.Serialize(s, failedDictionaries);
					}
				}
			}
			catch(Exception ex)
			{
				Debug.Fail("Error saving dictionary cache!", ex.Message);
			}
			finally
			{
			}
		}

		//-----------------------------------------------------------------------------
		private void InitCache()
		{
			if (!StringLoader.cachingEnabled)
			{
				dictionaries = new DictionaryTable();
				failedDictionaries = new Dictionary<string, bool>();
				return;
			}

			string path = CachePath;
			try
			{
 
				if (!PathFinder.PathFinderInstance.ExistFile(path)) return;
		
				using (Stream s = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					try
					{
                       // BinaryFormatter bf = new BinaryFormatter();                          TODO rsweb                 rimosso dalla piattaforma
                        //int ver = (int) bf.Deserialize(s);
                        //if (ver != CACHE_VERSION)
                        //	throw new Exception("Wrong cache version");
                        //string cacheId = (string) bf.Deserialize(s);
                        //if (cacheId != StringLoader.cacheIdentifier)
                        //	throw new Exception("Wrong cache version");
                        //dictionaries = bf.Deserialize(s) as DictionaryTable;
                        //failedDictionaries = bf.Deserialize(s) as DictionaryTable;
                    }
                    catch (Exception)
					{
                        //Trace.WriteLine("Error loading dictionary cache! - " + ex.Message);
                        PathFinder.PathFinderInstance.RemoveFile(path);
					}
				}
			}
			catch (Exception)
			{
				//Trace.WriteLine("Error loading dictionary cache! - " + ex.Message);
			}
			finally
			{
				if (dictionaries == null)
					dictionaries = new DictionaryTable();
				if (failedDictionaries == null)
					failedDictionaries = new Dictionary<string, bool>();
			}
		}
	}
}
