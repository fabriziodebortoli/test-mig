using System;

namespace Microarea.Library.Internet.BitsWrap
{
	public class JobProgress
	{
        //Wrapper class for the _BG_JOB_PROGRESS structure
        //http://msdn.microsoft.com/library/en-us/bits/refdrz1_9tmb.asp

        private _BG_JOB_PROGRESS m_Progress;
        internal JobProgress(_BG_JOB_PROGRESS jobProgress)
		{
            m_Progress = jobProgress;
        }

        public Decimal BytesTotal
		{
            get
			{
                return Convert.ToDecimal(m_Progress.BytesTotal);
            }
        }

        public Decimal BytesTransferred
		{
            get
			{
                return Convert.ToDecimal(m_Progress.BytesTransferred);
            }
        }

        public Decimal FilesTotal
		{
            get
			{
                return Convert.ToDecimal(m_Progress.FilesTotal);
            }
        }

        public Decimal FilesTransferred
		{
            get
			{
                return Convert.ToDecimal(m_Progress.FilesTransferred);
            }
        }
    }

    public class FileProgress
	{
        private _BG_FILE_PROGRESS m_Progress;

        internal FileProgress(_BG_FILE_PROGRESS fileProgress)
		{
            m_Progress = fileProgress;
        }

        public Decimal BytesTotal
		{
            get
			{
                return Convert.ToDecimal(m_Progress.BytesTotal);
            }
        }

        public Decimal BytesTransferred
		{
            get
			{
                return Convert.ToDecimal(m_Progress.BytesTransferred);
            }
        }

        public bool Completed
		{
            get
			{
                return m_Progress.Completed != 0;
            }
        }

    }
}