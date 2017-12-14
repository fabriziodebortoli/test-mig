using System;
using System.Diagnostics;
using System.IO;

namespace Microarea.EasyBuilder.DBScript
{
	/// <summary>
	/// Summary description for BackupFiles.
	/// </summary>
	public class TempFile : IDisposable
	{
		private string mError = string.Empty;
		private string mFilename = string.Empty;
		private string mTempFilename = string.Empty;

		/// <remarks/>
		public TempFile(string aFilename)
		{
			mFilename = aFilename;
			mTempFilename = mFilename + ".tmp";
		}

		/// <remarks/>
		public string GetError()
		{
			return mError;
		}

		/// <remarks/>
		public string GetTempFilename()
		{
			return mTempFilename;
		}

		/// <remarks/>
		public bool New()
		{
			if (!File.Exists(mFilename))
				return false;

			if (File.Exists(mTempFilename))
			{
				File.SetAttributes(mTempFilename, FileAttributes.Normal);
				File.Delete(mTempFilename);
			}
			File.Move(mFilename, mTempFilename);

			return true;
		}

		#region IDisposable Members

		/// <remarks/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		///<remarks />
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				try
				{
					if (File.Exists(mTempFilename))
					{
						File.SetAttributes(mTempFilename, FileAttributes.Normal);
						File.Delete(mTempFilename);
					}
				}
				catch (Exception ex)
				{
					Debug.Fail(ex.ToString());
				}
			}
		}

		#endregion
	}
}
