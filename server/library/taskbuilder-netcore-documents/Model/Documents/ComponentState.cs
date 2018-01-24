using System;
using System.Collections.Generic;
using System.Text;
using TaskBuilderNetCore.Documents.Model.Interfaces;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public class StateChangedEventArgs
    {
        int oldState;
        //-----------------------------------------------------------------------------------------------------
        public int OldState { get => oldState; set => oldState = value; }
        //-----------------------------------------------------------------------------------------------------
        public StateChangedEventArgs(int oldState) => this.oldState = oldState;
    }

    //====================================================================================    
    public class ComponentState
    {
        int state;
        int dirty;
        IComponent component;

        public event EventHandler StateChanging;
        public event EventHandler<StateChangedEventArgs> StateChanged;
        public event EventHandler DirtyChanging;
        public event EventHandler<StateChangedEventArgs> DirtyChanged;

        public int Dirty
        {
            get => dirty;
            set
            {
                DirtyChanging?.Invoke(component, EventArgs.Empty);
                int oldState = dirty;
                dirty = value;
                DirtyChanged?.Invoke(component, new StateChangedEventArgs(oldState));
            }
        }
        //-----------------------------------------------------------------------------------------------------
        public int State { get => state;
        set
            {
                StateChanging?.Invoke(component, EventArgs.Empty);
                int oldState = state;
                state = value;
                StateChanged?.Invoke(component, new StateChangedEventArgs(oldState));
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public ComponentState(IComponent component)
        {
            this.component = component;
        }

       
}
}
