using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microarea.Library.ReferenceManager
{
	public class DependentItemNode : IComparable
	{
		readonly IDependentItemBuilder builder;
		readonly string lookUpName;
		readonly string shortName;
		readonly bool missing;
		readonly IDependentItemCatalogue catalogue;
		Dictionary<string, DependentItemNode> references = null;

		//---------------------------------------------------------------------
		public DependentItemNode(IDependentItemBuilder builder) : this(builder, null) { }
		public DependentItemNode(IDependentItemBuilder builder, IDependentItemCatalogue catalogue)
		{
			this.builder = builder;
			this.lookUpName = builder.GetLookUpName();
			this.shortName = builder.GetShortName();
			this.missing = builder.Missing;
			this.catalogue = catalogue;
		}

		//---------------------------------------------------------------------
		public bool Missing
		{
			get { return missing; }
		}

		//---------------------------------------------------------------------
		private int depth = -1; // means not computed
		public int Depth
		{
			get { return this.depth; }
			set { this.depth = value; }
		}

		//---------------------------------------------------------------------
		public string LookUpName { get { return this.lookUpName; } }
		public string ShortName
		{
			get
			{
				return this.shortName;
			}
		}

		//---------------------------------------------------------------------
		public Dictionary<string, DependentItemNode> References
		{
			get
			{
				if (this.references == null)
					lock (this)
					{ // double check lock pattern
						if (this.references == null)
							BuildReferences();
					}
				return this.references;
			}
		}
		private void BuildReferences()
		{
			// late bound references building
			this.references = new Dictionary<string, DependentItemNode>();
			string[] refs = builder.GetReferences();
			if (refs == null)
				return;
			foreach (string refString in refs)
			{
				Debug.Assert(this.catalogue != null);
				DependentItemNode refAsm = this.catalogue.GetDependentItemNode(refString);
				Debug.Assert(refAsm != null, "missing item");
				if (refAsm != null)
					this.references[refString] = refAsm;
			}
		}

		#region overridden Object methods
		public override bool Equals(object obj)
		{
			DependentItemNode other = obj as DependentItemNode;
			if (Object.ReferenceEquals(other, null))
				return false;
			if (Object.ReferenceEquals(other, this))
				return true;
			return
				string.Compare(this.lookUpName, other.lookUpName, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		public override int GetHashCode()
		{
			return this.lookUpName.ToLower(CultureInfo.InvariantCulture).GetHashCode();
		}

		public override string ToString()
		{
			return this.lookUpName;
		}
		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			if (object.ReferenceEquals(obj, null))
				return 1;
			DependentItemNode objAsm = obj as DependentItemNode;
			if (object.ReferenceEquals(objAsm, null))
				return 1;
			return string.Compare(this.lookUpName, objAsm.lookUpName, StringComparison.InvariantCultureIgnoreCase);
		}

		#endregion
	}
}
