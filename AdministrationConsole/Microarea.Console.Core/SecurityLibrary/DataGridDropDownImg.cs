using System;
using System.Drawing;
using System.Windows.Forms;
namespace Microarea.Console.Core.SecurityLibrary
{
	//=========================================================================
	public class DataGridDropDownImg : ComboBox
	{
		private BitmapLoader bitmapLoader = null; 

		//---------------------------------------------------------------------
		public DataGridDropDownImg(BitmapLoader bmpLoader, System.Drawing.Font aFont)  
		{
			this.DropDownStyle		= ComboBoxStyle.DropDownList;
			this.DrawMode			= System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.Font				= aFont;
			this.BackColor			= System.Drawing.SystemColors.Window;
			this.ForeColor			= System.Drawing.SystemColors.WindowText;
			this.ItemHeight			= aFont.Height + 2;
			this.IntegralHeight		= true;
			this.MaxDropDownItems	= 4;
	
			this.bitmapLoader   = bmpLoader;

			this.Items.Clear();

			this.Items.Add(0);
			this.Items.Add(4);
			this.Items.Add(2); 
			this.Items.Add(3); 
			
			this.SelectedIndex = 0;
		}

		//---------------------------------------------------------------------
		protected override void OnDrawItem(System.Windows.Forms.DrawItemEventArgs e)
		{
			if (e.Index == -1) 
				return;

			int itemToDraw = (int)Items[e.Index];

			Bitmap bmp = bitmapLoader.GetBitmap(itemToDraw); 
			if (bmp == null) 
				return;
			
			if ((e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit)
			{
				// Centro la bitmap e non scrivo la stringa
				int bmpXCenteredPos = (this.Width - bmp.Width)/2;
				e.Graphics.DrawImage(bmp, bmpXCenteredPos,  e.Bounds.Y);
				return;
			}

			e.DrawBackground();

			e.DrawFocusRectangle();

			e.Graphics.DrawImage(bmp, e.Bounds.X,  e.Bounds.Y);

			string itemText = String.Empty;
			switch(itemToDraw)
			{
				case 0:
					itemText = SecurityLibraryStrings.NotExist;
					break;
				case 2:
					itemText = SecurityLibraryStrings.Deny;
					break;
				case 3:
					itemText = SecurityLibraryStrings.Inherit;
					break;
				case 4:
					itemText = SecurityLibraryStrings.Allow;
					break;
				default:
					return;
			}
			SolidBrush drawStringBrush = new SolidBrush(e.ForeColor);
	
			e.Graphics.DrawString(itemText, this.Font, drawStringBrush, e.Bounds.X + bmp.Width + 2, e.Bounds.Y);

			drawStringBrush.Dispose(); 
			
			base.OnDrawItem(e);
		}
	
		//---------------------------------------------------------------------
		public int GetItemIndexFromObject(object objectValue)
		{
			if(objectValue == null)
			{
				return -1;
			}

			return this.FindStringExact(objectValue.ToString());
		}

		private void InitializeComponent()
		{
			// 
			// DataGridDropDownImg
			// 
		

		}
	}

	//=========================================================================
	public class DataGridDropDownImgColumn : DataGridColumnStyle
	{		
		private BitmapLoader bitmapLoader = null; 

		private int                  xMargin = 2;
		private int                  yMargin = 1;
		private DataGridDropDownImg  dataGridDropDownImg;
		private string               oldVal  = new string(string.Empty.ToCharArray());
		private bool                 inEdit  = false;
	//	private int						editRowNumber = -1;

		private const int dropdownDefaultWidth = 132;

		public delegate void ModifyColumnValueHandle(object sender, int rowNumber);
		public event ModifyColumnValueHandle OnModifyColumnValueHandle;
		
		
		//---------------------------------------------------------------------
		public DataGridDropDownImgColumn(BitmapLoader bmpLoader, System.Drawing.Font aFont)  
		{
			this.bitmapLoader = bmpLoader;
		
			dataGridDropDownImg = new DataGridDropDownImg(bmpLoader, aFont);
			
			dataGridDropDownImg.Visible       = false;
			dataGridDropDownImg.Location      = new System.Drawing.Point(8, 0);
			dataGridDropDownImg.Size          = new System.Drawing.Size(dropdownDefaultWidth, aFont.Height + 4);
			dataGridDropDownImg.DropDownWidth = dropdownDefaultWidth;
			dataGridDropDownImg.TabIndex      = 0;
		}

		//---------------------------------------------------------------------
		protected override void Abort(int rowNumber)
		{
			System.Diagnostics.Debug.WriteLine("Abort()");
			RollBack();
			HideComboBox();
			EndEdit();
		}
		
		//---------------------------------------------------------------------
		protected override bool Commit(CurrencyManager dataSource,int rowNumber)
		{
			HideComboBox();
			
			if(!inEdit)
			{
				return true;
			}
			try
			{
				object Value = dataGridDropDownImg.SelectedItem;
				if (Value == null) return false;
				if(NullText.Equals(Value))
				{
					Value = System.Convert.DBNull; 
				}
				SetColumnValueAtRow(dataSource, rowNumber, Value);
			}
			catch
			{
				RollBack();
				return false;	
			}
			
			this.EndEdit();
			return true;
		}
		
