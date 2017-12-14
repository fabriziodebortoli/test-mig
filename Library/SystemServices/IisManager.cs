//Following creates a new virtual directory under the Root on the localhost, and calls AppCreate2 to create an application in the COM+
//catalog (not stricly required for asp.net).
//
// TODO: is creat the physical directory, and check whether the Vdir doesn't exists allready.
//
//
using System;
using System.Collections;
using System.Diagnostics;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;	// server solo per fare un catch

using Microsoft.Win32;

namespace Microarea.Library.SystemServices
{
	/// <summary>
	/// Summary description for IisManager.
	/// </summary>
	public class IisManager
	{
		// Following flags taken from MSDN IIS Docs will generate warnings when compiling
		// Authorization flags
		private const int  MD_AUTH_ANONYMOUS			= 0x00000001; // Anonymous authentication available.
		private const int  MD_AUTH_BASIC				= 0x00000002; // Basic authentication available.
		private const int  MD_AUTH_NT					= 0x00000004; // Windows authentication schemes available.
		// Browse flags
		private const int MD_DIRBROW_SHOW_DATE			= 0x00000002; // Show date.
		private const int MD_DIRBROW_SHOW_TIME			= 0x00000004; // Show time.
		private const int MD_DIRBROW_SHOW_SIZE			= 0x00000008; // Show file size.
		private const int MD_DIRBROW_SHOW_EXTENSION		= 0x00000010; // Show file name extension.
		private const int MD_DIRBROW_LONG_DATE			= 0x00000020; // Show full date.
		private const int MD_DIRBROW_LOADDEFAULT		= 0x40000000; // Load default page, if it exists.
		private const uint MD_DIRBROW_ENABLED			= 0x80000000;
		// Access Flags
		private const int MD_ACCESS_READ				= 0x00000001; // Allow read access.
		private const int MD_ACCESS_WRITE				= 0x00000002; // Allow write access.
		private const int MD_ACCESS_EXECUTE				= 0x00000004; // Allow file execution (includes script permission).
		private const int MD_ACCESS_SOURCE				= 0x00000010; // Allow source access.
		private const int MD_ACCESS_SCRIPT				= 0x00000200; // Allow script execution.
		private const int MD_ACCESS_NO_REMOTE_WRITE		= 0x00000400; // Local write access only.
		private const int MD_ACCESS_NO_REMOTE_READ		= 0x00001000; // Local read access only.
		private const int MD_ACCESS_NO_REMOTE_EXECUTE	= 0x00002000; // Local execution only.
		private const int MD_ACCESS_NO_REMOTE_SCRIPT	= 0x00004000; // Local host access only.

		private const string iisRootMask = "IIS://{0}/W3SVC/{1}/Root";

		public const string MagoNetDevAppPool = "MagoNetDevAppPool";

		//---------------------------------------------------------------------
		public IisManager()
		{
		}

