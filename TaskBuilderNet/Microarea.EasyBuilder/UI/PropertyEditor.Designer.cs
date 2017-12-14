using System.Windows.Forms;
namespace Microarea.EasyBuilder.UI
{
	partial class PropertyEditor
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertyEditor));
			this.lblSelectedObject = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.ObjectEventsPropertyGrid = new Microarea.EasyBuilder.UI.TBPropertyGrid();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblSelectedObject
			// 
			resources.ApplyResources(this.lblSelectedObject, "lblSelectedObject");
			this.lblSelectedObject.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblSelectedObject.Name = "lblSelectedObject";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.lblSelectedObject);
			this.panel1.Controls.Add(this.ObjectEventsPropertyGrid);
			resources.ApplyResources(this.panel1, "panel1");
			this.panel1.Name = "panel1";
			// 
			// ObjectEventsPropertyGrid
			// 
			resources.ApplyResources(this.ObjectEventsPropertyGrid, "ObjectEventsPropertyGrid");
			this.ObjectEventsPropertyGrid.CategoryForeColor = System.Drawing.Color.Purple;
			this.ObjectEventsPropertyGrid.LineColor = System.Drawing.SystemColors.InactiveCaption;
			this.ObjectEventsPropertyGrid.Name = "ObjectEventsPropertyGrid";
			this.ObjectEventsPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.ObjectEventsPropertyGrid_PropertyValueChanged);
			this.ObjectEventsPropertyGrid.SelectedObjectsChanged += new System.EventHandler(this.ObjectEventsPropertyGrid_SelectedObjectsChanged);
			// 
			// PropertyEditor
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panel1);
			this.Name = "PropertyEditor";
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private TBPropertyGrid ObjectEventsPropertyGrid;
		private Label lblSelectedObject;
		private Panel panel1;
	}
}
