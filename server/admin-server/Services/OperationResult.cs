using System;
using System.Collections;

namespace Microarea.AdminServer.Services
{
    //================================================================================
    public class OperationResult
    {
        bool result;
        ArrayList messages;

        public bool Result { get { return this.result; } }
        public string Description {
            get {
                string allMessages = String.Empty;
                foreach (string item in this.messages)
                {
                    allMessages += item;
                }
                return allMessages;
            }
        }

        //--------------------------------------------------------------------------------
        OperationResult()
        {
            this.result = false;
            this.messages = new ArrayList();
        }

        //--------------------------------------------------------------------------------
        public void SetResult(bool result)
        {
            this.result = result;
        }

        //--------------------------------------------------------------------------------
        public void AddMessage(string message)
        {
            this.messages.Add(message);
        }

    }
}
