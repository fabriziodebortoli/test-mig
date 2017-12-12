using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Microarea.EasyAttachment.Components;

namespace Microarea.EasyAttachment.UI.Controls
{
	///<summary>
	/// Combobox derivata che consente di associare un ImageList al control, in modo
	/// da poter visualizzare a fianco di ogni item un'immagine predefinita
	/// Questa combobox ospita degli item del tipo ImageComboItem (classe qui dentro definita)
	///</summary>
	//================================================================================
	public class ExtensionImageComboBox : ComboBox
	{
		private ImageList ExtensionsImageList; // imagelist della combobox
		private System.ComponentModel.IContainer components;

		// eventi da ruotare all'esterno
		public delegate void ImageComboBoxDelegate(EventArgs e);
		public event ImageComboBoxDelegate ImageComboBoxSelectedIndex;

		//--------------------------------------------------------------------------------
		public ExtensionImageComboBox()
		{
			InitializeComponent();
			this.DrawMode = DrawMode.OwnerDrawFixed;
		}

		//--------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtensionImageComboBox));
			this.ExtensionsImageList = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// ExtensionsImageList
			// 
			this.ExtensionsImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ExtensionsImageList.ImageStream")));
			this.ExtensionsImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.ExtensionsImageList.Images.SetKeyName(0, "Ext_BMP16x16.png");
			this.ExtensionsImageList.Images.SetKeyName(1, "Ext_DOC16x16.png");
			this.ExtensionsImageList.Images.SetKeyName(2, "Ext_GIF16x16.png");
			this.ExtensionsImageList.Images.SetKeyName(3, "Ext_HTML16x16.png");
			this.ExtensionsImageList.Images.SetKeyName(4, "Ext_JPG16x16.png");
			this.ExtensionsImageList.Images.SetKeyName(5, "Ext_PDF16x16.png");
			this.ExtensionsImageList.Images.SetKeyName(6, "Ext_PNG16x16.png");
			this.ExtensionsImageList.Images.SetKeyName(7, "Ext_TXT16x16.png");
			this.ExtensionsImageList.Images.SetKeyName(8, "Ext_XLS16x16.png");
			this.ExtensionsImageList.Images.SetKeyName(9, "Ext_WMV16x16.png");
			this.ExtensionsImageList.Images.SetKeyName(10, "Ext_MPEG16x16.png");
			this.ExtensionsImageList.Images.SetKeyName(11, "Ext_AVI16x16.png");
			this.ExtensionsImageList.Images.SetKeyName(12, "Ext_WAV16x16.png");
			this.ExtensionsImageList.Images.SetKeyName(13, "Ext_MP316x16.png");
			this.ExtensionsImageList.Images.SetKeyName(14, "Ext_TIFF16x16.png");
			this.ExtensionsImageList.Images.SetKeyName(15, "Ext_Default16x16.png");
			this.ExtensionsImageList.Images.SetKeyName(16, "Ext_MAIL16x16.png");
			this.ResumeLayout(false);

		}

