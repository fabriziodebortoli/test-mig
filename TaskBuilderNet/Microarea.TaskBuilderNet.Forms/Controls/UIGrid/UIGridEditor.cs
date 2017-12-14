using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	/// <summary>
	/// BaseUIGridEditor
	/// </summary>
	//=============================================================================================
	internal class UIGridEditor : BaseGridEditor, IDisposable
	{
		Size lastPanelSize;

		TBCUIControl tbCui;
        IUIControl editorUIControl;

        UIPanel panel;

		public MDataObj DataObj { get; set; }
		public IMAbstractFormDoc Document { get; set; }
		public TBWFCUIGrid GridController { get; set; }

        //-----------------------------------------------------------------------------------------
		public UIGridEditor(IUIControl editorUIControl)
        {
			this.editorUIControl = editorUIControl;
        }

		//-----------------------------------------------------------------------------------------
		public TBCUIControl TBCui { get { return tbCui; } protected set { tbCui = value; } }

		//-----------------------------------------------------------------------------------------
		public override void BeginEdit()
		{
			base.BeginEdit();
			
			RadElement focusableElement = ((IUIGridEditorControl)editorUIControl).GetFocusableElement() as RadElement;
			if (focusableElement != null)
			{
				focusableElement.Focus();
			}
		}

		//-----------------------------------------------------------------------------------------
		public override bool EndEdit()
		{
			tbCui.ExtendersManager.ClearExtenders();

			return base.EndEdit();
		}

		//-----------------------------------------------------------------------------------------
		protected override RadElement CreateEditorElement()
		{
			panel = new UIPanel();
			panel.Name = "UIPanelGridEditor";
			editorUIControl.Name = DataObj.Name;
			panel.Controls.Add(editorUIControl as Control);
  
            panel.Size = editorUIControl.Size;
			this.lastPanelSize = panel.Size;
			
			panel.SizeChanged += new EventHandler(Panel_SizeChanged);

			((TBCUI) panel.CUI).ParentChanged(GridController.Grid, EventArgs.Empty);

			TBCui = editorUIControl.CUI as TBCUIControl;
			TBCui.AttachDataObj(DataObj, Document);

			RadHostItem hostItem = new RadHostItem(panel);
            hostItem.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(hostItem_PropertyChanged);
			hostItem.FitToSizeMode = RadFitToSizeMode.FitToParentBounds;

		
			IList<ITBUIExtenderProvider> extenders = GridController.GetExtenders(GridController.Grid.CurrentCell.ColumnInfo.Name);
			if (extenders != null && extenders.Count > 0)
			{
				foreach (var extender in extenders)
				{
					TBCui.ExtendersManager.AddExtender(extender);
				}
			}
		
			return hostItem;
		}

        //-----------------------------------------------------------------------------------------
		void hostItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Bounds") //equivale al sizeChanged
            {
                panel.Size = ((RadHostItem)sender).Size;
            }
        }

		//-----------------------------------------------------------------------------------------
		void Panel_SizeChanged(object sender, EventArgs e)
		{
			UIPanel thePanel = sender as UIPanel;

			int offset = thePanel.Size.Width - this.lastPanelSize.Width;

            editorUIControl.Size = new Size(editorUIControl.Size.Width + offset, panel.Size.Height);

			this.lastPanelSize = thePanel.Size;
		}

		//-----------------------------------------------------------------------------------------
		public override object Value
		{
			get
			{
				return ((IUIGridEditorControl)editorUIControl).UIValue;
			}
			set
			{

				((IUIGridEditorControl)editorUIControl).UIValue = value;
			}
		}

		//-----------------------------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//-----------------------------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.EditorElement != null)
				{
					this.EditorElement.Dispose();
				}
				if (panel != null)
				{
					if (!panel.IsDisposed)
					{
						panel.Dispose();
					}
					panel = null;
				}
			}
		}
	}
}
