using System;
using System.Threading;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	/// <summary>
	/// Summary description for WaitingWindow.
	/// </summary>
	//=====================================================================
	public partial class WaitingWindow : System.Windows.Forms.Form
	{
		private	Thread			thread;
		private	bool			stopThread					= false;
		private int				waitValue;

		//---------------------------------------------------------------------
		public WaitingWindow(string message, int waitValue = 400)
		{
			InitializeComponent();
			PbWaiting.Step = 1;
			this.Cursor = Cursors.WaitCursor;
			if (message == null || message == String.Empty)
				LblMessage.Text = WinControlsStrings.WaitingWindowMessage;
			else
				LblMessage.Text = message;

			this.waitValue = waitValue;
		}

		bool isWaiting = true;
		bool closed = false;
		/// <summary>
		/// Incrementa la progress.
		/// </summary>
		//---------------------------------------------------------------------
		private void ActivateProgress()
		{
			isWaiting = true;
			this.Cursor = Cursors.WaitCursor;
			Application.DoEvents();
			
			try
			{
				//Attendo un secondo prima di mostrare la progress
				System.Threading.Thread.Sleep(2000);
			}
			catch(Exception exc)
			{
				System.Diagnostics.Debug.Fail(exc.Message);
			}
			
			isWaiting = false;
			if (!this.Disposing && closed)
				return;
			Visible = showWindow;

			try
			{
				while (!stopThread || !closed)
				{
					Application.DoEvents();
					Thread.Sleep(waitValue);

					if (Visible != showWindow)
						Visible = showWindow;

					if (!showWindow)
						continue;

					if (PbWaiting.Value == PbWaiting.Maximum)
						PbWaiting.Value = PbWaiting.Minimum;
					PbWaiting.PerformStep();
					Application.DoEvents();
				}
			}
			catch { }
			
			PbWaiting.Value = PbWaiting.Maximum;
			this.Cursor = Cursors.Default;
		}

		/// <summary>
		/// Mostra la finestra
		/// </summary>
		//---------------------------------------------------------------------
		public new void Show()
		{
			if (thread == null)
			{
				thread = new Thread(new ThreadStart(ActivateProgress));
				thread.Start();
			}
			
			Application.DoEvents(); //necessario per poter visualizzare la splash
		}

		bool showWindow = true;
		//---------------------------------------------------------------------
		public void Invisibilize(bool show)
		{
			if (isWaiting && show)
				return;

			showWindow = show;
		}

		delegate void WaitingWindowCloseCallback();
		/// <summary>
		/// Effettua la close della Splash, forzando la progressione della progressBar fino alla fine.
		/// </summary>
		//---------------------------------------------------------------------
		public new void Close()
		{
			// InvokeRequired required compares the thread ID of the calling thread to the 
			// thread ID of the creating thread. If these threads are different, it returns true.
			if (this.InvokeRequired)
			{
				WaitingWindowCloseCallback d = new WaitingWindowCloseCallback(Close);
				this.Invoke(d);
			}
			else
			{
				stopThread = true;
				closed = true;
				base.Close();
			}
		}

		delegate void WaitingWindowSetDescriptionCallback(string text);
		/// <summary>
		/// Effettua la close della Splash, forzando la progressione della progressBar fino alla fine.
		/// </summary>
		//---------------------------------------------------------------------
		public void SetDescription(string text)
		{
			// InvokeRequired required compares the thread ID of the calling thread to the 
			// thread ID of the creating thread. If these threads are different, it returns true.
			if (this.InvokeRequired)
			{
				WaitingWindowSetDescriptionCallback d = new WaitingWindowSetDescriptionCallback(SetDescription);
				this.Invoke(d, new object[] { text });
			}
			else
				LblMessage.Text = text;
		}
	}
}
