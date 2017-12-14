using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.TaskBuilderNet.Core.SerializableTypes
{
	[Serializable]
	public class ApplicationCache : Serializable
	{
		private const string fileName = "AppData.xml";
		private List<CachedObject> cachedObjects = new List<CachedObject>();
		private static Type[] serializadTypes = //necessario, altrimenti non riesce a serializzare questi tipi
		{
			typeof(LoggedUser),
			typeof(ListRecentForms),
			typeof(RecentForm),
			typeof(ShownMessages),
			typeof(ShownMessage), 
			typeof(CachedObject)
		};
		private static string file = "";

		public List<CachedObject> CachedObjects
		{
			get { return cachedObjects; }
		}
		
		//---------------------------------------------------------------------------
		static ApplicationCache()
		{
			try
			{
				file = Path.Combine(BasePathFinder.BasePathFinderInstance.GetAppDataPath(true), fileName);
			}
			catch 
			{
			}
		}

		//---------------------------------------------------------------------------
		public ApplicationCache()
		{
		}
		//---------------------------------------------------------------------------
		public void PutObject<PropertyType>(PropertyType obj) where PropertyType : class
		{
			lock (typeof(ApplicationCache))
			{
				for (int i = 0; i < CachedObjects.Count; i++)
				{
					CachedObject co = CachedObjects[i];
					if (co.Name == typeof(PropertyType).ToString())
					{
						co.Object = obj;
						return;
					}
				}
				CachedObject co1 = new CachedObject();
				co1.Name = typeof(PropertyType).ToString();
				co1.Object = obj;
				CachedObjects.Add(co1);
				
			}
		}
		//---------------------------------------------------------------------------
		public PropertyType GetObject<PropertyType>() where PropertyType : class
		{
			lock (typeof(ApplicationCache))
			{
				foreach (CachedObject co in CachedObjects)
					if (co.Name == typeof(PropertyType).ToString())
						return co.Object as PropertyType;
				return null;
			}
		}

		//---------------------------------------------------------------------------
		public void Save()
		{
			lock (typeof(ApplicationCache))
			{
				Save(file, serializadTypes);
			}
		}

		/// <summary>
		/// Questo metodo è un truschetto ad hoc per il provisioningconfigurator.
		/// </summary>
		//---------------------------------------------------------------------------
		public void SaveLoggedUserForMagoFromProvisioningConfidurator()
		{
			lock (typeof(ApplicationCache))
			{
				FileInfo fi = new FileInfo(file);
				foreach (DirectoryInfo d in fi.Directory.Parent.GetDirectories())
				{
					if (d.Name.StartsWith("tbappmanager", StringComparison.InvariantCultureIgnoreCase))
					{
						Save(Path.Combine(d.FullName, fi.Name), serializadTypes);
						return;
					}
				}
			}
		}

		//---------------------------------------------------------------------------
		public static ApplicationCache Load()
		{
			lock (typeof(ApplicationCache))
			{
				return Load<ApplicationCache>(file, serializadTypes);
			}
		}
	}

	[Serializable]
	public class CachedObject
	{
		public string Name { get; set; }
		public Object Object { get; set; }
	}

	[Serializable]
	public class ShownMessages : List<ShownMessage>
	{
	}
	[Serializable]
	public class ShownMessage
	{
		public string Message { get; set; }
		public DialogResult Result { get; set; }
	}
}
