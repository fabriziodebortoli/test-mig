using System;
using System.Collections.Generic;
using System.Text;

namespace Aga.Controls.Tree
{
	public class TreeModelEventArgs: TreePathEventArgs
	{
		private object[] _children;
		public object[] Children
		{
			get { return _children; }
		}

		private int[] _indices;
		public int[] Indices
		{
			get { return _indices; }
		}

        private string _changedProperty;
        public string NodeChangedProperty
        {
            get 
            {
                return _changedProperty;
            }

            private set 
            {
                _changedProperty = value;
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parent">Path to a parent node</param>
		/// <param name="children">Child nodes</param>
		public TreeModelEventArgs(TreePath parent, object[] children)
			: this(parent, null, children)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parent">Path to a parent node</param>
		/// <param name="indices">Indices of children in parent nodes collection</param>
		/// <param name="children">Child nodes</param>
		public TreeModelEventArgs(TreePath parent, int[] indices, object[] children)
			: this(parent, indices, children, string.Empty)
		{
			if (children == null)
				throw new ArgumentNullException();

			if (indices != null && indices.Length != children.Length)
				throw new ArgumentException("indices and children arrays must have the same length");

			_indices = indices;
			_children = children;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">Path to a parent node</param>
        /// <param name="indices">Indices of children in parent nodes collection</param>
        /// <param name="children">Child nodes</param>
        /// <param name="nodeChangedProperty">Changed Property Name of current Node</param>
        public TreeModelEventArgs(TreePath parent, int[] indices, object[] children, string nodeChangedProperty)
            : base(parent)
        {
            if (children == null)
                throw new ArgumentNullException();

            if (indices != null && indices.Length != children.Length)
                throw new ArgumentException("indices and children arrays must have the same length");

            _indices = indices;
            _children = children;
            NodeChangedProperty = nodeChangedProperty;
        }
	}
}
