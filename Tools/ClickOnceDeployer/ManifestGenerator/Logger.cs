using System;
using System.Threading;
using System.Windows.Forms;

namespace ManifestGenerator
{
	//================================================================================
	class Logger : ManifestGenerator.ILogger
	{
		Output outputWindow;
		bool running = false;

		#region ILogger Members
		//--------------------------------------------------------------------------------
		public void WriteLine (string message, params object[] args)
		{
			//Inserito try/catch vuoto perchè con determinate stringhe di errore nel messaggio
			//(vedi ad esempio errori sulle chiamate a LoginManager.ReloadConfiguration)
			// si hanno delle FormatException sul WriteLine
			try
			{
				Console.Out.WriteLine(message, args);
			}
			catch (Exception exc)
			{
				//Loggo in Console in maniera che rimanga almeno nel log file
				Console.Out.WriteLine(exc.ToString());
			}

			outputWindow.Invoke(
				new ThreadStart(
					() => {
						try
						{
							outputWindow.ShortDescriptionLabel.Text = string.Format(message, args);
						}
						catch (Exception exc)
						{
							//Loggo in Console in maniera che rimanga almeno nel log file
							Console.Out.WriteLine(exc.ToString());
						}
					}
				)
			);

			if (running)
				outputWindow.Invoke(new ThreadStart(() =>
					{
						try
						{
							outputWindow.LogTextBox.AppendText(string.Format(message, args));
							outputWindow.LogTextBox.AppendText(Environment.NewLine);
						
						}
						catch (Exception exc)
						{
							//Loggo in Console in maniera che rimanga almeno nel log file
							Console.Out.WriteLine(exc.ToString());
						}
					}));
		}

		//--------------------------------------------------------------------------------
		public void Start ()
		{
			using (ManualResetEvent evt = new ManualResetEvent(false))
			{
				Thread actionThread = new Thread(a =>
				{
					StartOutputThread(evt);

				});
				actionThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				actionThread.Start();
				evt.WaitOne();
			}
		}

		//--------------------------------------------------------------------------------
		private void StartOutputThread (ManualResetEvent evt)
		{
			outputWindow = new Output();
			outputWindow.FormClosing += new FormClosingEventHandler(outputWindow_FormClosing);
			outputWindow.Disposed += new EventHandler(outputWindow_Disposed);
			outputWindow.HandleCreated += new EventHandler((object sender, EventArgs args) =>
			{
				evt.Set();
			});

			outputWindow.Show();
			running = true;

			while (running)
				Application.DoEvents();
		}

		//--------------------------------------------------------------------------------
		void outputWindow_Disposed (object sender, EventArgs e)
		{
			running = false;
		}

		//--------------------------------------------------------------------------------
		void outputWindow_FormClosing (object sender, FormClosingEventArgs e)
		{
			running = false;
		}

		//--------------------------------------------------------------------------------
		public void Stop ()
		{
			outputWindow.BeginInvoke(new ThreadStart(() =>
			{
				if (outputWindow != null)
				{
					outputWindow.Close();
					outputWindow = null;
				}
			}));
		}

		//--------------------------------------------------------------------------------
		public void PerformStep ()
		{
			outputWindow.BeginInvoke(new ThreadStart(() =>
			{
				outputWindow.PerformStep();
			}));
		}

		//--------------------------------------------------------------------------------
		public void SetProgressTop (int top)
		{
			outputWindow.Invoke(new ThreadStart(() =>
			{
				outputWindow.SetProgressTop(top);
			}));
		}

		#endregion
	}
}
