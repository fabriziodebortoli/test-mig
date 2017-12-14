using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using Microarea.Library.LogManager.Config;
using Microarea.Library.LogManager.Loggables;

namespace Microarea.Library.LogManager.Loggers
{
	/// <summary>
	/// BaseLogger.
	/// </summary>
	//=========================================================================
	public class BaseLogger : IDisposable, ILogger
	{
		#region Tags

		public const string ApplicationConfigSectionTag = "logConfigSection";
		public const string LoggersTag = "loggers";
		public const string LoggerTag = "logger";
		public const string TypeTag = "type";
		public const string ParamsTag = "params";
		public const string ParamTag = "param";
		public const string AssemblyAttribute = "assembly";
		public const string NameAttribute = "name";
		public const string TypeAttribute = "type";
		public const string ValueAttribute = "value";

		#endregion

		private static readonly object staticLockTicket = new object();
		private readonly object lockTicket = new object();

		private IList<ILogger> loggers;
		private	EventTypes eventsFilter;

		private bool propagateEventToChildren;
		private bool bubbleEventToParents;

		//---------------------------------------------------------------------
		public bool BubbleEventToParents
		{
			get
			{
				return bubbleEventToParents;
			}
			set
			{
				bubbleEventToParents = value;
			}
		}

		//---------------------------------------------------------------------
		public bool PropagateEventToChildren
		{
			get
			{
				return propagateEventToChildren;
			}
			set
			{
				propagateEventToChildren = value;
			}
		}

		//---------------------------------------------------------------------
		protected IMessageFormatter MessageFormatter
		{
			get
			{
				return InitializeMessageFormatter();
			}
		}

		//---------------------------------------------------------------------
		public EventTypes EventsFilter
		{
			get
			{
				return eventsFilter;
			}
			set
			{
				this.eventsFilter = value;
			}
		}

		//---------------------------------------------------------------------
		public BaseLogger()
		{
			this.loggers = new List<ILogger>();
			this.eventsFilter =
				EventTypes.Error | EventTypes.Information |
				EventTypes.Success | EventTypes.Warning;

			this.propagateEventToChildren = true;
			this.bubbleEventToParents = true;
		}

		//---------------------------------------------------------------------
		protected virtual IMessageFormatter InitializeMessageFormatter()
		{
			return new MessageFormatterFactory().CreateMessageFormatter(MessageFormatterType.Text);
		}

		#region ILogger management

		//---------------------------------------------------------------------
		protected IList<ILogger> Loggers
		{
			get
			{
				return this.loggers;
			}
		}

		//---------------------------------------------------------------------
		public int LoggersCount
		{
			get
			{
				return this.loggers.Count;
			}
		}

		//---------------------------------------------------------------------
		public virtual ILogger AddLogWriter(ILogger logger)
		{
			lock (lockTicket)
			{
				if (!Object.ReferenceEquals(logger, null))
				{
					logger.BubbleLoggableEvent += new EventHandler<Loggables.LoggableEventArgs>(OnLogAndBubbleEvent);
					loggers.Add(logger);
				}
				return this;
			}
		}

		//---------------------------------------------------------------------
		public virtual ILogger RemoveLogWriter(ILogger logger)
		{
			lock (lockTicket)
			{
				if (!Object.ReferenceEquals(logger, null))
				{
					logger.BubbleLoggableEvent -= new EventHandler<Loggables.LoggableEventArgs>(OnLogAndBubbleEvent);
					loggers.Remove(logger);
				}
				return this;
			}
		}

		#endregion

		#region Logger creation

		//---------------------------------------------------------------------
		public void GetLoggersFromApplicationConfiguration()
		{
			LogConfigSection logConfigSection =
				ConfigurationManager.GetSection(ApplicationConfigSectionTag) as LogConfigSection;

			if (logConfigSection == null)
				return;

			if (logConfigSection.Loggers == null || logConfigSection.Loggers.Count == 0)
				return;

			ILogger logger = null;
			lock (lockTicket)
			{
				foreach (LoggerConfigElement item in logConfigSection.Loggers)
				{
					logger = GetLoggerFromConfigElement(item);
					if (logger != null)
						AddLogWriter(logger);
				}
			}
		}

