using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Microarea.Library.CommonDeploymentFunctions
{
	[Serializable]
	public class ReleaseVersion : IComparable
	{
		protected readonly int major;
		protected readonly int minor;
		protected readonly int sp;
		protected readonly int dateInt;
		protected readonly string sortableDate;
		protected readonly string toString;

		public ReleaseVersion(string releaseString)
		{
			if (releaseString == null)
				throw new ArgumentNullException();

			string[] vals = releaseString.Split('.');
			if (vals.Length != 4)
				throw new ArgumentException();

			if (!Int32.TryParse(vals[0], out this.major))
				throw new ArgumentException();
			if (!Int32.TryParse(vals[1], out this.minor))
				throw new ArgumentException();
			if (!Int32.TryParse(vals[2], out this.sp))
				throw new ArgumentException();
			if (!Int32.TryParse(vals[3], out this.dateInt))
				throw new ArgumentException();

			this.sortableDate = vals[3];
			this.toString = string.Concat(
				this.major.ToString(CultureInfo.InvariantCulture), '.',
				this.minor.ToString(CultureInfo.InvariantCulture), '.',
				this.sp.ToString(CultureInfo.InvariantCulture), '.',
				this.sortableDate);
		}

		public int Major { get { return this.major; } }
		public int Minor { get { return this.minor; } }
		public int SP { get { return this.sp; } }
		public string SortableDate { get { return this.sortableDate; } }

		#region IComparable Members

		public int CompareTo(object obj)
		{
			if (object.ReferenceEquals(obj, this))
				return 0;
			if (object.ReferenceEquals(obj, null))
				return 1;
			ReleaseVersion rv = obj as ReleaseVersion;
			if (object.ReferenceEquals(rv, null))
				throw new ArgumentException();

			// Less than zero    | The current Version object is a version before version. 
			// Zero              | The current Version object is the same version as version. 
			// Greater than zero | The current Version object is a version subsequent to version.
			//                     -or- version is a null reference

			if (this.major > rv.major)
				return 1;
			if (this.major == rv.major)
			{
				if (this.minor > rv.minor)
					return 1;
				if (this.minor == rv.minor)
				{
					//return this.sp.CompareTo(rv.sp);
					int comp = this.sp.CompareTo(rv.sp);
					if (comp == 0)
						return this.sortableDate.CompareTo(rv.sortableDate);
					return comp;
				}

				return this.sortableDate.CompareTo(rv.sortableDate);
				//return -1;
			}
			else
				return -1;
		}

		#endregion

		#region overridden object methods

		public override string ToString()
		{
			return this.toString;
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(obj, null))
				return false;
			ReleaseVersion rv = obj as ReleaseVersion;
			if (object.ReferenceEquals(rv, null))
				return false;

			if (this.toString == rv.toString)
				return true;

			return false;
		}

		public override int GetHashCode()
		{
			return this.toString.GetHashCode();
		}

		#endregion

		#region Operators overloads

		public static bool operator ==(ReleaseVersion v1, ReleaseVersion v2)
		{
			bool v1IsNull = Object.ReferenceEquals(null, v1);
			bool v2IsNull = Object.ReferenceEquals(null, v2);
			if (v1IsNull && v2IsNull)
				return true;
			if (v1IsNull || v2IsNull)
				return false;
			return v1.Equals(v2);
		}

		public static bool operator !=(ReleaseVersion v1, ReleaseVersion v2)
		{
			bool v1IsNull = Object.ReferenceEquals(null, v1);
			bool v2IsNull = Object.ReferenceEquals(null, v2);
			if (v1IsNull && v2IsNull)
				return false;
			if (v1IsNull || v2IsNull)
				return true;
			return !v1.Equals(v2);
		}

		public static bool operator >(ReleaseVersion v1, ReleaseVersion v2)
		{
			if (Object.ReferenceEquals(null, v1))
				return false;
			return v1.CompareTo(v2) > 0;
		}

		public static bool operator >=(ReleaseVersion v1, ReleaseVersion v2)
		{
			if (Object.ReferenceEquals(null, v1))
			{
				if (!Object.ReferenceEquals(null, v2))
					return false;
				else
					return true;
			}
			return v1.CompareTo(v2) >= 0;
		}

		public static bool operator <(ReleaseVersion v1, ReleaseVersion v2)
		{
			if (Object.ReferenceEquals(null, v1))
			{
				if (!Object.ReferenceEquals(null, v2))
					return true;
				else
					return false;
			}
			return v1.CompareTo(v2) < 0;
		}

		public static bool operator <=(ReleaseVersion v1, ReleaseVersion v2)
		{
			if (Object.ReferenceEquals(null, v1))
				return true;
			return v1.CompareTo(v2) <= 0;
		}

		#endregion

		/// <summary>
		/// Sorts a releases list
		/// </summary>
		/// <param name="releases">release list in string format</param>
		/// <returns>the sorted list</returns>
		public static ReleaseVersion[] GetSortedReleases(string[] releases)
		{
			if (releases.Length == 0)
				return new ReleaseVersion[] { };
			List<ReleaseVersion> relList = new List<ReleaseVersion>();
			foreach (string rel in releases)
				relList.Add(new ReleaseVersion(rel));
			relList.Sort();
			return relList.ToArray();
		}

		/// <summary>
		/// Drop from the given list of available updates the indicated one and the lower ones.
		/// </summary>
		/// <param name="releases">list of available updates, in the form of release strings</param>
		/// <param name="release">release to drop from the list</param>
		/// <returns>updated list</returns>
		public static string[] DropLowerFromAvailableUpdates(string[] releases, string release)
		{
			if (releases == null || releases.Length == 0)
				return releases;

			ReleaseVersion refRelease = new ReleaseVersion(release);
			ReleaseVersion aRelease;

			List<ReleaseVersion> relList = new List<ReleaseVersion>();
			foreach (string rel in releases)
			{
				aRelease = new ReleaseVersion(rel);
				if (aRelease > refRelease)
					relList.Add(aRelease);
			}
			relList.Sort();

			List<string> strList = new List<string>();
			foreach (ReleaseVersion sRelease in relList)
				strList.Add(sRelease.ToString());

			return strList.ToArray();
		}
	}
}
