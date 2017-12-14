// Fred 2006-06-06:
// Flyweight (well-known design pattern) class based on an initial port from 
// (MS) Duncan Mackenzie's work 
// "Using Windows XP Background Intelligent Transfer Service (BITS) with Visual Studio .NET"
// (which could be found at
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnwxp/html/WinXP_BITS.asp)
// ported to C# and extended to support proxies;
// This flyweight class methods represent a rough implementation that does not leverage fully
// on all BITS features (idle bandwidth usage for background scheduled tasks, auto resume, ...),
// but for the first implementation are meant to simply provide an alternative to the used FTP
// protocol which could pass through proxies that require authenticated requests.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

using Microarea.Library.Internet.BitsWrap;

namespace Microarea.Library.Internet
{
	//=========================================================================
	public class BitsDownloader
	{		
		private JobEvents myEvents = new JobEvents();
		private Manager myBits = new Manager();
		
		private string serverUser;
		private string serverPassword;
		
		private string proxyUser;
		private string proxyPassword;
		
		private Hashtable myDownloads = new Hashtable();

		//---------------------------------------------------------------------
		private bool interactive = true;
		public bool Interactive
		{
			get { return this.interactive; }
			set { this.interactive = value; }
		}

		private string proxyAddress;
		public string ProxyAddress
		{
			get { return this.proxyAddress; }
			set { this.proxyAddress = value; }
		}

		//---------------------------------------------------------------------
		public BitsDownloader()
		{
			myEvents.JobError += new JobEvents.JobErrorEventHandler(myEvents_JobError);
			myEvents.JobModification += new JobEvents.JobModificationEventHandler(myEvents_JobModification);
			myEvents.JobTransferred += new JobEvents.JobTransferredEventHandler(myEvents_JobTransferred);
		}

		public IList GetListOfJobs()
		{
			JobCollection myJobs = myBits.GetListofJobs(JobType.CurrentUser);
			return myJobs;
		}

		public IList GetListOfJobs(JobType jobType)
		{
			return myBits.GetListofJobs(jobType);
		}

		public void FixPendingJobs()
		{
			FixPendingJobs(JobType.CurrentUser);
		}
		public void FixPendingJobs(JobType jobType)
		{
			JobCollection myJobs = myBits.GetListofJobs(jobType);

			foreach(Job currentJob in myJobs)
			{
				if (currentJob.State == JobState.Transferred)
					currentJob.Complete();
				else if (currentJob.State == JobState.Errors || 
					currentJob.State == JobState.TransientError ||
					currentJob.State == JobState.Suspended)
					currentJob.Cancel();
			}
		}

		public void SetCredentialsForServer(string serverUser, string serverPassword)
		{
			this.serverUser = serverUser;
			this.serverPassword = serverPassword;
		}

		public void SetCredentialsForProxy(string proxyUser, string proxyPassword)
		{
			this.proxyUser = proxyUser;
			this.proxyPassword = proxyPassword;
		}

		public void BeginDownloadFile(string local, string url, string jobName)
		{
			Job job = myBits.CreateJob(jobName);
			BeginDownloadFile(job, local, url);
		}

		public void BeginDownloadFile(Job job, string local, string url)
		{
			myDownloads.Add(job.ID, new PendingJob(job)); // means not completed

			//Job job = myBits.CreateJob("TestJob");
			job.AddFile(local, url);

			// add proxy settings
			if (this.serverPassword != null)
			{
				ProxySettings ps = job.ProxySettings;
				ps.Usage = ProxyUsage.Override;
				ps.ProxyList = proxyAddress;//"firewall.microarea.it:8080";
				job.ProxySettings = ps;
			}

			// add server credentials
			if (this.serverUser != null)
				job.SetCredentialsForServer(this.serverUser, this.serverPassword);

			// add server credentials
			if (this.proxyUser != null)
				job.SetCredentialsForProxy(this.proxyUser, this.proxyPassword);

			myEvents.AddJob
				(
				job, 
				NotificationTypes.JobError | NotificationTypes.JobTransferred | NotificationTypes.JobModification
				);

			job.ResumeJob();
		}