		/// <example>
		/// <![CDATA[
		/// <loggers>
		///    <logger name="Logger1">
		///        <type assembly="assembly file name" name=".Net Type name" />
		///        <params>
		///            <param name="name of parameter" type="string" value="value of parameter" />
		///            <param name="name of parameter" type="bool" value="value of parameter" />
		///            <param name="name of parameter" type="int" value="value of parameter" />
		///            <param name="name of parameter" type="eventtypes" value="value of parameter" />
		///        </params>
		///    </logger>
		///</loggers>
		/// ]]>
		/// </example>
		//---------------------------------------------------------------------
		public void GetLoggersFromFile(string filePath)
		{
			if (
				filePath == null ||
				filePath.Trim().Length == 0 ||
				!System.IO.File.Exists(filePath)
				)
				return;

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(filePath);// non sono catch-ate eccezioni affinchè vengano rilevate.

			GetLoggersFromXml(xmlDoc);
		}

		/// <example>
		/// <![CDATA[
		/// <loggers>
		///    <logger name="Logger1">
		///        <type assembly="assembly file name" name=".Net Type name" />
		///        <params>
		///            <param name="name of parameter" type="string" value="value of parameter" />
		///            <param name="name of parameter" type="bool" value="value of parameter" />
		///            <param name="name of parameter" type="int" value="value of parameter" />
		///            <param name="name of parameter" type="eventtypes" value="value of parameter" />
		///        </params>
		///    </logger>
		///</loggers>
		/// ]]>
		/// </example>
		//---------------------------------------------------------------------
		public void GetLoggersFromXml(string xmlFragment)
		{
			if (xmlFragment == null || xmlFragment.Trim().Length == 0)
				return;

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xmlFragment);
			GetLoggersFromXml(xmlDoc);
		}

		/// <example>
		/// <![CDATA[
		/// <loggers>
		///    <logger name="Logger1">
		///        <type assembly="assembly file name" name=".Net Type name" />
		///        <params>
		///            <param name="name of parameter" type="string" value="value of parameter" />
		///            <param name="name of parameter" type="bool" value="value of parameter" />
		///            <param name="name of parameter" type="int" value="value of parameter" />
		///            <param name="name of parameter" type="eventtypes" value="value of parameter" />
		///        </params>
		///    </logger>
		///</loggers>
		/// ]]>
		/// </example>
		//---------------------------------------------------------------------
		public void GetLoggersFromXml(System.Xml.XPath.IXPathNavigable navigableDocument)
		{
			XmlDocument xmlDoc = navigableDocument as XmlDocument;

			if (Object.ReferenceEquals(xmlDoc, null))
				return;

			string xPathQuery = String.Concat("descendant::", LoggersTag);
			XmlNodeList nodes = xmlDoc.SelectNodes(xPathQuery);

			if (nodes == null || nodes.Count == 0)
				return;

			xPathQuery = String.Concat("descendant::", LoggerTag);
			nodes = nodes[0].SelectNodes(xPathQuery);

			if (nodes == null || nodes.Count == 0)
				return;

			ILogger logger = null;
			lock (lockTicket)
			{
				foreach (XmlNode loggerNode in nodes)
				{
					logger = GetLoggerFromXml(loggerNode);
					if (logger != null)
						AddLogWriter(logger);
				}
			}
		}

		//---------------------------------------------------------------------
		private static ILogger GetLoggerFromXml(XmlNode loggerNode)
		{
			lock (staticLockTicket)
			{
				string xPathQuery = String.Concat("descendant::", TypeTag);
				XmlNodeList nodes = loggerNode.SelectNodes(xPathQuery);
				if (nodes == null || nodes.Count == 0)
					return null;

				Assembly assembly = null;
				string typeName = nodes[0].Attributes[TypeAttribute].InnerText;
				string assemblyFileName = nodes[0].Attributes[AssemblyAttribute].InnerText;
				if (assemblyFileName != null && assemblyFileName.Trim().Length > 0)
				{
					Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
					bool alreadyLoaded = false;
					foreach (Assembly loadedAssembly in loadedAssemblies)
					{
						if (String.Compare(loadedAssembly.GetName().Name, assemblyFileName, StringComparison.OrdinalIgnoreCase) == 0)
						{
							assembly = loadedAssembly;
							alreadyLoaded = true;
							break;
						}
					}
					if (!alreadyLoaded)
					{
						try
						{
							assembly =AppDomain.CurrentDomain.Load(assemblyFileName);
						}
						catch (FileNotFoundException fileNotFoundExc)
						{
							throw new LoggerCreationException(assemblyFileName + " not found", fileNotFoundExc);
						}
						catch (BadImageFormatException badImageFormatExc)
						{
							throw new LoggerCreationException(assemblyFileName + " is not a valid assembly or version 2.0 or later of the common language runtime is currently loaded and assemblyRef was compiled with a later version.", badImageFormatExc);
						}
						catch (AppDomainUnloadedException appDomainUnloadedExc)
						{
							throw new LoggerCreationException(assemblyFileName + " is being loaded on an unloaded application domain. ", appDomainUnloadedExc);
						}
					}
				}

				xPathQuery = String.Concat("descendant::", ParamTag);
				nodes = loggerNode.SelectNodes(xPathQuery);
				if (nodes == null || nodes.Count == 0)
					return System.Activator.CreateInstance(Type.GetType(typeName)) as ILogger;

				Object[] parameters = new Object[nodes.Count];
				XmlAttribute aAttribute = null;
				for (int i = 0; i < nodes.Count; i++)
				{
					aAttribute = nodes[i].Attributes[TypeAttribute];
					if (aAttribute == null)
						parameters[i] = nodes[i].Attributes[ValueAttribute].InnerText;
					else
					{
						switch (aAttribute.InnerText)
						{
							case "bool":
								parameters[i] = Boolean.Parse(nodes[i].Attributes[ValueAttribute].InnerText);
								break;
							case "int":
								parameters[i] = Int32.Parse(
									nodes[i].Attributes[ValueAttribute].InnerText,
									CultureInfo.InvariantCulture
										);
								break;
							case "eventtypes":
								parameters[i] = ParseEventsFilter(nodes[i].Attributes[ValueAttribute].InnerText);
								break;
							default:
								parameters[i] = nodes[i].Attributes[ValueAttribute].InnerText;
								break;
						}
					}
				}

				return Activator.CreateInstance(
							assembly.GetType(typeName),
							parameters
							) as ILogger;
			}
		}

