using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Microarea.Library.Internet.BitsWrap
{
	public class Manager
	{
		//Wrapper class that corresponds to IBackgroundCopyManager
		//http://msdn.microsoft.com/library/en-us/bits/refdrz1_3cmq.asp

		//Const S_OK As Integer = 0
		//Const S_FALSE As Integer = 1
		private int DEFAULT_RETRY_PERIOD = 1209600; //20160 minutes (1209600 seconds)
		private int DEFAULT_RETRY_DELAY = 600; // 10 minutes (600 seconds)

		//Set variable to maximum UINT64 value
		private UInt64 BG_SIZE_UNKNOWN = UInt64.Parse("18446744073709551615");

		//Localization
		internal static string[] JobStates = {"Queued", 
												 "Connecting", 
												 "Transferring", 
												 "Suspended", 
												 "Error", 
												 "Transient Error", 
												 "Transferred", 
												 "Acknowledged", 
												 "Cancelled"};

		private IBackgroundCopyManager g_BCM;

		static Manager()
		{
			//Initialize JobStates with localized values
		}

		public Manager()
		{
			try
			{
				//The impersonation level must be at least RPC_C_IMP_LEVEL_IMPERSONATE
				Utilities.CoInitializeSecurity(IntPtr.Zero, -1, 
					IntPtr.Zero, IntPtr.Zero, 
					Utilities.RPC_C_AUTHN_LEVEL_CONNECT, 
					Utilities.RPC_C_IMP_LEVEL_IMPERSONATE, 
					IntPtr.Zero, 0, IntPtr.Zero);
			}
			catch (Exception generic)
			{
				//Localization
				throw new Exception("Error Initializing Security.", generic);
			}

			try
			{
				g_BCM = (IBackgroundCopyManager)new BackgroundCopyManager(); // fred cast
			}
			catch (COMException comX)
			{
				//COM error usually indicates BITS library error
				//Localization
				throw new BitsException("Error Initializing Background Copy Manager.", comX);
			}
		}


		public JobCollection GetListofJobs(JobType whichJobs)
		{
			JobCollection myJobList = new JobCollection();
			IEnumBackgroundCopyJobs jobs;
			IBackgroundCopyJob retrievedJob;
			//Job currentJob;

			//It is no longer normal practice to use hungarian notation,
			//but in this case the data type is the only difference between
			//these two variables.
			UInt32 uintFetched = Convert.ToUInt32(0);
			int intFetched;

			try
			{
				g_BCM.EnumJobs(Convert.ToUInt32(whichJobs), out jobs);
				do
				{
					jobs.Next(Convert.ToUInt32(1), out retrievedJob, out uintFetched);
					intFetched = Convert.ToInt32(uintFetched);
					if (intFetched == 1)
						myJobList.Add(new Job(retrievedJob));
				}
				while (intFetched == 1);
				return myJobList;
			}
			catch (COMException comX)
			{
				//COM error usually indicates BITS library error
				//Localization
				throw new BitsException(string.Format(CultureInfo.InvariantCulture, "Error Enumerating Jobs ({0}).", comX.Message), comX);
			}
		}

		public JobCollection GetListofJobs()
		{
			//Default to returning the current user's jobs
			return GetListofJobs(JobType.CurrentUser);
		}

		public Job CreateJob(string JobName)
		{
			return CreateJob(JobName, "");
		}
		public Job CreateJob(string JobName, string Description)
		{
			return CreateJob(JobName, Description, DEFAULT_RETRY_PERIOD, DEFAULT_RETRY_DELAY);
		}

		public Job CreateJob
			(
			string jobName, 
			string description, 
			Int64 retryPeriod, 
			Int64 retryDelay
			)
		{

			IBackgroundCopyJob newJob;
			Guid newJobID;

			try
			{
				g_BCM.CreateJob(jobName, BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD, out newJobID, out newJob);

				Job myJob = new Job(newJob);
				myJob.Description = description;

				if (retryPeriod != DEFAULT_RETRY_PERIOD)
					myJob.NoProgressTimeout = retryPeriod;

				if (retryDelay != DEFAULT_RETRY_DELAY)
					myJob.MinimumRetryDelay = retryDelay;

				return myJob;
			}
			catch (COMException comX)
			{
				//COM error usually indicates BITS library error
				//Localization
				throw new BitsException(string.Format(CultureInfo.InvariantCulture, "Error Creating Job ({0}).", comX.Message), comX);
			}
		}

		//Retrieve a specific job given its ID
		public Job GetJob(System.Guid jobID)
		{
			try
			{
				IBackgroundCopyJob foundJob;
				g_BCM.GetJob(ref jobID, out foundJob);
				//g_BCM.GetJob(Utilities.ConvertToBITSGUID(JobID), out foundJob);
				return new Job(foundJob);
			}
			catch (COMException comX)
			{
				//COM error usually indicates BITS library error
				//Localization
				throw new BitsException(string.Format(CultureInfo.InvariantCulture, "Error Getting Job ({0}).", comX.Message), comX);
			}
		}
	}
}

