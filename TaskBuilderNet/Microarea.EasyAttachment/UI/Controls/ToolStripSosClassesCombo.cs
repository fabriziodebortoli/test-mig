using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Microarea.EasyAttachment.UI.Controls
{
	[System.ComponentModel.DesignerCategory("code")]
	[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
	//================================================================================	
	public partial class ToolStripSosClassesCombo : ToolStripControlHost
	{
        //--------------------------------------------------------------------------------
        public SOSClassComboItem SelectedItem { get { return CmbClasses.SelectedItem as SOSClassComboItem; } set { CmbClasses.SelectedItem = value; } }
        public int SelectedIndex { get { return CmbClasses.SelectedIndex;} set{CmbClasses.SelectedIndex = value;} }
        public ComboBox.ObjectCollection Items { get { return CmbClasses.Items; } }

		public List<int> CollectionIDs { get { return ((SOSClassComboItem)SelectedItem).CollectionIDs; } }
        
        // set other defaults that are interesting
        //--------------------------------------------------------------------------------
        protected override Size DefaultSize { get { return new Size(200, 100); } }

        public event SOSClassesComboBox.SOSClassesComboBoxDelegate SOSClassesComboBoxSelectedIndexChanged;

		//--------------------------------------------------------------------------------
        public ToolStripSosClassesCombo() : base(new SOSClassesComboBox()) 
        { }

        //--------------------------------------------------------------------------------
        public SOSClassesComboBox CmbClasses { get { return Control as SOSClassesComboBox; } }

		/// <summary>
		/// Attach to events we want to re-wrap
		/// </summary>
		//--------------------------------------------------------------------------------
		protected override void OnSubscribeControlEvents(Control control)
		{
			base.OnSubscribeControlEvents(control);
            CmbClasses.SOSClassesComboBoxSelectedIndexChanged += new SOSClassesComboBox.SOSClassesComboBoxDelegate(CmbClasses_SOSClassesComboBoxSelectedIndexChanged);
		}

        //--------------------------------------------------------------------------------
        void CmbClasses_SOSClassesComboBoxSelectedIndexChanged(Object sender, EventArgs e)       
		{
            if (SOSClassesComboBoxSelectedIndexChanged != null)
                SOSClassesComboBoxSelectedIndexChanged(sender, e);
        }

		/// <summary>
		/// Detach from events.
		/// </summary>
		//--------------------------------------------------------------------------------
		protected override void OnUnsubscribeControlEvents(Control control)
		{
			base.OnUnsubscribeControlEvents(control);
		}

        //--------------------------------------------------------------------------------
        internal void BeginUpdate()
        {
            CmbClasses.BeginUpdate();
        }

        //--------------------------------------------------------------------------------
        internal void EndUpdate()
        {
            CmbClasses.EndUpdate();
        }
    }
}