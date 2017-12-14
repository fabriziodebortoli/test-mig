using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;
using System;
using Microarea.Framework.TBApplicationWrapper;
using System.Collections.Generic;
using Microarea.EasyBuilder.ComponentModel;

namespace Microarea.EasyBuilder.UI
{
	/// <summary>
	/// 
	/// </summary>
	public partial class ToolboxControl : UserControl
	{

		/// <remarks/>
		public event EventHandler OpenTwinPanel;
        private FormEditor TheFormEditor
        {  get
           {
                TBSite site = this.Site as TBSite;
                return site != null ? site.Editor as FormEditor : null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        //-----------------------------------------------------------------------------
        public ToolboxControl(bool isEasyStudioDesigner)
		{
			InitializeComponent();
			this.Text = Resources.ToolBoxTitle;
            this.toolBox.ToolBoxItemRenamed += ToolBox_ToolBoxItemRenamed;
            this.toolBox.ToolBoxItemDeleting += ToolBox_ToolBoxItemDeleting;
			this.toolBox.InitializeToolbox(isEasyStudioDesigner);
		}

        //-----------------------------------------------------------------------------
        private void ToolBox_ToolBoxItemDeleting(object sender, ToolBoxItemChangedEventArgs e)
        {
            if (TheFormEditor != null)
               e.Cancel = TheFormEditor.DeleteTemplate(e.Item.Caption) == false;
        }

        //-----------------------------------------------------------------------------
        private void ToolBox_ToolBoxItemRenamed(object sender, ToolBoxItemChangedEventArgs e)
        {
            if (TheFormEditor != null)
                TheFormEditor.RenameTemplate(e.OldLabel, e.Item.Caption);
        }


        /// <summary>
        /// 
        /// </summary>
        //--------------------------------------------------------------------------------
        protected override void OnVisibleChanged(EventArgs e)
		{
			if (Visible && OpenTwinPanel != null)
				OpenTwinPanel(this, e);
		}

        //--------------------------------------------------------------------------------
        internal void RefreshTemplates(List<EasyStudioTemplate> templates)
        {
            toolBox.RefreshTemplates(templates);
        }
    }
}
