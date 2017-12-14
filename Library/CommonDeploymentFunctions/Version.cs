using System;
using System.Collections;
using System.Globalization;

namespace Microarea.Library.CommonDeploymentFunctions
{
	/// <summary>
	/// Permette di istanziare degli oggetti Version e confrontarli.
	/// Utilizzata per gestire le versioni delle UniversalImages.
	/// </summary>
	/// <remarks>
	/// La classe è nata perché inizialmente si prevedeva di identificare
	/// ogni file identificato nei manifest con una release puntata.
	/// La Base Class Library mette già a disposizione una classe Version
	/// con funzionalità molto simili, utilizzata per il versioning degli
	/// assembly ma all'epoca non sapevo ancora quanti numeri potessero 
	/// fare parte della versione e la classe del framework sembrava limitante.
	/// Questa implementazione, molto simile a System.Version, cui è
	/// spudoratamente ispirata, permette di gestire e comparare istanze
	/// costituite ognuna da un qualunque numero di numeri di release.
	/// </remarks>
	[Serializable]
	public class Version : ICloneable, IComparable 
	{
		protected uint[] version;

		#region Constructors
		//---------------------------------------------------------------------
		public Version(uint[] ver)
		{
			version = ver;
		}
                        
		//---------------------------------------------------------------------
		public Version(string strVersion)
		{
			int n;
			string[] vals;
                        
			if (strVersion == null) 
				throw new ArgumentNullException("strVersion");

			vals = strVersion.Split('.');
			n = vals.Length;

			ArrayList al = new ArrayList();
			foreach (string s in vals)
			{
				try		{	al.Add(uint.Parse(s));	}
				catch	{	al.Add((uint)0);		}
			}
			version = (uint[])al.ToArray(typeof(uint));
		}
		#endregion

		#region Properties
		//--------------------------- Proprietà -------------------------------
		public uint[] Values	{	get	{	return version;		}	}
		public uint Major		{	get {	return version[0];	}	}
		public uint Minor 		{	get {	return (version.Length > 1) ? version[1] : 0;	}	}
		public uint Build 		{	get {	return (version.Length > 2) ? version[2] : 0;	}	}
		public uint Revision 	{	get {	return (version.Length > 3) ? version[3] : 0;	}	}
		#endregion

		#region Interfaces required methods
		//---------------------------------------------------------------------
		public object Clone()
		{
			return new Version(version);
		}

		//---------------------------------------------------------------------
		public int CompareTo(object verObj)
		{
			if (verObj == null)
				throw new ArgumentNullException("version");
			if (!(verObj is Version))
				throw new ArgumentException("Argument to Version.CompareTo must be a Version");

			Version comp = verObj as Version;

			int thisCount = this.version.Length;
			int compCount = comp.version.Length;
			int max = (thisCount > compCount) ? thisCount : compCount;

			uint thisValue;
			uint compValue;

			for (int i = 0; i < max; i++)
			{
				thisValue = GetValueInArray(this.version, i);
				compValue = GetValueInArray(comp.version, i);
				if (thisValue > compValue)
					return 1;
				else if (thisValue < compValue)
					return -1;
			}
			
			return 0;
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is Version))
				return false;

			Version comp = obj as Version;

			int thisCount = this.version.Length;
			int compCount = comp.version.Length;
			int max = (thisCount > compCount) ? thisCount : compCount;

			uint thisValue;
			uint compValue;

			for (int i = 0; i < max; i++)
			{
				thisValue = GetValueInArray(this.version, i);
				compValue = GetValueInArray(comp.version, i);
				if (thisValue != compValue)
					return false;
			}
			return true;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return version.ToString().GetHashCode();
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			if (version.Length == 1)
				return version[0].ToString(CultureInfo.InvariantCulture);
			
			string res = string.Empty;
			int i;
			for (i = 0; i < version.Length - 1; i++)
				res += version[i] + ".";
			res += version[i];

			return res;
		}
		#endregion

		#region Helper functions

		//---------------------------------------------------------------------
		private uint GetValueInArray(uint[] array, int pos)
		{
			return (array.Length > pos) ? array[pos] : 0;
		}
		#endregion

		#region Operators overloads
		//----------------------- Overloading degli operatori -----------------
		public static bool operator == (Version v1, Version v2) 
		{
			if (Object.ReferenceEquals(null, v1) &&  Object.ReferenceEquals(null, v2))
				return true;
			if (Object.ReferenceEquals(null, v1) ||  Object.ReferenceEquals(null, v2))
				return false;
			return v1.Equals(v2);
		}

		public static bool operator != (Version v1, Version v2)
		{
			if (Object.ReferenceEquals(null, v1) &&  Object.ReferenceEquals(null, v2))
				return false;
			if (Object.ReferenceEquals(null, v1) ||  Object.ReferenceEquals(null, v2))
				return true;
			return !v1.Equals(v2);
		}

		public static bool operator > (Version v1, Version v2)
		{
			return v1.CompareTo(v2) > 0;
		}

		public static bool operator >= (Version v1, Version v2)
		{
			return v1.CompareTo(v2) >= 0;
		}

		public static bool operator < (Version v1, Version v2)
		{
			return v1.CompareTo(v2) < 0;
		}

		public static bool operator <= (Version v1, Version v2)
		{
			return v1.CompareTo(v2) <= 0;
		}

		public static implicit operator String(Version value)
		{
			return value.ToString();
		}

		public static implicit operator Version(String value)
		{
			return new Version(value);
		}
		#endregion


		/// <summary>
		/// Ordina un elenco di releases
		/// </summary>
		/// <param name="releases">elenco releases in formato stringa</param>
		/// <returns>elenco ordinato</returns>
		//---------------------------------------------------------------------
		public static Version[] GetSortedReleases(string[] releases)
		{
			if (releases.Length == 0)
				return new Version[] {};
			ArrayList relList = new ArrayList();
			foreach (string rel in releases)
				relList.Add(new Version(rel));
			relList.Sort();
			return (Version[])relList.ToArray(typeof(Version));
		}

		/// <summary>
		/// Elimina dalla lista degli aggiornamenti disponibili quello indicato e gli inferiori
		/// </summary>
		/// <param name="releases">lista delle releases degli aggiornamenti disponibili</param>
		/// <param name="release">release da eliminare dalla lista</param>
		/// <returns>lista aggiornata</returns>
		//---------------------------------------------------------------------
		public static string[] DropLowerFromAvailableUpdates(string[] releases, string release)
		{
			if (releases == null || releases.Length == 0)
				return releases;

			Version refRelease = new Version(release);
			Version aRelease;

			ArrayList relList = new ArrayList();
			foreach (string rel in releases)
			{
				aRelease = new Version(rel);
				if (aRelease > refRelease)
					relList.Add(aRelease);
			}
			relList.Sort();

			ArrayList strList = new ArrayList();
			foreach (Version sRelease in relList)
				strList.Add(sRelease.ToString());

			return (string[])strList.ToArray(typeof(string));
		}

		//---------------------------------------------------------------------
	}
}
