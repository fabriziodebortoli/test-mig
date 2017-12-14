using System;
using System.Threading;

namespace Microarea.Library.Internet.BitsWrap
{
	public class JobEvents
	{
		//public delegate void JobEventHandler(object sender, JobEventArgs e);
		public delegate void JobModificationEventHandler(object sender, JobEventArgs e);
		public delegate void JobTransferredEventHandler(object sender, JobEventArgs e);
		public delegate void JobErrorEventHandler(object sender, JobErrorEventArgs e);

        public event JobErrorEventHandler JobError;
        public event JobModificationEventHandler JobModification;
        public event JobTransferredEventHandler JobTransferred;

        private InnerEvents myCallback = new InnerEvents();

        public JobEvents()
		{
			myCallback.JobEvents = this;
		}

		internal void ErrorEvent(Job errorJob, IBackgroundCopyError errorInfo)
		{
			if (JobError != null)
				JobError(this, new JobErrorEventArgs(errorJob, errorInfo));
		}

		protected internal void ModificationEvent(Job modifiedJob)
		{
			if (JobModification != null)
				JobModification(this, new JobEventArgs(modifiedJob));
		}

		protected internal void TransferredEvent(Job transferredJob)
		{
			if (JobTransferred != null)
				JobTransferred(this, new JobEventArgs(transferredJob));
		}

		public void AddJob(Job jobToMonitor, NotificationTypes notifyType)
		{
			jobToMonitor.NotifyInterface = myCallback;
			jobToMonitor.NotifyFlags = notifyType;
		}

		public void AddJob(Job jobToMonitor)
		{
			//'Note that, by default, JobModification is not included
			//'this is a frequently occuring event and should not be
			//'registered for unless it is needed.
			AddJob(jobToMonitor, NotificationTypes.JobError | NotificationTypes.JobTransferred);
		}
	}

	
	/// <summary>
	/// Summary description for InnerEvents.
	/// </summary>
	public class InnerEvents : IBackgroundCopyCallback
	{
        private JobEvents m_jobEvents;

		public JobEvents JobEvents
		{
			get { return m_jobEvents; }
			set { m_jobEvents = value; }
		}

		void IBackgroundCopyCallback.JobError
			(
			IBackgroundCopyJob pJob, 
			IBackgroundCopyError pError
			)
		{
			m_jobEvents.ErrorEvent(new Job(pJob), pError);
		}

		void IBackgroundCopyCallback.JobModification
			(
			IBackgroundCopyJob pJob, 
			System.UInt32 dwReserved
			)
		{
			m_jobEvents.ModificationEvent(new Job(pJob));
		}

		void IBackgroundCopyCallback.JobTransferred(IBackgroundCopyJob pJob)
		{
			m_jobEvents.TransferredEvent(new Job(pJob));
		}
	}

    public class JobEventArgs : System.EventArgs
	{
        private Job m_Job;
        private string m_displayName;

		protected internal JobEventArgs(Job eventJob)
		{
			this.m_Job = eventJob;
			this.m_displayName = eventJob.DisplayName;
		}

		public Job Job { get { return m_Job; } }
		public string JobName { get { return m_displayName; } }
    }


	public class JobErrorEventArgs : JobEventArgs
	{
		private IBackgroundCopyError mErrorInfo;

		//private BG_ERROR_CONTEXT errorContext;
		//private int errorCode;

//		public BG_ERROR_CONTEXT Context
//		{
//			get { return errorContext; }
//		}

//		public int Code
//		{
//			get { return errorCode; }
//		}

		private int GetCurrentLCID()
		{
			return Thread.CurrentThread.CurrentUICulture.LCID;
		}

		public string GetErrorDescription()
		{
			return GetErrorDescription(GetCurrentLCID());
		}
		public string GetErrorContextDescription()
		{
			return GetErrorContextDescription(GetCurrentLCID());
		}

		public string GetErrorDescription(int LCID)
		{
			string errorDesc;
			mErrorInfo.GetErrorDescription(Convert.ToUInt32(LCID), out errorDesc);
			return errorDesc;
		}
		public string GetErrorContextDescription(int LCID)
		{
			string errorContextDesc;
			mErrorInfo.GetErrorContextDescription(Convert.ToUInt32(LCID), out errorContextDesc);
			return errorContextDesc;
		}

		internal JobErrorEventArgs(Job eventJob, IBackgroundCopyError errorInfo)
			: base(eventJob)
		{
			this.mErrorInfo = errorInfo;
		}
	}
}
