using Microarea.TaskBuilderNet.Core.NameSolver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
	public enum ImageType { Flag=0, None=1};
	public enum NotificationStatus { Disconnected=0, Connected=1, IsConnecting=2, UnKnown=3};

	/// <summary>
	/// Menu item con numeri di notifiche
	/// </summary>
	public class NotificationMenuItem : System.Windows.Forms.ToolStripMenuItem
	{
		private int _number;
		private ImageType imageType;
		private NotificationStatus notificationStatus;

		public NotificationMenuItem(ImageType ImgType = ImageType.Flag) 
		{
			this.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
			this.imageType = ImgType;
			SetStatus(NotificationStatus.UnKnown);
			
			// Disabilito l'AutoToolTip a favore di uno personalizzato all'interno della OnPaint
			this.AutoToolTip = false;
			this.DropDown.DropShadowEnabled = true;

			//Chiudo il menu quando esco dalla sua area con il mouse
			//this.DropDown.MouseLeave += (sender, args) => { this.DropDown(); };

			//style
			this.Font = new Font("Segoe UI", 10);
			//this.ForeColor = Color.Black;
			//ITheme theme = DefaultTheme.GetTheme();
			//this.BackColor = theme.GetThemeElementColor("StatusbarBkgColor");
			//this.ForeColor = theme.GetThemeElementColor("StatusbarForeColor");
		}

		public int Number
		{
			get { return _number; }
			set
			{
				if(_number == value)
					return;
				_number = value;
				this.Invalidate();
			}
		}

		public void SetStatus(NotificationStatus status) 
		{
			notificationStatus = status;
			
			//per adesso gestisco solo il tipo immagine "flag", ovvero la bandiera
			if(imageType != ImageType.Flag)
			{
				this.Image = null;
				this.ForeColor = Color.Black;
				return;
			}
			switch(status)
			{
				case NotificationStatus.Disconnected:
					this.Image = Properties.Resources.flag_20_red;
					this.ForeColor = Color.Black;
					break;
				case NotificationStatus.Connected:
					this.Image = Properties.Resources.flag_20_green;
					this.ForeColor = Color.Black;
					break;
				case NotificationStatus.IsConnecting:
					this.Image = Properties.Resources.flag_20_yellow;
					this.ForeColor = Color.Black;
					break;
				case NotificationStatus.UnKnown:
					this.Image = Properties.Resources.flag_20_black;
					this.ForeColor = Color.White;
					break;
			}
			this.Invalidate();
		}

		/// <summary>
		/// Metodo utilizzato per scrivere rappresentare graficamente il valore della property Number (se numero >0)
		/// </summary>
		/// <param name="pEvent"></param>
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs pEvent)
		{
			base.OnPaint(pEvent);
			Graphics graphics = pEvent.Graphics;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;

			if(Number == 0)
			{
				//scommentare questa riga e il successivo commento per disabilitare il bottone quando non ci sono notifiche
				//this.Enabled = false;
				return;
			}
			//this.Enabled = true;

			string text = Number < 100 ? Number.ToString() : "99+";
			var newFont = new System.Drawing.Font(FontFamily.GenericSansSerif, 7, FontStyle.Regular);
			Brush brush = new SolidBrush(this.ForeColor);
			int x		= Number < 10 ? 10 : 8;
			int y		= 2;
			
			graphics.DrawString(text, newFont, brush, x, y);
			this.ToolTipText = string.Format(MenuManagerWindowsControlsStrings.NotificationButtonTooltip,Number.ToString());
		}

	}
}
