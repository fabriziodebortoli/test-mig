using System;
using System.ComponentModel;
using TaskBuilderNetCore.Documents.Model.Interfaces;
namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public abstract class BatchActivityComponent : DocumentComponent, IBatchActivity
    {
        #region events declarations

        public event CancelEventHandler ExecutingActivity;
        public event EventHandler ActivityExecuted;

        #endregion

        //-----------------------------------------------------------------------------------------------------
        public bool ExecuteActivity()
        {
            if (ExecutingActivity != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                ExecutingActivity(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            if (!OnExecuteActivity())
                return false;

            ActivityExecuted?.Invoke(this, EventArgs.Empty);
            return true;
        }

		//-----------------------------------------------------------------------------------------------------
		protected virtual bool OnExecuteActivity() { throw new NotImplementedException(); }
    }

}