		/// <summary>
		/// Crea una Virtual Directory sotto HTTP
		/// </summary>
		/// <example>
		///	CreateVirtualDirectory
		///		(
		///		Environment.MachineName, 
		///		1,
		///		"MyNewWeb", 
		///		new DirectoryInfo("c:\\somePath")
		///		);
		/// </example>
		/// <param name="machineName"></param>
		/// <param name="siteNumber"></param>
		/// <param name="virtDirectory"></param>
		/// <param name="physicalDir"></param>
		/// <param name="appPoolName">Application Pool name (use null for DefaultAppPool)</param>
		//---------------------------------------------------------------------
		public static DirectoryEntry CreateVirtualDirectory
			(
			string machineName,
			int siteNumber,
			string virtDirectory,
			DirectoryInfo physicalDir,
			string appPoolName,
			out string errorMessage
			)
		{
			errorMessage = String.Empty;
			// Create the FS directory if non existant
			if (!physicalDir.Exists) 
				physicalDir.Create();

			string iisRootPath = GetIisRootPath(machineName, siteNumber);

			DirectoryEntry newVirDir = null;
			try 
			{
				DirectoryEntry folderRoot = new DirectoryEntry(iisRootPath);
				folderRoot.RefreshCache();
								
				newVirDir = CreateVirtualDirectory
							(
							virtDirectory,
							physicalDir,
							folderRoot, 
							appPoolName,
							out errorMessage
							);
			}
			catch (COMException ex)
			{
				errorMessage = ex.Message;
				Debug.WriteLine("Error1 " + ex.Message);
			} 
			catch (InvalidOperationException ex)
			{
				errorMessage = ex.Message;
				Debug.WriteLine("Error2 " + ex.Message);
			}
			return newVirDir;
		}
		//---------------------------------------------------------------------
		private static DirectoryEntry CreateVirtualDirectory
			(
			string machineName,
			int siteNumber,
			string virtDirectory,
			DirectoryInfo physicalDir,
			string parentVirtualDir,
			string appPoolName,
			out string errorMessage
			)
		{
			errorMessage = String.Empty;

			// Create the FS directory if non existant
			if (!physicalDir.Exists) 
				physicalDir.Create();

			string iisRootPath = GetIisRootPath(machineName, siteNumber);

			DirectoryEntry newVirDir = null;
			try 
			{
				DirectoryEntry folderRoot = new DirectoryEntry(iisRootPath);
				folderRoot.RefreshCache();
				
				DirectoryEntry parentVirDir;
				parentVirDir = folderRoot.Children.Find(parentVirtualDir, "IIsWebVirtualDir");
				
				newVirDir = CreateVirtualDirectory
							(
							virtDirectory,
							physicalDir,
							parentVirDir,
							appPoolName,
							out errorMessage
							);
			}
			catch (COMException ex)
			{
				errorMessage = ex.Message;
				Debug.WriteLine("Error1 " + ex.Message);
			} 
			catch (InvalidOperationException ex)
			{
				errorMessage = ex.Message;
				Debug.WriteLine("Error2 " + ex.Message);
			}
			return newVirDir;
		}
		//---------------------------------------------------------------------
		public static DirectoryEntry CreateVirtualDirectory
			(
			string virtDirectory,
			DirectoryInfo physicalDir,
			DirectoryEntry parentVirDir,
			string appPoolName,
			out string errorMessage
			)
		{
			errorMessage = String.Empty;

			// Create the FS directory if non existant
			if (!physicalDir.Exists) 
				physicalDir.Create();

			DirectoryEntry newVirDir = null;
			try 
			{
				parentVirDir.RefreshCache();
				
				newVirDir = FindVirtualDirectory(parentVirDir, virtDirectory);
				if (newVirDir != null)
					return newVirDir;	// se esiste già non la creo (otterrei un'eccezione)

				newVirDir = parentVirDir.Children.Add(virtDirectory, "IIsWebVirtualDir");
				
				// Set Properties
				newVirDir.CommitChanges();
				newVirDir.Properties["Path"][0] = physicalDir.FullName;
				newVirDir.Properties["AuthFlags"][0] = MD_AUTH_ANONYMOUS | MD_AUTH_NT;
				newVirDir.Properties["EnableDefaultDoc"][0] = 1;
				newVirDir.Properties["DirBrowseFlags"][0] = MD_DIRBROW_SHOW_DATE | MD_DIRBROW_SHOW_TIME |
					MD_DIRBROW_SHOW_SIZE | MD_DIRBROW_SHOW_EXTENSION |
					MD_DIRBROW_LONG_DATE | MD_DIRBROW_LOADDEFAULT;
				newVirDir.Properties["AccessFlags"][0] = MD_ACCESS_READ |  MD_ACCESS_SCRIPT;
				//newVirDir.Properties["AccessRead"].Add(true);	// enable read access
				//newVirDir.Properties["AccessScript"].Add(true);	// enable script access

				if (appPoolName != null)
					newVirDir.Properties["AppPoolId"][0] = appPoolName;

				newVirDir.CommitChanges();
				
				// Call AppCreat2 to create a Web application (0 =In-proc, 1 = Out-proc, 2 = Pooled)
				object[] appType = new object[]{0};
				newVirDir.Invoke("AppCreate2", appType);
				
				// Save Changes
				newVirDir.CommitChanges();
				parentVirDir.CommitChanges();
				newVirDir.Close();
				parentVirDir.Close();
			}
			catch (COMException ex)
			{
				errorMessage = ex.Message;
				Debug.WriteLine("Error1 " + ex.Message);
			} 
			catch (InvalidOperationException ex)
			{
				errorMessage = ex.Message;
				Debug.WriteLine("Error2 " + ex.Message);
			}
			return newVirDir;
		}

