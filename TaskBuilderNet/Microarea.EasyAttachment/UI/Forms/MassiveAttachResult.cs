using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.UI.Controls;

namespace Microarea.EasyAttachment.UI.Forms
{
    //================================================================================
	public partial class MassiveAttachResult : UserControl
    {
        List<string> list = new List<string>();

		// evento da intercettare esternamente per il rendering del barcode nell'area del Gdviewer
		public delegate Barcode RenderingBarcodeDelegate(TypedBarcode barcode);
		public event RenderingBarcodeDelegate RenderingBarcode;
        public DMSOrchestrator Orchestrator = null;
	
        //--------------------------------------------------------------------------------
        public MassiveAttachResult()
        {
            InitializeComponent();

            dataGridView1.ShowCellToolTips = true;
            dataGridView1.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView1_EditingControlShowing);
            dataGridView1.UserDeletingRow += new DataGridViewRowCancelEventHandler(dataGridView1_UserDeletingRow);
            //var item = contextMenuStrip1.Items.Add("Change all...");
            //item.Click += new EventHandler(item_Click);

        }

        //--------------------------------------------------------------------------------
        void item_Click(object sender, EventArgs e)
        {
            List<ActionItem> actions = new List<ActionItem>();
            List<ActionItem> noactions = new List<ActionItem>();
            Dictionary<ActionItem, int> dic = new Dictionary<ActionItem, int>();

            if (dataGridView1.SelectedRows == null || dataGridView1.SelectedRows.Count == 0)
                return;

            foreach (DataGridViewRow dr in dataGridView1.SelectedRows)
            {


                DataGridViewComboBoxCell c = dr.Cells[3] as DataGridViewComboBoxCell;
                foreach (ActionItem i in c.Items)
                {
                    if (dic.ContainsKey(i)) { Debug.WriteLine("d"); }
                    int val = 0;
                    if (dic.TryGetValue(i, out val))
                        dic[i] = (val + 1);
                    else
                        dic.Add(i, 1);
                }


            }
            foreach (KeyValuePair<ActionItem, int> p in dic)
            {
                if (p.Value != dataGridView1.SelectedRows.Count) continue;
                Debug.WriteLine(p.Key.ActionValue.ToString());

            }
        }

