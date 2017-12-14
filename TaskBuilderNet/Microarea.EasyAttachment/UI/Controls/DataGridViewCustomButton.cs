using System.Windows.Forms;

namespace Microarea.EasyAttachment.UI.Controls
{
	# region Classi per avere una cella nel DataGridView con il pulsante di Check
	//================================================================================
	public class DataGridViewCheckImageButtonCell : DataGridViewImageButtonCell
	{
		//--------------------------------------------------------------------------------
		public override void LoadImages()
		{
			this.ToolTipText = string.Empty;
			_buttonImageNormal = Microarea.EasyAttachment.Properties.Resources.Transparent;
			_buttonImageHot = Microarea.EasyAttachment.Properties.Resources.Check16x16;
		}
	}

	//================================================================================
	public class DataGridViewCheckImageButtonColumn : DataGridViewButtonColumn
	{
		//--------------------------------------------------------------------------------
		public DataGridViewCheckImageButtonColumn()
		{
			this.CellTemplate = new DataGridViewCheckImageButtonCell();
			this.Width = 22;
			this.Resizable = DataGridViewTriState.False;
			this.ToolTipText = string.Empty;
		}
	}
	# endregion

	# region Classi per avere una cella nel DataGridView con il pulsante di Delete
	//================================================================================
	public class DataGridViewImageButtonDeleteCell : DataGridViewImageButtonCell
	{
		//--------------------------------------------------------------------------------
		public override void LoadImages()
		{
			_buttonImageNormal = Microarea.EasyAttachment.Properties.Resources.Remove16x16;
		}
	}

	//================================================================================
	public class DataGridViewImageButtonDeleteColumn : DataGridViewButtonColumn
	{
		//--------------------------------------------------------------------------------
		public DataGridViewImageButtonDeleteColumn()
		{
			this.CellTemplate = new DataGridViewImageButtonDeleteCell();
			this.Width = 22;
			this.Resizable = DataGridViewTriState.False;
		}
	}
	# endregion

	# region Classi per avere una cella nel DataGridView con il pulsante di Add
	//================================================================================
	public class DataGridViewImageButtonAddCell : DataGridViewImageButtonCell
	{
		//--------------------------------------------------------------------------------
		public override void LoadImages()
		{
			_buttonImageNormal = Microarea.EasyAttachment.Properties.Resources.Add16x16;
		}
	}

	//================================================================================
	public class DataGridViewImageButtonAddColumn : DataGridViewButtonColumn
	{
		//--------------------------------------------------------------------------------
		public DataGridViewImageButtonAddColumn()
		{
			this.CellTemplate = new DataGridViewImageButtonAddCell();
			this.Width = 22;
			this.Resizable = DataGridViewTriState.False;
		}
	}
	# endregion

	# region Classi per avere una cella nel DataGridView con il pulsante di Preview
	//================================================================================
	public class DataGridViewPreviewImageButtonCell : DataGridViewImageButtonCell
	{
		//--------------------------------------------------------------------------------
		public override void LoadImages()
		{
			this.ToolTipText = string.Empty;
			_buttonImageNormal = Microarea.EasyAttachment.Properties.Resources.Preview16x16;
			_buttonImageDisabled = Microarea.EasyAttachment.Properties.Resources.Transparent;
		}
	}

	//================================================================================
	public class DataGridViewPreviewImageButtonColumn : DataGridViewButtonColumn
	{
		//--------------------------------------------------------------------------------
		public DataGridViewPreviewImageButtonColumn()
		{
			this.CellTemplate = new DataGridViewPreviewImageButtonCell();
			this.Width = 22;
			this.Resizable = DataGridViewTriState.False;
			this.ToolTipText = string.Empty;
		}
	}
	# endregion
}