using System;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.TaskBuilderNet.Core.Generic;
using WeifenLuo.WinFormsUI.Docking;

namespace Microarea.EasyBuilder
{
	partial class FormEditor
	{
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
					components = null;
				}

				if (dummy != null)
				{
					dummy.Dispose();
					dummy = null;
				}

				if (foreBrush != null)
				{
					foreBrush.Dispose();
					foreBrush = null;
				}
				if (mainForeBrush != null)
				{
					mainForeBrush.Dispose();
					mainForeBrush = null;
				}

				if (forePen != null)
				{
					forePen.Dispose();
					forePen = null;
				}

				if (mainForePen != null)
				{
					mainForePen.Dispose();
					mainForePen = null;
				}

				if (controlsManager != null)
				{
					controlsManager.Dispose();
					controlsManager = null;
				}

				if (controller != null)
				{
					//designer json: le finestre nascono da json, quindi tutti i wrapper sono creati attorno 
					//a finestre già esistenti, ossia con HasCodeBehind a true
					//ma questo mi impedisce di editarne molte proprietà, nel caso di editor va rimosso
					//ma poi in fase di dispose va ripristinato, altrimenti il wrapper distrugge la finestra credendola sua
					//col risultato che viene distrutta due volte e crasha
					if (RunAsEasyStudioDesigner)
					{
						SetDirty(false);
						CloseJsonForm();
						SetHasCodeBehind(controller.View.Components, true);
					}

					controller.Dispose();
					controller = null;
				}

				if (selections != null)
				{
					ClearSelections();
					selections = null;
				}

				if (Sources != null)
				{
					Sources.CodeChanged -= new EventHandler<CodeChangedEventArgs>(Sources_CodeChanged);
					Sources.Dispose();
				}

				if (view != null)
				{
					view.SizeChanged -= new EventHandler<EasyBuilderEventArgs>(Parent_SizeChanged);
					view.ScrollChanged -= new EventHandler<EasyBuilderEventArgs>(Parent_ScrollChanged);
					view.SetFocus -= new EventHandler<EasyBuilderEventArgs>(Parent_SetFocus);
					view = null;
				}

				if (mainForm != null)
				{
					DockContent form = mainForm as DockContent;
					if (form != null)
					{
						form.FormClosed -= new FormClosedEventHandler(MainForm_FormClosed);
						form.FormClosing -= new FormClosingEventHandler(MainForm_FormClosing);
						if (!form.IsDisposed)
							form.Dispose();
					}
					mainForm = null;
				}
				if (ComponentChangeService != null)
				{
					ComponentChangeService.ComponentChanged -= new ComponentChangedEventHandler(ComponentChanged);
					ComponentChangeService.ComponentAdded -= new ComponentEventHandler(ComponentAdded);
					ComponentChangeService.ComponentRemoved -= new ComponentEventHandler(ComponentRemoved);
				}
				if (SelectionService != null)
				{
					SelectionService.SelectionChanged -= new EventHandler(SelectionService_SelectionChanged);
				}
				if (ComponentChangeService != null && SelectionService != null)
				{
					(SelectionService as EasyBuilderSelectionService).StopListeningTo(ComponentChangeService);
				}

				EventHandlers.RemoveEventHandlers(ref RestartDocument);
				EventHandlers.RemoveEventHandlers(ref ControllerChanged);
				EventHandlers.RemoveEventHandlers(ref ControllerChanging);
				EventHandlers.RemoveEventHandlers(ref SelectedObjectUpdated);
				EventHandlers.RemoveEventHandlers(ref RequestedOpenCodeEditor);
				EventHandlers.RemoveEventHandlers(ref HotLinkAdded);
			}

			base.Dispose(disposing);

			OnDesignerDisposed(new DesignerEventArgs(null));
		}

		private const string EditOnlyThisTileString = "Edit Only This Tile";
		private const string EditAllTilesString		= "Edit All Tiles";

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditor));
			this.cmsFormEditorContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.tsCutItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tsEditTile = new System.Windows.Forms.ToolStripMenuItem();		
			this.tsPromoteItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tsCopyItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tsPasteItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.tsProperties = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsDeleteItem = new System.Windows.Forms.ToolStripMenuItem();

			this.cmsFormEditorContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// cmsFormEditorContextMenu
			// 
			this.cmsFormEditorContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.tsDeleteItem,
				this.toolStripSeparator1,
				this.tsCutItem,
                this.tsCopyItem,
                this.tsPasteItem,
                this.toolStripSeparator2,
				this.tsPromoteItem,
				this.tsEditTile,
				this.tsProperties});
			this.cmsFormEditorContextMenu.Name = "cmsFormEditorContextMenu";
			resources.ApplyResources(this.cmsFormEditorContextMenu, "cmsFormEditorContextMenu");
			// 
			// tsCutItem
			// 
			this.tsCutItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Cut;
			this.tsCutItem.Name = "tsCutItem";
			this.tsCutItem.Text = "Cut";
			resources.ApplyResources(this.tsCutItem, "tsCutItem");
			// 
			// tsPromoteItem
			// 
			this.tsPromoteItem.Name = "tsPromoteItem";
			this.tsPromoteItem.Text = "Promote Generic Control to Parsed Control";
			resources.ApplyResources(this.tsPromoteItem, "tsPromoteItem");
			// 
			// tsEditTile
			// 
			this.tsEditTile.Image = global::Microarea.EasyBuilder.Properties.Resources.EditDocument;
			this.tsEditTile.Name = "tsEditTile";
			this.tsEditTile.Text = EditOnlyThisTileString;
			resources.ApplyResources(this.tsEditTile, "tsEditTile");
			// 
			// tsCopyItem
			// 
			this.tsCopyItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Copy;
			this.tsCopyItem.Name = "tsCopyItem";
			this.tsCopyItem.Text = "Copy";
			resources.ApplyResources(this.tsCopyItem, "tsCopyItem");
			// 
			// tsPasteItem
			// 
			this.tsPasteItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Paste;
			this.tsPasteItem.Name = "tsPasteItem";
			this.tsPasteItem.Text = "Paste";
			this.tsPasteItem.Enabled = false;
			resources.ApplyResources(this.tsPasteItem, "tsPasteItem");
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
			// 
			// tsProperties
			// 
			resources.ApplyResources(this.tsProperties, "tsProperties");
			this.tsProperties.Name = "tsProperties";
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			// 
			// tsDeleteItem
			// 
			this.tsDeleteItem.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
			this.tsDeleteItem.Name = "tsDeleteItem";
			resources.ApplyResources(this.tsDeleteItem, "tsDeleteItem");

			// 
			// FormEditor
			// 
			this.AllowDrop = true;
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FormEditor_DragDrop);
			this.DragOver += new System.Windows.Forms.DragEventHandler(this.FormEditor_DragOver);
			this.cmsFormEditorContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.ComponentModel.IContainer components;
		private ContextMenuStrip cmsFormEditorContextMenu;
		private ToolStripMenuItem tsPromoteItem;
		private ToolStripMenuItem tsCutItem;
		private ToolStripMenuItem tsCopyItem;
		private ToolStripMenuItem tsPasteItem;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripMenuItem tsDeleteItem;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripMenuItem tsProperties;
		private ToolStripMenuItem tsEditTile;


	}
}