		//---------------------------------------------------------------------
		protected override void ConcedeFocus()
		{
			dataGridDropDownImg.Visible = false;
		}
		//---------------------------------------------------------------------
		protected override void Edit
								   (
										CurrencyManager source,
									    int rowNumber,
										Rectangle bounds, 
										bool readOnly,
										string instantText, 
										bool cellIsVisible
									)
		{
			Rectangle originalBounds = bounds;
			oldVal = dataGridDropDownImg.Text;
	
			if(cellIsVisible)
			{
				bounds.Offset(xMargin, yMargin);
				bounds.Width -= xMargin * 2;
				bounds.Height -= yMargin;
				dataGridDropDownImg.Bounds = bounds;
				dataGridDropDownImg.Visible = true;
				if (OnModifyColumnValueHandle != null)
					OnModifyColumnValueHandle(this, rowNumber);
				
			}
			else
			{
				dataGridDropDownImg.Bounds = originalBounds;
				dataGridDropDownImg.Visible = false;
			}
				
			UpdateUI(source,rowNumber, instantText);

			if(dataGridDropDownImg.Visible)
				DataGridTableStyle.DataGrid.Invalidate(originalBounds);

			inEdit = true;
		}

		//---------------------------------------------------------------------
		protected override int GetMinimumHeight()
		{
			return dataGridDropDownImg.PreferredHeight + yMargin;
		}

		//---------------------------------------------------------------------
		protected override int GetPreferredHeight(Graphics g ,object objectValue)
		{
			return dataGridDropDownImg.PreferredHeight + yMargin;
		}

		//---------------------------------------------------------------------
		protected override Size GetPreferredSize(Graphics g, object objectValue)
		{
			return new Size(dataGridDropDownImg.Width + xMargin, dataGridDropDownImg.Height + yMargin);
		}

		//---------------------------------------------------------------------
		protected override void SetDataGridInColumn(DataGrid dataGrid)
		{
			base.SetDataGridInColumn(dataGrid);
			if(dataGridDropDownImg.Parent!=dataGrid)
			{
				if(dataGridDropDownImg.Parent!=null)
				{
					dataGridDropDownImg.Parent.Controls.Remove(dataGridDropDownImg);
				}
			}
			if(dataGrid!=null) 
			{
				dataGrid.Controls.Add(dataGridDropDownImg);
			}
		}

		//---------------------------------------------------------------------
		protected override void UpdateUI(CurrencyManager source,int rowNumber, string instantText)
		{
			if(instantText != null)
				dataGridDropDownImg.SelectedIndex = dataGridDropDownImg.FindStringExact(instantText);
			else
				dataGridDropDownImg.SelectedIndex = dataGridDropDownImg.GetItemIndexFromObject(GetColumnValueAtRow(source, rowNumber));
		}	
														 
		//----------------------------------------------------------------------
		private int DataGridTableGridLineWidth
		{
			get
			{
				if(this.DataGridTableStyle.GridLineStyle == DataGridLineStyle.Solid) 
				{ 
					return 1;
				}
				else
				{
					return 0;
				}
			}
		}

		//---------------------------------------------------------------------
		public void EndEdit()
		{
			inEdit = false;
			Invalidate();
		}

		//---------------------------------------------------------------------
		private void HideComboBox()
		{
			if(dataGridDropDownImg.Focused)
			{
				this.DataGridTableStyle.DataGrid.Focus();
			}
			dataGridDropDownImg.Visible = false;
		}

		//---------------------------------------------------------------------
		private void RollBack()
		{
			dataGridDropDownImg.Text = oldVal;
			if (oldVal == string.Empty)
				dataGridDropDownImg.Text = "0";

		}

		//---------------------------------------------------------------------
		protected Bitmap GetColumnImgAtRow(CurrencyManager source, int rowNumber)
		{
			try
			{
				object myObject = GetColumnValueAtRow(source, rowNumber);
				if (myObject == null) return null;
				return bitmapLoader.GetBitmap((int)myObject );
			}
			catch (Exception)
			{
				return bitmapLoader.GetBitmap(0);
			}
		}

		//---------------------------------------------------------------------
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNumber) 
		{
			Paint(g, bounds, source, rowNumber, this.Alignment == System.Windows.Forms.HorizontalAlignment.Right);
		}

		//---------------------------------------------------------------------
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNumber, bool alignToRight) 
		{
			SolidBrush backBrush = new SolidBrush(Color.White);
			g.FillRectangle(backBrush, bounds.X, bounds.Y, bounds.Width, bounds.Height);
			Bitmap imagePic = GetColumnImgAtRow(source, rowNumber);
			if (imagePic == null) 
				return;
			g.DrawImage((Image) imagePic, bounds.X + ((bounds.Width - imagePic.Width)>>1), bounds.Y+3, imagePic.Width, imagePic.Height);
		}

		//---------------------------------------------------------------------
		protected override void Paint
									(
										Graphics g,
										Rectangle bounds,
										CurrencyManager source, 
										int rowNumber, 
										Brush backBrush,
										Brush foreBrush,
										bool alignToRight
									) 
		{
			g.FillRectangle(backBrush, bounds.X, bounds.Y, bounds.Width, bounds.Height);
			Bitmap imagePic = GetColumnImgAtRow(source, rowNumber);
			if (imagePic == null)
				return;

			g.DrawImage((Image) imagePic, bounds.X + ((bounds.Width - imagePic.Width)>>1), bounds.Y+3, imagePic.Width, imagePic.Height);
		}
	}
}