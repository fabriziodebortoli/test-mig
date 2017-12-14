using System;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.UI.WinControls.Dock
{
	partial class TBDockContent<T>
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
					components.Dispose();

				try
				{
					if (hosting != null)
					{
						ExternalAPI.DestroyIcon(hosting.Icon.Handle);
						//hosting.Dispose();
						hosting = null;
					}

					if (hostedControl != null)
					{
						hostedControl.TextChanged -= new EventHandler(hostedControl_TextChanged);
						hostedControl.Dispose();
						hostedControl = null;
					}

					//trick: aggiunto per annullare la variabile cachedLayoutEventArgs
					//che mi tiene in vita il controllo ospitato e impedisce al garbage collector di rimuoverlo
					this.PerformLayout();
				}
				catch { }
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Text = "DockContent";
		}

		#endregion
	}
}