		//---------------------------------------------------------------------
		public static void BrowseVirtualDirectories(string machineName)
		{
			BrowseVirtualDirectories(machineName, 1);
		}

		//---------------------------------------------------------------------
		public static void BrowseVirtualDirectories
			(
			string machineName,
			int siteNumber
			)
		{
			string iisRootPath = GetIisRootPath(machineName, siteNumber);

			DirectoryEntry objDE = new DirectoryEntry(iisRootPath);
			objDE.RefreshCache();

			string[] propNames
				=	{
						"Path", 
						"AppFriendlyName", 
						"AppRoot", 
						"AccessFlags", 
						"AppIsolated",
						"AuthFlags",
						"EnableDefaultDoc",
						"DirBrowseFlags",
						"AccessFlags",
						"AccessRead",
						"AccessScript"
					};
			foreach(DirectoryEntry de in objDE.Children) 
			{

				DirectoryEntry se = new DirectoryEntry(de.SchemaEntry.Path);
				if (se.Name == "IIsWebVirtualDir") 
				{
					Debug.WriteLine("=============================" );
					Debug.WriteLine("Schema entry name: " + de.SchemaClassName);
					System.DirectoryServices.PropertyCollection props = de.Properties;
					foreach (string propName in propNames)
					{
						Debug.Write(propName + " = ");
						try	{ Debug.WriteLine(props[propName][0]); } 
						catch (COMException ex) { Debug.WriteLine(ex.Message); }
					}
				}
			}
		}

		/// <summary>
		/// Trova una directory virtuale tra le figlie (di primo livello) di
		/// un'altra directory virtuale (può essere anche la root)
		/// </summary>
		/// <param name="parentVirDir"></param>
		/// <param name="virtualDirectoryName"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private static DirectoryEntry FindVirtualDirectory(DirectoryEntry parentVirDir, string virtualDirectoryName)
		{
			//foreach (DirectoryEntry virtualDirectory in parentVirDir.Children)
			//{
			//	string name = virtualDirectory.Name;
			//	string csName = virtualDirectory.SchemaClassName;
			//	if (string.Compare(name, virtualDirectoryName, true, CultureInfo.InvariantCulture) == 0 &&
			//		string.Compare(csName, "IIsWebVirtualDir", true, CultureInfo.InvariantCulture) == 0)
			//		return virtualDirectory;
			//}
			//return null;
			return FindChildDirectoryEntry(parentVirDir, virtualDirectoryName, "IIsWebVirtualDir");
			// TODO check also for "IIsWebDirectory" schema
		}

		//---------------------------------------------------------------------
		public static DirectoryEntry FindChildDirectoryEntry
			(
			DirectoryEntry parentEntry, 
			string nameToFind,
			string schemaClassNameToFind
			)
		{
			foreach (DirectoryEntry childEntry in parentEntry.Children)
			{
				if (string.Compare(childEntry.Name, nameToFind, true, CultureInfo.InvariantCulture) == 0 &&
					string.Compare(childEntry.SchemaClassName, schemaClassNameToFind, true, CultureInfo.InvariantCulture) == 0)
					return childEntry;
			}
			return null;
		}

