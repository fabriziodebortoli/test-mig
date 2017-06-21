using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Collections;

namespace Microarea.AdminServer.Services
{
    //================================================================================
    public class ServiceResult
    {
        bool result;
        int affectedItem;
        ArrayList messages;
        IAdminModel objectResult;

		//--------------------------------------------------------------------------------
		public bool Result { get { return this.result; } }
        public int AffectedItem { get { return this.affectedItem; } }
        public string Description
		{
            get
			{
                string allMessages = String.Empty;
                foreach (string item in this.messages)
                    allMessages += item;
                return allMessages;
            }
        }

        public IAdminModel ObjectResult { get { return this.objectResult; } }

        //--------------------------------------------------------------------------------
        public ServiceResult()
        {
            this.result = false;
            this.affectedItem = 0;
            this.messages = new ArrayList();
        }

        //--------------------------------------------------------------------------------
        public void SetResult(bool result)
        {
            this.result = result;
        }

        //--------------------------------------------------------------------------------
        public void SetObjectResult(IAdminModel objResult)
        {
            this.objectResult = objResult;
        }

        //--------------------------------------------------------------------------------
        public void AddMessage(string message)
        {
            this.messages.Add(message);
        }
    }

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
