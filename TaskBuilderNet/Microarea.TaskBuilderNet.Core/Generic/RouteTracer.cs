using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	/// <summary>
	/// Summary description for RouteTracer.
	/// </summary>
	public class RouteTracer
	{
		//---------------------------------------------------------------------
		private const string tracertFile = "tracert.exe";
		private string output = string.Empty;
		private string error = string.Empty;

		//---------------------------------------------------------------------
		public string Output { get { return this.output; } }
		public string Error { get { return this.error; } }

		//---------------------------------------------------------------------
		public bool Trace(string address)
		{
			lock (this)
			{
				string arguments = address;

				this.output = string.Empty;
				this.error = string.Empty;

				try
				{
					Process myProcess = new Process();
					myProcess.StartInfo.FileName = tracertFile;
					myProcess.StartInfo.CreateNoWindow = true;
					myProcess.StartInfo.UseShellExecute = false;
					myProcess.StartInfo.RedirectStandardError = true;
					myProcess.StartInfo.RedirectStandardOutput = true;
					myProcess.StartInfo.Arguments = arguments;
					myProcess.Start();

					// NOTE (Fred):
					// We have to use StandardOutput before the WaitForExit because if
					// the child process filled the buffer, it would wait forever for the
					// parent process would empty the buffer to terminate, and we would
					// have a deadlock.
					// Same happens for StandardError.
					// To solve the issue, we got to use separate threads fo reading the two streams.
					/*
					http://groups.google.com/groups?hl=it&lr=&ie=UTF-8&threadm=ghWd
					9%23XjDHA.2456%40cpmsftngxa06.phx.gbl&rnum=2&prev=/groups%3Fas_
					q%3DRedirectStandardOutput%2520block%2520thread%26ie%3DUTF-8%26
					as_ugroup%3Dmicrosoft.public.dotnet.*%26lr%3D%26num%3D50%26hl%3
					Dit
					*/
					ThreadOutputHolder outHolder = new ThreadOutputHolder(myProcess.StandardOutput);
					ThreadOutputHolder errHolder = new ThreadOutputHolder(myProcess.StandardError);
					Thread outThread = new Thread(new ThreadStart(outHolder.Read));
					Thread errThread = new Thread(new ThreadStart(errHolder.Read));
					outThread.Start();
					errThread.Start();

					myProcess.WaitForExit();
					outThread.Join();
					errThread.Join();
					myProcess.Close();

					this.output = outHolder.Result;
					this.error = errHolder.Result;

					//if (output == string.Empty)
					//	//error

					//if (error != string.Empty)
					//	//error

					return true;
				}
				catch (Exception exc)
				{
					this.error = exc.Message;
					return false;
				}
			}
		}

		//---------------------------------------------------------------------
	}

	//=========================================================================
	public class ThreadOutputHolder
	{
		public string Result = string.Empty;
		private StreamReader stream;

		public ThreadOutputHolder(StreamReader stream)
		{
			this.stream = stream;
		}

		public void Read()
		{
			this.Result = stream.ReadToEnd();
			//Thread.CurrentThread.Abort();	// qui è sicuro e evita di fare un Thread.Join
			return;
		}
	}
}
