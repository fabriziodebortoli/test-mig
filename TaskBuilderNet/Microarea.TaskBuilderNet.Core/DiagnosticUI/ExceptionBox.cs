using System;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
	/// <summary>
	/// Permette di catturare le eccezioni non gestite dall'applicazione.
	/// </summary>
	/// <example>
	/// static void Main(string[] args)
	/// {
	///		...
	///		Application.ThreadException
	///			+= new ThreadExceptionEventHandler(ExceptionBox.ShowExceptionBox);
	///		Application.Run(new MainForm());
	///		...
	///	}
	/// </example>
	public partial class ExceptionBox : Form
	{
		private string exceptionString;

		#region Constructors
		//---------------------------------------------------------------------
		public ExceptionBox(string exceptionString)
		{
			this.exceptionString = exceptionString;
			InitializeComponent();
			exceptionTextBox.Text = GetErrorString();
		}
		#endregion
		
		//---------------------------------------------------------------------
		private string GetErrorString()
		{
			return
				"Exception thrown:" + Environment.NewLine +
				exceptionString +
				Environment.NewLine + Environment.NewLine +
				".NET Version\t: " + Environment.Version.ToString() + Environment.NewLine +
				"OS Version\t: " + Environment.OSVersion.ToString() + Environment.NewLine +
				"Boot Mode\t: " + SystemInformation.BootMode + Environment.NewLine +
				"Working Set Memory\t: " + (Environment.WorkingSet / 1024) + " Kb" + Environment.NewLine + Environment.NewLine;
		}

		//---------------------------------------------------------------------
		public static void ShowExceptionBox(object sender, Exception exception)
		{
			ShowExceptionBox(sender, exception.ToString());
		}

		//---------------------------------------------------------------------
		public static void ShowExceptionBox(object sender, string exceptionString)
		{
			DialogResult result = new ExceptionBox(exceptionString).ShowDialog();

			switch (result)
			{
				case DialogResult.Ignore:
					break;
				case DialogResult.Abort:
					Application.Exit();
					break;
				default:
					break;
			}
		}
	}
}