		//---------------------------------------------------------------------
		private static string GetIisRootPath(string machineName, int siteNumber)
		{
			return string.Format(CultureInfo.InvariantCulture, iisRootMask, machineName, siteNumber);
		}
		
		//---------------------------------------------------------------------
		public static DirectoryEntry FindVirtualDirectory
			(
			string machineName,
			int siteNumber,
			string virtualName
			)
		{
			DirectoryEntry res = null;
			string iisRootPath = GetIisRootPath(machineName, siteNumber);

			try 
			{
				DirectoryEntry folderRoot = new DirectoryEntry(iisRootPath);
				res = FindVirtualDirectory(folderRoot, virtualName);
			}
			catch (Exception exc)
			{
				Debug.Assert(false, exc.Message);
			}
			return res;
		}
		/*
		public static DirectoryEntry FindVirtualDirectory
			(
			DirectoryEntry parentVirtual,
			string virtualName
			)
		{
			DirectoryEntry res = null;
			parentVirtual.RefreshCache();
			bool found = false;
			try
			{
				res = parentVirtual.Children.Find(virtualName, "IIsWebVirtualDir");
				found = true;
			}
			catch (Exception exc)
			{
			}
			
			if (!found)	// cerca tra le figlie delle directory virtuali nestate
			{
				foreach (DirectoryEntry dir in parentVirtual.Children)
				{
					res = FindVirtualDirectory(dir, virtualName);
					if (res != null)
						break;
				}
			}

			return res;
		}
		*/

