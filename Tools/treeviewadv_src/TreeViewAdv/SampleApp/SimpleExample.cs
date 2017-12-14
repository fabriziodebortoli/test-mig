using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Aga.Controls.Tree;
using System.Diagnostics;

namespace SampleApp
{
	public partial class SimpleExample : UserControl
	{
        //private class ToolTipProvider : IToolTipProvider
        //{
        //    public string GetToolTip(TreeNodeAdv node)
        //    {
        //        //return "You can Drag&Drop nodes to move them";
        //        return String.Format("Mouse over on {0}", node.Level);
        //    }
        //}

		//private TreeModel _model;
        private Boolean _loaded = false;
        private Boolean _expanded = false;
        //private Boolean _hide = false;
        
		public SimpleExample()
		{
			InitializeComponent();
			//_nodeTextBox.ToolTipProvider = new ToolTipProvider();
			//_model = new TreeModel();
			//_tree.Model = _model;
            //_tree.SetCheckBoxControls(true);
            //_tree.NodeTextBox.Font = new Font("Verdana", 9, FontStyle.Regular);
            _tree.NodeTextBox.EditEnabled = true;
            //_tree.NodeXtraTextBox.EditEnabled = true;

            _tree.EditEnabled = true;
            
            //_tree.SetNodeStateIcon(true);
            //_tree.AddControls();
            //_tree.NodeControls.Add(NodeXtraTextBox);
            _tree.SetNodeCheckBoxProperty("CheckState");
            _tree.SetNodeCheckBoxThreeState(false);
            _tree.SetNodeTextBoxProperty("Text");
            //_tree.SetNodeXtraTextBoxProperty("Text");
           // _tree.NodeTextBox.ToolTipProvider = new ToolTipProvider();
            //_tree.SetToolTip();
            //_tree.SetCustomToolTip(Color.LightSkyBlue, Color.Blue);
            //_tree.SetBalloonToolTip(true);
            
            //_tree.setcu
            
            _tree.RowHeight = 23;
            _tree.DblClickWithExpandCollapse = true;
			ChangeButtons();
		}

		private void ClearClick(object sender, EventArgs e)
		{
			//_model.Nodes.Clear();
            _tree.Model.Nodes.Clear();
        }

		private void AddRootClick(object sender, EventArgs e)
		{
			//Node node = new Node("root" + _model.Nodes.Count.ToString());
			//_model.Nodes.Add(node);
			//_tree.SelectedNode = _tree.FindNode(_model.GetPath(node));
            //Node node = new Node("root" + _tree.Model.Nodes.Count.ToString());
            //_tree.Model.Nodes.Add(node);
            //_tree.SelectedNode = _tree.FindNode(_tree.Model.GetPath(node));
            _tree.AddTreeNode("root","root");
		}

		private void AddChildClick(object sender, EventArgs e)
		{
            
            //if (_tree.SelectedNode != null)
            //{
            //    Node parent = _tree.SelectedNode.Tag as Node;
            //    Node node = new Node("child" + parent.Nodes.Count.ToString());
            //    parent.Nodes.Add(node);
            //    _tree.SelectedNode.IsExpanded = true;
            //}
            _tree.SetNodeStateIcon(true);
            //_tree.TextBoxConfirm = "The operation will be saved. Do you want to continue?";
            //_tree.CaptionBoxConfirm = "Mago.Net";
            _tree.Font = new Font("Verdana", 9, FontStyle.Regular);
            _tree.ForeColor = Color.Black;
            _tree.AddControls();

            _tree.SetToolTip();
            _tree.SetCustomToolTip(Color.LightSkyBlue, Color.Blue);

            _tree.AllowDragOver = true;
            _tree.AddTreeNode("root", "prova");
            _tree.CreateNewNode("child", "child", "prova cori", "");

            //if (_tree.GetOldNodesCount() > 0)
            //    _tree.OldNodesPop();

            _tree.OldNodesPush();
            _tree.AddToSelectedNode();
		}

        //private void _tree_Click(object sender, EventArgs e)
        //{
        //    if (_tree.ContextMenu != null)
        //        _tree.ContextMenu.MenuItems.Clear();

           
        //}

        //4112
        //private void _tree_MouseUp(object sender, MouseEventArgs e)
        //{

        //    if (e.Button == MouseButtons.Right && _tree.SelectedNode != null)
        //    {
        //        _tree.ContextMenu.Show(_tree, new Point(e.X, e.Y));
        //    }
        //}
        //

