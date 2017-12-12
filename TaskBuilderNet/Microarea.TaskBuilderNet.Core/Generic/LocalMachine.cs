using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Management;
using System.Text;
using System.Windows.Forms;	// wrap su WMI

namespace Microarea.TaskBuilderNet.Core.Generic
{
	/// <summary>
	/// Summary description for LocalMachine.
	/// </summary>
	public class LocalMachine
	{
		//---------------------------------------------------------------------
		public LocalMachine()
		{
		}

		/// <summary>
		/// Ricava a run-time l'indirizzo MAC della prima scheda di rete trovata
		/// installata sulla macchina corrente.
		/// </summary>
		/// <returns>Mac Address</returns>
		//---------------------------------------------------------------------
		public static string GetMacAddress()
		{
			string mac = string.Empty;

			try
			{
				ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
				ManagementObjectCollection moc = mc.GetInstances();

				foreach (ManagementObject mo in moc)
				{
					if ((bool)mo["IPEnabled"] == true && mac == string.Empty)
						mac = mo["MacAddress"].ToString();
					mo.Dispose();
				}

				return mac;
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
				throw ex;
			}
		}

		//---------------------------------------------------------------------
		public static StringCollection GetMacAddresses()
		{
			StringCollection sc = new StringCollection();

			try
			{
				ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
				ManagementObjectCollection moc = mc.GetInstances();

				foreach (ManagementObject mo in moc)
				{
					if ((bool)mo["IPEnabled"] == true)
						sc.Add(mo["MacAddress"].ToString());
					mo.Dispose();
				}
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
			}

			return sc;
		}

		//---------------------------------------------------------------------
		public static string GetIPAddress()
		{
			string[] mac = null;

			try
			{
				ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
				ManagementObjectCollection moc = mc.GetInstances();

				foreach (ManagementObject mo in moc)
				{
					if ((bool)mo["IPEnabled"] == true && mac == null)
						mac = mo["IPAddress"] as string[];
					mo.Dispose();
				}
				if (mac != null && mac.Length > 0)
					return mac[0];
				return null;
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
				throw ex;
			}
		}


		//---------------------------------------------------------------------
		public static Drive[] GetCdDrives()
		{
			ArrayList list = new ArrayList();

			foreach (Drive aDrive in GetDrives())
				if (aDrive.Type == DriveType.CompactDisc)
					list.Add(aDrive);

			return (Drive[])list.ToArray(typeof(Drive));
		}

		//---------------------------------------------------------------------
		public static Drive[] GetDrives()
		{
			ArrayList list = new ArrayList();
			Drive aDrive;

			try
			{
				ManagementClass disks = new ManagementClass("Win32_LogicalDisk");
				ManagementObjectCollection moc = disks.GetInstances();

				foreach (ManagementObject mo in moc)
				{
					aDrive = Drive.GetDriveObject(mo);
					mo.Dispose();
					list.Add(aDrive);
				}

				disks.Dispose();
				return (Drive[])list.ToArray(typeof(Drive));
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
				throw ex;
			}
		}

		//---------------------------------------------------------------------
		public static Int64 GetFreeSpaceInBytes(string logicalDrive)
		{
			Int64 nRet = 0;

			String strSQL = "SELECT FreeSpace FROM Win32_LogicalDisk WHERE DeviceID='" + logicalDrive + "'" ;
			SelectQuery query = new SelectQuery(strSQL);
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

			foreach (ManagementBaseObject drive in searcher.Get())
			{
				UInt64 u = (UInt64)drive["FreeSpace"]; // Get freespace property
				nRet = (Int64)u;
			}

			return nRet; // return KB
		}

		//---------------------------------------------------------------------
		public static Drive GetDriveObject(string logicalDrive)
		{
			String strSQL = "SELECT * FROM Win32_LogicalDisk WHERE DeviceID='" + logicalDrive + "'" ;
			SelectQuery query = new SelectQuery(strSQL);
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

			foreach (ManagementObject driveMo in searcher.Get())
				return Drive.GetDriveObject(driveMo); // there can be only one

			return null;
		}

		/// <summary>
		/// Restituisce l'utente che sta eseguendo il processo identificato dall'id
		/// </summary>
		/// <param name="processId">Id del processo</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static string GetProcessOwner(int processId)
		{

			string query = "Select * From Win32_Process Where ProcessID = " + processId;
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
			ManagementObjectCollection processList = searcher.Get();

			foreach (ManagementObject obj in processList)
			{
				string[] argList = new string[] { string.Empty };
				int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
				if (returnVal == 0)
					return argList[0];
			}

			return "";

		}

		/// <summary>
		/// Ritorno in out l'ammontare di memoria libera e totale sulla macchina
		/// </summary>
		//---------------------------------------------------------------------
		public static void GetMemoryState(out double freeMemoryInMB, out double totalMemoryInMB)
		{
			freeMemoryInMB = 0;
			totalMemoryInMB = 0;

			try
			{
				ManagementScope scope = new ManagementScope("\\root\\cimv2");
				scope.Connect();

				ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
				ManagementObjectSearcher searcher = new
				ManagementObjectSearcher(scope, query);

				ManagementObjectCollection queryCollection = searcher.Get();
				foreach (ManagementObject m in queryCollection)
				{
					long l = long.Parse(m["FreePhysicalMemory"].ToString());
					freeMemoryInMB = l / 1024;
				}

				query = new ObjectQuery("SELECT * FROM Win32_ComputerSystem");
				searcher = new ManagementObjectSearcher(scope, query);
				queryCollection = searcher.Get();
				foreach (ManagementObject m in queryCollection)
				{
					long l = long.Parse(m["TotalPhysicalMemory"].ToString());
					totalMemoryInMB = l / (1024 * 1024);
				}
			}
			catch (Exception)
			{
			}
		}

