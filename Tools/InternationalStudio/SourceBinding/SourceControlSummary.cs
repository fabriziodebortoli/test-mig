using System;
using System.IO;
using System.Windows.Forms;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer.SourceBinding
{
	public partial class SourceControlSummary : Form
	{
		ISourceControlItem[] items;
		Action genericAction;
		ILogger logWriter;
		int checkedItems = 0;

		//---------------------------------------------------------------------
		public SourceControlSummary(ISourceControlItem[] items, Action genericAction, ILogger logWriter)
		{
			InitializeComponent();
			this.items = items;
			this.genericAction = genericAction;
			this.logWriter = logWriter;
			PostInitializeComponent();
		}

		//---------------------------------------------------------------------
		private void PostInitializeComponent()
		{
			foreach (ISourceControlItem i in items)
			{
				ListViewItem item = new ListViewItem();
				item.Checked = true;
				item.SubItems.Add(Path.GetFileName(i.LocalPath));
				item.SubItems.Add(genericAction.ToString());
				item.SubItems.Add(Path.GetDirectoryName(i.LocalPath));
				item.Tag = i;

				lwItems.Items.Add(item);
			}

			checkedItems = items.Length;

			txtComment.Enabled = genericAction == Action.CheckIn;
		}

		//---------------------------------------------------------------------
		private void SourceControlSummary_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (DialogResult != DialogResult.OK)
				return;

			foreach (ListViewItem lvi in lwItems.Items)
			{
				if (lvi.Checked)
					PerformAction((ISourceControlItem)lvi.Tag);
			}
		}

		//---------------------------------------------------------------------
		private void PerformAction(ISourceControlItem item)
		{
			switch (genericAction)
			{ 
				case Action.CheckIn:
					string comment = txtComment.Text.Length == 0 ? Strings.LocalizerComment : txtComment.Text;
					item.CheckIn(item.LocalPath, comment);
					logWriter.WriteLog(string.Format("Checking '{0}' in...", item.Path), TypeOfMessage.info);
					break;
				case Action.CheckOut:
					item.CheckOut(item.LocalPath, "", true);
					logWriter.WriteLog(string.Format("Checking '{0}' out...", item.Path), TypeOfMessage.info);
					break;
				case Action.UndoCheckout:
					item.UndoCheckOut(item.LocalPath);
					logWriter.WriteLog(string.Format("Undoing check out on '{0}'...", item.Path), TypeOfMessage.info);
					break;				
			}
		}

		//---------------------------------------------------------------------
		private void SourceControlSummary_Load(object sender, EventArgs e)
		{
			chPath.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
			chName.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
			chAction.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
	
		}

		//---------------------------------------------------------------------
		private void lwItems_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			if (e.Item.Checked)
				checkedItems++;
			else
				checkedItems--;

			OK.Enabled = checkedItems > 0;
		}
	}
}
