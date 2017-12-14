using System;

namespace ICSharpCode.SharpZipLib 
{
	
	/// <summary>
	/// Is thrown during the creation or input of a zip file.
	/// </summary>
	public class ZipException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the ZipException class with default properties.
		/// </summary>
		public ZipException()
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the ZipException class with a specified error message.
		/// </summary>
		public ZipException(string msg) : base(msg)
		{
		}
	}
}
