using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TaskBuilderNetCore.Documents.Model.Interfaces;

namespace TaskBuilderNetCore.Documents.Model
{
    /// <summary>
    /// da qui eventualmente si puo' ampliare andando a fissare stati specifici (Filtri, risultato, summary)
    /// </summary>
    //====================================================================================    
    public class ActivityDocument : Document, IBatchActivity
    {
        #region events declarations

        public event CancelEventHandler ExecutingActivity;
        public event EventHandler ActivityExecuted;

        #endregion


        //-----------------------------------------------------------------------------------------------------
        public int NumberOfSteps { get => Components.Count; }

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

            bool executed = false;
            if (CallerContext.Mode == ExecutionMode.Unattended)
                executed = ExecuteActivityUnattended();
            else
            {
                IBatchActivity activity = GetCurrentActivity();
                if (activity != null)
                    return activity.ExecuteActivity();
            }

            ActivityExecuted?.Invoke(this, EventArgs.Empty);
            return executed;
        }

        //----------------------------------------------------------------------------------------------------
        protected virtual bool ExecuteActivityUnattended()
        {
            bool executed = true;
            foreach (Interfaces.IComponent component in Components)
            {
                IBatchActivity batchActivity = component as IBatchActivity;
                if (batchActivity != null)
                    executed = executed && batchActivity.ExecuteActivity();
                NextState();
            }
            return executed;
        }

         //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnExecuteActivity()
        {
            return true;
        }    

        //-----------------------------------------------------------------------------------------------------
        public IBatchActivity GetCurrentActivity()
        {
            // if lo stato corrente corrisponde ad una esecuzione la ritorno
            if (NumberOfSteps > 0 && DocumentState.State < NumberOfSteps)
            {
                IBatchActivity batchActivity = Components[DocumentState.State] as IBatchActivity;
                if (batchActivity != null)
                    return batchActivity;
           }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------
        public void NextState()
        {
            if (DocumentState.State < NumberOfSteps)
                DocumentState.State = DocumentState.State + 1;
        }
        
        //-----------------------------------------------------------------------------------------------------
        public void PreviousState()
        {
            if (DocumentState.State > 0)
                DocumentState.State = DocumentState.State - 1;
        }
    }
}
