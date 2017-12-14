//using System;

namespace Microarea.Library.ReferenceManager
{
	//=========================================================================
	public interface IDependentItemBuilder
	{
		string GetLookUpName();
		string GetShortName();
		string[] GetReferences();
		bool Missing { get; }
	}

	//=========================================================================
	public class MissingDependentItemBuilder : MockDependentItemBuilder, IDependentItemBuilder
	{
		public MissingDependentItemBuilder(string lookUpName) : base(lookUpName)
		{
			this.missing = true;
		}
	}

	//=========================================================================
	public class MockDependentItemBuilder : IDependentItemBuilder
	{
		readonly string lookUpName;
		readonly string[] references;
		protected bool missing;
		public MockDependentItemBuilder(string lookUpName) : this(lookUpName, null) { }
		public MockDependentItemBuilder(string lookUpName, string[] references)
		{
			this.lookUpName = lookUpName;
			this.references = references;
			this.missing = false;
		}

		#region IDependentItemBuilder Members

		string IDependentItemBuilder.GetLookUpName()
		{
			return this.lookUpName;
		}

		string IDependentItemBuilder.GetShortName()
		{
			return this.lookUpName;
		}

		string[] IDependentItemBuilder.GetReferences()
		{
			return this.references;
		}

		bool IDependentItemBuilder.Missing { get { return this.missing; } }

		#endregion
	}
}