        //--------------------------------------------------------------------------------
        void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (e == null) return;
            DeletingRow(e.Row);
        }

        //--------------------------------------------------------------------------------
        internal void DeletingRow(DataGridViewRow row)
        {
            if (row == null) return;
            AttachmentInfoOtherData aiod = row.Cells[1].Tag as AttachmentInfoOtherData;
            if (aiod == null) return;
            string id = aiod.Attachment.ArchivedDocId < 0 ? aiod.Attachment.TempPath.ToLowerInvariant() : aiod.Attachment.ArchivedDocId.ToString();
            list.Remove(id);
            RemoveBarcode(aiod.Attachment.TBarcode.Value);
        }
       
        //per gestire il cambio item della combo della griglia
        //--------------------------------------------------------------------------------
        void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            ComboBox cb = (ComboBox)e.Control;
            cb.SelectedIndexChanged += new EventHandler(cb_SelectedIndexChanged);
        }

        //--------------------------------------------------------------------------------
        void cb_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = dataGridView1.CurrentCell.RowIndex;
            AttachmentInfoOtherData aid = dataGridView1.Rows[i].Cells[1].Tag as AttachmentInfoOtherData;
            ComboBox cb = sender as ComboBox;
            aid.ActionToDo = ((ActionItem)cb.SelectedItem).ActionValue;
        }

        //--------------------------------------------------------------------------------
        public DataGridViewRowCollection Rows {get { return dataGridView1.Rows; } }

		private Dictionary<string, int> bcs = new Dictionary<string, int>();
        public List<String> Messages = new List<string>();

		//--------------------------------------------------------------------------------
        public void Add(AttachmentInfoOtherData aiod)
        {
            try
            {
                if (aiod == null || aiod.Attachment == null) return;

                if (Exists(aiod))
                {
                    aiod.Result = MassiveResult.PreFailed;
                    aiod.BarCodeStatus = MassiveStatus.ItemDuplicated;
                }

                if (!string.IsNullOrWhiteSpace(aiod.Attachment.TBarcode.Value) && BarcodeExists(aiod.Attachment.TBarcode.Value))
                    aiod.BarCodeStatus = MassiveStatus.BCDuplicated;

                AddBarcode(aiod.Attachment.TBarcode.Value);

                DataGridViewRow row = new DataGridViewRow();

                DataGridViewImageCell statusImgCell = new DataGridViewImageCell();
                statusImgCell.Value = MassiveAttachImageList.GetStatusImage(aiod.BarCodeStatus);
                row.Cells.Add(statusImgCell);

                DataGridViewCell docNameCell = new DataGridViewTextBoxCell();
                docNameCell.Value = aiod.Attachment.Name;
                docNameCell.Tag = aiod;
                row.Cells.Add(docNameCell);

                DataGridViewCell detailsCell = new DataGridViewButtonCell();
                detailsCell.Value = "...";
                row.Cells.Add(detailsCell);

                DataGridViewComboBoxCell actionCell = new DataGridViewComboBoxCell();
                ActionItem actionItemNone = new ActionItem(MassiveAction.None);
                actionCell.Items.Add(actionItemNone);
                actionCell.DisplayMember = "Text";
                actionCell.ValueMember = "ActionValue";

                if (aiod.BarCodeStatus == MassiveStatus.BCDuplicated && Orchestrator != null)
                {
                    switch (Orchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BCActionForBatch)
                    {
                        case DuplicateDocumentAction.ReplaceExistingDoc:
                            aiod.ActionToDo = MassiveAction.Substitute; break;//se i setting dicono di sostituire i doc con stesso barcode uso la stessa impostazione anche qua
                        case DuplicateDocumentAction.ArchiveAndKeepBothDocs:
                            aiod.ActionToDo = MassiveAction.Archive; break;
                    }
                }

                if (aiod.ActionToDo != MassiveAction.None && aiod.Result != MassiveResult.PreFailed)//se none o fallito non aggiungo altre opzioni
                {
                    ActionItem actionItem = new ActionItem(aiod.ActionToDo);
                    actionCell.Items.Add(actionItem);
                    actionCell.Value = actionItem;
                }
                else actionCell.Value = actionItemNone;

                row.Cells.Add(actionCell);

                dataGridView1.Rows.Add(row);
                Refresh();
                list.Add(aiod.Attachment.ArchivedDocId < 0 ? aiod.Attachment.TempPath.ToLowerInvariant() : aiod.Attachment.ArchivedDocId.ToString());
            }
            catch (Exception EXC) 
			{ 
				System.Diagnostics.Debug.WriteLine("ADD ROW IN MASSIVE: "+ EXC.ToString()); 
			}
        }

        //--------------------------------------------------------------------------------
        private bool Exists(AttachmentInfoOtherData aiod)
        {
            if(aiod.Attachment.ArchivedDocId < 0)
                return list.Contains(aiod.Attachment.TempPath.ToLowerInvariant()); 
            return list.Contains(aiod.Attachment.ArchivedDocId.ToString());
        }

        //--------------------------------------------------------------------------------
        void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((e.ColumnIndex == this.dataGridView1.Columns[0].Index) && e.Value != null)
            {
                AttachmentInfoOtherData cellai =
                    this.dataGridView1.Rows[e.RowIndex].Cells[1].Tag as AttachmentInfoOtherData;

                if (cellai == null) return;

                DataGridViewCell cell =
                    this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                
                cell.ToolTipText = GetStatusText(cellai.BarCodeStatus);
            }
        }

        //--------------------------------------------------------------------------------
        private string GetStatusText( MassiveStatus s)
        {
            switch (s)
            {
                case MassiveStatus.NoBC:
                    return Strings.NoBarcode;
                case MassiveStatus.OnlyBC:
                    return Strings.OnlyBarcode;
                case MassiveStatus.Papery:
                    return Strings.PaperyFound;
                case MassiveStatus.BCDuplicated:
					return Strings.DuplicatedBC;
                case MassiveStatus.ItemDuplicated:
                    return Strings.ItemDuplicated;
            }
            return null;
        }

        //--------------------------------------------------------------------------------
        internal void Clear()
        {
            dataGridView1.Rows.Clear();
            list.Clear();
            bcs.Clear();
        }

        //--------------------------------------------------------------------------------
        internal void DeleteSelectedRows()
        {
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                DeletingRow(row);
                dataGridView1.Rows.Remove(row);
            }
        }

        //--------------------------------------------------------------------------------
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 2) return;
            AttachmentInfoOtherData aid = dataGridView1.Rows[e.RowIndex].Cells[1].Tag as AttachmentInfoOtherData;

            if (aid.Result != MassiveResult.Todo && aid.Result != MassiveResult.PreFailed )
			{
				MassiveAttachResultDetails detailsForm = new MassiveAttachResultDetails(aid);
                detailsForm.StartPosition = FormStartPosition.CenterParent;
				detailsForm.ShowDialog(this);			
			}
			else
			{
				MassiveAttachDetails detailsForm = new MassiveAttachDetails(aid);
                detailsForm.StartPosition = FormStartPosition.CenterParent;
				detailsForm.RenderingBarcode += new MassiveAttachDetails.RenderingBarcodeDelegate(detailsForm_RenderingBarcode);
				detailsForm.ShowDialog(this);
			}
        }

		//--------------------------------------------------------------------------------
		private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex != 1) return;
			AttachmentInfoOtherData aid = dataGridView1.Rows[e.RowIndex].Cells[1].Tag as AttachmentInfoOtherData;

			if (aid.Attachment != null && File.Exists(aid.Attachment.TempPath))
				Process.Start(aid.Attachment.TempPath);
		}

		//--------------------------------------------------------------------------------
		private BusinessLogic.Barcode detailsForm_RenderingBarcode(TypedBarcode barcode)
		{
			return (RenderingBarcode != null) ? RenderingBarcode(barcode) : null;
		}

		//--------------------------------------------------------------------------------
		private void RemoveBarcode(string barcode)
		{
			if (BarcodeExists(barcode))
				bcs[barcode]--;
			//non metto in else, perche passo di qua dopo aver fatto il -- e potrebbe essere arrivato a zero
			if (!BarcodeExists(barcode))
				bcs.Remove(barcode);
		}

		//--------------------------------------------------------------------------------
		private void AddBarcode(string barcode)
		{
			if (bcs.ContainsKey(barcode))
				bcs[barcode]++;
			else
				bcs.Add(barcode, 1);
		}

		//--------------------------------------------------------------------------------
		private bool BarcodeExists(string barcode)
		{
			return (bcs.ContainsKey(barcode) && bcs[barcode] > 0);
		}

        //--------------------------------------------------------------------------------
        internal void RowProcessing(int index)
        {
            if (index < 0 && index >= dataGridView1.Rows.Count) return;

            DataGridViewRow row = dataGridView1.Rows[index];
           
            if (row == null) 
				return;
			 
			AttachmentInfoOtherData aid = row.Cells[1].Tag as AttachmentInfoOtherData;            
			if (aid != null)
				row.Cells[0].Value = MassiveAttachImageList.GetResultImage(aid.Result);
			
           	ActionItem actionItemNone = new ActionItem((aid != null) ? aid.Result : MassiveResult.Failed);

            DataGridViewComboBoxCell c2 = ((DataGridViewComboBoxCell)row.Cells[3]);
            c2.Value = null;
            c2.Items.Clear();
            c2.Items.Add(actionItemNone);
            c2.Value = actionItemNone;
            RemoveBarcode(aid.Attachment.TBarcode.Value);
			Refresh();
        }

        ////--------------------------------------------------------------------------------
        //private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        //{
        //    //dataGridView1.ClearSelection();
        //    if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.Button == MouseButtons.Right)
        //        if (!dataGridView1.Rows[e.RowIndex].Selected)
               
        //    {dataGridView1.ClearSelection();
        //        dataGridView1.Rows[e.RowIndex].Selected = true;
        //    }
        //}

      

	}

    //oggetto che rappresenta valore e testo dell'enumerativo action, utile per popolare la combobox
    //=========================================================================
    public class ActionItem
    {
        //--------------------------------------------------------------------------------
		public string Text { get; set; }
        //--------------------------------------------------------------------------------
		public MassiveAction ActionValue { get; set; }
        //--------------------------------------------------------------------------------
		public MassiveResult ResultValue { get; set; }

        //--------------------------------------------------------------------------------
        public ActionItem(MassiveAction action)
        {
            ActionValue = action;
            Text = GetText();
        }

        //--------------------------------------------------------------------------------
        public ActionItem(MassiveResult result)
        {
            ResultValue = result;
            Text = GetText();
        }

        //--------------------------------------------------------------------------------
        private string GetText()
        {   
            //se è fatto con successo o  con errori
            if (ResultValue == MassiveResult.Done)
                return Strings.Done;
			if (ResultValue == MassiveResult.WithError)
				return Strings.WithError;
            if (ResultValue == MassiveResult.Failed)
                return Strings.Failed;          
            if (ResultValue == MassiveResult.PreFailed)//prefailed quando ci sono barcode  o file duplicati , errori prima del processing
                return Strings.Failed;
            if (ResultValue == MassiveResult.Ignored)//prefailed quando ci sono barcode  o file duplicati , errori prima del processing
                return Strings.None;
            //se no è ancora da fare
            if (ActionValue == MassiveAction.Archive)
                return Strings.Archive;
            if (ActionValue == MassiveAction.Attach)
                return Strings.Attach;
			if (ActionValue == MassiveAction.Substitute)
				return Strings.Substitute;
            return Strings.None;
        }

        //--------------------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ActionItem))
                return false;
            ActionItem i = obj as ActionItem;
            return i.ActionValue == this.ActionValue && i.ResultValue == ResultValue;

        }

        //--------------------------------------------------------------------------------
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}