using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microarea.Library.ReferenceManager
{
	public class ReferenceGraph : IEnumerable<DependentItemNode>
	{
		private IDependentItemCatalogue catalogue;

		public ReferenceGraph(IDependentItemCatalogue catalogue, string rootLookUpName)
		{
			this.catalogue = catalogue;
			this.root = catalogue.GetDependentItemNode(rootLookUpName);
		}

		private DependentItemNode root;
		public DependentItemNode Root { get { return this.root; } }

		public int ComputeNodeDepth(DependentItemNode node)
		{
			if (node.Depth != -1) // already computed
				return node.Depth;

			if (node == root)
			{
				root.Depth = 0;
				return 0; // no recursion is possible
			}
			node.Depth = ComputeDepthNoRecursion(node);
			//int lDepth = ComputeDepthWithIteration(node);
			//Debug.Assert(node.Depth == lDepth);
			return nodeDepth;
		}

		int nodeDepth = -1; // for workbench only (not thread safe); not passed as ref for performance reasons
		private int ComputeDepthNoRecursion(DependentItemNode nodeToFind)
		{
			this.nodeDepth = 0;
			ComputeDepth(root, nodeToFind, this.nodeDepth);
			return this.nodeDepth;
		}
		private void ComputeDepth(DependentItemNode currentNode, DependentItemNode nodeToFind, int currentDepth)
		{
			++currentDepth;
			foreach (DependentItemNode child in currentNode.References.Values)
			{
				if (child == nodeToFind)//(object.ReferenceEquals(child, nodeToFind))
				{
					if (currentDepth > this.nodeDepth)
						this.nodeDepth = currentDepth;
					continue; // no point recursing looking for it again below itself
				}
				if (child.References.Count == 0)
					continue; // shortcut, no point making a recursion here
				ComputeDepth(child, nodeToFind, currentDepth);
			}
		}

		/*
		// TODO replace recursion with iteration
		private int ComputeDepthWithIteration(DependentItemNode nodeToFind)
		{
			int nodeToFindDepth = 1;
			Stack<PseudoRecursionElement> stack = new Stack<PseudoRecursionElement>();
			stack.Push(new PseudoRecursionElement(this.root, 0));
			PseudoRecursionElement currentElement = stack.Peek();

			List<DependentItemNode> list = new List<DependentItemNode>();
			bool invalidateList = true;
			while (stack.Count != 0) // no more references to add
			{
				Debug.Assert(currentElement != null);

				if (invalidateList)
				{
					list.Clear();
					list.AddRange(currentElement.ItemNode.References.Values);
				}
				PseudoRecursionElement prevElement = currentElement;
				
				int iCurr = currentElement.LastVisitedRefIndex + 1;
				for (int i = iCurr; i < list.Count; ++i)
				{
					currentElement.LastVisitedRefIndex = i + 1;
					DependentItemNode currentNode = list[i];
					if (currentNode == nodeToFind)
					{
						if (currentElement.IterationDepth + 1 > nodeToFindDepth)
							nodeToFindDepth = currentElement.IterationDepth + 1;
						//Debug.WriteLine("Skipped (" + currentNode + ") as equal");
						continue; // no point looking for it again below itself
					}
					if (currentNode.References.Count == 0)
					{
						//Debug.WriteLine("Skipped " + currentNode + " as with no references");
						continue;
					}
					currentElement.LastVisitedRefIndex = i; // correction, so it will be processed again
					currentElement = new PseudoRecursionElement(currentNode, currentElement.IterationDepth + 1);
					stack.Push(currentElement);
					invalidateList = true;
					//Debug.WriteLine("Pushed " + currentElement);
					break;
				}

				while (stack.Count != 0)
				{
					currentElement = stack.Peek();
					if (currentElement.LastVisitedRefIndex < currentElement.ItemNode.References.Count - 1)
						break;
					currentElement = stack.Pop(); // throw away this one, we are done with it
					//Debug.WriteLine("Popped " + currentElement);
				}

				invalidateList = prevElement != currentElement;
			}

			return nodeToFindDepth;
		}

		private class PseudoRecursionElement
		{
			DependentItemNode itemNode;
			int iterationDepth;
			int lastVisitedRefIndex;
			public PseudoRecursionElement(DependentItemNode itemNode, int iterationDepth)
			{
				this.itemNode = itemNode;
				this.iterationDepth = iterationDepth;
				this.lastVisitedRefIndex = -1;
			}
			public DependentItemNode ItemNode { get { return this.itemNode; } }
			public int IterationDepth { get { return this.iterationDepth; } }
			public int LastVisitedRefIndex
			{
				get { return this.lastVisitedRefIndex; }
				set { this.lastVisitedRefIndex = value; }
			}

			public override string ToString()
			{
				return this.itemNode.ToString() + " - iteration: " 
					+ this.iterationDepth.ToString()
					+ " - lastVisitedRefIndex: " + this.lastVisitedRefIndex.ToString()
					+ " - n. refs.: " + this.itemNode.References.Count.ToString();
			}
		}
		*/

		public Dictionary<int, List<DependentItemNode>> ComputeDepthsAndWidths()
		{
			Dictionary<int, List<DependentItemNode>> dic = new Dictionary<int, List<DependentItemNode>>();
			foreach (DependentItemNode itemNode in this)
			{
				int depth = ComputeNodeDepth(itemNode);
				List<DependentItemNode> list;
				dic.TryGetValue(depth, out list);
				if (list == null)
				{
					list = new List<DependentItemNode>();
					dic[depth] = list;
				}
				list.Add(itemNode);
			}
			return dic;
		}

		#region IEnumerable<DependentItemNode> Members

		public IEnumerator<DependentItemNode> GetEnumerator()
		{
			return new ReferenceGraphEnumerator(this);
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new ReferenceGraphEnumerator(this);
		}

		#endregion
	}

	public class ReferenceGraphEnumerator : IEnumerator, IEnumerator<DependentItemNode>
	{
		private List<DependentItemNode> list = new List<DependentItemNode>();
		private int position = -1;

		public ReferenceGraphEnumerator(ReferenceGraph graph)
		{
			lock (graph)
				Fill(graph.Root);
		}

		private void Fill(DependentItemNode itemNode)
		{
			list.Add(itemNode);
			foreach (DependentItemNode refItem in itemNode.References.Values)
				if (!list.Contains(refItem))
					Fill(refItem);
		}

		#region IEnumerator Members

		object IEnumerator.Current
		{
			get { return this.Current; }
		}

		public bool MoveNext()
		{
			position++;
			return position < list.Count;
		}

		public void Reset()
		{
			throw new NotSupportedException(); // no need to implement it: no COM interop.
		}

		#endregion

		#region IEnumerator<DependentItemNode> Members

		public DependentItemNode Current
		{
			get
			{
				try
				{
					return list[position];
				}
				catch (ArgumentOutOfRangeException)
				{
					throw new InvalidOperationException();
				}
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			;
		}

		#endregion
	}
}
