using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=========================================================================
	/// <summary>
	/// UIClickablePanel
	/// 
	/// </summary>
	/// <remarks>This class simulates a button without causing a focus loss on the Extendee
	/// </remarks>
	[ToolboxItem(true)]
	public class UIClickablePanel : UIPanel
	{
		UIBorderStyle borderStyle = UIBorderStyle.None;
		
		//-------------------------------------------------------------------------
		public UIClickablePanel() 
		{
			this.BackgroundImageLayout = ImageLayout.Stretch;
			this.TabStop = false;
		}

		//-------------------------------------------------------------------------
		public UIBorderStyle BorderStyle
		{
			get
			{
				return borderStyle;
			}
			set
			{
				borderStyle = value;
				UpdateStyles();
			}
		}

		//-------------------------------------------------------------------------
		protected override System.Windows.Forms.CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle &= -513;
				createParams.Style &= -8388609;
				switch (this.BorderStyle)
				{
					case UIBorderStyle.Single:
						createParams.Style |= 0x800000;
						return createParams;

					case UIBorderStyle.Fixed3D:
						createParams.ExStyle |= 0x200;
						return createParams;
					case UIBorderStyle.None:
						return createParams;
				}
				return createParams;
			}
		}

		//-------------------------------------------------------------------------
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (!this.Enabled)
			{
				this.BorderStyle = UIBorderStyle.None;
				return;
			}

			this.BorderStyle = UIBorderStyle.Fixed3D;
		}

		//-------------------------------------------------------------------------
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (!this.Enabled)
			{
				this.BorderStyle = UIBorderStyle.None;
				return;
			}

			this.BorderStyle = UIBorderStyle.Single;
		}

		//-------------------------------------------------------------------------
		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);

			if (!this.Enabled)
			{
				this.BorderStyle = UIBorderStyle.None;
				return;
			}

			this.BorderStyle = UIBorderStyle.Single;
		}

		//-------------------------------------------------------------------------
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			this.BorderStyle = UIBorderStyle.None;
		}
	}
}
