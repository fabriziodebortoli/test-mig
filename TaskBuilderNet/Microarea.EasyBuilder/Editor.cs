using System;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.Localization;
using Microarea.EasyBuilder.UI;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.EasyBuilder
{
	//=========================================================================
	/// <summary>
	/// Base class for all editors.
	/// </summary>
	public class Editor : Control, IDirtyManager, IDesignerEventService, IServiceProvider
	{
		/// <summary>
		/// Occurs when a save operation is requested by the user.
		/// </summary>
		//-----------------------------------------------------------------------------
		public event EventHandler RequestedSave;

		/// <summary>
		/// Occurs when the user request to open the Options window.
		/// </summary>
		//-----------------------------------------------------------------------------
		public event EventHandler RequestedOptions;

		/// <summary>
		///  Occurs when the user request to open the Object model treeview window.
		/// </summary>
		//-----------------------------------------------------------------------------
		public event EventHandler RequestedObjectModel;

		/// <summary>
		/// Occurs when a component has been deleted
		/// </summary>
		//-----------------------------------------------------------------------------
		public event EventHandler<DeleteObjectEventArgs> ComponentDeleted;

		//-----------------------------------------------------------------------------
		internal event EventHandler<CodeMethodEditorEventArgs> OpenCodeEditor;

		/// <remarks />
		public event EventHandler<DirtyChangedEventArgs> DirtyChanged;
		bool isDirty;
		bool suspendDirtyChanges;

		IUIService uiService;
		ITBComponentChangeService compChangeSvc = new TBComponentChangeService();
		ISelectionService selectionService = new EasyBuilderSelectionService();

		ComponentDeclarator componentDeclarator;

		private LocalizationManager localizationManager;
		private EventsManager eventsManager;

		///<remarks />
		//-----------------------------------------------------------------------------
		protected LocalizationManager LocalizationManager
		{
			get { return localizationManager; }
			set { localizationManager = value; }
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		protected EventsManager EventsManager
		{
			get { return eventsManager; }
			set { eventsManager = value; }
		}


		///<remarks />
		//-----------------------------------------------------------------------------
		protected virtual void OnComponentDeleted(DeleteObjectEventArgs e)
		{
			if (ComponentDeleted != null)
				ComponentDeleted(this, e);
		}

		///<remarks />
		//---------------------------------------------------------------------
		protected virtual void OnRequestedObjectModel(EventArgs args)
		{
			if (RequestedObjectModel != null)
				RequestedObjectModel(this, args);
		}
		///<remarks />
		//---------------------------------------------------------------------
		protected virtual void OnRequestedSave(EventArgs args)
		{
			if (RequestedSave != null)
				RequestedSave(this, args);
		}
		///<remarks />
		//---------------------------------------------------------------------
		protected virtual void OnRequestedOptions(EventArgs args)
		{
			if (RequestedOptions != null)
				RequestedOptions(this, args);
		}

		//-----------------------------------------------------------------------------
		internal virtual bool CanUpdateObjectModel
		{
			get { return true; }
		}

		//-----------------------------------------------------------------------------
        /// <summary>
        /// business objects declarator 
        /// </summary>
		public ComponentDeclarator ComponentDeclarator { get { return componentDeclarator; } set { componentDeclarator = value; } }

		//---------------------------------------------------------------------
		internal ISelectionService SelectionService
		{
			get { return selectionService; }
		}

		//---------------------------------------------------------------------
		internal ITBComponentChangeService ComponentChangeService
		{
			get { return compChangeSvc; }
		}

		///<remarks />
		//---------------------------------------------------------------------
		protected Editor()
		{
			uiService = new FormEditorUIService(this);
		}

		///<remarks />
		//---------------------------------------------------------------------
		public virtual void SetDirty(bool dirty){}

		///<remarks />
		//-----------------------------------------------------------------------------
		protected virtual void OnDirtyChanged(DirtyChangedEventArgs e)
		{
			if (DirtyChanged != null)
				DirtyChanged(this, e);
		}

		///<remarks />
		//---------------------------------------------------------------------
		public bool IsDirty
		{
			get { return this.isDirty; }
			protected set { this.isDirty = value; }
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Suspends dirty changes
		/// </summary>
		public bool SuspendDirtyChanges
		{
			get
			{
				return this.suspendDirtyChanges;
			}
			set
			{
				this.suspendDirtyChanges = value;
			}
		}

		#region IDesignerEventService Members

		/// <summary>
		/// Internal use.
		/// </summary>
		//-----------------------------------------------------------------------------
		public IDesignerHost ActiveDesigner
		{
			get { return null; }
		}

		/// <summary>
		/// Internal use.
		/// </summary>
		//-----------------------------------------------------------------------------
		public event ActiveDesignerEventHandler ActiveDesignerChanged;

		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		protected virtual void OnActiveDesignerChanged(ActiveDesignerEventArgs e)
		{
			if (ActiveDesignerChanged != null)
				ActiveDesignerChanged(this, e);
		}

		/// <summary>
		/// Internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public event DesignerEventHandler DesignerCreated;

		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		protected virtual void OnDesignerCreated(DesignerEventArgs e)
		{
			if (DesignerCreated != null)
				DesignerCreated(this, e);
		}

		/// <summary>
		/// Internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public event DesignerEventHandler DesignerDisposed;

		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		protected virtual void OnDesignerDisposed(DesignerEventArgs e)
		{
			if (DesignerDisposed != null)
				DesignerDisposed(this, e);
		}

		/// <summary>
		/// Internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public DesignerCollection Designers
		{
			get { return new DesignerCollection(new IDesignerHost[] { }); }
		}

		/// <summary>
		/// Occurs when the current selection changed.
		/// </summary>
		//-----------------------------------------------------------------------------
		public event EventHandler SelectionChanged;

		//-----------------------------------------------------------------------------
		internal void OnSelectedObjectChanged(SelectedObjectEventArgs args)
		{
			if (SelectionChanged != null)
				SelectionChanged(this, args);
		}

		#endregion

		///<remarks />
		//-----------------------------------------------------------------------------
		protected void OnOpenCodeEditor(CodeMethodEditorEventArgs e)
		{
			if (OpenCodeEditor != null)
				OpenCodeEditor(this, e);
		}

		#region IServiceProvider Members

		/// <summary>
		/// Internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public new virtual object GetService(Type serviceType)
		{
			if (serviceType == typeof(IDesignerEventService))
				return this;

			if (serviceType == typeof(IUIService))
				return uiService;

			if (serviceType == typeof(ISelectionService))
				return SelectionService;

			if (
				serviceType == typeof(IComponentChangeService) ||
				serviceType == typeof(ITBComponentChangeService)
				)
				return ComponentChangeService;

			return null;
		}

		#endregion

		///<remarks />
		//---------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			EasyBuilderSelectionService s = selectionService as EasyBuilderSelectionService;
			if (s != null)
			{
				s.StopListeningTo(this.compChangeSvc);
				selectionService = null;
			}

			if (LocalizationManager != null)
			{
				LocalizationManager.Dispose();
				LocalizationManager = null;
			}
			if (EventsManager != null)
			{
				EventsManager.Dispose();
				EventsManager = null;
			}
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		internal virtual void InitSources(Sources aSources)
		{

		}

		internal virtual void AddHotLink(AddHotLinkEventArgs e) { }
		internal virtual void DeclareComponent(DeclareComponentEventArgs e) { }
		internal virtual bool Build(bool debug, string filePath, bool generateSources = true) { return false; }
		internal virtual bool Save(bool askForOptions) { return false; }
		internal virtual Sources Sources { get { return null; } }
	}
}
