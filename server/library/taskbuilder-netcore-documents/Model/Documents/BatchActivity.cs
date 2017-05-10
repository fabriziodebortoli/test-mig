using System;
using System.ComponentModel;
using TaskBuilderNetCore.Documents.Interfaces;
namespace TaskBuilderNetCore.Documents.Model.Documents
{
    //====================================================================================    
    public abstract partial class Document : IBatchActivity
    {
        //-----------------------------------------------------------------------------------------------------
        int steps;

        #region events declarations

        public event CancelEventHandler ExecutingBatch;
        public event EventHandler BatchExecuted;
        public event CancelEventHandler PausingBatch;
        public event EventHandler BatchPaused;
        public event CancelEventHandler ResumingBatch;
        public event EventHandler BatchResumed;
        public event CancelEventHandler StartingBatch;
        public event EventHandler BatchStarted;
        public event CancelEventHandler StoppingBatch;
        public event EventHandler BatchStopped;

        #endregion

        //-----------------------------------------------------------------------------------------------------
        public int Steps
        {
            get
            {
                return steps;
            }
            set
            {
                steps = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public void ExecuteBatch(int nStep = 1)
        {
            if (ExecutingBatch != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                ExecutingBatch(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return;
            }

            if (!OnExecuteBatch(nStep))
                return;

            BatchExecuted?.Invoke(this, EventArgs.Empty);
        }

        //-----------------------------------------------------------------------------------------------------
        protected abstract bool OnExecuteBatch(int nStep = 1);

        //-----------------------------------------------------------------------------------------------------
        public void PauseBatch()
        {
            if (PausingBatch != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                PausingBatch(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return;
            }

            if (!OnBatchPause())
                return;

            BatchPaused?.Invoke(this, EventArgs.Empty);
        }

        //-----------------------------------------------------------------------------------------------------
        protected abstract bool OnBatchPause();


        //-----------------------------------------------------------------------------------------------------
        public void ResumeBatch()
        {
            if (ResumingBatch != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                ResumingBatch(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return;
            }

            if (!OnBatchResume())
                return;

            BatchResumed?.Invoke(this, EventArgs.Empty);
        }

        //-----------------------------------------------------------------------------------------------------
        protected abstract bool OnBatchResume();

        //-----------------------------------------------------------------------------------------------------
        public void StartBatch()
        {
            if (StartingBatch != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                StartingBatch(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return;
            }

            if (!OnBatchStart())
                return;

            BatchStarted?.Invoke(this, EventArgs.Empty);
        }

        //-----------------------------------------------------------------------------------------------------
        protected abstract bool OnBatchStart();

        //-----------------------------------------------------------------------------------------------------
        public void StopBatch()
        {
            if (StoppingBatch != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                StoppingBatch(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return;
            }

            if (!OnBatchStop())
                return;

            BatchStopped?.Invoke(this, EventArgs.Empty);
        }

        //-----------------------------------------------------------------------------------------------------
        protected abstract bool OnBatchStop();
    }

}
