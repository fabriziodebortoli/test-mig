using System;

namespace Microarea.AdminServer.Services
{
	//================================================================================
	public class OperationResult
	{
		public bool Result { get; set; }
		public string Message { get; set; }
		public object ObjectResult { get; set; }

		//--------------------------------------------------------------------------------
		public OperationResult()
		{
			this.Result = false;
			this.Message = String.Empty;
		}
		public OperationResult(bool result, string message)
		{
			this.Result = result;
			this.Message = message;
		}
	}
}
