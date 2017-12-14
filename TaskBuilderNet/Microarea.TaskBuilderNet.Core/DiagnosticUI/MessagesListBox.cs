using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
	/// <summary>
	/// MessagesListBox
	/// ListBox customizzata per gli errori
	/// Deriva da ResizableListBox che è in grado di effettuare il resize della
	/// finestra e del testo
	/// </summary>
	//=========================================================================
	public partial class MessagesListBox : ResizableListBox
	{
		#region private data members
		
		private const int	mainTextOffset = 20;
		private Font		headingFont;
		private Font        messageFont;
		
        #endregion
		
		#region Costruttori
		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------
		public MessagesListBox(System.ComponentModel.IContainer container)
		{
			container.Add(this);
			InitializeComponent();
			//Header lo scrivo in Bold
			headingFont		= new Font(this.Font, FontStyle.Bold);
			//Il messaggio (sottotitolo) lo metto in Italic
			//messageFont	= new Font(this.Font, FontStyle.Italic);
			messageFont		= this.Font;
			this.MeasureItem += new MeasureItemEventHandler(this.MeasureItemHandler);
		}

		/// <summary>
		/// Costruttore (vuoto)
		/// </summary>
		//---------------------------------------------------------------------
		public MessagesListBox()
		{
			InitializeComponent();
			headingFont			= new Font(this.Font, FontStyle.Bold);
			messageFont			= this.Font;
			//messageFont		  = new Font(this.Font, FontStyle.Italic);
			this.MeasureItem   += new MeasureItemEventHandler(this.MeasureItemHandler);
		}
		#endregion


		#region OnDrawItem - Ridisegno
		/// <summary>
		/// OnDrawItem
		/// </summary>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			Rectangle				bounds		= e.Bounds;
			Color					TextColor   = System.Drawing.SystemColors.ControlText;
			ParseMessageEventArgs	item		= (ParseMessageEventArgs) Items[e.Index];
			
			//draw selected item background
			if(e.State == DrawItemState.Selected)
			{
				using ( Brush b = new SolidBrush(System.Drawing.SystemColors.Highlight) )
				{
					// Fill background;
					e.Graphics.FillRectangle(b, e.Bounds);
				}	
				TextColor = System.Drawing.SystemColors.HighlightText;
			}

			e.DrawBackground();
		
			//draw image
			Image itemImage = GetImageFromType(item.MessageType);
			System.Drawing.GraphicsUnit graphicsUnit = System.Drawing.GraphicsUnit.Display;
			ImageAttributes imgAttrs = new ImageAttributes();
			imgAttrs.SetColorKey(imageList.TransparentColor, imageList.TransparentColor, ColorAdjustType.Bitmap);
				
			Rectangle destRect = new Rectangle((int)bounds.Left + 1,(int)bounds.Top + 2, (int)itemImage.Width, (int)itemImage.Height);
			RectangleF srcRect = itemImage.GetBounds(ref graphicsUnit);
			e.Graphics.DrawImage
				(
				itemImage, 
				destRect,
				srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height,
				graphicsUnit, 
				imgAttrs
				);
			
			using(SolidBrush TextBrush = new SolidBrush(TextColor))
			{
				e.Graphics.DrawString
					(
						item.Time.ToLocalTime().ToString("G", CultureInfo.CurrentCulture), // visualizza la data in formato 08/17/2000 16:32:32
						headingFont, 
						TextBrush, 
						bounds.Left + imageList.ImageSize.Width + 5, 
						bounds.Top  + imageList.ImageSize.Height - headingFont.Height
					);
						
				//disegno il titolo
				int LinesFilled, CharsFitted=0, top;
				Size oneLine = new Size(this.Width - mainTextOffset, this.messageFont.Height);
				string TextToDraw = item.MessageText;
				string TextOfLine = "";
				
				top = bounds.Top + imageList.ImageSize.Height + 2;

				while(TextToDraw.Length > 0)
				{
					e.Graphics.MeasureString
						(
							item.MessageText, 
							this.messageFont, 
							oneLine, 
							StringFormat.GenericDefault, 
							out CharsFitted, 
							out LinesFilled
						);
					
					if (TextToDraw.Length <= CharsFitted)
					{
						e.Graphics.DrawString
							(
								TextToDraw, 
								this.messageFont, 
								TextBrush , 
								bounds.Left + mainTextOffset, 
								top
							);					
						break;
					}
					else				
					{
						TextOfLine = TextToDraw.Substring(0,CharsFitted);
						
						if (TextOfLine.LastIndexOf(" ") > 0)
							TextOfLine = TextOfLine.Substring(0,TextOfLine.LastIndexOf(" "));
						
						e.Graphics.DrawString
							(
								TextOfLine, 
								this.messageFont, 
								TextBrush, 
								bounds.Left + mainTextOffset, 
								top
							);
					}

					top += this.messageFont.Height;
					TextToDraw = TextToDraw.Substring(TextOfLine.Length + 1);
				}

				e.DrawFocusRectangle();			
			}
		}
		#endregion

		#region GetIndexImageType - Dato il tipo ritorna l'indexImage
		/// <summary>
		/// GetIndexImageType
		/// </summary>
		//---------------------------------------------------------------------
		private System.Drawing.Image GetImageFromType(DiagnosticType itemType)
		{
			if (imageList == null || imageList.Images.Count == 0)
				return null;

			if (itemType == DiagnosticType.Error)
				return imageList.Images[IconsIndexer.Errors];
			else if (itemType == DiagnosticType.Warning)
				return imageList.Images[IconsIndexer.Warnings];
			else 
				return imageList.Images[IconsIndexer.Informations];
		}
		#endregion

		#region MeasureItemHandler - Richiamato al fire dell'evento MeasureItem
		/// <summary>
		/// MeasureItemHandler
		/// Richiamato al fire dell'evento MeasureItem
		/// </summary>
		//---------------------------------------------------------------------
		private void MeasureItemHandler(object sender, MeasureItemEventArgs e)
		{
			int MainTextHeight, LinesFilled, CharsFitted;			
			ParseMessageEventArgs item =  (ParseMessageEventArgs) Items[e.Index];
			
			//as we do not use the same algorithm to calculate the size of the text (for performance reasons)
			//we need to add some safety margin ( the 0.9 factor ) to ensure that always all text is displayed
			int width = (int)((this.Width - mainTextOffset) * 0.9);
			int height = 300;
			Size sz = new Size(width , height);    

			//sostituisco i caratteri di escape con degli spazi
			//questo perchè l'algoritmo non "riconosce" che sono caratteri di escape
			//e li tratta come caratteri normali, quindi per il momento non sono usabili
			item.MessageText	= item.MessageText.Replace("\n"," ");
			item.MessageText	= item.MessageText.Replace("\t"," ");
			item.MessageText	= item.MessageText.Replace("\r\n", " ");
			
			e.Graphics.MeasureString
				(
					item.MessageText,
					//this.Font,
					messageFont,
					sz,
					StringFormat.GenericDefault,
					out CharsFitted, 
					out LinesFilled
				);
			
			MainTextHeight = LinesFilled * messageFont.Height /*this.Font.Height*/;
			e.ItemHeight = imageList.ImageSize.Height + MainTextHeight + 4;
		}
		#endregion
	}

	/// <summary>
	/// Classe specifica per numerare le Icone
	/// </summary>
	//=========================================================================
	internal class IconsIndexer
	{
		public const int None           = -1;
		public const int Errors			= 0;
		public const int Warnings		= 1;
		public const int Informations	= 2;
	}
}