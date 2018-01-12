using System;

namespace Microarea.ProvisioningDatabase.Infrastructure
{
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
			this.Code = 0;
		}

		//--------------------------------------------------------------------------------
		public OperationResult(bool result, string message) : this()
		{
			this.result = result;
			this.message = message;
		}

		//--------------------------------------------------------------------------------
		public OperationResult(bool result, string message, int code)
		{
			this.result = result;
			this.message = message;
			this.code = code;
		}
	}
}
