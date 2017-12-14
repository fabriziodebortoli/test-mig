using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyAttachment.UI.Forms
{
    public partial class CustomFilter : Form
    {
        public Dictionary<int, IList<int>> Filters = new Dictionary<int, IList<int>> { };

        //--------------------------------------------------------------------------------
        public CustomFilter()
        {
            InitializeComponent();
        }

        //--------------------------------------------------------------------------------
        internal Dictionary<int, IList<int>> GetCustomFilter(Control parent, CollectorsResultDataTable table, Dictionary<int, IList<int>> previousCustomFilter)
        {
            if (table == null || table.Rows.Count == 0) return null;

           
            //cerco salendo tra i parent  del controllo che mi ha chiamato la form che farà da owner a questa
            //se o è null la form verrà posizionata in location 0,0 senza owner
            this.Owner = parent.FindForm();

            if (previousCustomFilter == null) previousCustomFilter = new Dictionary<int, IList<int>> { };
            IList<int> ids =  null;
            TreeNode root =new TreeNode("All",0 ,0);
            root.Name = "root";
          
            treeView1.Nodes.Add(root);
            for (int i = table.Rows.Count - 1; i >= 0; i--)
            {
                TreeNode n = new TreeNode(table.Rows[i][CommonStrings.Collector] as string,1,1);
                n.Name = "Collector";
                int collectorid = ((int)table.Rows[i][CommonStrings.CollectorID]);
                n.Tag = collectorid;
               
                if (previousCustomFilter.ContainsKey(collectorid))
                {
                    ids = previousCustomFilter[collectorid];
                    n.Checked = (ids != null || ids.Count > 0);
                }
                foreach (DMS_Collection r in table.Rows[i][CommonStrings.Collections] as DMS_Collection[])
                {
                    TreeNode n1 = new TreeNode(r.Name, 2, 2);
                    n1.Name = "Collection";
                    n1.Tag = r.CollectionID ;
                    n1.Checked = (ids!= null  && ids.Contains(r.CollectionID));
                    if (n1.Checked) n.Expand();
                    n.Nodes.Add(n1);
                }

                root.Nodes.Add(n);
            }

            root.Expand();


			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				ShowDialog(this.Owner);
			}
            return Filters;
        }


        public Dictionary<int, IList<int>> filters = new Dictionary<int, IList<int>>{};
        //--------------------------------------------------------------------------------
        private void BtnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;

            foreach (TreeNode n in treeView1.Nodes[0].Nodes)
                foreach (TreeNode nn in n.Nodes)
                {
                    if (!nn.Checked) continue;
                    if (!Filters.Keys.Contains(((int)nn.Parent.Tag)))
                    {
                        IList<int> list = new List<int> { };
                        list.Add(((int)nn.Tag));
                        Filters.Add(((int)nn.Parent.Tag), list);
                    }
                    else
                        Filters[(int)nn.Parent.Tag].Add(((int)nn.Tag));
                }
        
            Close();
        }

        //--------------------------------------------------------------------------------
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Filters = new Dictionary<int, IList<int>>{};
            Close();
        }


        //--------------------------------------------------------------------------------
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null) return;
            // se seleziono/deseleziono un parent seleziono anche tutti i figli
            foreach (TreeNode child in e.Node.Nodes)
            {
                child.Checked = e.Node.Checked;
               
                if (child.Nodes != null && child.Nodes.Count>0)
                    foreach (TreeNode child2 in child.Nodes)
                        child2.Checked = child.Checked;
                        
            }

            if (e.Node.Parent == null) return;

            if (!e.Node.Checked && e.Node.Parent.Checked)//se un nodo child viene deselezionato il parent viene deselezionato se è selezionato
                e.Node.Parent.Checked = false;

            if (e.Node.Checked && !e.Node.Parent.Checked)//se tutti i nodi child sono selezionati seleziono anche il parent
            {
                bool selectparent = true;
                foreach (TreeNode brother in e.Node.Parent.Nodes)
                {
                    if (!brother.Checked)
                    {
                        selectparent = false;
                        break;
                    }
                }
                e.Node.Parent.Checked = selectparent;
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
          // if ( e.Node.Checked) e.Node.Expand(); 
        }
    }
}
