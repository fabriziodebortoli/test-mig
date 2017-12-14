namespace SampleApp
{
	partial class SimpleExample
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
            this._addRoot = new System.Windows.Forms.Button();
            this._clear = new System.Windows.Forms.Button();
            this._addChild = new System.Windows.Forms.Button();
            this._deleteNode = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.btnExpand = new System.Windows.Forms.Button();
            this.btnParentRecursive = new System.Windows.Forms.Button();
            this.btn_GetNode = new System.Windows.Forms.Button();
            this.btnInsertChild = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.btnToolTip = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this._tree = new Aga.Controls.Tree.TreeViewAdv();
            this.btnHide = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _addRoot
            // 
            this._addRoot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._addRoot.Location = new System.Drawing.Point(362, 150);
            this._addRoot.Name = "_addRoot";
            this._addRoot.Size = new System.Drawing.Size(75, 23);
            this._addRoot.TabIndex = 1;
            this._addRoot.Text = "Add Root";
            this._addRoot.UseVisualStyleBackColor = true;
            this._addRoot.Click += new System.EventHandler(this.AddRootClick);
            // 
            // _clear
            // 
            this._clear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._clear.Location = new System.Drawing.Point(362, 121);
            this._clear.Name = "_clear";
            this._clear.Size = new System.Drawing.Size(75, 23);
            this._clear.TabIndex = 2;
            this._clear.Text = "Clear Tree";
            this._clear.UseVisualStyleBackColor = true;
            this._clear.Click += new System.EventHandler(this.ClearClick);
            // 
            // _addChild
            // 
            this._addChild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._addChild.Location = new System.Drawing.Point(362, 179);
            this._addChild.Name = "_addChild";
            this._addChild.Size = new System.Drawing.Size(75, 23);
            this._addChild.TabIndex = 3;
            this._addChild.Text = "Add Child";
            this._addChild.UseVisualStyleBackColor = true;
            this._addChild.Click += new System.EventHandler(this.AddChildClick);
            // 
            // _deleteNode
            // 
            this._deleteNode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._deleteNode.Location = new System.Drawing.Point(362, 208);
            this._deleteNode.Name = "_deleteNode";
            this._deleteNode.Size = new System.Drawing.Size(75, 23);
            this._deleteNode.TabIndex = 5;
            this._deleteNode.Text = "Delete Node";
            this._deleteNode.UseVisualStyleBackColor = true;
            this._deleteNode.Click += new System.EventHandler(this.DeleteClick);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(362, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "WMS";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(362, 92);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "Checked";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(362, 34);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 8;
            this.button3.Text = "WMS new";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnExpand
            // 
            this.btnExpand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExpand.Location = new System.Drawing.Point(362, 63);
            this.btnExpand.Name = "btnExpand";
            this.btnExpand.Size = new System.Drawing.Size(75, 23);
            this.btnExpand.TabIndex = 9;
            this.btnExpand.Text = "Expand";
            this.btnExpand.UseVisualStyleBackColor = true;
            this.btnExpand.Click += new System.EventHandler(this.btnExpand_Click);
            // 
            // btnParentRecursive
            // 
            this.btnParentRecursive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnParentRecursive.Location = new System.Drawing.Point(362, 237);
            this.btnParentRecursive.Name = "btnParentRecursive";
            this.btnParentRecursive.Size = new System.Drawing.Size(75, 23);
            this.btnParentRecursive.TabIndex = 10;
            this.btnParentRecursive.Text = "Par Rec";
            this.btnParentRecursive.UseVisualStyleBackColor = true;
            this.btnParentRecursive.Click += new System.EventHandler(this.btnParentRecursive_Click);
            // 
            // btn_GetNode
            // 
            this.btn_GetNode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_GetNode.Location = new System.Drawing.Point(362, 266);
            this.btn_GetNode.Name = "btn_GetNode";
            this.btn_GetNode.Size = new System.Drawing.Size(75, 23);
            this.btn_GetNode.TabIndex = 11;
            this.btn_GetNode.Text = "Get Node";
            this.btn_GetNode.UseVisualStyleBackColor = true;
            this.btn_GetNode.Click += new System.EventHandler(this.btn_GetNode_Click);
            // 
            // btnInsertChild
            // 
            this.btnInsertChild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInsertChild.Location = new System.Drawing.Point(362, 295);
            this.btnInsertChild.Name = "btnInsertChild";
            this.btnInsertChild.Size = new System.Drawing.Size(75, 23);
            this.btnInsertChild.TabIndex = 12;
            this.btnInsertChild.Text = "Insert Child";
            this.btnInsertChild.UseVisualStyleBackColor = true;
            this.btnInsertChild.Click += new System.EventHandler(this.btnInsertChild_Click);
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.Location = new System.Drawing.Point(362, 324);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 13;
            this.button4.Text = "Set Image";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button5.Location = new System.Drawing.Point(362, 353);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 35);
            this.button5.TabIndex = 14;
            this.button5.Text = "Navigate all visible";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button6.Location = new System.Drawing.Point(362, 394);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 15;
            this.button6.Text = "Elsa";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // btnToolTip
            // 
            this.btnToolTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToolTip.Location = new System.Drawing.Point(362, 423);
            this.btnToolTip.Name = "btnToolTip";
            this.btnToolTip.Size = new System.Drawing.Size(75, 23);
            this.btnToolTip.TabIndex = 16;
            this.btnToolTip.Text = "ToolTip";
            this.btnToolTip.UseVisualStyleBackColor = true;
            this.btnToolTip.Click += new System.EventHandler(this.btnToolTip_Click);
            // 
            // button7
            // 
            this.button7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button7.Location = new System.Drawing.Point(362, 452);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 23);
            this.button7.TabIndex = 17;
            this.button7.Text = "Upd Text";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // _tree
            // 
            this._tree.AllowDragOver = false;
            this._tree.AllowDrop = true;
            this._tree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tree.BackColor = System.Drawing.SystemColors.Window;
            this._tree.CanDirectlyEditing = false;
            this._tree.Cursor = System.Windows.Forms.Cursors.Default;
            this._tree.DefaultTextForEditing = "";
            this._tree.DragDropMarkColor = System.Drawing.Color.Black;
            this._tree.IsContextMenuVisible = false;
            this._tree.LineColor = System.Drawing.SystemColors.ControlDark;
            this._tree.Location = new System.Drawing.Point(0, 0);
            this._tree.Name = "_tree";
            this._tree.SelectedNode = null;
            this._tree.SelectionMode = Aga.Controls.Tree.TreeSelectionMode.MultiSameParent;
            this._tree.SelectOnlyOnLevel = -1;
            this._tree.ShowNodeToolTips = true;
            this._tree.Size = new System.Drawing.Size(356, 539);
            this._tree.TabIndex = 0;
            this._tree.Text = "treeViewAdv1";
            this._tree.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this._tree_ItemDrag);
            this._tree.ContextMenuItemClick += new System.EventHandler(this._tree_ContextMenuItemClick);
            this._tree.SelectionChanged += new System.EventHandler(this._tree_SelectionChanged);
            this._tree.DragDrop += new System.Windows.Forms.DragEventHandler(this._tree_DragDrop);
            this._tree.DragOver += new System.Windows.Forms.DragEventHandler(this._tree_DragOver);
            this._tree.KeyDown += new System.Windows.Forms.KeyEventHandler(this._tree_KeyDown);
            this._tree.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this._tree_DblClick);
            this._tree.MouseDown += new System.Windows.Forms.MouseEventHandler(this._tree_MouseDown);
            this._tree.MouseHover += new System.EventHandler(this._tree_MouseHover);
            // 
            // btnHide
            // 
            this.btnHide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHide.Location = new System.Drawing.Point(362, 481);
            this.btnHide.Name = "btnHide";
            this.btnHide.Size = new System.Drawing.Size(75, 23);
            this.btnHide.TabIndex = 18;
            this.btnHide.Text = "Hide/Show";
            this.btnHide.UseVisualStyleBackColor = true;
            this.btnHide.Click += new System.EventHandler(this.btnHide_Click);
            // 
            // button8
            // 
            this.button8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button8.Location = new System.Drawing.Point(362, 503);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(75, 23);
            this.button8.TabIndex = 18;
            this.button8.Text = "Toggle";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // SimpleExample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnHide);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.btnToolTip);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.btnInsertChild);
            this.Controls.Add(this.btn_GetNode);
            this.Controls.Add(this.btnParentRecursive);
            this.Controls.Add(this.btnExpand);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this._deleteNode);
            this.Controls.Add(this._addChild);
            this.Controls.Add(this._clear);
            this.Controls.Add(this._addRoot);
            this.Controls.Add(this._tree);
            this.Name = "SimpleExample";
            this.Size = new System.Drawing.Size(440, 542);
            this.ResumeLayout(false);

		}

		#endregion

		private Aga.Controls.Tree.TreeViewAdv _tree;
		private System.Windows.Forms.Button _addRoot;
		private System.Windows.Forms.Button _clear;
		//private Aga.Controls.Tree.NodeControls.NodeCheckBox _nodeCheckBox;
        //private Aga.Controls.Tree.NodeControls.NodeStateIcon _nodeStateIcon;
        //private Aga.Controls.Tree.NodeControls.NodeTextBox _nodeTextBox;
		private System.Windows.Forms.Button _addChild;
		private System.Windows.Forms.Button _deleteNode;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button btnExpand;
        private System.Windows.Forms.Button btnParentRecursive;
        private System.Windows.Forms.Button btn_GetNode;
        private System.Windows.Forms.Button btnInsertChild;
        private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button btnToolTip;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button btnHide;
		private System.Windows.Forms.Button button8;
	}
}
