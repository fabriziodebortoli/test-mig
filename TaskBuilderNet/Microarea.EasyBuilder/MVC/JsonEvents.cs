using System.Collections.Generic;

namespace Microarea.EasyBuilder.MVC
{ 
    //=============================================================================
    /// <summary>
    /// class for static json events
    /// </summary>
    public class JsonEvents
    {
        List<JsonEvent> itms;
        /// <summary>
        /// items
        /// </summary>
        public List<JsonEvent> items { get => itms; set => itms = value; }

        /// <summary>
        /// constructor
        /// </summary>
        //-----------------------------------------------------------------------------
        public JsonEvents()
        {

        }
    }

    /// <summary>
    /// Json event class
    /// </summary>
    //=============================================================================
    public class JsonEvent
    {
        string eventName;
        /// <summary>
        /// event name
        /// </summary>
        public string EventName { get => eventName; set => eventName = value; }

        string eventHandlerName;
        /// <summary>
        /// event handler name
        /// </summary>
        public string EventHandlerName { get => eventHandlerName; set => eventHandlerName = value; }

        string ownerNameSpace;
        /// <summary>
        /// namespace owner object
        /// </summary>
        public string OwnerNameSpace { get => ownerNameSpace; set => ownerNameSpace = value; }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// constructor
        /// </summary>
        public JsonEvent()
        {
        }
    }
}
