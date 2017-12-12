using System;
using System.ComponentModel;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Localization;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	////=======================================================================
	public class EasyBuilderBehaviourEventArgs : EventArgs
	{
		private EasyBuilderComponent component;
		private string eventName;

		//-----------------------------------------------------------------------------
		public virtual string EventName { get { return eventName; } }

		//-----------------------------------------------------------------------------
		public virtual object Component { get { return component; } }

		//-----------------------------------------------------------------------------
		public EasyBuilderBehaviourEventArgs(EasyBuilderComponent component, string eventName)
		{
			this.component = component;
			this.eventName = eventName;
		}
	}

	////=======================================================================
	public class EasyBuilderBehaviourFormMode
	{
		private bool inNew;
		private bool inEdit;
		private bool inBrowse;
		private bool inFind;
		private bool inNewEnabled;
		private bool inEditEnabled;
		private bool inBrowseEnabled;
		private bool inFindEnabled;

		public static string AllModes = "Always";
		public static string InBrowseMode = "is browsing data";
		public static string InNewMode = "is editing new data";
		public static string InEditMode = "is editing existing data";
		public static string InFindMode = "is finding data";
		//-----------------------------------------------------------------------------
		internal string Mode
		{
			get
			{
				if (inBrowse && inNew && inEdit && inFind)
					return AllModes;
			

				if (!inBrowse && !inNew && !inEdit && !inFind)
					return "-";

				string s = string.Empty;
				if (inBrowse)
					s += InBrowseMode;
				if (inNew)
					s += InNewMode;
				if (inEdit)
					s += InEditMode;
				if (inFind)
					s += InFindMode;
				return s;
			}

			set
			{

				if (value.Contains(AllModes))
				{
					inNew = inEdit = inBrowse = inFind = true;
					return;
				}

				inNew = inNewEnabled && value.Contains(InNewMode);
				inEdit = inEditEnabled && value.Contains(InEditMode);
				inBrowse = inBrowseEnabled && value.Contains(InBrowseMode);
				inFind = inFindEnabled && value.Contains(InFindMode);
			}
		}

		public bool InNew { get { return inNew; } set { inNew = value; } }
		public bool InEdit { get { return inEdit; } set { inEdit = value; } }
		public bool InBrowse { get { return inBrowse; } set { inBrowse = value; } }
		public bool InFind { get { return inFind; } set { inFind = value; } }

		public bool InNewEnabled { get { return inNewEnabled; } set { inNewEnabled = value; } }
		public bool InEditEnabled { get { return inEditEnabled; } set { inEditEnabled = value; } }
		public bool InBrowseEnabled { get { return inBrowseEnabled; } set { inBrowseEnabled = value; } }
		public bool InFindEnabled { get { return inFindEnabled; } set { inFindEnabled = value; } }

		//-----------------------------------------------------------------------------
		internal EasyBuilderBehaviourFormMode()
		{
			inNewEnabled = true;
			inEditEnabled = true;
			inBrowseEnabled = true;
			inFindEnabled = true;
		}

		//-----------------------------------------------------------------------------
		internal void Assign(EasyBuilderBehaviourFormMode mode)
		{
			Mode = mode.Mode;
		}

		//-----------------------------------------------------------------------------
		internal bool InMode(FormModeType formMode)
		{
			switch (formMode)
			{
				case FormModeType.New:
					return inNew;
				case FormModeType.Edit:
					return inEdit;
				case FormModeType.Find:
					return inFind;
				default:
					return inBrowse;
			}
		}
	}

	////=======================================================================
	public class EasyBuilderBehaviour : EasyBuilderComponent
	{
		private string name;
		private EasyBuilderComponent eventOwner;
		private System.Reflection.EventInfo eventInfo;
		private string eventPath = string.Empty;
		private Type appliesTo = null;
		private EasyBuilderBehaviourFormMode formModeRule;
		private string description;

		[Browsable(false)]
		public event EventHandler<EasyBuilderBehaviourEventArgs> Execute;

		//-----------------------------------------------------------------------------
		[LocalizedCategory("GeneralCategory", typeof(EBCategories))]
		public override string Name { get { return name; } set { name = value; } }
		//-----------------------------------------------------------------------------
		[LocalizedCategory("GeneralCategory", typeof(EBCategories))]
		public string Description { get { return description; } set { description = value; } }
		//-----------------------------------------------------------------------------
		[LocalizedCategory("BehaviourCategory", typeof(EBCategories)), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual EasyBuilderComponent EventOwner { get { return eventOwner; } set { eventOwner = value; } }
		//-----------------------------------------------------------------------------
		[LocalizedCategory("BehaviourCategory", typeof(EBCategories)), TBPropertyFilter(TBPropertyFilters.ComponentState), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual string Event { get { return eventInfo == null ? string.Empty : eventInfo.Name; } set { if (EventOwner != null) eventInfo = EventOwner.GetType().GetEvent(value); if (eventInfo != null) eventPath = string.Format("{0}.{1}", ReflectionUtils.GetComponentFullPath(EventOwner), eventInfo.Name); } }
		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public string EventPath { get { return eventPath; } set { eventPath = value; } }
		//----------------------------------------------------------------------------
		[Browsable(false), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
		public virtual Type AppliesTo { get { return appliesTo; } set { appliesTo = value; } }
		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public EasyBuilderBehaviourFormMode FormModeRule { get { return formModeRule; } }
		//-----------------------------------------------------------------------------
		[LocalizedCategory("DocumentStatusCategory", typeof(EBCategories))]
		public virtual string WhenDocument { get { return formModeRule.Mode; } set { formModeRule.Mode = value; } }
		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public virtual bool IsEmpty { get { return EventOwner == null || Event == null || string.IsNullOrEmpty(name); } }
		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public virtual bool IsValid { get { return !IsEmpty; } }
		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public EventDescriptor EventDescriptor { get { return IsValid ? EventOwner.GetEventDescriptor(Event) : null; } }
		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public virtual string Explain { get { return description; } }
	
		//-----------------------------------------------------------------------------
		public EasyBuilderBehaviour()
		{
			formModeRule = new EasyBuilderBehaviourFormMode();
		}

		//-----------------------------------------------------------------------------
		public bool IsToExecute(EventArgs e)
		{
			EasyBuilderBehaviourEventArgs eventArg = e as EasyBuilderBehaviourEventArgs;
			return eventArg != null && IsToExecute(eventArg.EventName, eventArg.Component as EasyBuilderComponent);
		}

		//-----------------------------------------------------------------------------
		protected virtual bool IsToExecute(string eventName, EasyBuilderComponent component)
		{
			// evento diverso
			if (string.Compare(eventName, this.Event, true) != 0)
				return false;

			// tipo oggetto diverso
			if (appliesTo != null && component.GetType() != appliesTo)
				return false;

			// stato documento
			if (!formModeRule.InMode(component.Document.FormMode))
				return false;
			
			return true;
		}

		//-----------------------------------------------------------------------------
		public virtual void OnExecute(object sender, EventArgs e)
		{
			EasyBuilderBehaviourEventArgs eventArg = e as EasyBuilderBehaviourEventArgs;

			if (Execute != null)
				Execute(sender, eventArg);
		}

		//-----------------------------------------------------------------------------
		public string GetFireEventHandlerName(EasyBuilderComponent component) 
		{
			string objectName = ReflectionUtils.GetComponentFullPath(component);
			objectName = objectName.Replace("Document.", "");
			objectName = objectName.Replace(".", "_");

			return string.Format("{0}_{1}_{2}", objectName, Event, name); 
		}

		//-----------------------------------------------------------------------------
		public override string ToString()
		{
			return Explain;
		}

		//-----------------------------------------------------------------------------
		public override bool CanChangeProperty(string propertyName)
		{
			if (propertyName == "Event")
				return EventOwner != null;
			return true;
		}

		//-----------------------------------------------------------------------------
		internal virtual void ResolveEvent(EasyBuilderComponent owner)
		{
			if (string.IsNullOrEmpty(eventPath))
			{
				EventOwner = null;
				Event = null;
				return;
			}

			IEasyBuilderContainer rootContainer = GetTopContainer(owner);

			int nPos = eventPath.LastIndexOf(".");
			if (nPos < 0 || rootContainer == null)
				return;

			string path = eventPath.Mid(0, nPos);
			// il controller non è risolto
			EventOwner = string.IsNullOrEmpty(path) ? rootContainer as EasyBuilderComponent : ReflectionUtils.GetComponentFromPath(rootContainer, path) as EasyBuilderComponent;
			if (EventOwner != null)
				Event = eventPath.Mid(nPos+1);
			
		}

		//-----------------------------------------------------------------------------
		public static IEasyBuilderContainer GetTopContainer(EasyBuilderComponent owner)
		{
			IEasyBuilderContainer rootContainer = null;
			EasyBuilderComponent cmp = owner.ParentComponent == null ? owner.Document as EasyBuilderComponent : owner;
			do
			{
				cmp = cmp.ParentComponent;
				if (cmp is IEasyBuilderContainer)
					rootContainer = cmp as IEasyBuilderContainer;
			}
			while (cmp != null);

			return rootContainer;
		}
	}


}
