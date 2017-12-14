using Microarea.Framework.TBApplicationWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.EasyBuilder.UI
{
    public partial class ToolBox : UserControl
    {
        private static IList<TbToolBoxItem> toolBoxItems = null;
        /// <remarks/>
        internal event EventHandler<ToolBoxItemChangedEventArgs> ToolBoxItemRenamed;
        internal event EventHandler<ToolBoxItemChangedEventArgs> ToolBoxItemDeleting;

        /// <summary>
        /// Internal use
        /// </summary>
        public static bool HasToolboxItems { get { return toolBoxItems != null; } }

        /// <summary>
        /// 
        /// </summary>
        //-----------------------------------------------------------------------------
        public static void SetToolBoxItems(IList<TbToolBoxItem> items)
        {
            if (toolBoxItems != null)
                return;

            toolBoxItems = items;
        }

        /// <summary>
        /// 
        /// </summary>
        internal ToolBox()
        {
            InitializeComponent();
  
            listView.Width = columnHeader1.Width = this.Width;

        }

        /// <summary>
        /// Inizializza la toolbox con un generico point e tutti i parsed control di Mago
        /// </summary>
        //-----------------------------------------------------------------------------
        internal void InitializeToolbox(bool isEasyStudioDesigner)
        {
            listView.SuspendLayout();
            listView.BeginUpdate();
            listView.Items.Clear();

            listView.SmallImageList = new ImageList();

            if (toolBoxItems == null || toolBoxItems.Count == 0)
                return;

            foreach (TbToolBoxItem item in toolBoxItems)
            {
                ListViewItem viewItem;
                if (item.ControlType == typeof(EasyStudioTemplate))
                {
					//template salvati con ES, visibili solo nella toolbox di ES
					if (isEasyStudioDesigner)
						continue;
                    viewItem = new ListViewItem(item.Caption, templates);
                }
                else if (item.Caption.Contains("Generic") || item.Caption.Contains("Label"))
                {
					// ES: solo Generic Group Box e Label
					// ESD: tutti i generici e Label
					if (!isEasyStudioDesigner && (item.Caption != ("Generic Group Box") && item.Caption != ("Label")))
                        continue;
                    viewItem = new ListViewItem(item.Caption, generic_group);
                }
				else
				{
					// ES: tutti i parsed tranne la propertygrid e l'headerStrip
					// ESD: tutti i parsed tranne tileGroup e TileManager
					if (!isEasyStudioDesigner && item.Caption.Contains("PropertyGrid") || item.Caption.Contains("HeaderStrip"))
						continue;
					if (isEasyStudioDesigner && (item.Caption.Contains("Tile Group") || item.Caption.Contains("Tile Manager")))
						continue;
                    viewItem = new ListViewItem(item.Caption, parsed_group);
				}


                Image image = item.Bitmap;
                if (image != null)
                {
                    viewItem.ImageIndex = listView.SmallImageList.Images.Count;
                    listView.SmallImageList.Images.Add(image);
                }
                viewItem.Tag = item;
                listView.Items.Add(viewItem);
            }
            listView.EndUpdate();
            listView.ResumeLayout();

        }

        /// <summary>
        /// Inizializza la toolbox con un generico point e tutti i parsed control di Mago
        /// </summary>
        //-----------------------------------------------------------------------------
        internal void RefreshTemplates(List<EasyStudioTemplate> templates)
        {
            List<TbToolBoxItem> itemsToRemove = new List<TbToolBoxItem>();
            // prima pulisco i vecchi
            foreach (TbToolBoxItem item in toolBoxItems)
                if (item.TypeName == typeof(EasyStudioTemplate).FullName)
                    itemsToRemove.Add(item);

            // prima pulisco i vecchi
            foreach (TbToolBoxItem item in itemsToRemove)
                if (item.TypeName == typeof(EasyStudioTemplate).FullName)
                    toolBoxItems.Remove(item);

            // prima pulisco i vecchi
            foreach (EasyStudioTemplate template in templates)
            {
                TbToolBoxItem item = new TbToolBoxItem();
                item.TypeName = typeof(EasyStudioTemplate).FullName;
                item.Caption = template.Name;
                item.ControlType = typeof(EasyStudioTemplate);
                item.Bitmap = global::Microarea.EasyBuilder.Properties.Resources.Enums;
                toolBoxItems.Add(item);
            }
            InitializeToolbox(false);
        }

        /// <summary>
        /// Metodo usato per il drag n drop degli oggetti della toolbox sul documento di Mago
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
            //-----------------------------------------------------------------------------
        private void Listview_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            try
            {
                if (listView.SelectedItems != null && listView.SelectedItems.Count > 0)
                    DoDragDrop(listView.SelectedItems[0].Tag, DragDropEffects.Copy);
            }
            catch { }
        }

        //-----------------------------------------------------------------------------
        private void listView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            ListViewItem listViewItem = listView.Items[e.Item];
            if (listViewItem.Tag == null || e.Label == null)
                return;
            TbToolBoxItem item = listViewItem.Tag as TbToolBoxItem;
            if (item == null)
                return;
            string oldLabel = item.Caption.Clone() as string;
            item.Caption = e.Label;


            if (ToolBoxItemRenamed != null)
                ToolBoxItemRenamed(this, new ToolBoxItemChangedEventArgs(oldLabel, listViewItem.Tag as TbToolBoxItem));
        }

        //-----------------------------------------------------------------------------
        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView.LabelEdit = false;
            deleteToolStripMenuItem.Enabled = false;
            if (listView.SelectedItems != null && listView.SelectedItems.Count > 0)
            {
                ListViewItem viewItem = listView.SelectedItems[0] as ListViewItem;
                if (viewItem != null && viewItem.Tag != null)
                {
                    TbToolBoxItem item = viewItem.Tag as TbToolBoxItem;
                    listView.LabelEdit = item.ControlType == typeof(EasyStudioTemplate);
                    deleteToolStripMenuItem.Enabled = listView.LabelEdit;
                }
            }
        }

        //-----------------------------------------------------------------------------
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems == null || listView.SelectedItems.Count == 0)
                return;

            ListViewItem viewitem = listView.SelectedItems[0];
            if (viewitem == null || viewitem.Tag == null)
                return;

            TbToolBoxItem item = viewitem.Tag as TbToolBoxItem;
            if (item != null)
                DoDeleteTemplate(item);
        }

        //-----------------------------------------------------------------------------
        private void DoDeleteTemplate(TbToolBoxItem item)
        {
            ToolBoxItemChangedEventArgs args = new ToolBoxItemChangedEventArgs(ToolBoxItemChangedEventArgs.ItemChangeAction.Delete, item);
            if (ToolBoxItemDeleting != null)
                ToolBoxItemDeleting(this, args);

            if (args.Cancel == true)
            {
                toolBoxItems.Remove(item);
                InitializeToolbox(false);
            }

        }
    }
    /// <summary>
    /// 
    /// </summary>
    //===============================================================================
    public class TbToolBoxItem : System.Drawing.Design.ToolboxItem
    {
        private string caption;
        private Type controlType;

        /// <summary>
        /// 
        /// </summary>
        public Type ControlType { get { return controlType; } set { controlType = value; } }

        /// <summary>
        /// 
        /// </summary>
        public string Caption { get { return caption; } set { caption = value; } }
    }

    /// <summary>
    /// 
    /// </summary>
    //================================================================================
    internal class ToolBoxItemChangedEventArgs : CancelEventArgs
    {
        internal enum ItemChangeAction { Rename, Delete }
        ItemChangeAction action;
        string oldLabel;
        TbToolBoxItem item;

        internal string OldLabel
        {
            get
            {
                return oldLabel;
            }
        }

        internal TbToolBoxItem Item
        {
            get
            {
                return item;
            }
        }

        //--------------------------------------------------------------------------------
        internal ToolBoxItemChangedEventArgs(string oldLabel, TbToolBoxItem item)
        {
            action = ItemChangeAction.Rename;
            this.oldLabel = oldLabel;
            this.item = item;
        }

        //--------------------------------------------------------------------------------
        internal ToolBoxItemChangedEventArgs(ItemChangeAction action, TbToolBoxItem item)
        {
            this.action = action;
            this.item = item;
        }
    }
}
