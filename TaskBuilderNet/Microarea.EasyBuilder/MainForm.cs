using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using Microarea.EasyBuilder.UI;
using WeifenLuo.WinFormsUI.Docking;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder
{
	//--------------------------------------------------------------------------------
	/// <remarks/>
	public class MainForm : System.Windows.Forms.UserControl, IDisposable
	{
		/// <remarks/>
		internal virtual string AddItem(INameSpace ns, string folder, bool isFromDocOutline) { return String.Empty; }

		/// <remarks/>
		public virtual void ChangeEnablePropertyEditor() { }

		/// <remarks/>
		public virtual void ChangeFilterViewTree(bool ImEditingOnlyThisTile) { }

		/// <remarks/>
		public virtual void CloseCodeEditorsThreadContext() { }
		
		/// <remarks/>
		public virtual bool CollectAndSaveCodeChanges(bool runIfClosed) { return true; }
	
		/// <remarks/>
		public virtual void CollectOriginalMethodBodys() { }

		/// <remarks/>
		public virtual void CreatePropertyEditor() { }
		
		/// <remarks/>
		public virtual void EnableCodeEditorButton() { }

		/// <remarks/>
		public virtual void ExitFromCustomization() { }

		/// <remarks/>
		internal virtual bool OpenDocOutlineIfNeeded(JsonFormSelectedEventArgs e) { return false; }

		/// <remarks/>
		public virtual void PopulateLocalizationStrings() { }

		/// <remarks/>
		public virtual void RefreshTemplates() { }

		/// <remarks/>
		public virtual void SelectFileInTree(string file) { }

		/// <remarks/>
		public virtual void UpdateObjectViewModel(IComponent component) { }

		/// <remarks/>
		public virtual bool UpdateCodeEditor(ControllerSources sources) { return true; }

		/// <remarks/>
		internal virtual void UpdateDocOutline(string code) { }

		/// <remarks/>
		internal virtual string SaveDocOutlineSerialized() { return String.Empty; }

		internal enum PanelItems
		{
			ToolboxControl,
			OpenCodeEditor,
			Localization,
			PropertyEditor,
			ObjectModelTreeControl,
			ViewOutlineTreeControl,
			DatabaseExplorer,
			HotLinksExplorer,
			BusinessObjectsExplorer,
			EnumsTreeControl,
			ModuleObjectModelTreeControl,
			JsonFormsTreeControl,
			DocOutlineTree
		}

	}
}



