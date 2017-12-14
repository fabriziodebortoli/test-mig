using System;

namespace Microarea.Console.Core.FileConverter
{
	//================================================================================
	public class FileConverterException : ApplicationException
	{
		public long Line;
		public int Position; 
		public string File;

		//--------------------------------------------------------------------------------
		public FileConverterException(string message, string file, long line, int position)
			: base(message)
		{
			this.File = file;
			this.Line = line;
			this.Position = position;
		}

		//--------------------------------------------------------------------------------
		public override string Message
		{
			get
			{
				return base.Message +
					string.Format(FileConverterStrings.ExceptionError, File, Line, Position);
			}
		}

	}
}
