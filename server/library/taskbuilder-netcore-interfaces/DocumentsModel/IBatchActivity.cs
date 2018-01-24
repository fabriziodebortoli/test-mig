using System;
using System.ComponentModel;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Model.Interfaces
{
    //====================================================================================    
    public interface IBatchActivity
    {
        #region events declarations

        event CancelEventHandler ExecutingActivity;
        event EventHandler ActivityExecuted;

        #endregion

        INameSpace NameSpace { get; }
        ICallerContext CallerContext { get; }

        bool ExecuteActivity();
    }
}