		//---------------------------------------------------------------------
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);
			if (ImageComboBoxSelectedIndex != null)
				ImageComboBoxSelectedIndex(e);
		}

		/// <summary>
		/// Override OnDrawItem, To Be able To Draw Images, Text, And Font Formatting
		/// </summary>
		//--------------------------------------------------------------------------------
		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			e.DrawBackground(); //Draw Background Of Item
			e.DrawFocusRectangle(); //Draw Its rectangle

			SolidBrush foreBrush = new SolidBrush(e.ForeColor);
			SolidBrush currForeBrush = new SolidBrush(e.ForeColor);
            
			Font f = new Font(e.Font, FontStyle.Bold);

            try
            {
                if (e.Index < 0) //Do We Have A Valid List Just Draw Indented Text
                    e.Graphics.DrawString(this.Text, e.Font, foreBrush, e.Bounds.Left + ExtensionsImageList.ImageSize.Width, e.Bounds.Top);
                else //We Have A List
                {
                    if (this.Items[e.Index].GetType() == typeof(ImageComboItem))  //Is It A ImageCombo Item 
                    {
                        ImageComboItem ICCurrItem = (ImageComboItem)this.Items[e.Index]; //Get Current Item

                        //Obtain Current Item's ForeColour
                        Color ICCurrForeColour = (ICCurrItem.ICForeColour != Color.FromKnownColor(KnownColor.Transparent)) ? ICCurrItem.ICForeColour : e.ForeColor;
                        currForeBrush = new SolidBrush(ICCurrForeColour);
                        //Obtain Current Item's Font
                        Font ICCurrFont = ICCurrItem.ICHighLight ? f : e.Font;

                        //Draw Image
                        e.Graphics.DrawImage(ICCurrItem.ICImage, e.Bounds.Left, e.Bounds.Top);

                        //Then, Draw Text In Specified Bounds
                        e.Graphics.DrawString(ICCurrItem.ICText, ICCurrFont, currForeBrush, e.Bounds.Left + ExtensionsImageList.ImageSize.Width, e.Bounds.Top);
                    }
                    else //Not An ImageCombo Box Item, Just Draw The Text
                        e.Graphics.DrawString(this.Items[e.Index].ToString(), e.Font, foreBrush, e.Bounds.Left + ExtensionsImageList.ImageSize.Width, e.Bounds.Top);
                }
            }
            catch (ArgumentNullException)
            {
            }
            finally 
			{
               foreBrush.Dispose();
			   currForeBrush.Dispose();
			   f.Dispose();
            }
			base.OnDrawItem(e);
		}

		#region Generico item da aggiungere all'ImageComboBox
		///<summary>
		/// Generico item da aggiungere all'ImageComboBox
		/// Consente di avere un testo affiancato da un'immagine (estrapolata tramite il numero di indice
		/// in cui e' stata inserita nell'imagelist associata alla combo)
		///</summary>
		//================================================================================
		public class ImageComboItem
		{
			private string imageComboItemText = null;	//ImageCombo Item Text
			private bool imageComboItemHighlight = false; //Highlighted
			private Color imageComboItemForeColour = Color.FromKnownColor(KnownColor.Transparent); //ImageCombo Item ForeColour
			private Image comboItemImage = null;

            public bool IgnoreCaseComparison { get; set; }

			/// <summary>
			/// Text & Image Index Only
			/// </summary>
			//--------------------------------------------------------------------------------
			public ImageComboItem(string itemText)
			{
				imageComboItemText = itemText;
				comboItemImage = Utils.GetSmallImage(itemText);
			}

			/// <summary>
			/// ImageCombo Item Text
			/// </summary>
			//--------------------------------------------------------------------------------
			public string ICText { get { return imageComboItemText; } set { imageComboItemText = value; } }

			/// <summary>
			/// Highlighted
			/// </summary>
			//--------------------------------------------------------------------------------
			public bool ICHighLight { get { return imageComboItemHighlight; } set { imageComboItemHighlight = value; } }

			/// <summary>
			/// ForeColour
			/// </summary>
			//--------------------------------------------------------------------------------
			public Color ICForeColour { get { return imageComboItemForeColour; } set { imageComboItemForeColour = value; } }

			/// <summary>
			/// Image 
			/// </summary>
			//--------------------------------------------------------------------------------
			public Image ICImage { get { return comboItemImage; } set { comboItemImage = value; } }

			/// <summary>
			/// Override ToString To Return Item Text
			/// </summary>
			//--------------------------------------------------------------------------------
			public override string ToString()
			{
				return imageComboItemText;
			}

            //--------------------------------------------------------------------------------
            public override bool Equals(object obj)
            {
                ImageComboItem val = obj as ImageComboItem;
                if (val == null) return false;
                return String.Compare(val.ICText, ICText, IgnoreCaseComparison, CultureInfo.InvariantCulture) == 0;
            }
            //--------------------------------------------------------------------------------
            public override int GetHashCode()
            {
                return ICText.GetHashCode();
            }

		}
		# endregion
	}
}