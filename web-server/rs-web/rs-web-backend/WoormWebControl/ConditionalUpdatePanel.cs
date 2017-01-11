using System.Web.UI;
using System.Web.UI.WebControls;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
	//================================================================================
	/// <summary>
	/// Classe che è in grado di simulare di volta in volta un UptatePanel o un Panel normale
	/// questo perché nel Localizer non abbiamo lo ScriptManager
	/// </summary>
	class ConditionalUpdatePanel
	{
		Control innerControl = null;
		private bool useUpdatePanel;

		//--------------------------------------------------------------------------------
		public ConditionalUpdatePanel(bool useUpdatePanel)
		{
			this.useUpdatePanel = useUpdatePanel;
			innerControl = useUpdatePanel
				? (Control)new UpdatePanel()
				: (Control)new Panel();
		}

		//--------------------------------------------------------------------------------
		internal bool ChildrenAsTriggers
		{
			set
			{
				if (useUpdatePanel)
					((UpdatePanel)innerControl).ChildrenAsTriggers = value;
			}
		}

		//--------------------------------------------------------------------------------
		internal UpdatePanelUpdateMode UpdateMode
		{
			set
			{
				if (useUpdatePanel)
					((UpdatePanel)innerControl).UpdateMode = value;
			}
		}

		//--------------------------------------------------------------------------------
		internal string ID
		{
			set { innerControl.ID = value; }
		}

		//--------------------------------------------------------------------------------
		internal Control ContentTemplateContainer 
		{
			get
			{
				return useUpdatePanel
					? ((UpdatePanel)innerControl).ContentTemplateContainer
					: innerControl;
			}
		}

		//--------------------------------------------------------------------------------
		internal AttributeCollection Attributes 
		{
			get
			{
				return useUpdatePanel
				  ? ((UpdatePanel)innerControl).Attributes
				  : ((Panel)innerControl).Attributes;
			} 
		}

		//--------------------------------------------------------------------------------
		internal string ClientID 
		{
			get { return innerControl.ClientID; }
		}

		//--------------------------------------------------------------------------------
		internal void Update()
		{
			if (useUpdatePanel)
				((UpdatePanel)innerControl).Update();
		}

		//--------------------------------------------------------------------------------
		public static implicit operator Control(ConditionalUpdatePanel panel)
		{
			return panel.innerControl;
		}

	}
}
