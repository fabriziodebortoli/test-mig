using System;

namespace Microarea.AdminServer.Services
{
	//================================================================================
	//================================================================================
	public class OperationResult
	{
		bool result;
		int code;
		string message;
		object objectResult;

		public bool Result { get { return this.result; } set { this.result = value; } }
		public string Message { get { return this.message; } set { this.message = value; } }
		public object Content { get { return (this.objectResult == null) ? new object() : this.objectResult; } set { this.objectResult = value; } }
		public int Code { get => code; set => code = value; }

		//--------------------------------------------------------------------------------
		public OperationResult()
		{
			this.result = false;
			this.message = String.Empty;
		}

		//--------------------------------------------------------------------------------
		public OperationResult(bool result, string message)
		{
			this.result = result;
			this.message = message;
		}
	}
}