		//---------------------------------------------------------------------
		private static ILogger GetLoggerFromConfigElement(LoggerConfigElement item)
		{
			lock (staticLockTicket)
			{
				Assembly assembly = null;
				string typeName = item.Type.TypeName;
				string assemblyFileName = item.Type.Assembly;
				if (assemblyFileName != null && assemblyFileName.Trim().Length > 0)
				{
					Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
					bool alreadyLoaded = false;
					foreach (Assembly loadedAssembly in loadedAssemblies)
					{
						if (String.Compare(loadedAssembly.GetName().Name, assemblyFileName, StringComparison.OrdinalIgnoreCase) == 0)
						{
							assembly = loadedAssembly;
							alreadyLoaded = true;
							break;
						}
					}
					if (!alreadyLoaded)
					{
						try
						{
							assembly = AppDomain.CurrentDomain.Load(assemblyFileName);
						}
						catch (FileNotFoundException fileNotFoundExc)
						{
							throw new LoggerCreationException(assemblyFileName + " not found", fileNotFoundExc);
						}
						catch (BadImageFormatException badImageFormatExc)
						{
							throw new LoggerCreationException(assemblyFileName + " is not a valid assembly or version 2.0 or later of the common language runtime is currently loaded and assemblyRef was compiled with a later version.", badImageFormatExc);
						}
						catch (AppDomainUnloadedException appDomainUnloadedExc)
						{
							throw new LoggerCreationException(assemblyFileName + " is being loaded on an unloaded application domain. ", appDomainUnloadedExc);
						}
					}
				}

				Object[] parameters = new Object[item.Params.Count];
				IEnumerator paramsEnumerator = item.Params.GetEnumerator();
				int i = -1;
				ParamConfigElement current = null;
				while (paramsEnumerator.MoveNext())
				{
					i++;
					current = paramsEnumerator.Current as ParamConfigElement;
					switch (current.Type)
					{
						case "bool":
							parameters[i] = Boolean.Parse(current.Value);
							break;
						case "int":
							parameters[i] = Int32.Parse(
								current.Value,
								CultureInfo.InvariantCulture
									);
							break;
						case "eventtypes":
							parameters[i] = ParseEventsFilter(current.Value);
							break;
						default:
							parameters[i] = current.Value;
							break;
					}
				}

				return Activator.CreateInstance(
							assembly.GetType(typeName),
							parameters
							) as ILogger;
			}
		}

		//---------------------------------------------------------------------
		private static EventTypes ParseEventsFilter(string toBeParsed)
		{
			lock (staticLockTicket)
			{
				EventTypes eventsFilter = EventTypes.None;

				if (toBeParsed == null || toBeParsed.Length == 0)
					return eventsFilter;

				string[] strEventTypes = toBeParsed.Split(',');
				if (strEventTypes == null || strEventTypes.Length == 0)
					return eventsFilter;

				foreach (string strEventType in strEventTypes)
					eventsFilter |= ((EventTypes)Enum.Parse(typeof(EventTypes), strEventType));

				return eventsFilter;
			}
		}

		#endregion

