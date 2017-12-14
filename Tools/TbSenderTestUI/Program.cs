using System;
using System.Windows.Forms;

namespace TbSenderTestUI
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			try
			{
				Program.mainForm = new MainForm();

				AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
				Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

				Application.Run(Program.mainForm);
			}
			catch (Exception exc)
			{
				ShowErrorMessage(exc);
			}
		}

		private static MainForm mainForm;

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Exception exc = (Exception)e.ExceptionObject;
			ShowErrorMessage(exc);
		}

		static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			Exception exc = e.Exception;
			ShowErrorMessage(exc);
		}

		private delegate void ShowErrorMessageDelegate(Exception exception);
		private static void ShowErrorMessage(Exception exception)
		{
			if (mainForm != null && mainForm.InvokeRequired)
			{
				object[] parameters = { exception };
				mainForm.Invoke(new ShowErrorMessageDelegate(ShowErrorMessage), parameters);
			}
			else
			{
				MessageBox.Show(mainForm, exception.Message);
				Console.WriteLine(exception.ToString());
			}
		}
	}
}
