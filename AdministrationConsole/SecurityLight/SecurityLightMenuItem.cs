using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.SecurityLight
{
	/// <summary>
	/// Summary description for SecurityLightMenuItem.
	/// </summary>
	public class SecurityLightMenuItem : System.Windows.Forms.MenuItem
	{
		private System.Drawing.Image image = null;
		private System.Drawing.Color imageTransparentColor = System.Drawing.Color.White;

		private static int IMAGE_WIDTH = SystemInformation.SmallIconSize.Width ;
		private static int IMAGE_HEIGHT = SystemInformation.SmallIconSize.Height ;
		private const int X_OFFSET = 1;
		private const int LBUFFER_WIDTH = 15 ;

		//--------------------------------------------------------------------------------------------------------
		public SecurityLightMenuItem(string text) : base(text)
		{
		}

		//--------------------------------------------------------------------------------------------------------
		public SecurityLightMenuItem(string text, System.EventHandler onClick) : base(text, onClick)
		{
		}

		//--------------------------------------------------------------------------------------------------------
		public SecurityLightMenuItem(string text, System.EventHandler onClick, Shortcut shortcut) : base(text, onClick, shortcut)
		{
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public System.Drawing.Image Image 
		{
			get { return image; } 
			set 
			{
				image = value; 
				this.OwnerDraw = (image != null);
			} 
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsSeparator { get { return (String.Compare(this.Text, "-") == 0); }}

		//--------------------------------------------------------------------------------------------------------------------------------
		private string GetShortcutText()
		{
			if (!this.ShowShortcut || this.Shortcut == Shortcut.None)
				return String.Empty;

			Keys keys = (Keys)this.Shortcut ;
			return Convert.ToChar(Keys.Tab) + System.ComponentModel.TypeDescriptor.GetConverter(keys.GetType()).ConvertToString(keys) ;
		}
		
		//--------------------------------------------------------------------------------------------------------
		protected override void OnMeasureItem(MeasureItemEventArgs e)
		{
			if (image == null || IsSeparator)
			{
				base.OnMeasureItem (e);
				return;
			}
	
			if (SystemInformation.MenuFont.Height > SystemInformation.SmallIconSize.Height)
				e.ItemHeight = SystemInformation.MenuFont.Height + 2*SystemInformation.BorderSize.Height;
			else
				e.ItemHeight = SystemInformation.SmallIconSize.Height + 2*SystemInformation.BorderSize.Height;

			StringFormat sf = new StringFormat() ;
			sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Show ;
			
			// set the menu width by measuring the string, icon and buffer spaces
			int width = (int) e.Graphics.MeasureString(this.Text, SystemInformation.MenuFont, 1000, sf).Width ;
			
			width += SystemInformation.MenuCheckSize.Width;

			width += IMAGE_WIDTH;

			if (!this.ShowShortcut || this.Shortcut == Shortcut.None)
			{
				string shortcutText = GetShortcutText();
				if (shortcutText != null && shortcutText.Length > 0)
					width += (int) e.Graphics.MeasureString(shortcutText, SystemInformation.MenuFont, 1000, sf).Width;
			}

			e.ItemWidth = width;
		}

		//--------------------------------------------------------------------------------------------------------
		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			if (image == null || IsSeparator)
			{
				base.OnDrawItem (e);
				return;
			}
			
			// draw the menu item background
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
				e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
			else
				e.Graphics.FillRectangle(SystemBrushes.Menu, e.Bounds) ;
			
			Rectangle bounds = e.Bounds;
			if (this.Checked)
			{
				// draw the Glyph
				if (this.RadioCheck)
				{				
					// Purtroppo, ho notato che il disegno di un glyph di tipo MenuGlyph.Bullet eseguito
					// direttamente con ControlPaint.DrawMenuGlyph sul membro e.Graphics non usa in maniera
					// corretta il colore di background del menu item: infatti, quando si seleziona la
					// corrispondente voce di menù il pallino non viene disegnato con lo sfondo trasparente.
					// Pertanto uso il suddetto metodo per disegnare il pallino su una bitmap e poi riproduco
					// quest'ultima con lo sfondo trasparente...
					Bitmap bmp = new Bitmap(SystemInformation.MenuCheckSize.Width, SystemInformation.MenuCheckSize.Height, e.Graphics);
					ControlPaint.DrawMenuGlyph(Graphics.FromImage(bmp), 0, 0, SystemInformation.MenuCheckSize.Width, SystemInformation.MenuCheckSize.Height, MenuGlyph.Bullet) ;
					bmp.MakeTransparent(Color.White);
						
					System.Drawing.GraphicsUnit graphicsUnit = System.Drawing.GraphicsUnit.Display;
					RectangleF srcRect = bmp.GetBounds(ref graphicsUnit);
					Rectangle destRect = new Rectangle(bounds.Left, bounds.Top + ((bounds.Height - SystemInformation.MenuCheckSize.Height) / 2), SystemInformation.MenuCheckSize.Width, SystemInformation.MenuCheckSize.Height);				
					ImageAttributes imgAttrs = new ImageAttributes();
					imgAttrs.SetColorKey(SystemColors.Menu, imageTransparentColor, ColorAdjustType.Default);
					
					e.Graphics.DrawImage(bmp, destRect, (int)srcRect.Left, (int)srcRect.Top,(int)srcRect.Width, (int)srcRect.Height, graphicsUnit, imgAttrs);
				}
				else
					ControlPaint.DrawMenuGlyph(e.Graphics, bounds.Left, bounds.Top + ((bounds.Height - SystemInformation.MenuCheckSize.Height) / 2), SystemInformation.MenuCheckSize.Width, SystemInformation.MenuCheckSize.Height, MenuGlyph.Checkmark) ;
			}
			
			bounds.X += SystemInformation.MenuCheckSize.Width;
		
			// Draw the menu item image
			if (image != null)
			{
				// if the menu item is enabled, then draw the image normally
				// otherwise draw it as disabled
				if (this.Enabled)
				{
					System.Drawing.GraphicsUnit graphicsUnit = System.Drawing.GraphicsUnit.Display;
					RectangleF srcRect = image.GetBounds(ref graphicsUnit);
					Rectangle destRect = new Rectangle(bounds.Left, bounds.Top + ((bounds.Height - IMAGE_HEIGHT) / 2), IMAGE_WIDTH, IMAGE_HEIGHT);				
					ImageAttributes imgAttrs = new ImageAttributes();
					imgAttrs.SetColorKey(SystemColors.Menu, imageTransparentColor, ColorAdjustType.Default);
					
					e.Graphics.DrawImage(image, destRect, (int)srcRect.Left, (int)srcRect.Top,(int)srcRect.Width, (int)srcRect.Height, graphicsUnit, imgAttrs);

					imgAttrs.Dispose();
				}
				else
					ControlPaint.DrawImageDisabled(e.Graphics, image, bounds.Left, bounds.Top + ((bounds.Height - IMAGE_HEIGHT) / 2), SystemColors.Menu);
			}
	
			bounds.X += IMAGE_WIDTH;

			// Draw the menu item text
			Font menuFont = SystemInformation.MenuFont ;
			SolidBrush menuBrush = null ;
			if (!this.Enabled)
				menuBrush = new SolidBrush(SystemColors.GrayText);
			else
			{
				if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
					menuBrush = new SolidBrush(SystemColors.HighlightText);
				else
					menuBrush = new SolidBrush(SystemColors.MenuText);
			}
			
			// draw the menu text
			StringFormat sfMenu = new StringFormat() ;
			sfMenu.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Show;
			e.Graphics.DrawString(this.Text, menuFont, menuBrush, bounds.Left + X_OFFSET, bounds.Top + ((bounds.Height - menuFont.Height) / 2), sfMenu ) ;

			// if the menu has a shortcut, then also 
			// draw the shortcut right aligned
			if (!this.ShowShortcut || this.Shortcut == Shortcut.None)
			{
				string shortcutText = GetShortcutText();
				if (shortcutText != null && shortcutText.Length > 0)
				{
					StringFormat sfShortcut = new StringFormat() ;
					sfShortcut.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Show ;
					sfShortcut.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
					int shortcutWidth = (int) e.Graphics.MeasureString(shortcutText, menuFont, 1000, sfShortcut).Width ;
					e.Graphics.DrawString(shortcutText, menuFont, menuBrush, (bounds.Width) - LBUFFER_WIDTH , bounds.Top + ((bounds.Height - menuFont.Height) / 2), sfShortcut);
				}
			}

			menuBrush.Dispose();
				
			// since icons make the menu height longer,
			// paint a custom arrow if the menu is a parent
			// to augment the one painted by the control
			// HACK: The default arrow shows up even for ownerdrawn controls ???
			if (this.IsParent)
			{
				int glyphSize = SystemInformation.MenuFont.Height;

				ControlPaint.DrawMenuGlyph(e.Graphics, bounds.Left + bounds.Width - SystemInformation.MenuCheckSize.Width, bounds.Top + ((bounds.Height - SystemInformation.MenuCheckSize.Height) / 2), SystemInformation.MenuCheckSize.Width, SystemInformation.MenuCheckSize.Height, MenuGlyph.Arrow) ;	
			}
		}
	}
}