		#region IDisposable Members

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		//---------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			lock (lockTicket)
			{
				if (disposing && loggers != null)
				{
					foreach (IDisposable disposable in loggers)
						disposable.Dispose();

					loggers = null;
				}
			}
		}

		#region ILogger Members

		private EventHandler<Loggables.LoggableEventArgs> bubbleLoggableEventDelegate;

		//---------------------------------------------------------------------
		public event EventHandler<Loggables.LoggableEventArgs> BubbleLoggableEvent
		{
			add
			{
				lock (lockTicket)
				{
					bubbleLoggableEventDelegate += value;
				}
			}
			remove
			{
				lock (lockTicket)
				{
					bubbleLoggableEventDelegate -= value;
				}
			}
		}

		//---------------------------------------------------------------------
		public virtual ILogger ListenTo(ILoggable loggable)
		{
			if (!Object.ReferenceEquals(loggable, null))
				loggable.Event += new EventHandler<Loggables.LoggableEventArgs>(OnLogAndBubbleEvent);

			return this;
		}

		//---------------------------------------------------------------------
		public virtual ILogger StopListeningTo(ILoggable loggable)
		{
			if (!Object.ReferenceEquals(loggable, null))
				loggable.Event -= new EventHandler<Loggables.LoggableEventArgs>(OnLogAndBubbleEvent);

			return this;
		}

		//---------------------------------------------------------------------
		protected virtual void OnBubbleLoggableEvent(
			LoggableEventArgs ea
			) 
		{
			if (bubbleLoggableEventDelegate != null)
				bubbleLoggableEventDelegate(this, ea); 
		}

		// Called by ILoggable and ILogger
		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		private void OnLogAndBubbleEvent(
			object sender,
			LoggableEventArgs le
			)
		{
			if (Object.ReferenceEquals(sender, null))
				throw new ArgumentNullException("sender", "'sender' cannot be null");
			if (Object.ReferenceEquals(le, null))
				throw new ArgumentNullException("le", "'le' cannot be null");

			Log(le);

			if (BubbleEventToParents)// Notify loggers that precede this one in the loggers chain.
				OnBubbleLoggableEvent(le);
		}

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public virtual void LogAndBubbleEvent(LoggableEventArgs le)
		{
			if (Object.ReferenceEquals(le, null))
				throw new ArgumentNullException("le", "'le' cannot be null");

			OnLogAndBubbleEvent(this, le);
		}

		//---------------------------------------------------------------------
		public virtual void Log(ILoggableEventDescriptor eventDescriptor)
		{
			lock (lockTicket)
			{
				if (Object.ReferenceEquals(eventDescriptor, null))
					throw new ArgumentNullException("eventDescriptor", "'eventDescriptor' cannot be null");

				if (!PropagateEventToChildren || loggers == null || loggers.Count == 0)
					return;

				bool imTheBubblingLogger = false;
				ILogger bubblingLogger = null;
				if (eventDescriptor.LoggableEventInfo != null)
					bubblingLogger = eventDescriptor.LoggableEventInfo.BubblingLogger;
				else
				{
					eventDescriptor.LoggableEventInfo = new LoggableEventInfo(this);
					bubblingLogger = this;
					imTheBubblingLogger = true;
				}

				foreach (ILogger logWriter in loggers)
				{
					if (imTheBubblingLogger || !Object.ReferenceEquals(bubblingLogger, logWriter))
						logWriter.Log(eventDescriptor.Clone() as ILoggableEventDescriptor);
				}
			}
		}

		//---------------------------------------------------------------------
		protected virtual bool MatchMyEventsFilter(EventTypes eventType)
		{
			switch (eventType)
			{
				case EventTypes.Error:
					return ((EventTypes.Error & eventsFilter) == EventTypes.Error);
				case EventTypes.Information:
					return ((EventTypes.Information & eventsFilter) == EventTypes.Information);
				case EventTypes.Success:
					return ((EventTypes.Success & eventsFilter) == EventTypes.Success);
				case EventTypes.Warning:
					return ((EventTypes.Warning & eventsFilter) == EventTypes.Warning);
				default: return false;
			}
		}

		#endregion
	}

	//=========================================================================
	[Serializable]
	public class LoggerCreationException : Exception
	{
		//---------------------------------------------------------------------
		public LoggerCreationException(
			string message,
			Exception innerException
			)
			: base(message, innerException)
		{}

		// Needed for xml serialization.
		//---------------------------------------------------------------------
		protected LoggerCreationException(
			SerializationInfo info,
			StreamingContext context
			)
			: base(info, context)
		{}

		//---------------------------------------------------------------------
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public override void GetObjectData(
			SerializationInfo info,
			StreamingContext context
			)
		{
			if (Object.ReferenceEquals(info, null))
				throw new ArgumentNullException("info", "'info' cannot be null");

			base.GetObjectData(info, context);
		}
	}
}
