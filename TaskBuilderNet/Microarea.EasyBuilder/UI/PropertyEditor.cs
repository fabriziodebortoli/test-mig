using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing.Design;
using System.Windows.Forms;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.Properties;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Core.NameSolver;
using System.Text;
using System.Windows.Forms.Design;

namespace Microarea.EasyBuilder.UI
{
	//================================================================================
	/// <remarks/>
	public partial class PropertyEditor : UserControl
	{
		private ICollection lastSelectedObjects;
		private ToolTip lblSelectedObjectTooltip = new ToolTip();
		private ISite propertyGridSite;
		private ISelectionService[] selectionServices;
		//--------------------------------------------------------------------------------
		/// <remarks/>
		public PropertyEditor(Editor editor, params ISelectionService[] selectionServices)
		{
			InitializeComponent();
			propertyGridSite = new TBSite(ObjectEventsPropertyGrid, null, editor, "ObjectEventsPropertyGrid");
			ObjectEventsPropertyGrid.Site = propertyGridSite;
			this.Text = Resources.PropertiesTitle;

			this.selectionServices = selectionServices;

			if (selectionServices != null && selectionServices.Length > 0)
			{
				foreach (var selectionService in selectionServices)
				{
					selectionService.SelectionChanged += new System.EventHandler(SelectionService_SelectionChanged);
				}
			}
		}
		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnHelpRequested(HelpEventArgs hevent)
		{
			base.OnHelpRequested(hevent);
			if (ObjectEventsPropertyGrid.SelectedObject != null)
			{
				FormEditor.ShowHelp();
				hevent.Handled = true;
			}
		}

		//--------------------------------------------------------------------------------
		private Type GetTBType(Type t)
		{
			if (t.FullName.StartsWith(NameSolverStrings.Customization) || t.FullName.StartsWith(NameSolverStrings.Standardization))
				return GetTBType(t.BaseType);
			return t;
		}
		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			//Di tutti i selection service che ho a disposizione utilizzo il primo.
			if (selectionServices != null && selectionServices.Length > 0)
				SelectionService_SelectionChanged(selectionServices[0], null);
		}

		//--------------------------------------------------------------------------------
		void SelectionService_SelectionChanged(object sender, EventArgs e)
		{
			ISelectionService selectionService = sender as ISelectionService;
			if (selectionService != null)
				SetSelectedObjects(selectionService.GetSelectedComponents());
		}

