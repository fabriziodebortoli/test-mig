//using System;
//using System.Collections;
using System.Collections.Generic;

namespace Microarea.Library.ReferenceManager
{
	//=========================================================================
	public interface IDependentItemCatalogue : IEnumerable<DependentItemNode>
	{
		DependentItemNode GetDependentItemNode(string lookUpName);
	}

	//=========================================================================
	public class DependentItemCatalogue : IDependentItemCatalogue, IEnumerable<DependentItemNode>
	{
		protected Dictionary<string, DependentItemNode> items = new Dictionary<string, DependentItemNode>();

		public void SetDependentItemNode(IDependentItemBuilder itemBuilder)
		{
			DependentItemNode itemNode = new DependentItemNode(itemBuilder, this);
			this.items[itemNode.LookUpName] = itemNode;
		}

		#region IDependentItemCatalogue Members

		DependentItemNode IDependentItemCatalogue.GetDependentItemNode(string lookUpName)
		{
			DependentItemNode itemNode;
			items.TryGetValue(lookUpName, out itemNode);
			if (itemNode == null)
			{
				itemNode = new DependentItemNode(new MissingDependentItemBuilder(lookUpName));
				//this.items[itemNode.LookUpName] = itemNode;
			}
			return itemNode;
		}

		#endregion

		#region IEnumerable<DependentItemNode> Members

		IEnumerator<DependentItemNode> IEnumerable<DependentItemNode>.GetEnumerator()
		{
			return this.items.Values.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.items.Values.GetEnumerator();
		}

		#endregion
	}
}
