using System;
using System.IO;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	//============================================================================
	public class ApplicationSemaphore
	{
		/// <summary>
		/// Perform a lock in FileAccess.Read used like a semaphore for installation purposes
		/// </summary>
		/// <param name="filePath">File used for lock purposes</param>
		/// <returns>Returns true if the lock has been succesfully placed, false if the file is 
		/// Write locked by someone else</returns>
		//-----------------------------------------------------------------------------------------
		public static ApplicationLockToken Lock (string filePath)
		{
			return new ApplicationLockToken(filePath); 
		}
	}

	//============================================================================
	public class ApplicationSemaphoreException : ApplicationException
	{
		//--------------------------------------------------------------------------------
		public ApplicationSemaphoreException (string message) : base(message) { }
	}

	//================================================================================
	public class ApplicationLockToken : IDisposable
	{
		private FileStream fs = null;
		#region IDisposable Members

		//--------------------------------------------------------------------------------
		~ApplicationLockToken ()
		{
			Dispose();
		}
		
		//--------------------------------------------------------------------------------
		public ApplicationLockToken (string filePath)
		{
			try
			{
				fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			}
			catch (IOException)
			{
				throw new ApplicationSemaphoreException(GenericStrings.ApplicationLocked);
			}
		}
		//--------------------------------------------------------------------------------
		public void Dispose ()
		{
			if (fs != null)
			{
				try
				{
					fs.Dispose();
				}
				catch
				{
				}
				finally 
				{
					fs = null;
				}
			}
		}

		#endregion
	}
}