		//---------------------------------------------------------------------
		public static bool RemoveVirtualDirectory(DirectoryEntry virtualDirectory)
		{
			try
			{
				// questo codice è consigliato da MSDN se l'oggetto è un ramo,
				// ma da un'eccezione, sembra x un baco del framework
//				Console.WriteLine("Deleting: {0}", virtualDirectory.Name); //<==Force a bind (workaround non funzionante!)
//				virtualDirectory.DeleteTree();	// qui da eccezione "This object cannot be deleted"
//				virtualDirectory.CommitChanges();
//				virtualDirectory.Close();

				// questo codice funzione, ma per i rami MSDN raccomanda di usare DeleteTree
				DirectoryEntry parent = virtualDirectory.Parent;
				parent.RefreshCache();
				parent.Children.Remove(virtualDirectory);
				// Save Changes
				virtualDirectory.CommitChanges();
				parent.CommitChanges();
				virtualDirectory.Close();
				parent.Close();

//				// questo codice funziona correttamente, mi sembra + sicuro
//				// del semplice parent.Children.Remove(virtualDirectory)
//				// anche se forse meno portabile
//				DirectoryEntry parent = virtualDirectory.Parent;
//				parent.Invoke("Delete", "IIsWebVirtualDir", virtualDirectory.Name);
//				parent.CommitChanges();
//				parent.Close();

				return true;
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------
		public static void RemoveChildren(DirectoryEntry parent)
		{
			Hashtable ht = new Hashtable();
			foreach (DirectoryEntry child in parent.Children) 
			{
				DirectoryEntry se = new DirectoryEntry(child.SchemaEntry.Path);
				if (se.Name != "IIsWebVirtualDir") 
					continue;

				string propName = "Path";
				System.DirectoryServices.PropertyCollection props = child.Properties;
				Debug.Write(propName + " = ");
				string path = null;
				try
				{
					path = props[propName][0] as string;
					Debug.WriteLine(props[propName][0]);
				}
				catch (COMException ex) { Debug.WriteLine(ex.Message); }

				RemoveChildren(child);

				// we cannot remove it while navigating the collection, so we store it in an helper collection
				if (path != null)
					ht[path] = child;
				// we use the path as key, so that we can retrieve it later with no expence
				// (btw, trying to retrieve it later when enumerating the helper collection
				// ends up in an exception, i don't know why)
				// note: actually two different children could point to the same location,
				// but we accept this limitation
			}

			foreach (DictionaryEntry entry in ht)
			{
				DirectoryEntry child = entry.Value as DirectoryEntry;
				string path = entry.Key as string;

				// remove child;
				parent.RefreshCache();
				parent.Children.Remove(child);
				// Save Changes
				//child.CommitChanges();
				parent.CommitChanges();
				child.Close();
				//parent.Close();

				// attempt killing the asp.net process
				try
				{
					// kill it by renaming the web.config
					string wc = Path.Combine(path, "Web.config");
					if (!File.Exists(wc))
						continue; // give up
					string wc2 = string.Concat(wc, "_");
					File.Move(wc, wc2);
					// then, set it back
					File.Move(wc2, wc);
				}
				catch (Exception ex)
				{
					Debug.Fail(ex.Message);
				}
			}
		}

		//---------------------------------------------------------------------
		private static Hashtable GetMappingsTable(DirectoryEntry virtualDirectory)
		{
			object[] mappings = GetMappings(virtualDirectory);
			Hashtable h = new Hashtable(mappings.Length);
			string[] pieces;
			string extention;
			foreach (string mapping in mappings)
			{
				pieces = mapping.Split(',');
				extention = pieces[0].ToLower(CultureInfo.InvariantCulture);
				if (!h.Contains(extention))
					h.Add(extention, mapping);
				Debug.WriteLine(mapping);
			}
			return h;
		}

		//---------------------------------------------------------------------
		private static object[] GetMappings(DirectoryEntry virtualDirectory)
		{
			return (object[])virtualDirectory.Properties["ScriptMaps"].Value;
		}

		//---------------------------------------------------------------------
		public static void AddMappings
			(
			DirectoryEntry virtualDirectory,
			string[] extensions,
			string mappingMask
			)
		{
			Hashtable h = GetMappingsTable(virtualDirectory);
			foreach (string extension in extensions)
			{
				string lowerExtension = extension.ToLower(CultureInfo.InvariantCulture);
				string aMapping = string.Format(CultureInfo.InvariantCulture, mappingMask, lowerExtension);
				h[lowerExtension] = aMapping;	// se c'è la sostituisce
			}

			object[] mappings = new object[h.Count];
			int i = 0;
			foreach (string extension in h.Keys)
				mappings[i++] = h[extension];

			virtualDirectory.Properties["ScriptMaps"].Value = mappings;
			virtualDirectory.CommitChanges();
		}

		//---------------------------------------------------------------------
		public static string GetExtensionMappingMask(string extension, DirectoryEntry virtualDirectory)
		{
			object[] mappings = GetMappings(virtualDirectory);
			Array.Sort(mappings);
			string[] pieces;
			string extensionMappingMask = string.Empty;
			foreach (string mapping in mappings)
			{
				pieces = mapping.Split(',');
				if (string.Compare(extension, pieces[0], true, CultureInfo.InvariantCulture) == 0)
				{
					extensionMappingMask = mapping;
					break;
				}
			}
			if (extensionMappingMask == string.Empty)
			{
				Debug.WriteLine("extensionMappingMask is empty string");
				return null;
			}
			extensionMappingMask = extensionMappingMask.Replace(extension, "{0}");
			return extensionMappingMask;
		}

		/// <remarks>can throw exception</remarks>
		//---------------------------------------------------------------------
		public static int GetIisMajorVersion()
		{
			using (RegistryKey rk = Registry.LocalMachine)
			{
				using (RegistryKey subKey = rk.OpenSubKey(@"SOFTWARE\Microsoft\INetStp"))
				{
					Debug.Assert(subKey != null);
					return (int)subKey.GetValue("MajorVersion");
				}
			}
		}

		//---------------------------------------------------------------------
		public static bool EnableWebServiceExtension(string extensionName) // eg. "ASP.NET v1.1.4322"
		{
			ManagementObjectSearcher searcher = null;
			ManagementObjectCollection coll = null;
			ManagementObjectCollection.ManagementObjectEnumerator en = null;
			ManagementObject mc = null;
			ManagementBaseObject inPar = null;
			ManagementBaseObject outPar = null;
			try
			{
				searcher = new ManagementObjectSearcher
					(
					"root\\MicrosoftIISv2", 
					"select * from IIsWebService where name = 'W3SVC'"
					);

				coll = searcher.Get();

				if (coll == null)
					return false;
				int count = 0; // coll.Count causes a subsequent exception in Framework1.1SP1, patched afterward
				// System.Management.ManagementException: 
				// "COM object that has been separated from its underlying RCW can not be used."
				// The exception happened when trying to enumerate the collection after having used Count,
				// probably because the unfortunate SP1 made Count disposing some underlying COM object.
				// The workaround is to simply avoid using Count
				foreach (ManagementObject mo in coll)
				{
					++count;
					if (count != 1)
					{
						Debug.Fail("Collection count should be 1");
						break;
					}
					mc = mo;
				}

				if (mc == null)
				{
					Debug.Fail("IIsWebService not found");
					return false;
				}

				inPar = mc.GetMethodParameters( "EnableWebServiceExtension" );
				Debug.Assert(inPar != null);
				inPar["Extension"] = extensionName;
				
				outPar = mc.InvokeMethod("EnableWebServiceExtension", inPar, null);
				//Debug.Assert(outPar != null); // commented: in .Net2.0 outPar is null if the method has no return value.
				//object retv = outPar["ReturnValue"]; // commented: i don't know how to consume, Properties is null and raise NullReference exception

				return true;
			}
			finally
			{
				if (searcher != null)
					searcher.Dispose();
				if (coll != null)
					coll.Dispose();
				if (en != null)
					en.Dispose();
				if (mc != null)
					mc.Dispose();
				if (inPar != null)
					inPar.Dispose();
				if (outPar != null)
					outPar.Dispose();
			}
		}

		//---------------------------------------------------------------------
		public static void SetAppPoolSafeSettings(string appPoolName)
		{
			string path = string.Format(CultureInfo.InvariantCulture, "IIS://localhost/w3svc/AppPools/{0}", appPoolName);
			if (!MyDirectoryEntryExists(path))
				return;
			DirectoryEntry appPool = new DirectoryEntry(path);
			SetAppPoolSafeSettings(appPool);
		}

		//---------------------------------------------------------------------
		private static void SetFramework(DirectoryEntry appPool)
		{
			// In case of IIS7, we want (by now) the runtime to be set to version 1.1
			int iisMajorVersion = -1;
			try
			{
				iisMajorVersion = IisManager.GetIisMajorVersion();
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message); // should never happen, no point handling the case, just give up
			}
			if (iisMajorVersion >= 7) // just for safety
			{
				SetPropertyValueIfEntryExist(appPool, "ManagedRuntimeVersion", "v4.0"); // should anyway do nothing if IIS version < 7
			}
		}

		//---------------------------------------------------------------------
		private static void SetAppPoolSafeSettings(DirectoryEntry appPool)
		{
			SetFramework(appPool);
			
			// DefaultAppPool properties:

			// Recycling - "Recycle worker processes (in minutes):"
			// PeriodicRestartTime  (default 1740)
			// When disabled, the property value is 0
			SetPropertyValueIfEntryExist(appPool, "PeriodicRestartTime", 0);

			// Recycling - "Recycle worker processes (number of requests):"
			// PeriodicRestartRequests (default 35000 disabled ==> 0)
			// When disabled (as by default), property value is 0
			SetPropertyValueIfEntryExist(appPool, "PeriodicRestartRequests", 0);

			// Recycling - "Recycle worker processes at the following times:"
			// PeriodicRestartSchedule (default: property not present)
			// When selected with no value, property is still not present in the collection;
			// when selected with a time value, it is present (eg. PeriodicRestartSchedule  15:16 (string, local time));
			// when deselected, it is present with null value.
			SetPropertyValueIfEntryExist(appPool, "PeriodicRestartSchedule", null);

			// Performance - "Shutdown worker process after being idle for (time in minutes):"
			// IdleTimeout	(default 20)
			// When disabled, the property has value 0
			SetPropertyValueIfEntryExist(appPool, "IdleTimeout", 0);

			//foreach (string myKey in appPool.Properties.PropertyNames)
			//	Console.WriteLine(myKey + "  " + appPool.Properties[myKey].Value);

			appPool.Close();
		}

		//---------------------------------------------------------------------
		private static bool SetPropertyValueIfEntryExist(DirectoryEntry entry, string propertyName, object propertyValue)
		{
			bool exists = MyPropertyCollectionContains(entry.Properties, propertyName);
			if (!exists)
				return false;
			object actualValue = entry.Properties[propertyName].Value;
			if (actualValue == null && propertyValue == null)
				return false;
			if (actualValue == null && propertyValue != null ||
				actualValue != null && propertyValue == null ||
				!actualValue.Equals(propertyValue)) // now I'm sure that actualValue != null
			{
				entry.Properties[propertyName].Value = propertyValue;
				entry.CommitChanges();
				return true;
			}
			return false;
		}

		//---------------------------------------------------------------------
		public static bool MyDirectoryEntryExists(string path)
		{
			bool exists = false;
			try // this is a workaround to a DirectoryEntry.Exists bug, which should not throw an exception, but it does!
			{
				exists = DirectoryEntry.Exists(path);
			}
			catch (COMException ex)
			{
				Debug.WriteLine(ex.Message);
			}
			return exists;
		}

		//---------------------------------------------------------------------
		private static bool MyPropertyCollectionContains(PropertyCollection propertyCollection, string propertyName)
		{
			bool contains = false;
			try // this is a workaround to a PropertyCollection.Contains bug, which should not throw an exception, but it does!
			{
				contains = propertyCollection.Contains(propertyName);
			}
			catch (COMException ex)
			{
				Debug.WriteLine(ex.Message);
			}
			return contains;
		}

		//---------------------------------------------------------------------
		public static bool CreateApplicationPoolIfNotExistent(string appPoolName)
		{
			try
			{
				DirectoryEntry appPools = new DirectoryEntry("IIS://localhost/W3SVC/AppPools");
				DirectoryEntry newAppPool = null;
				foreach (DirectoryEntry appPool in appPools.Children)
				{
					string name = appPool.Name;
					string csName = appPool.SchemaClassName;
					if (string.Compare(name, appPoolName, true, CultureInfo.InvariantCulture) == 0 &&
						string.Compare(csName, "IIsApplicationPool", true, CultureInfo.InvariantCulture) == 0)
					{
						//foreach (string myKey in appPool.Properties.PropertyNames)
						//	Console.WriteLine(myKey + "  " + appPool.Properties[myKey].Value);
						newAppPool = appPool; // app pool already exists. I want to apply safe settings anyway, to be sure
						break;
					}
				}

				if (newAppPool == null)
					newAppPool = appPools.Children.Add(appPoolName, "IIsApplicationPool");

				// Set Properties
				newAppPool.Properties["AppPoolIdentityType"][0] = 2;

				newAppPool.InvokeSet("ManagedPipelineMode", new object[] { 0 });

				newAppPool.CommitChanges();
				
				SetFramework(newAppPool);

				// Save Changes
				appPools.CommitChanges();
				newAppPool.Close();
				appPools.Close();

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		//---------------------------------------------------------------------
	}


	//=========================================================================
}
