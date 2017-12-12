using System;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyBuilder
{
	//================================================================================
	interface IDirtyManager
	{
		//-----------------------------------------------------------------------------
		void SetDirty(bool dirty);
		/// <summary>
		/// Occurs when a modification is made by the user to signal that it has to be saved.
		/// </summary>
		event EventHandler<DirtyChangedEventArgs> DirtyChanged;

		bool IsDirty { get;}
		bool SuspendDirtyChanges { get; set; }
	}

	/// <summary>
	/// Provides data for the DirtyChanged event.
	/// </summary>
	//================================================================================
	public class DirtyChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the DirtyChangedEventArgs.
		/// </summary>
		/// <seealso cref="Microarea.TaskBuilderNet.Core.Generic.NameSpace"/>
		//--------------------------------------------------------------------------------
		public DirtyChangedEventArgs(bool dirty, NameSpace nameSpace = null)
		{
			this.Dirty = dirty;

			if (nameSpace == null)
				nameSpace = NameSpace.Empty;

			this.NameSpace = nameSpace;
		}

		/// <summary>
		/// Gets or sets the bool value indicating if a document has changed and need to
		/// be saved.
		/// </summary>
		//--------------------------------------------------------------------------------
		public bool Dirty { get; set; }

		/// <summary>
		/// Gets or sets the bool value indicating the document namepsace.
		/// </summary>
		//--------------------------------------------------------------------------------
		public NameSpace NameSpace { get; set; }
	}
}
