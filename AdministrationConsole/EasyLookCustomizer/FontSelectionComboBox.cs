using System;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.EasyLookCustomizer
{
	//=========================================================================
	public class FontSelectionComboBox : ComboBox
	{
		private const int TextSampleRectLeftOffset = 4;
		private const int TextSampleRectTopOffset = 2;
		private const int TextSampleRectWidth = 26;
		private const int ItemTextMargin = 10;
			
		//---------------------------------------------------------------------------------
		public FontSelectionComboBox() 
		{
			this.DrawMode = DrawMode.OwnerDrawFixed;
			this.DropDownStyle = ComboBoxStyle.DropDownList;
			this.Sorted = true;

			this.Items.Clear();
		}
				
		//---------------------------------------------------------------------------------
		protected override void OnDrawItem(DrawItemEventArgs e) 
		{
			base.OnDrawItem(e);

			if 
				(
				e.Index == -1 || 
				this.Items[e.Index] == null || 
				!(this.Items[e.Index] is FontFamily) || 
				((FontFamily)this.Items[e.Index]).Name == null ||
				((FontFamily)this.Items[e.Index]).Name == String.Empty
				) 
				return;
				
			e.DrawBackground();

			if (e.State == DrawItemState.Focus) 
				e.DrawFocusRectangle();

			Graphics g = e.Graphics;

			g.FillRectangle(new SolidBrush(Color.RoyalBlue), TextSampleRectLeftOffset, e.Bounds.Top + TextSampleRectTopOffset,TextSampleRectWidth, ItemHeight - 2 * TextSampleRectTopOffset);

			FontFamily currentFontFamily = (FontFamily)this.Items[e.Index];
			FontStyle  currentFontStyle = FontStyle.Regular;
			if (!currentFontFamily.IsStyleAvailable(FontStyle.Regular))
			{
				if (currentFontFamily.IsStyleAvailable(FontStyle.Bold))
					currentFontStyle = FontStyle.Bold;
				else if (currentFontFamily.IsStyleAvailable(FontStyle.Italic))
					currentFontStyle = FontStyle.Italic;
				else if (currentFontFamily.IsStyleAvailable(FontStyle.Bold | FontStyle.Italic))
					currentFontStyle = FontStyle.Bold | FontStyle.Italic;
			}

			StringFormat abcFormat = new StringFormat();
			abcFormat.LineAlignment = StringAlignment.Center;
			abcFormat.Trimming = StringTrimming.None;
			abcFormat.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit;
			g.DrawString
				(
				"abc",
				new Font(currentFontFamily.Name, this.Font.Size-1, currentFontStyle),
				new SolidBrush(Color.White),
				new Rectangle(TextSampleRectLeftOffset + 2, e.Bounds.Top, TextSampleRectWidth - 4 , e.Bounds.Height),
				abcFormat
				);

			g.DrawRectangle(Pens.Black, TextSampleRectLeftOffset, e.Bounds.Top + TextSampleRectTopOffset, TextSampleRectWidth, ItemHeight - 2 * TextSampleRectTopOffset);

			int textLeftOffset = TextSampleRectLeftOffset + TextSampleRectWidth + ItemTextMargin;
			StringFormat format = new StringFormat();
			format.LineAlignment = StringAlignment.Center;
			format.Trimming = StringTrimming.EllipsisCharacter;
			format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit;
			g.DrawString
				(
				((FontFamily)this.Items[e.Index]).Name,
				e.Font,
				new SolidBrush(ForeColor),
				new Rectangle(textLeftOffset, e.Bounds.Top, e.Bounds.Width - textLeftOffset,ItemHeight),
				format
				);
		}

		//---------------------------------------------------------------------------------
		protected override void OnDropDownStyleChanged(EventArgs e) 
		{
			if(this.DropDownStyle != ComboBoxStyle.DropDownList) 
				this.DropDownStyle = ComboBoxStyle.DropDownList;
		}	

		//---------------------------------------------------------------------------------
		public string GetFontNameAt(int aIndex)
		{
			if (aIndex < 0 || aIndex >= this.Items.Count) 
				return String.Empty;

			FontFamily currentFontFamily = (FontFamily)this.Items[aIndex];
			return (currentFontFamily != null) ? currentFontFamily.Name : String.Empty;
		}

		//---------------------------------------------------------------------------------
		public string SelectedFontName
		{
			get { return GetFontNameAt(this.SelectedIndex);	}

			set 
			{
				if (value != null && value != String.Empty)
				{
					foreach (FontFamily aFontFamily in this.Items)
					{
						if (String.Compare(aFontFamily.Name, value, true, CultureInfo.InvariantCulture) == 0)
						{
							this.SelectedItem = aFontFamily;
							return;
						}
					}
				}
				this.SelectedIndex = -1;
				return;
			}
		}

		//---------------------------------------------------------------------------------
		public void FillWithAllInstalledFont() 
		{
			InstalledFontCollection installedFonts = new InstalledFontCollection();

			this.Items.AddRange(installedFonts.Families);
		}	
	}
}