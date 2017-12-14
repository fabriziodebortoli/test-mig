using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.Combo
{
	//=========================================================================
	public class WebColorPickerComboBox : ComboBox
	{
		private const int ColorRectLeftOffset = 4;
		private const int ColorRectTopOffset = 2;
		private const int ColorRectWidth = 26;
		private const int ItemTextMargin = 10;
	
		private WebColorCollection colors = null;

		//---------------------------------------------------------------------------------
		public WebColorPickerComboBox() 
		{
			this.DrawMode = DrawMode.OwnerDrawFixed;
			this.DropDownStyle = ComboBoxStyle.DropDownList;
		}

		//---------------------------------------------------------------------------------
		protected override void Dispose(bool disposing) 
		{
			if(disposing) 
			{
			}
			base.Dispose(disposing);
		}

		#region WebColorPickerComboBox public properties

		//---------------------------------------------------------------------------------
		public new WebColorCollection Items 
		{
			get { return colors; }
			set 
			{
				if(colors != value && value != null ) 
				{
					colors = value;
					
					base.Items.Clear();
					if(!colors.CustomColor.Equals(Color.Empty)) 
					{
						base.Items.Add(WebColorCollection.CustomColorName);

						for (int i = 1; i < colors.Count; i++) 
							base.Items.Add(colors[i].Name);
					}
					else
					{
						foreach(Color color in value) 
							base.Items.Add(color.Name);
					}
					Refresh();
				}
			}
		}

		//---------------------------------------------------------------------------------
		public new string SelectedText 
		{
			get { return (colors != null && SelectedIndex >= 0) ? colors.ColorNameToDisplay(SelectedIndex) : String.Empty; }
			set 
			{
				if (colors == null)
					return;

				int selIndex = colors.IndexOf(value);
				SelectedIndex = selIndex;
			}
		}

		//---------------------------------------------------------------------------------
		public Color SelectedColor
		{
			get { return (colors != null && SelectedIndex >= 0) ? colors[SelectedIndex] : Color.Empty; }

			set 
			{
				if (colors == null)
					return;

				int selIndex = colors.IndexOf(value);
				if (!value.Equals(Color.Empty) && selIndex == -1)
				{
					CustomColor = value;
					selIndex = 0;
				}

				SelectedIndex = selIndex;
			}
		}

		//---------------------------------------------------------------------------------
		public Color CustomColor
		{
			get { return (colors != null) ? colors.CustomColor : Color.Empty; }
			set 
			{
				if (colors != null) 
				{
					if(colors.CustomColor.Equals(Color.Empty)  && !value.Equals(Color.Empty)) 
					{
						colors.CustomColor = value; 

						base.Items.Clear();

						base.Items.Add(WebColorCollection.CustomColorName);

						for (int i = 1; i < colors.Count; i++) 
							base.Items.Add(colors[i].Name);

						Refresh();
					}
					else
						colors.CustomColor = value; 
				}
			}
		}
		
		#endregion

		#region WebColorPickerComboBox overridden methods

		//---------------------------------------------------------------------------------
		protected override void OnCreateControl() 
		{
			base.OnCreateControl();

			base.Items.Clear();

			this.Items = new WebColorCollection();
		}
		
		//---------------------------------------------------------------------------------
		protected override void OnDrawItem(DrawItemEventArgs e) 
		{
			base.OnDrawItem(e);

			if (colors == null || e.Index == -1) 
				return;
				
			e.DrawBackground();

			if (e.State == DrawItemState.Focus) 
				e.DrawFocusRectangle();

			Color rectColor = colors[e.Index];

			Graphics g = e.Graphics;

			g.FillRectangle(new SolidBrush(rectColor), ColorRectLeftOffset, e.Bounds.Top + ColorRectTopOffset,ColorRectWidth, ItemHeight - 2 * ColorRectTopOffset);

			g.DrawRectangle(Pens.Black, ColorRectLeftOffset, e.Bounds.Top + ColorRectTopOffset, ColorRectWidth, ItemHeight - 2 * ColorRectTopOffset);

			int textLeftOffset = ColorRectLeftOffset + ColorRectWidth + ItemTextMargin;
			g.DrawString
				(
				colors.ColorNameToDisplay(e.Index),
				e.Font,
				new SolidBrush(ForeColor),
				new Rectangle(textLeftOffset, e.Bounds.Top,e.Bounds.Width-textLeftOffset,ItemHeight)
				);
		}

		//---------------------------------------------------------------------------------
		protected override void OnDropDownStyleChanged(EventArgs e) 
		{
			if(this.DropDownStyle != ComboBoxStyle.DropDownList) 
				this.DropDownStyle = ComboBoxStyle.DropDownList;
		}
	
		#endregion

	}

	//=====================================================================================
	public interface IColorCollection : IEnumerable
	{
		int Count { get; }
		Color this[int i] { get; }
		Color this[string s] { get; }
		new IEnumerator GetEnumerator();
		int IndexOf(string ColorName);
		int IndexOf(Color aColor);
	}

	//=====================================================================================
	public class WebColorCollection : IColorCollection 
	{
		public const string CustomColorName = "CustomColor";

		private const int FirstWebKnownColor = 28;
		private const int WebKnownColorsCount = 140;

		private Color	customColor = Color.Empty;
		private int		colorsCount = WebKnownColorsCount;
		//---------------------------------------------------------------------------------
		public WebColorCollection() 
		{
		}
		
		//---------------------------------------------------------------------------------
		public Color CustomColor
		{
			get { return customColor; }
			set 
			{
				if (customColor.Equals(Color.Empty) && !value.Equals(Color.Empty))
					colorsCount++;
				customColor = value;
			}
		}

		//---------------------------------------------------------------------------------
		public int Count 
		{
			get { return colorsCount; }
		}

		//---------------------------------------------------------------------------------
		public IEnumerator GetEnumerator() 
		{
			return new WebColorEnumerator(this);
		}

		//---------------------------------------------------------------------------------
		public Color this[int aColorIndex] 
		{
			get 
			{ 
				if (aColorIndex < 0 || aColorIndex >= Count) 
					throw new ArgumentOutOfRangeException();
				
				if (aColorIndex == 0 && customColor != Color.Empty)
					return customColor;

				if (!customColor.Equals(Color.Empty))
					return Color.FromKnownColor((KnownColor)aColorIndex - 1 + FirstWebKnownColor);

				return Color.FromKnownColor((KnownColor)aColorIndex + FirstWebKnownColor);
			}
		}

		//---------------------------------------------------------------------------------
		public Color this[string aColorName] 
		{
			get 
			{
				if (aColorName == null || aColorName == String.Empty) 
					throw new ArgumentNullException();

				if (String.Compare(aColorName, CustomColorName, true, CultureInfo.InvariantCulture) == 0)
					return customColor;

				return Color.FromName(aColorName);
			} 
		}

		//---------------------------------------------------------------------------------
		public int IndexOf(string aColorName) 
		{
			if (aColorName == null || aColorName == String.Empty)
				return -1;

			if (!customColor.Equals(Color.Empty) && String.Compare(aColorName, CustomColorName, true, CultureInfo.InvariantCulture) == 0)
				return 0;

			for(int i = FirstWebKnownColor; i < FirstWebKnownColor + WebKnownColorsCount; i++) 
			{
				if(Color.FromKnownColor((KnownColor)i).Name.Equals(aColorName)) 
					return customColor.Equals(Color.Empty) ? (i - FirstWebKnownColor) : (i - FirstWebKnownColor + 1);
			}
			return -1;
		}
	
		//---------------------------------------------------------------------------------
		public int IndexOf(Color aColor) 
		{
			if (aColor.Equals(Color.Empty))
				return -1;

			if (!customColor.Equals(Color.Empty) && aColor.Equals(customColor))
				return 0;

			for(int i = FirstWebKnownColor; i < FirstWebKnownColor + WebKnownColorsCount; i++) 
			{
				int webColor = Color.FromKnownColor((KnownColor)i).ToArgb();
				string webColorName = Color.FromKnownColor((KnownColor)i).Name;
				int currColor = aColor.ToArgb();
				if(Color.FromKnownColor((KnownColor)i).ToArgb() == aColor.ToArgb()) 
					return customColor.Equals(Color.Empty) ? (i - FirstWebKnownColor) : (i - FirstWebKnownColor + 1);
			}
			return -1;
		}
	
		//---------------------------------------------------------------------------------
		public string ColorNameToDisplay(int aColorIndex) 
		{
			if (aColorIndex < 0 || aColorIndex >= Count) 
				return String.Empty;
			
			if (!customColor.Equals(Color.Empty))
			{
				if (aColorIndex == 0)
					return String.Format("{0};{1};{2}", customColor.R.ToString(), customColor.G.ToString(), customColor.B.ToString());
				else
					return Color.FromKnownColor((KnownColor)aColorIndex - 1 + FirstWebKnownColor).Name;
			}
			return Color.FromKnownColor((KnownColor)aColorIndex + FirstWebKnownColor).Name;
		}

		#region WebColorEnumerator class

		private class WebColorEnumerator : IEnumerator
		{
			private IColorCollection colors;
			private int location;

			//---------------------------------------------------------------------------------
			public WebColorEnumerator(IColorCollection aColorsCollection) 
			{
				colors = aColorsCollection;
				location = -1;
			}

			//---------------------------------------------------------------------------------
			public bool MoveNext() 
			{
				return (++location < colors.Count);
			}
			
			//---------------------------------------------------------------------------------
			public Object Current 
			{
				get 
				{
					if (location < 0 || location > colors.Count) 
						throw new InvalidOperationException();
					return colors[location];
				}
			}
			//---------------------------------------------------------------------------------
			public void Reset() 
			{
				location = -1;
			}
		}
		#endregion
	}
}