        private void _tree_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.F2)
            //   e.SuppressKeyPress = true;
        }

        private void DeleteClick(object sender, EventArgs e)
		{
            if (_tree.SelectedNode == null)
                return;

			(_tree.SelectedNode.Tag as Node).Parent = null;
        }

        private void _tree_ContextMenuItemClick(object sender, EventArgs e)
        {
            //5071 - simulazione editing
           ////test per editabilità
            if (_tree.IdxContextMenuItemClicked == 2)
            {
                //top level calls for wrapping
                //_tree.NodeTextBox.BeginEdit();        
                _tree.BeginEdit();
            }
            if (_tree.IdxContextMenuItemClicked == 0)
            {
                //_tree.AddChild("prova testo", "pippo");
                _tree.AddTreeNode(_tree.DefaultTextForEditing, "pippo");
                _tree.NodeTextBox.BeginEdit();
               

            }
            if (_tree.IdxContextMenuItemClicked == 1)
            {
                (_tree.SelectedNode.Tag as Node).Parent = null;
            }

                            
            //end test editabilità

            //if (_tree.ContextMenuItemClickResponse)
            //    MessageBox.Show("Response Yes!!!!");
            //else
            //    MessageBox.Show("Response No!!!");

            //if (_tree.SelectedNode.Level == 1)
            //{
                _tree.SetImage("ZONE2", "prova");
            //}

            //4112 - esempio vecchio
        //public void AddContextSubMenu(string parentItemMenu, String[] itemMenu, Boolean confirm)
        //{
        //    if (SelectedNode != null && !string.IsNullOrEmpty(parentItemMenu) && _ctxMenu != null)
        //    {
        //        MenuItem[] arr = new MenuItem[2];
        //        arr.SetValue(new MenuItem("child1", new EventHandler(OnContextMenuItemClick)), 0);
        //        arr.SetValue(new MenuItem("child2", new EventHandler(OnContextMenuItemClick)), 1);
        //        MenuItem mi;
        //        mi = _ctxMenu.MenuItems.Add(parentItemMenu, arr);
        //        mi.Click += new EventHandler(OnContextMenuItemClick);
        //    }
        //}
        }

		private void _tree_SelectionChanged(object sender, EventArgs e)
		{
            //_tree.NodeTextBox.EditEnabled = false;
            //prova on selection changed
            if (!_loaded)
                return;
            if (_tree.SelectedNode == null)
                return;
            //if (_tree.SelectedNode.Level == 3)
            //{
                //_tree.SelectedNode.XtraForeColor = Color.Green;
                //_tree.SelectedNode.HasXtraText = true;
                //_tree.SelectedNode.InitialXtraText = "Enter here the qty. to move: ";
                //_tree.SelectedNode.FinalXtraText = "Qty. to move: ";
                
            //}
            
            //5071
            _tree.AddContextMenuItem("Insert node", false);
            _tree.AddContextMenuItem("Remove node", false);
            _tree.AddContextMenuItem("Edit Node", false);
            //switch (_tree.SelectedNode.Level)
            //{
            //    case 1:
            //        _tree.AddContextMenuItem("Insert node", false);
            //        _tree.AddContextMenuItem("Remove node", false);
            //        _tree.AddContextMenuItem("Edit Node", false);
            //        break;
            //    case 2:
            //        _tree.AddContextMenuItem("Zone Menu Item 1", false);
            //        _tree.AddContextMenuItem("Zone Menu Item 2", true);
            //        _tree.AddContextMenuItem("Zone Menu Item 3", false);
            //        break;
            //    case 3:
            //        _tree.AddContextMenuItem("Section Menu Item 1", false);
            //        _tree.AddContextMenuItem("Section Menu Item 2", false);
            //        _tree.AddContextMenuItem("Section Menu Item 3", false);
            //        break;
            //    case 4:
            //        _tree.AddContextMenuItem("Bin Menu Item 1", false);
            //        _tree.AddContextMenuItem("Bin Menu Item 2", false);
            //        _tree.AddContextMenuItem("Bin Menu Item 3", false);
            //        break;
            //}
            ChangeButtons();
		}

		private void ChangeButtons()
		{
			//_addChild.Enabled = _deleteNode.Enabled = (_tree.SelectedNode != null);
		}

        

        private void _tree_ItemDrag(object sender, ItemDragEventArgs e)
        {
            
        }

        private void _tree_DragOver(object sender, DragEventArgs e)
        {
            //e.Effect = DragDropEffects.None;
            //_tree.CancelDragOver = false;
        }

		private void _tree_DragDrop(object sender, DragEventArgs e)
		{
            ////e.Effect = DragDropEffects.None;
            ////_tree.CancelDragDrop = false;
            string sSelectedNodeKey = (_tree.SelectedNode.Tag as Node).Key;
            if (!string.IsNullOrEmpty(sSelectedNodeKey))
            {
                MessageBox.Show(_tree.GetParentKey(sSelectedNodeKey));
            }
            //if (_tree.ExistsNode(sSelectedNodeKey))
            //    MessageBox.Show("Ok exists!!!");

            if (_tree.ExistsNode("pippo"))
                MessageBox.Show("Ok exists!!!");
            else
                MessageBox.Show("Not exists!!!");
		}

        private void button1_Click(object sender, EventArgs e)
        {
            Node parent;
            Node parent1;
            Node parent2;
            //root = Storage
            _tree.AddControls();
            Node storage = new Node("SEDE", "SEDE", "sede");
            _tree.Model.Nodes.Add(storage);

            _tree.SelectedNode = _tree.FindNode(_tree.Model.GetPath(storage));
            //
            if (_tree.SelectedNode != null)
            {
                parent = _tree.SelectedNode.Tag as Node;
                //crea Zone
                for (int i = 0; i < 3; i++)
                {
                    Node zone = new Node("Zone " + i.ToString(), "zone" + i.ToString(), string.Empty, string.Empty);
                    parent.Nodes.Add(zone);

                    //_tree.SelectedNode = _tree.FindNode(_model.GetPath(zone));
                    _tree.SelectedNode = _tree.FindNode(_tree.Model.GetPath(zone));
                    _tree.SelectedNode.IsExpanded = false;
                   
                    //crea Sections
                    if (_tree.SelectedNode != null)
                    {
                        parent1 = _tree.SelectedNode.Tag as Node;
                        for (int j = 0; j < 2; j++)
                        {
                            Node section = new Node("Section " + j.ToString(), "Section" + i.ToString() + j.ToString(), string.Empty, string.Empty);
                            parent1.Nodes.Add(section);
                            //_tree.SelectedNode = _tree.FindNode(_model.GetPath(section));
                            _tree.SelectedNode = _tree.FindNode(_tree.Model.GetPath(section));
                            _tree.SelectedNode.IsExpanded = false;
                            //crea Bin
                            if (_tree.SelectedNode != null)
                            {
                                parent2 = _tree.SelectedNode.Tag as Node;
                                for (int k = 0; k < 3; k++)
                                {
                                    Node bin = new Node("Bin " + k.ToString(), "Bin" + i.ToString() + j.ToString() + k.ToString(), string.Empty, string.Empty);
                                    parent2.Nodes.Add(bin);
                                    //_tree.SelectedNode = _tree.FindNode(_model.GetPath(bin));
                                    _tree.SelectedNode = _tree.FindNode(_tree.Model.GetPath(bin));
                                    _tree.SelectedNode.IsExpanded = false;
                                }
                            }
                        }
                    }
                    _tree.SelectedNode.IsExpanded = false;

                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach(Aga.Controls.Tree.Node node in _tree.Model.Nodes)
            {
                //node.st
                //if (((Node)node.Tag).CheckState == CheckState.Checked)
                //    MessageBox.Show(((Node)node.Tag).Text);
                if (node.CheckState == CheckState.Checked)
                    MessageBox.Show(node.Text);
                while (node.Nodes != null)
                {
                    for (int i = 0; i < node.Nodes.Count; i++)
                    {
                        if (node.Nodes[i].CheckState == CheckState.Checked)
                            MessageBox.Show(node.Nodes[i].Text);
                    }
                }
            }
            



        }

        private void _tree_DblClick(object sender, MouseEventArgs e)
        {
            if (_tree.SelectedNode == null)
                return;

            MessageBox.Show((_tree.SelectedNode.Tag as Node).Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Stack<Node> stack = new Stack<Node>();
            Node crtNode = null;
            Node newNode = null;
            _loaded = false;
            
            _tree.AddImage("Storage", @"C:\WMSIcons\icon\icon\treemenu\deposito.bmp");
            _tree.AddImage("Zone", @"C:\WMSIcons\icon\icon\treemenu\sezione_deposito.bmp");
            _tree.AddImage("Section", @"C:\WMSIcons\icon\icon\treemenu\sezione_zona.bmp");
            _tree.AddImage("Bin", @"C:\WMSIcons\icon\icon\treemenu\ubicazione.bmp");
            _tree.AddImage("prova", @"C:\WMSIcons\icon\icon\Logistica.gif");
            _tree.AddTreeNode("Storage Unit Content", "SEDE", "storage");
            _tree.SetStyleForSelectedNode(true, false, false, false);
            _tree.SetCheckBoxControls(true);
            _tree.Click +=_tree_Click;
            _tree.SetNodeStateIcon(true);
            _tree.TextBoxConfirm = "The operation will be saved. Do you want to continue?";
            _tree.CaptionBoxConfirm = "Mago.Net";
            _tree.Font = new Font("Verdana", 9, FontStyle.Regular);
            _tree.AddControls();

            //prova per editabilita' del nodo
            //_tree.NodeTextBox.EditEnabled = true;
            //5071
            _tree.EditEnabled = true;
            _tree.DefaultTextForEditing = @"New Element";
            _tree.CanDirectlyEditing = false;
            //

            _tree.AllowDragOver = true;

            //_tree.DragAndDropOnSameLevel = true;
            
            _tree.SelectionMode = TreeSelectionMode.MultiSameParent;
            _tree.SelectOnlyOnLevel = 3;
            //
            if (_tree.SelectedNode != null)
            {
                for (int i = 5; i <= 5; i++)
                {
                    newNode = new Node("ZONE" + i.ToString(), "ZONE"+i.ToString(), "zone");
                    if (stack.Count > 0)
                        stack.Pop();
                    stack.Push(newNode);
                    (_tree.SelectedNode.Tag as Node).Nodes.Add(newNode);
                    if (i % 2 == 0)
                    {
                        //prova a mettere un colore diverso per provare la perdita di performance
                        // :-)
                        _tree.FindNode(_tree.Model.GetPath(newNode)).ForeColor = Color.Red;
                        //prova cori
                       
                    }
                    for (int j = 1; j <= 5; j++)
                    {
                        //Node n1 = new Node("SECTION" + j.ToString(), "SECTION");
                        //if (j == 1)
                        //{
                        crtNode = stack.Peek();
                        //}
                        newNode = new Node("SECTION" + i.ToString() + j.ToString(), "SECTION" + i.ToString() + j.ToString(), "Section");
                        stack.Push(newNode);
                       crtNode.Nodes.Add(newNode);
                        for (int k = 1; k <= 5; k++)
                        {
                            if (k == 1)
                            {
                                crtNode = stack.Pop();
                            }
                            newNode = new Node("BIN" + i.ToString() + j.ToString() + k.ToString(), "BIN" + i.ToString() + j.ToString() +k.ToString(), "Bin");
                            crtNode.Nodes.Add(newNode);
                        //    crtNode.Nodes.Add(new Node("BIN" + k.ToString(), "BIN"));
                        //    //Node n2 = new Node("BIN" + k.ToString(), "BIN");
                        //    //n1.Nodes.Add(n2);
                        }
                    }
                    //_tree.AddChild("ZONE"+i.ToString(), "ZONE");
                    //crea Sections
                    //if (_tree.SelectedNode != null)
                    //{
                    //    for (int j = 1; j <= 10; j++)
                    //    {
                    //        _tree.AddChild("SECTION" + j.ToString(), "SECTION");
                    //        ////crea Bin
                    //        //if (_tree.SelectedNode != null)
                    //        //{
                    //        //    for (int k = 1; k <= 2; k++)
                    //        //    {
                    //        //        _tree.AddTreeNode("BIN" + k.ToString(), "BIN", Color.Green);
                    //        //        //_tree.SelectedNode = _tree.SelectedNode.Parent;
                    //        //        if (_tree.SelectedNode != null)
                    //        //            for (int l = 1; l <= 3; l++)
                    //        //            {
                    //        //                //_tree.AddTreeNode("Stock" + l.ToString(), "STOCK", Color.Red);
                    //        //                //_tree.SelectedNode = _tree.SelectedNode.Parent;
                    //        //                _tree.AddChild("Stock" + l.ToString(), "STOCK", Color.Blue);
                    //        //            }
                    //        //        _tree.SelectedNode = _tree.SelectedNode.Parent;
                    //        //    }

                    //        //}
                    //        //_tree.SelectedNode = _tree.SelectedNode.Parent;

                    //    }
                    //}
                    //_tree.SelectedNode = _tree.SelectedNode.Parent;
                }
                //if (stack.Count > 0)
                //{
                    //stack.Pop();
                    stack.Clear();
                //}
                //_tree.SelectedNode = _tree.SelectedNode.Children[0];
                //while (_tree.SelectedNode != null)
                //{
                //    for (int j = 1; j <= 6; j++)
                //    {
                //        _tree.AddChild("SECTION" + j.ToString(), "SECTION");
                //    }
                //    _tree.SelectedNode = _tree.SelectedNode.NextNode;
                //}
            }

            _tree.InsertChild("zone5", "pippo", "pippo", "", Color.Black, string.Empty);
            _tree.InsertChild("zone5", "pippo1", "pippo1", "", Color.Black, string.Empty);
            _tree.InsertChild("zone5", "pippo2", "pippo2", "", Color.Black, string.Empty);
            //_tree.InsertChild("pippo", "pippo1", "pippo1", "", Color.Black);
            //_tree.InsertChild("pippo1", "pippo2", "pippo2", "", Color.Black);

            
            //if (_tree.ExistsNode("pippo2"))
            //    _tree.SetFontBoldForNode("pippo2", true);
            //string parent = _tree.GetParentKey("pippo2");

            //MessageBox.Show(parent);
            _loaded = true;
        }

        private void _tree_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void _tree_Click(object sender, EventArgs e)
        {

        }

        private void _tree_MouseDown(object sender, MouseEventArgs e)
        {
           
        }

        private void _tree_LabelChanged(object sender, EventArgs e)
        {
            //if (_tree.SelectedNode != null)
                MessageBox.Show("Text=" + _tree.CurrentTextEdited);
        }

        private void btnExpand_Click(object sender, EventArgs e)
        {
            if (_expanded)
                _tree.CollapseAll(true);
            else
                _tree.ExpandAll(true);
            _expanded = !_expanded;
        }

        private void btnParentRecursive_Click(object sender, EventArgs e)
        {
            //SelectRoot!!!
            //_tree.SelectedNode = _tree.Root.Children[0];
            
            //if (_tree.SelectedNode != null)
            //    _tree.SelectedNode = _tree.SelectedNode.NextNode;          

            Node root = _tree.Root.Children[0].Tag as Node;

            if (_tree.SelectedNode != null)
            {
                if ((_tree.SelectedNode.Tag as Node).Parent == root)
                {
                    _tree.MoveChildrenOfSelectedNodeToChildrenOfRoot(true);
                }
                else
                {
                    _tree.MoveSelectedNodeToChildrenOfRoot();
                    //(_tree.SelectedNode.Tag as Node).Parent = root;
                }
            }
        }

        private void btn_GetNode_Click(object sender, EventArgs e)
        {
            //Node root = _tree.Root.Children[0].Tag as Node;
            // _tree.SetNodeAsSelected("Bin1");
            MessageBox.Show(_tree.GetParentKey("5"));
        }

        private void btnInsertChild_Click(object sender, EventArgs e)
        {
            _tree.AddControls();
            _tree.AllowDragOver = true;

            _tree.AddTreeNode("azienda", "azienda", "", Color.Black, "prova cori\nsdkfjh skjdfh sd\nsd,f nsbdkfjsd");
            _tree.InsertChild("azienda", "dirgen", "dirgen", "", Color.Black, string.Empty);
            _tree.InsertChild("dirgen", "marros", "marros", "", Color.Black, string.Empty);

            _tree.InsertChild("dirgen", "ammictrl", "ammictrl", "", Color.Black, string.Empty);
            _tree.InsertChild("ammictrl", "franconero", "franconero", "", Color.Black, string.Empty);
            _tree.InsertChild("ammictrl", "lucanervi", "lucanervi", "", Color.Black, string.Empty);
            _tree.InsertChild("ammictrl", "martinanervi", "martinanervi", "", Color.Black, string.Empty);

            _tree.InsertChild("dirgen", "markcomm", "markcomm", "", Color.Black, string.Empty);
            _tree.InsertChild("markcomm", "matteoricci", "matteoricci", "", Color.Black, string.Empty);
            _tree.InsertChild("markcomm", "areamarketing", "areamarketing", "", Color.Black, string.Empty);
            _tree.InsertChild("areamarketing", "giuseppetalli", "giuseppetalli", "", Color.Black, string.Empty);
            _tree.InsertChild("areamarketing", "silviapinzi", "silviapinzi", "", Color.Black, string.Empty);
            _tree.InsertChild("areamarketing", "rosafussia", "rosafussia", "", Color.Black, string.Empty);

            _tree.InsertChild("markcomm", "areavendite", "areavendite", "", Color.Black, string.Empty);
            _tree.InsertChild("areavendite", "matteoricci", "matteoricci", "", Color.Black, string.Empty);

            _tree.InsertChild("areavendite", "areainter", "areainter", "", Color.Black, string.Empty);
            _tree.InsertChild("areainter", "1", "1", "", Color.Black, string.Empty);
            _tree.InsertChild("areainter", "2", "2", "", Color.Black, string.Empty);
            _tree.InsertChild("areainter", "3", "3", "", Color.Black, string.Empty);

            _tree.InsertChild("areavendite", "areait", "areait", "", Color.Black, string.Empty);
            _tree.InsertChild("areait", "it1", "it1", "", Color.Black, string.Empty);
            _tree.InsertChild("areait", "it2", "it2", "", Color.Black, string.Empty);
            _tree.InsertChild("areait", "it3", "it3", "", Color.Black, string.Empty);
            _tree.InsertChild("areait", "it4", "it4", "", Color.Black, string.Empty);

            _tree.InsertChild("dirgen", "personale", "personale", "", Color.Black, string.Empty);
            _tree.InsertChild("dirgen", "ricsvil", "ricsvil", "", Color.Black, string.Empty);
            _tree.InsertChild("dirgen", "segrgen", "segrgen", "", Color.Black, string.Empty);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _tree.SetImage("ZONE2", "prova");
        }

		private void button5_Click(object sender,EventArgs e)
		{
			//navigo il tree e chiedo proprieta dei nodi

			TreeNodeAdv root = _tree.Root;
			int count = 0;
			if (root.IsExpanded)
			{
				for (int i = 0; i < root.Children.Count; i++)
				{
					GetNodeDescritpion(root.Children[i], ref count);
				}
			}

			MessageBox.Show("Node Count: " + count.ToString());
		}
	
		private void GetNodeDescritpion(TreeNodeAdv node, ref int count)
		{
			count++;
			string key = ((Node)node.Tag).Key;
			string text = ((Node)node.Tag).Text;

			Rectangle rect = _tree.GetNodeBounds(key);
			bool exp = node.IsExpanded;
			Bitmap b = ((Node)node.Tag).Bitmap;
			if (b != null)
				b.GetHbitmap();


			if (node.IsExpanded)
			{
				for (int i = 0; i < node.Children.Count; i++)
				{
					GetNodeDescritpion(node.Children[i], ref count);
				}
			}
		}

        private void button6_Click(object sender, EventArgs e)
        {
            _tree.SetNodeStateIcon(true);
            _tree.TextBoxConfirm = "The operation will be saved. Do you want to continue?";
            _tree.CaptionBoxConfirm = "Mago.Net";
            _tree.Font = new Font("Verdana", 9, FontStyle.Regular);
            _tree.AddControls();

            _tree.InsertChild("", "root", ".", "", Color.Black, string.Empty);
            _tree.InsertChild(".", "1", "1", "", Color.Black, string.Empty);
            _tree.InsertChild("1", "10", "10", "", Color.Black, string.Empty);
            _tree.InsertChild("10", "101", "101", "", Color.Black, string.Empty);
            _tree.InsertChild("101", "1011", "1011", "", Color.Black, string.Empty);
            _tree.InsertChild("101", "1012", "1012", "", Color.Black, string.Empty);
            _tree.InsertChild("101", "1015", "1015", "", Color.Black, string.Empty);
            _tree.InsertChild("101", "1016", "1016", "", Color.Black, string.Empty);
            _tree.InsertChild("10", "104", "104", "", Color.Black, string.Empty);
            _tree.InsertChild("104", "1041", "1041", "", Color.Black, string.Empty);
            _tree.InsertChild("104", "1042", "1042", "", Color.Black, string.Empty);
            _tree.InsertChild("104", "1043", "1043", "", Color.Black, string.Empty);
            _tree.InsertChild("104", "1044", "1044", "", Color.Black, string.Empty);
            _tree.InsertChild("10", "105", "105", "", Color.Black, string.Empty);
            _tree.InsertChild("10", "106", "106", "", Color.Black, string.Empty);
            _tree.InsertChild("106", "1061", "1061", "", Color.Black, string.Empty);
            _tree.InsertChild("106", "1063", "1063", "", Color.Black, string.Empty);
            _tree.InsertChild("106", "1065", "1065", "", Color.Black, string.Empty);
            _tree.InsertChild("106", "1068", "1068", "", Color.Black, string.Empty);
            _tree.InsertChild("10", "109", "109", "", Color.Black, string.Empty);
            _tree.InsertChild("109", "1091", "1091", "", Color.Black, string.Empty);
            _tree.InsertChild("109", "1092", "1092", "", Color.Black, string.Empty);

            _tree.InsertChild("1", "11", "11", "", Color.Black, string.Empty);
            _tree.InsertChild("11", "117", "117", "", Color.Black, string.Empty);
            _tree.InsertChild("117", "1171", "1171", "", Color.Black, string.Empty);
            _tree.InsertChild("117", "1172", "1172", "", Color.Black, string.Empty);
            _tree.InsertChild("117", "1174", "1174", "", Color.Black, string.Empty);
            _tree.InsertChild("117", "1178", "1178", "", Color.Black, string.Empty);

            _tree.InsertChild("1", "12", "12", "", Color.Black, string.Empty);
            _tree.InsertChild("12", "121", "121", "", Color.Black, string.Empty);
            _tree.InsertChild("12", "129", "129", "", Color.Black, string.Empty);

            _tree.InsertChild("1", "13", "13", "", Color.Black, string.Empty);
            _tree.InsertChild("13", "131", "131", "", Color.Black,string.Empty);
            _tree.InsertChild("13", "132", "132", "", Color.Black, string.Empty);
            _tree.InsertChild("13", "133", "133", "", Color.Black, string.Empty);
            _tree.InsertChild("13", "134", "134", "", Color.Black, string.Empty);
            _tree.InsertChild("13", "138", "138", "", Color.Black, string.Empty);

            _tree.InsertChild("1", "15", "15", "", Color.Black, string.Empty);
            _tree.InsertChild("15", "151", "151", "", Color.Black, string.Empty);
            _tree.InsertChild("151", "1511", "1511", "", Color.Black, string.Empty);
            _tree.InsertChild("151", "1512", "1512", "", Color.Black, string.Empty);
            _tree.InsertChild("151", "1513", "1513", "", Color.Black, string.Empty);
            _tree.InsertChild("151", "1514", "1514", "", Color.Black, string.Empty);
            _tree.InsertChild("151", "1515", "1515", "", Color.Black, string.Empty);
            _tree.InsertChild("151", "1516", "1516", "", Color.Black, string.Empty);
            _tree.InsertChild("151", "1518", "1518", "", Color.Black, string.Empty);

            _tree.InsertChild("1", "16", "16", "", Color.Black, string.Empty);
            _tree.InsertChild("16", "161", "161", "", Color.Black, string.Empty);
            _tree.InsertChild("161", "1614", "1614", "", Color.Black, string.Empty);
            _tree.InsertChild("161", "1615", "1615", "", Color.Black, string.Empty);
            _tree.InsertChild("161", "1617", "1617", "", Color.Black, string.Empty);
            _tree.InsertChild("161", "1618", "1618", "", Color.Black, string.Empty);

            _tree.InsertChild("16", "162", "162", "", Color.Black, string.Empty);
            _tree.InsertChild("162", "1621", "1621", "", Color.Black, string.Empty);
            _tree.InsertChild("162", "1622", "1622", "", Color.Black, string.Empty);
            _tree.InsertChild("162", "1623", "1623", "", Color.Black, string.Empty);
            _tree.InsertChild("162", "1624", "1624", "", Color.Black, string.Empty);
            _tree.InsertChild("162", "1625", "1625", "", Color.Black, string.Empty);
            _tree.InsertChild("162", "1626", "1626", "", Color.Black, string.Empty);
            _tree.InsertChild("162", "1627", "1627", "", Color.Black, string.Empty);

            _tree.InsertChild("16", "166", "166", "", Color.Black, string.Empty);
            _tree.InsertChild("166", "1661", "1661", "", Color.Black, string.Empty);
            _tree.InsertChild("166", "1663", "1663", "", Color.Black, string.Empty);

            _tree.InsertChild("16", "167", "167", "", Color.Black, string.Empty);
            _tree.InsertChild("16", "168", "168", "", Color.Black, string.Empty);
            _tree.InsertChild("168", "1681", "1681", "", Color.Black, string.Empty);
            _tree.InsertChild("168", "1682", "1682", "", Color.Black, string.Empty);
            _tree.InsertChild("168", "1685", "1685", "", Color.Black, string.Empty);
            _tree.InsertChild("168", "1686", "1686", "", Color.Black, string.Empty);
            _tree.InsertChild("168", "1687", "1687", "", Color.Black, string.Empty);

            _tree.InsertChild("16", "169", "169", "", Color.Black, string.Empty);

            _tree.InsertChild(".", "2", "2", "", Color.Black, string.Empty);
            _tree.InsertChild("2", "20", "20", "", Color.Black, string.Empty);
            _tree.InsertChild("20", "201", "201", "", Color.Black, string.Empty);
            _tree.InsertChild("20", "203", "203", "", Color.Black, string.Empty);
            _tree.InsertChild("20", "205", "205", "", Color.Black, string.Empty);
            _tree.InsertChild("20", "207", "207", "", Color.Black, string.Empty);
            _tree.InsertChild("207", "2071", "2071", "", Color.Black, string.Empty);

            _tree.InsertChild("2", "21", "21", "", Color.Black, string.Empty);
            _tree.InsertChild("21", "211", "211", "", Color.Black, string.Empty);
            _tree.InsertChild("211", "2111", "2111", "", Color.Black, string.Empty);
            _tree.InsertChild("211", "2112", "2112", "", Color.Black, string.Empty);

            _tree.InsertChild("21", "212", "212", "", Color.Black, string.Empty);
            _tree.InsertChild("21", "213", "213", "", Color.Black, string.Empty);
            _tree.InsertChild("213", "2131", "2131", "", Color.Black, string.Empty);
            _tree.InsertChild("213", "2132", "2132", "", Color.Black, string.Empty);
            _tree.InsertChild("213", "2132", "2132", "", Color.Black, string.Empty);
            _tree.InsertChild("213", "2134", "2134", "", Color.Black, string.Empty);

            _tree.InsertChild("21", "214", "214", "", Color.Black, string.Empty);

            _tree.InsertChild("2", "23", "23", "", Color.Black, string.Empty);
            _tree.InsertChild("23", "231", "231", "", Color.Black, string.Empty);
            _tree.InsertChild("23", "232", "232", "", Color.Black, string.Empty);
            _tree.InsertChild("23", "233", "233", "", Color.Black, string.Empty);
            _tree.InsertChild("23", "234", "234", "", Color.Black, string.Empty);

            _tree.InsertChild("2", "26", "26", "", Color.Black, string.Empty);
            _tree.InsertChild("26", "261", "261", "", Color.Black, string.Empty);
            _tree.InsertChild("26", "263", "263", "", Color.Black, string.Empty);
            _tree.InsertChild("26", "265", "265", "", Color.Black, string.Empty);
            _tree.InsertChild("26", "267", "267", "", Color.Black, string.Empty);
            _tree.InsertChild("267", "2671", "2671", "", Color.Black, string.Empty);
            _tree.InsertChild("267", "2672", "2672", "", Color.Black, string.Empty);
            _tree.InsertChild("267", "2673", "2673", "", Color.Black, string.Empty);
            _tree.InsertChild("267", "2674", "2674", "", Color.Black, string.Empty);
            _tree.InsertChild("267", "2675", "2675", "", Color.Black, string.Empty);
            _tree.InsertChild("267", "2676", "2676", "", Color.Black, string.Empty);
            _tree.InsertChild("267", "2678", "2678", "", Color.Black, string.Empty);
            _tree.InsertChild("267", "2679", "2679", "", Color.Black, string.Empty);

            _tree.InsertChild("26", "269", "269", "", Color.Black, string.Empty);
            _tree.InsertChild("269", "2691", "2691", "", Color.Black, string.Empty);
            _tree.InsertChild("269", "2692", "2692", "", Color.Black, string.Empty);
            _tree.InsertChild("269", "2693", "2693", "", Color.Black, string.Empty);

            _tree.InsertChild("2", "28", "28", "", Color.Black, string.Empty);
            _tree.InsertChild("28", "280", "280", "", Color.Black, string.Empty);
            _tree.InsertChild("280", "2801", "2801", "", Color.Black, string.Empty);
            _tree.InsertChild("280", "2803", "2803", "", Color.Black, string.Empty);
            _tree.InsertChild("280", "2805", "2805", "", Color.Black, string.Empty);
            _tree.InsertChild("280", "2807", "2807", "", Color.Black, string.Empty);
            _tree.InsertChild("280", "2808", "2808", "", Color.Black, string.Empty);

            _tree.InsertChild("28", "281", "281", "", Color.Black, string.Empty);
            _tree.InsertChild("281", "2811", "2811", "", Color.Black, string.Empty);
            _tree.InsertChild("281", "2812", "2812", "", Color.Black, string.Empty);
            _tree.InsertChild("281", "2813", "2813", "", Color.Black, string.Empty);
            _tree.InsertChild("281", "2814", "2814", "", Color.Black, string.Empty);

            _tree.InsertChild("2", "29", "29", "", Color.Black, string.Empty);
            _tree.InsertChild("29", "290", "290", "", Color.Black, string.Empty);
            _tree.InsertChild("290", "2903", "2903", "", Color.Black, string.Empty);
            _tree.InsertChild("290", "2905", "2905", "", Color.Black, string.Empty);
            _tree.InsertChild("290", "2907", "2907", "", Color.Black, string.Empty);
            _tree.InsertChild("290", "2908", "2908", "", Color.Black, string.Empty);

            _tree.InsertChild("29", "291", "291", "", Color.Black, string.Empty);
            _tree.InsertChild("291", "2911", "2911", "", Color.Black, string.Empty);
            _tree.InsertChild("291", "2912", "2912", "", Color.Black, string.Empty);
            _tree.InsertChild("291", "2913", "2913", "", Color.Black, string.Empty);
            _tree.InsertChild("291", "2914", "2914", "", Color.Black, string.Empty);

            _tree.InsertChild("29", "293", "293", "", Color.Black, string.Empty);
            _tree.InsertChild("293", "2931", "2931", "", Color.Black, string.Empty);
            _tree.InsertChild("293", "2933", "2933", "", Color.Black, string.Empty);

            _tree.InsertChild("29", "296", "296", "", Color.Black, string.Empty);
            _tree.InsertChild("296", "2961", "2961", "", Color.Black, string.Empty);
            _tree.InsertChild("296", "2962", "2962", "", Color.Black, string.Empty);
            _tree.InsertChild("296", "2963", "2963", "", Color.Black, string.Empty);
            _tree.InsertChild("296", "2964", "2964", "", Color.Black, string.Empty);
            _tree.InsertChild("296", "2965", "2965", "", Color.Black, string.Empty);
            _tree.InsertChild("296", "2966", "2966", "", Color.Black, string.Empty);
            _tree.InsertChild("296", "2968", "2968", "", Color.Black, string.Empty);
            _tree.InsertChild("296", "a", "a", "", Color.Black, string.Empty);
            _tree.InsertChild("296", "s", "s", "", Color.Black, string.Empty);
            _tree.InsertChild("296", "d", "d", "", Color.Black, string.Empty);
            _tree.InsertChild("296", "f", "f", "", Color.Black, string.Empty);
            _tree.InsertChild("296", "g", "g", "", Color.Black, string.Empty);

            _tree.InsertChild(".", "3", "3", "", Color.Black, string.Empty);
            _tree.InsertChild(".", "4", "4", "", Color.Black, string.Empty);
            _tree.InsertChild(".", "5", "5", "", Color.Black, string.Empty);
            _tree.InsertChild(".", "6", "6", "", Color.Black, string.Empty);
            _tree.InsertChild("4", "aa", "aa", "", Color.Black, string.Empty);

            if (_tree.ExistsNode("3"))
                MessageBox.Show("SI");
            else
                MessageBox.Show("NO");
        }

        private void btnToolTip_Click(object sender, EventArgs e)
        {
            _tree.SetToolTip();
        }

        protected void _tree_MouseHover(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            _tree.SetUpdateTextNode("section51", "SECTION 1 MODIF!!!");
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            if (_tree.SelectedNode == null)
                return;

            TreeNodeAdv n = _tree.SelectedNode;

            //_tree.SetNodeVisibility("zone2", _hide);
           // _tree.SetNodeVisibility((n.Tag as Node).Key, _hide);
            _tree.SetForeColorForNode("bin012", Color.Green);
            //_hide = !_hide;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            String errorMsg = string.Empty;
            if (!_tree.Root.Children[0].ToggleCheck(out errorMsg))
            {
                Debug.Assert(false, errorMsg);
            }
        }
	
	}
}
