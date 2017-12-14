using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Microarea.Library.Internet.BitsWrap
{
	/// <summary>
	/// Wrapper class that corresponds to IBackgroundCopyJob
	/// </summary>
	public class Job
	{
		internal Job(IBackgroundCopyJob bitsJob)
		{
            this.m_Job = bitsJob;
        }

        internal IBackgroundCopyJob m_Job;

		public override string ToString()
		{
			//Override default ToString behaviour to support direct use in
			//Console.WriteLine and other locations where ToString is used.
			return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", this.DisplayName, this.StateString);
		}

//		public DateTime CreationTime()
//		{
//			//Time the Job was created
//			//http://msdn.microsoft.com/library/en-us/bits/refdrz1_6r3n.asp
//			_BG_JOB_TIMES times;
//			m_Job.GetTimes(out times);
//
//			return ConvertToDateTime(times.CreationTime);
//		}

		public BitsError GetError()
		{
			IBackgroundCopyError err;
			m_Job.GetError(out err);

			if (err != null)
				return new BitsError(err);
			else
				return null;
		}

//		public DateTime ModificationTime()
//		{
//			//Time the job was last modified or bytes were transferred.
//			//http://msdn.microsoft.com/library/en-us/bits/refdrz1_6r3n.asp
//			_BG_JOB_TIMES times;
//			m_Job.GetTimes(out times);
//			return ConvertToDateTime(times.ModificationTime);
//		}

//        public DateTime TransferCompletionTime()
//		{
//            //Time the job entered the Transferred state (Job.State)
//            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_6r3n.asp
//            _BG_JOB_TIMES times;
//            m_Job.GetTimes(out times);
//            return ConvertToDateTime(times.TransferCompletionTime);
//        }

//        private DateTime ConvertToDateTime(_FILETIME v)
//		{
//            //Utility Function to Convert from the _FILETIME structures returned
//            //by BITS into a Sytem.DateTime value usable by .NET programs.
//            //Localization
//            byte[] fileTime = new byte[7];
//            //Dim fileTime(7) As Byte
//			fileTime.cop
//			Array.Copy(fileTime
//            fileTime.Copy(BitConverter.GetBytes(v.dwHighDateTime), 0, fileTime, 4, 4)
//
//            fileTime.Copy(BitConverter.GetBytes(v.dwLowDateTime), 0, fileTime, 0, 4)
//            return DateTime.FromFileTime(BitConverter.ToInt64(fileTime, 0))
//        }

        public FileCollection Files()
			{
            //Returns a collection of all the Files in this particular job
            //Uses the IBackgroundCopyJob::EnumFiles routine
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_3v77.asp

            FileCollection myFileList = new FileCollection();
            IEnumBackgroundCopyFiles jobFiles;
            IBackgroundCopyFile retrievedFile;
            //BITSFile currentFile;

            //It is no longer normal practice to use hungarian notation,
            //but in this case the data type is the only difference between
            //these two variables.
            UInt32 uintFetched = Convert.ToUInt32(0);
            int intFetched;
            m_Job.EnumFiles(out jobFiles);
            do
			{
                jobFiles.Next(Convert.ToUInt32(1), out retrievedFile, out uintFetched);
                intFetched = Convert.ToInt32(uintFetched);
                if (intFetched == 1)
                    myFileList.Add(new BITSFile(retrievedFile));
			}
            while (intFetched == 1);
            return myFileList;
        }

		public void Cancel()
		{
			//Possible Errors
			//BG_S_UNABLE_TO_DELETE_FILES    Job was successfully canceled; however, the service was unable to delete the temporary files associated with the job. 
			//BG_E_INVALID_STATE             Cannot cancel a job whose state is BG_JOB_STATE_CANCELLED or BG_JOB_STATE_ACKNOWLEDGED.
			//Corresponds to IBackgroundCopyJob::Cancel
			//http://msdn.microsoft.com/library/en-us/bits/refdrz1_02i4.asp
			m_Job.Cancel();
		}

        public void Complete()
		{
            //Corresponds to IBackgroundCopyJob::Complete
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_0o6d.asp
            m_Job.Complete();
        }

        public void Suspend()
		{
            //Corresponds to IBackgroundCopyJob::Suspend
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_5sdg.asp
            //TODO Check if job is already suspended?
            m_Job.Suspend();
        }

        public void ResumeJob()
		{
            //Corresponds to IBackgroundCopyJob::Resume
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_8c2t.asp
            m_Job.Resume();
        }

        public void TakeOwnership()
		{
            //Corresponds to IBackgroundCopyJob::TakeOwnership
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_3els.asp
            //Converts ownership to the current user
            m_Job.TakeOwnership();
        }

        public void AddFile(string localFileName, string remoteFileName)
		{
            //Corresponds to IBackgroundCopyJob::AddFile
            //Note: AddFileSet not implemented in this wrapper
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_406d.asp
            //TODO Check if local file already exists?
			try
			{
				m_Job.AddFile(remoteFileName, localFileName);
			}
			catch (COMException comX)
			{
				//COM error usually indicates BITS library error
				//Localization
				throw new BitsException(string.Format(CultureInfo.InvariantCulture, "Error Adding File ({0}).", comX.Message), comX);
			}
        }

        public string OwnerSID
		{
            //BITS only returns the SID, SIDUtilities.LookupSID is used
            //to convert into an account name.
            //Corresponds to IBackgroundCopyJob::GetOwner
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_3prm.asp
            get
			{
                string SID;
                try
				{
                    m_Job.GetOwner(out SID);
					return SID;
				}
                catch (COMException comX)
				{
                    //COM Error, indicates error in BITS call, likely one of these;
                    //E_INVALIDARG   SID cannot be null 
                    //E_ACCESSDENIED User does not have permission to 
                    //               retrieve the ownership information for the job. 
                    throw new BitsException(string.Format(CultureInfo.InvariantCulture, "Error Getting Owner SID ({0}).", comX.Message), comX);
                }
            }
        }

        public string GetOwnerName()
		{
            //See OwnerSID above
			try
			{
				string SID = this.OwnerSID;
				return Utilities.GetAccountName(SID);
			}
			catch (COMException comException)
			{
				//COM Error, indicates error in BITS call 
				throw new Exception(comException.Message, comException);
			}
			catch (Exception genericException)
			{
				throw new Exception(genericException.Message, genericException);
			}
        }

        public System.Guid ID
		{
            //Corresponds to IBackgroundCopyJob::GetId
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_1p5w.asp
            get
			{
				try
				{
					Guid jobID;
					m_Job.GetId(out jobID);
					return jobID;
//					BackgroundCopyManager.GUID jobID;
//					m_Job.GetId(out jobID);
//					return Utilities.ConvertToGUID(jobID);
					//BITS returns a GUID structure, which ConvertToGUID converts into
					//a System.GUID for ease of use within .NET
				}
				catch (COMException comX)
				{
					//COM error usually indicates BITS library error
					//Localization
					throw new BitsException(string.Format(CultureInfo.InvariantCulture, "Error Getting Job ID ({0}).", comX.Message), comX);
				}
            }
        }

        public JobState State
		{
            //Corresponds to IBackgroundCopyJob::GetState
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_444l.asp
            get
			{
                try
				{
                    BG_JOB_STATE bgState;
                    m_Job.GetState(out bgState);
					return (JobState)bgState;
//					return CType(bgState, JobState);
				}
                catch (COMException comX)
				{
                    //COM error usually indicates BITS library error
                    //Localization
                    throw new BitsException(string.Format(CultureInfo.InvariantCulture, "Error Getting Job State ({0}).", comX.Message), comX);
                }
            }
        }

        public string StateString
		{
            //Returns the state (see State() property above) as a string
            get
			{
				try
				{
					JobState myState;
					string myStateString;
					myState = this.State;
					myStateString = Manager.JobStates[(int)myState];
					return myStateString;
				}
				catch (COMException comX)
				{
					//COM error usually indicates BITS library error
					//Localization
					throw new BitsException(string.Format(CultureInfo.InvariantCulture, "Error Getting Job State ({0}).", comX.Message), comX);
				}
            }
        }

        public JobProgress Progress
		{
            //Corresponds to IBackgroundCopyJob::GetProgress
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_9ewj.asp
            get
			{
                try
				{
                    _BG_JOB_PROGRESS tmpProgress;
                    m_Job.GetProgress(out tmpProgress);
                    return new JobProgress(tmpProgress);
				}
                catch (COMException comX)
				{
                    //COM error usually indicates BITS library error
                    //Localization
                    throw new BitsException(string.Format(CultureInfo.InvariantCulture, "Error Getting Job Progress ({0}).", comX.Message), comX);
                }
            }
        }

        public Int64 ErrorCount
		{
            //sometimes called GetInterruptionCount in the online SDK
            //Corresponds to IBackgroundCopyJob::GetErrorCount
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_8flg.asp
            get
			{
                UInt32 countErrors;
                m_Job.GetErrorCount(out countErrors);
                return Convert.ToInt64(countErrors);
            }
        }

        public string DisplayName
		{
            //Corresponds to IBackgroundCopyJob::GetDisplayName/SetDisplayName
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_604l.asp (GET)
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_0gpx.asp (SET)
            get
			{
				try
				{
					string jobName;
					m_Job.GetDisplayName(out jobName);
					return jobName;
				}
				catch (COMException comX)
				{
					//COM error usually indicates BITS library error
					//Localization
					throw new BitsException(string.Format(CultureInfo.InvariantCulture, "Error Getting Job Display Name ({0}).", comX.Message), comX);
				}
            }
            set
			{
				try
				{
					m_Job.SetDisplayName(value);
				}
				catch (COMException comX)
				{
					//COM error usually indicates BITS library error
					//Localization
					throw new BitsException(string.Format(CultureInfo.InvariantCulture, "Error Setting Job Name ({0}).", comX.Message), comX);
				}
            }
        }

        public string Description
		{
            //Corresponds to IBackgroundCopyJob::GetDescription/SetDescription
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_8h7y.asp (GET)
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_2xta.asp (SET)
            get
			{
                string jobDesc;
                m_Job.GetDescription(out jobDesc);
                return jobDesc;
            }
            set
			{
                m_Job.SetDescription(value);
            }
        }

        internal IBackgroundCopyCallback NotifyInterface
		{
            //Corresponds to IBackgroundCopyJob::GetNotifyInterface/SetNotifyInterface
            //Retrieves the current object set for callbacks on events
            //Nothing if no callback has been set.
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_849x.asp (GET)
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_0q79.asp (SET)
            //The JobEvents class wraps this functionality, so this property
            //is only available within the BITS assembly
            get
			{
                IBackgroundCopyCallback notify;
                object callback;
                m_Job.GetNotifyInterface(out callback);
                notify = (IBackgroundCopyCallback)callback;
                return notify;
            }
            set
			{
                object callback;
                callback = (object)value;
                m_Job.SetNotifyInterface(callback);
            }
        }

        protected internal NotificationTypes NotifyFlags
		{
            //Corresponds to IBackgroundCopyJob::GetNotifyFlags/SetNotifyFlags
            //See NotifyInterface above
            //These Flags control what events are sent to the callback interface
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_0s6r.asp (GET)
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_58s3.asp (SET)
            get
			{
                //NotificationTypes flags;
                UInt32 v;
                m_Job.GetNotifyFlags(out v);
                return (NotificationTypes)Convert.ToInt32(v);
            }
            set
			{
                UInt32 uintvalue;
                uintvalue = Convert.ToUInt32(value);
                m_Job.SetNotifyFlags(uintvalue);
            }
        }

        public JobPriority Priority
		{
            //Corresponds to IBackgroundCopyJob::SetPriority/GetPriority
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_2fp5.asp (GET)
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_9sah.asp (SET)
            get
			{
                BG_JOB_PRIORITY myPriority;
                m_Job.GetPriority(out myPriority);
                return (JobPriority)myPriority;
            }
            set
			{
                m_Job.SetPriority((BG_JOB_PRIORITY)value);
            }
        }

        public ProxySettings ProxySettings
		{
            //Corresponds to IBackgroundCopyJob::GetProxySettings/SetProxySettings
            //Wrapped using the ProxySettings class
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_7usz.asp (GET)
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_9q2b.asp (SET)
            get
			{
                BG_JOB_PROXY_USAGE jobUsage;
                string jobProxyList;
                string jobProxyBypassList;
                m_Job.GetProxySettings(out jobUsage, out jobProxyList, out jobProxyBypassList);
                return new ProxySettings(jobUsage, jobProxyList, jobProxyBypassList);
            }
            set
			{
                BG_JOB_PROXY_USAGE jobUsage;
                jobUsage = (BG_JOB_PROXY_USAGE)value.Usage;
                m_Job.SetProxySettings(jobUsage, value.ProxyList, value.ProxyBypassList);
            }
        }

		// BY FRED
		public void SetCredentialsForServer(string usr, string pwd)
		{
			//http://msdn.microsoft.com/library/default.asp?url=/library/en-us/bits/bits/ibackgroundcopyjob2_setcredentials.asp
			BG_AUTH_CREDENTIALS ac;
			ac.Target = BG_AUTH_TARGET.BG_AUTH_TARGET_SERVER;
			ac.Scheme = BG_AUTH_SCHEME.BG_AUTH_SCHEME_BASIC;
			ac.Credentials.Basic.UserName = usr;
			ac.Credentials.Basic.Password = pwd;
			((IBackgroundCopyJob2)m_Job).SetCredentials(ref ac);
		}

		// BY FRED
		public void SetCredentialsForProxy(string usr, string pwd)
		{
			// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/bits/bits/ibackgroundcopyjob2_setcredentials.asp
			IBackgroundCopyJob2 job2 = ((IBackgroundCopyJob2)m_Job);

			BG_AUTH_CREDENTIALS ac1;
			ac1.Target = BG_AUTH_TARGET.BG_AUTH_TARGET_PROXY;
			ac1.Scheme = BG_AUTH_SCHEME.BG_AUTH_SCHEME_BASIC;
			ac1.Credentials.Basic.UserName = usr;
			ac1.Credentials.Basic.Password = pwd;
			job2.SetCredentials(ref ac1);

			BG_AUTH_CREDENTIALS ac2;
			ac2.Target = BG_AUTH_TARGET.BG_AUTH_TARGET_PROXY;
			ac2.Scheme = BG_AUTH_SCHEME.BG_AUTH_SCHEME_DIGEST;
			ac2.Credentials.Basic.UserName = usr;
			ac2.Credentials.Basic.Password = pwd;
			job2.SetCredentials(ref ac2);

			BG_AUTH_CREDENTIALS ac3;
			ac3.Target = BG_AUTH_TARGET.BG_AUTH_TARGET_PROXY;
			ac3.Scheme = BG_AUTH_SCHEME.BG_AUTH_SCHEME_NEGOTIATE;
			ac3.Credentials.Basic.UserName = usr;
			ac3.Credentials.Basic.Password = pwd;
			job2.SetCredentials(ref ac3);

			BG_AUTH_CREDENTIALS ac4;
			ac4.Target = BG_AUTH_TARGET.BG_AUTH_TARGET_PROXY;
			ac4.Scheme = BG_AUTH_SCHEME.BG_AUTH_SCHEME_NTLM;
			ac4.Credentials.Basic.UserName = usr;
			ac4.Credentials.Basic.Password = pwd;
			job2.SetCredentials(ref ac4);
		}

        public Int64 MinimumRetryDelay
		{
            //Corresponds to IBackgroundCopyJob::SetMinimumRetryDelay/GetMinimumRetryDelay
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_6im1.asp (GET)
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_377d.asp (SET)
            get
			{
                UInt32 delay;
                m_Job.GetMinimumRetryDelay(out delay);
                return Convert.ToInt64(delay);
            }
            set
			{
				UInt32 delay = Convert.ToUInt32(value);
                m_Job.SetMinimumRetryDelay(delay);
            }
        }
        public Int64 NoProgressTimeout
		{
            //Corresponds to IBackgroundCopyJob::GetNoProgressTimeout/SetNoProgressTimeout
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_09tg.asp (GET)
            //http://msdn.microsoft.com/library/en-us/bits/refdrz1_6yes.asp (SET)
            get
			{
                UInt32 timeout;
                m_Job.GetNoProgressTimeout(out timeout);
                return Convert.ToInt64(timeout);
            }
            set
			{
                UInt32 timeout = Convert.ToUInt32(value);
                m_Job.SetNoProgressTimeout(timeout);
            }
        }
    }

}