		//--------------------------------------------------------------------------------
		private void ObjectEventsPropertyGrid_SelectedObjectsChanged(object sender, EventArgs e)
		{
			if (
				ObjectEventsPropertyGrid.SelectedObjects != null &&
				ObjectEventsPropertyGrid.SelectedObjects.Length > 0
				)
			{
				IComponent selectedObject = ObjectEventsPropertyGrid.SelectedObject as IComponent;
				if (selectedObject != null)
				{
					//deve essere allineato all'oggetto selezionato, per gestire correttamente gli eventi e le proprietà
					ISite objectSite = selectedObject.Site;
					//se l'oggetto selezionato non ha site, metto quello di default della property grid
					ObjectEventsPropertyGrid.Site = objectSite == null ? propertyGridSite : objectSite;
					UpdateSelectedObjectLabel(ObjectEventsPropertyGrid);

					return;
				}
			}
			ObjectEventsPropertyGrid.Site = propertyGridSite;
			UpdateSelectedObjectLabel(null);
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public void UpdateSelectedObjectLabel(IComponent component)
		{
			if (component == null)
			{
				lblSelectedObject.Text = string.Empty;
				return;
			}

			TBPropertyGrid grid = component as TBPropertyGrid;
			StringBuilder text = new StringBuilder();
			if (grid != null)
			{
				object[] selectedObjects = grid.SelectedObjects;
				if (selectedObjects.Length < 5)
				{
					foreach (IComponent c in selectedObjects)
					{
						if (text.Length > 0)
							text.Append(", ");
						text.Append(GetComponentName(c));
					}
					lblSelectedObject.Text = text.ToString();
				}
				else
					lblSelectedObject.Text = "..so many objects!";

				return;
			}

			lblSelectedObjectTooltip.SetToolTip(lblSelectedObject, GetComponentName(component));
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		private string GetComponentName(IComponent component)
		{
			if (component == null)
				return "";

			if (component is BaseWindowWrapper)
			{
				return ((BaseWindowWrapper)component).Id;
			}
			if (component is DocOutlineProperties)
			{
				if (!string.IsNullOrEmpty(((DocOutlineProperties)component).Id))
					return ((DocOutlineProperties)component).Id;
				if (!string.IsNullOrEmpty(((DocOutlineProperties)component).Href))
					return ((DocOutlineProperties)component).Href;
				if (!string.IsNullOrEmpty(((DocOutlineProperties)component).Name))
					return ((DocOutlineProperties)component).Name;
				if (!string.IsNullOrEmpty(((DocOutlineProperties)component).Type.ToString()))
					return ((DocOutlineProperties)component).Type.ToString();
			}
			if (component.Site != null)
			{
				return component.Site.Name;
			}

			return "";
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public void SetSelectedObjects(ICollection objects)
		{
			lastSelectedObjects = objects;
			if (Visible)
				ObjectEventsPropertyGrid.SelectedObjects = objects as Object[];
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public void RefreshPropertyGrid()
		{
			if (Visible)
				ObjectEventsPropertyGrid.Refresh();
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public void OnClose()
		{
			if (selectionServices != null && selectionServices.Length > 0)
			{
				foreach (var selectionService in selectionServices)
				{
					selectionService.SelectionChanged -= new System.EventHandler(SelectionService_SelectionChanged);
				}
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnVisibleChanged(EventArgs e)
		{
			//Il giochino sull'OnVisible Changed serve per assicurarsi che vega mostrato il pulsante con il fulminino per gli eventi.
			//Se la property grid non è visibile infatti (come accade a noi poichè è dock-ata e può venire nascosta)
			//allore i controlli hanno sempre Visible=false.
			//Se selezioniamo per esempio un dataobj dalla finestra dell'object model (che nasconde la property grid)
			//allora, anche se il dataobj ha eventi, il pulsante con il fulminino rimarrà con Visible=false poichè il suo parent control non è visibile.
			//Quando poi facciamo click sulla tab della property grid e la rendiamo visibile, la logica di abilitaizone dei pulsanti non viene riscatenata
			//e quindi il pulsante degli eventi rimane invisibile.
			if (Visible)
				SetSelectedObjects(lastSelectedObjects);

			//N.B.:è necessario riallineare PRIMA l'oggetto puntato dalla property grid e solo dopo
			//permettere di propagare l'evento.
			//Questo perchè altrimenti la property grid potrebbe andare a lavorare su un oggetto puntato che è vecchio,
			//che non è il corrente attualmente puntato da lastSelectedObjects.
			base.OnVisibleChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void ObjectEventsPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			var uiSvc = GetService(typeof(IUIService)) as IUIService;
            if (uiSvc != null)
            {
                uiSvc.SetUIDirty();
            }
			//Chiamo l'aggiornamento della label anche nel caso di IComponent per coprire gli enumerativi.
			//EasyBuilderComponent è un IComponent => non cambio il vecchio comportamento.
			IComponent component = ObjectEventsPropertyGrid.SelectedObject as IComponent;
			if (component != null && component.Site != null)
				UpdateSelectedObjectLabel(component);
		}
	}

	//================================================================================
	class TBPropertyGrid : PropertyGrid
	{
		DataObjUITypeEditor dataObjEditor;
		//--------------------------------------------------------------------------------
		protected override void OnSelectedGridItemChanged(SelectedGridItemChangedEventArgs e)
		{
			base.OnSelectedGridItemChanged(e);
			if (e.OldSelection != null)
			{
				PropertyDescriptor pd = e.OldSelection.PropertyDescriptor;
				if (pd != null)
				{
					DataObjUITypeEditor oldEditor = pd.GetEditor(typeof(UITypeEditor)) as DataObjUITypeEditor;
					if (oldEditor != null)
					{
						oldEditor.OnDetachPropertyEditor((TextBox)FindGridViewEdit(Controls), (MDataObj)e.OldSelection.Value);
						Debug.Assert(oldEditor == dataObjEditor);
						dataObjEditor = null;
					}
				}
			}
			if (e.NewSelection != null)
			{
				PropertyDescriptor pd = e.NewSelection.PropertyDescriptor;
				if (pd != null)
				{
					if (dataObjEditor != null)
						dataObjEditor.Clear();

					dataObjEditor = pd.GetEditor(typeof(UITypeEditor)) as DataObjUITypeEditor;
					if (dataObjEditor != null)
						dataObjEditor.OnAttachPropertyEditor((TextBox)FindGridViewEdit(Controls), (MDataObj)e.NewSelection.Value);
				}
			}
		}

		//--------------------------------------------------------------------------------
		private Control FindGridViewEdit(ControlCollection controls)
		{
			foreach (Control c in controls)
			{
				if (c.GetType().Name == "GridViewEdit")
					return c;
				Control child = FindGridViewEdit(c.Controls);
				if (child != null)
					return child;
			}
			return null;
		}

		//--------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (null != dataObjEditor)
				dataObjEditor.Clear();
		}
		//--------------------------------------------------------------------------------
		protected override object GetService(Type service)
		{
			if (service == typeof(IUIService))
			{
				return new FormEditorUIService(this);
			}
			return base.GetService(service);
		}
	}
}
