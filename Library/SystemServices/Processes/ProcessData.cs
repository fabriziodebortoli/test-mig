using System;

namespace Microarea.Library.SystemServices.Processes
{
	// holds the process data
	public class ProcessData : IComparable
	{
		public uint ID;
		public string Name;
		public string FullPath;
		public int Index;

		public ProcessData(uint ID, string Name, string fullPath)
		{
			this.ID = ID;
			this.Name = Name;
			this.FullPath = fullPath;
		}

		#region IComparable Members

		public int CompareTo(object obj)
		{
			uint objID = ((ProcessData)obj).ID;
			return this.ID.CompareTo(objID);
		}

		#endregion
	}
}
