using System;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;
using System.Collections.Generic;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	//=========================================================================
	[Serializable]
	public class EventInfo
	{
        static IDictionary<string, string> eventArgsCache = InitEventArgsCache();

        private static IDictionary<string, string> InitEventArgsCache()
        {
            var cache = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            cache.Add(typeof(System.ComponentModel.PropertyChangingEventHandler).FullName, typeof(System.ComponentModel.PropertyChangingEventArgs).FullName);
            cache.Add(typeof(System.ComponentModel.PropertyChangedEventHandler).FullName, typeof(System.ComponentModel.PropertyChangedEventArgs).FullName);

            return cache;
        }

        private string sourceEvent;
		private string eventHandlerName;
		private string eventHandlerTypeName;
		private string componentFullPath;
		private string owner = EasyBuilderSerializer.DocumentControllerClassName;
        private bool useGenericEventHandler = true;

		public string SourceEvent			{ get  { return sourceEvent; } set { sourceEvent = value; }	}
		public string EventHandlerName		{ get { return eventHandlerName; } set { eventHandlerName = value; } }
		public string EventHandlerTypeName	{ get { return eventHandlerTypeName; } set { eventHandlerTypeName = value; } }
		public string ComponentFullPath		{ get { return componentFullPath; } set { componentFullPath = value; } }
		public string Owner					{ get { return owner; } set { owner = value; } }
		public string EventArgsTypeName
		{
			get
			{
				//Traduzione tipo evento per passaggio da System.CodeDom a NRefactory.
				//Ci serve per la nuova serializzazione in quanto NRefactory non è
				//capace a serializzare correttamente un eventhandler generico a partire
				//dal Type in memoria.Dobbiamo esplicitargli i tipi generici.
				System.CodeDom.CodeTypeReference eventHandlerTypeReference =
					new System.CodeDom.CodeTypeReference(eventHandlerTypeName);

				if (eventHandlerTypeReference.TypeArguments != null && eventHandlerTypeReference.TypeArguments.Count > 0)
					return eventHandlerTypeReference.TypeArguments[0].BaseType;

                string eventArgsFullName = null;
                if (!eventArgsCache.TryGetValue(eventHandlerTypeName, out eventArgsFullName))
                {
                    return typeof(EventArgs).FullName;
                }
                return eventArgsFullName;
            }
		}

		public string EventName
		{
			get
			{
				int indexOfLastDot = sourceEvent.LastIndexOf(".") + 1;
				if (indexOfLastDot == 0)
					return sourceEvent;

				return sourceEvent.Substring(indexOfLastDot);
			}
		}

        public bool UseGenericEventHandler
        {
            get
            {
                return useGenericEventHandler;
            }
        }

        //necessario per la serializzazione
        //-----------------------------------------------------------------------------
        public EventInfo()
		{ 
		}
		//-----------------------------------------------------------------------------
		public EventInfo(string sourceEvent, string eventHandlerName, string eventHandlerTypeName, string componentFullPath, bool useGenericEventHandler = true)
		{
			this.sourceEvent = sourceEvent;
			this.eventHandlerName = eventHandlerName;
			this.eventHandlerTypeName = eventHandlerTypeName;
			this.componentFullPath = componentFullPath;
            this.useGenericEventHandler = useGenericEventHandler;
		}

		//-----------------------------------------------------------------------------
		public EventInfo(string sourceEvent, string eventHandlerName, string eventHandlerTypeName, string componentFullPath, string owner, bool useGenericEventHandler = true)
			: this (sourceEvent, eventHandlerName, eventHandlerTypeName, componentFullPath, useGenericEventHandler)
		{
			this.owner = owner;
		}

		//-----------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			EventInfo toBeCompared = obj as EventInfo;
			if (toBeCompared == null)
				return false;

			if (object.ReferenceEquals(this, toBeCompared))
				return true;

			return
				String.Compare(sourceEvent, toBeCompared.sourceEvent, StringComparison.InvariantCultureIgnoreCase) == 0 &&
				String.Compare(eventHandlerName, toBeCompared.eventHandlerName, StringComparison.InvariantCultureIgnoreCase) == 0 &&
				String.Compare(eventHandlerTypeName, toBeCompared.eventHandlerTypeName, StringComparison.InvariantCultureIgnoreCase) == 0 &&
				String.Compare(componentFullPath, toBeCompared.componentFullPath, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		//-----------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		//-----------------------------------------------------------------------------
		public static string CalculateFullEventName(EasyBuilderComponent component, string sourceEvent)
		{
			string componentFullPath = ReflectionUtils.GetComponentFullPath(component);
			StringBuilder strBld = new StringBuilder();

			String[] componentFullPathTokens = componentFullPath.Split('.');
			String[] sourceEventTokens = sourceEvent.Split('.');

			foreach (string cfpToken in componentFullPathTokens)
			{
				if (String.Compare(cfpToken, sourceEventTokens[0], StringComparison.InvariantCulture) != 0)
					strBld.Append(cfpToken).Append(".");
				else
					break;
			}

			int idx = sourceEventTokens.Length - 1;
			for (int i = 0; i < idx; i++)
				strBld.Append(sourceEventTokens[i]).Append(".");

			return strBld.Append(sourceEventTokens[idx]).ToString();
		}
	};
}
