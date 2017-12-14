using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.Console.Core.SecurityLibrary
{
	/// <summary>
	/// Summary description for EllipsisLabel.
	/// </summary>
	public class EllipsisLabel : System.Windows.Forms.Label
	{
		private const int X_IMAGE_OFFSET = 2;
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public new System.Windows.Forms.FlatStyle FlatStyle { get { return System.Windows.Forms.FlatStyle.Standard; } }
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public new bool AutoSize { get { return false; }  set { base.AutoSize = false; } }

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public new System.Drawing.ContentAlignment TextAlign 
		{
			get { return ContentAlignment.MiddleLeft; } 
			set { base.TextAlign = ContentAlignment.MiddleLeft; } 
		}

		//---------------------------------------------------------------------------
		[DefaultValue(System.Drawing.ContentAlignment.MiddleLeft), Category("Appearance")]
		public new System.Drawing.ContentAlignment ImageAlign 
		{
			get 
			{
				if (base.ImageAlign != ContentAlignment.MiddleLeft && base.ImageAlign != ContentAlignment.MiddleRight)
					base.ImageAlign = ContentAlignment.MiddleLeft;
				return base.ImageAlign;
			} 
			set 
			{
				if (value != ContentAlignment.MiddleLeft && value != ContentAlignment.MiddleRight)
					base.ImageAlign = ContentAlignment.MiddleLeft;
				base.ImageAlign = value;
			} 
		}

		//---------------------------------------------------------------------------
		protected override void InitLayout()
		{
			base.InitLayout();

			base.AutoSize = false;
			// Se non forzo il FlatStyle a standard e qualcuno me lo mette a System
			// non si va più nel OnPaint da me reimplementato!!!
			base.FlatStyle = System.Windows.Forms.FlatStyle.Standard;

			this.ForeColor = Color.Navy;
		}
		
		//--------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaintBackground(e);

			if (e == null || e.Graphics == null)
				return;

			System.Drawing.SolidBrush transparentBrush = new SolidBrush(this.BackColor);
			e.Graphics.FillRectangle(transparentBrush, this.ClientRectangle);
			transparentBrush.Dispose();

			System.Drawing.Rectangle textRect = new System.Drawing.Rectangle(0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height);
			
			if (this.Image != null)
			{
				int imageTopCoord = textRect.Top + (textRect.Height - this.Image.Height) / 2;
				if (this.ImageAlign == ContentAlignment.MiddleLeft)
				{
					e.Graphics.DrawImageUnscaled(this.Image, 0, imageTopCoord, this.Image.Width, this.Image.Height);
					if (this.Image.Width >= textRect.Width)
						return;
					textRect.X += (this.Image.Width + X_IMAGE_OFFSET);
				}
				else if (this.ImageAlign == ContentAlignment.MiddleRight)
				{
					e.Graphics.DrawImageUnscaled(this.Image, (textRect.Width - this.Image.Width), imageTopCoord, this.Image.Width, this.Image.Height);
					if (this.Image.Width >= textRect.Width)
						return;
					textRect.Width -= (this.Image.Width + X_IMAGE_OFFSET);
				}
			}

			System.Drawing.RectangleF labelRectF = new System.Drawing.RectangleF((float)textRect.Left, (float)textRect.Top, (float)textRect.Width, (float)this.FontHeight);

			labelRectF.Y += (labelRectF.Height - (float)this.FontHeight) / 2;

			StringFormat format = new StringFormat();
			format.Trimming = StringTrimming.EllipsisWord;
			format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit;
			format.LineAlignment = StringAlignment.Center;
			
			System.Drawing.SolidBrush textBrush = new SolidBrush(this.ForeColor);
			e.Graphics.DrawString(this.Text, this.Font, textBrush, labelRectF, format);	
			textBrush.Dispose();
		}	
	}
}