		// dirty and ugly, it just works. No resume support
		public bool DownloadFile(string local, string url, string jobName, out string stateOnFalse)
		{
			stateOnFalse = null;
			Job job = myBits.CreateJob(jobName);

			if (this.interactive)
				job.Priority = JobPriority.Foreground;

			BeginDownloadFile(job, local, url);

			System.Diagnostics.Debug.WriteLine("state is " + job.State.ToString());

			PendingJob pendingJob = (PendingJob)myDownloads[job.ID];

			while ((job.State == JobState.Connecting || job.State == JobState.Transferring)
				&& !pendingJob.ToAbort);
			while (job.State == JobState.Transferring && 
				!pendingJob.ToAbort);
			bool ack = job.State == JobState.Acknowledged; // for debug only
			if (job.State == JobState.Transferred ||
				job.State == JobState.Acknowledged)
			{
				if (!myDownloads.Contains(job.ID))
					return true; //?
				while (!pendingJob.Completed);
				myDownloads.Remove(job.ID);
				return true;
			}

			stateOnFalse = job.State.ToString();

			if (pendingJob.ToAbort
				|| job.State == JobState.TransientError // no resume supported
				|| job.State == JobState.Suspended)
			{
				try
				{
					pendingJob.Job.Cancel();
					if (pendingJob.ToAbort)
						OnJobCancelled(new JobCancelledEventArgs("Job cancelled after an abort request."));
					else
						OnJobCancelled(new JobCancelledEventArgs("Job cancelled after its state went " + job.State.ToString() + " after Transferred."));
				}
				catch(Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}

			// brute approach: I do no leverage resuming by now
			myDownloads.Remove(job.ID);

			return false;
		}

		public void Abort(string jobName)
		{
			foreach (PendingJob pendingJob in myDownloads.Values)
				if (string.Compare(jobName, pendingJob.Job.DisplayName, true, CultureInfo.InvariantCulture) == 0)
				{
					// don't care about locking
					pendingJob.ToAbort = true;
					return;
				}
		}

		public event JobEvents.JobErrorEventHandler JobError
		{
			add		{ myEvents.JobError += value; }
			remove	{ myEvents.JobError -= value; }
		}

		public event JobEvents.JobModificationEventHandler JobModification
		{
			add		{ myEvents.JobModification += value; }
			remove	{ myEvents.JobModification -= value; }
		}

		public event JobEvents.JobTransferredEventHandler JobTransferred
		{
			add		{ myEvents.JobTransferred += value; }
			remove	{ myEvents.JobTransferred -= value; }
		}

		private void myEvents_JobError(object sender, JobErrorEventArgs e)
		{
			e.Job.Cancel();
			OnJobCancelled(new JobCancelledEventArgs("Job cancelled due to a job error."));
		}

		public event JobCancelledEventHandler JobCancelled;
		protected void OnJobCancelled(JobCancelledEventArgs e)
		{
			if (JobCancelled != null)
				JobCancelled(this, e);
		}

		private void myEvents_JobModification(object sender, JobEventArgs e)
		{
			if (interactive && e.Job.State == JobState.TransientError)
			{
				try
				{
					e.Job.Cancel();
					OnJobCancelled(new JobCancelledEventArgs("Job cancelled after its state went TransientError."));
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}
		}

		private void myEvents_JobTransferred(object sender, JobEventArgs e)
		{
			e.Job.Complete();
			((PendingJob)myDownloads[e.Job.ID]).Completed = true; // means completed
		}

	}

	//=========================================================================
	public class PendingJob
	{
		private bool completed;
		public bool Completed
		{
			get { return this.completed; }
			set { this.completed = value; }
		}

		private bool toAbort;
		public bool ToAbort
		{
			get { return this.toAbort; }
			set { this.toAbort = value; }
		}

		private Job job;
		public Job Job { get { return this.job; } }

		public PendingJob(Job job)
		{
			this.job = job;
		}
	}

	//=========================================================================
	public delegate void JobCancelledEventHandler(object sender, JobCancelledEventArgs e);
	public class JobCancelledEventArgs : EventArgs
	{
		private readonly string message;
		public JobCancelledEventArgs(string message) : base()
		{
			this.message = message;
		}
		public string Message { get { return this.message; } }
	}

	//=========================================================================
}