        //-----------------------------------------------------------------------
        /// <summary>
        /// Torna una stringa formattata come (Name, AddressWidth, CurrentClockSpeed, NumberOfCores, NumberOfLogicalProcessors), utile per i dati statistici nel ping
        /// </summary>
        public static string GetProcessorData()
        {
            ManagementClass clsMgtClass = new ManagementClass("Win32_Processor");
            ManagementObjectCollection colMgtObjCol = clsMgtClass.GetInstances();

            StringBuilder sb = new StringBuilder();
            string processorData = "{{{0}, {1}, {2}, {3}, {4}}}";
            foreach (ManagementObject objMgtObj in colMgtObjCol)
            {
                string AddressWidth = objMgtObj.Properties["AddressWidth"].Value.ToString();
                string Name = objMgtObj.Properties["Name"].Value.ToString();
                string CurrentClockSpeed = objMgtObj.Properties["CurrentClockSpeed"].Value.ToString();
                string NumberOfCores = objMgtObj.Properties["NumberOfCores"].Value.ToString();
                string NumberOfLogicalProcessors = objMgtObj.Properties["NumberOfLogicalProcessors"].Value.ToString();

                processorData = string.Format(processorData, Name, AddressWidth, CurrentClockSpeed, NumberOfCores, NumberOfLogicalProcessors);
                sb.Append(processorData);
            }
            return sb.ToString();
        }

        //-----------------------------------------------------------------------
        /// <summary>
        /// Torna una stringa formattata come (location,risoluzione), utile per i dati statistici nel ping
        /// </summary>
        public static string GetScreenData()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Screen s in Screen.AllScreens)
            {
                if (s == null) continue;

                string screenVal = String.Format("{{{0}}}", s.Bounds.ToString());
                screenVal = screenVal.Replace("{{", "{");
                screenVal = screenVal.Replace("}}", "}");
                sb.Append(screenVal);
            }

            return sb.ToString();
        }


        //-----------------------------------------------------------------------
        /// <summary>
        /// Torna una stringa formattata come (tipoRam, dimensione), utile per i dati statistici nel ping
        /// </summary>
        public static string GetRAMData()
        {
            var searcher = new ManagementObjectSearcher("Select * from Win32_PhysicalMemory");
            if (searcher == null) return string.Empty;

            int type = 0;
            StringBuilder sb = new StringBuilder();
            foreach (ManagementObject obj in searcher.Get())
            {
                if (obj == null) continue;
                object o1 = obj.GetPropertyValue("MemoryType");

                type = Int32.Parse(o1.ToString());
                string ramCapacity = obj.Properties["Capacity"].Value.ToString();

                string ramType = "PhysicalMemory";
                switch (type)
                {
                    case 20:
                        ramType = "DDR";
                        break;
                    case 21:
                        ramType = "DDR-2";
                        break;
                    case 17:
                        ramType = "SDRAM";
                        break;
                    default:
                        if (type == 0 || type > 22)
                            ramType = "DDR-3";
                        break;
                }
                string ram = "{{{0}, {1}}}";
                ram = string.Format(ram, ramType, ramCapacity);
                sb.Append(ram);
            }

            return sb.ToString();

        }
	}

	//=========================================================================
	public enum DriveType
	{
		Unknown = 0, 
		NoRootDirectory = 1, 
		RemovableDisk = 2, 
		LocalDisk = 3, 
		NetworkDrive = 4, 
		CompactDisc = 5, 
		RamDisk = 6
	}

	//=========================================================================
	public class Drive
	{
		private DriveType type;
		private string name;
		private string deviceID;
		private string description;
		private string fileSystem;

		public DriveType Type     { get { return this.type; } }
		public string Name        { get { return this.name; } }
		public string DeviceID    { get { return this.deviceID; } }
		public string Description { get { return this.description; } }
		public string FileSystem  { get { return this.fileSystem; } }

		private Drive(){} // I want to force using factory method
		
		//---------------------------------------------------------------------
		public static Drive GetDriveObject(ManagementObject mo) // factory method
		{
			//foreach (PropertyData prop in mo.Properties)
			//	Debug.WriteLine(prop.Name);

			Drive aDrive = new Drive();
			aDrive.type = (DriveType)((UInt32)mo["DriveType"]);
			aDrive.name = mo["Name"] as string;
			aDrive.deviceID = mo["DeviceID"] as string;
			aDrive.description = mo["Description"] as string;
			aDrive.fileSystem = mo["FileSystem"] as string; // property can be null

			return aDrive;
		}

	}

	//=========================================================================
	public class RemoteMachine	// TEMP - codice ridondato temporaneo in attesa di derivare LocalMachine da RemoteMachine
	{
		//---------------------------------------------------------------------
		public static Drive[] GetDrives(string serverName)
		{
			ArrayList list = new ArrayList();
			Drive aDrive;

			try
			{
				ManagementPath path = new ManagementPath();
				path.Server = serverName;
				path.NamespacePath = @"root\CIMV2";
				path.RelativePath = @"Win32_LogicalDisk";

				ManagementClass disks = new ManagementClass(path, new ObjectGetOptions(null, new TimeSpan(0,0,0,2), true));
				ManagementObjectCollection moc = disks.GetInstances();

				foreach (ManagementObject mo in moc)
				{
					aDrive = Drive.GetDriveObject(mo);
					mo.Dispose();
					list.Add(aDrive);
				}

				disks.Dispose();
				return (Drive[])list.ToArray(typeof(Drive));
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
				throw ex;
			}
		}
	}
}