using System;
using System.ServiceProcess;
using System.Threading;

namespace Microarea.TaskBuilderNet.TbLoaderService
{
	public partial class TBLoader : ServiceBase
	{
		ManualResetEvent evt = null;
		public TBLoader()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			AsynchronousSocketListener.StartListening();
		}

		protected override void OnStop()
		{
			AsynchronousSocketListener.StopListening();
		}

		internal void TestStartupAndStop(string[] args)
		{
			this.OnStart(args);
			evt = new ManualResetEvent(false);
			Console.WriteLine("Application started. Press CTRL+C to exit.");
			Console.CancelKeyPress += (sender, a)=> { evt.Set(); };
			evt.WaitOne();
			Console.WriteLine("Stopping service, please wait for the ending of all started tasks.");
			this.OnStop();
		}

	}
}
