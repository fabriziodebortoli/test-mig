using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	/// <summary>
	/// Classe helper che permette di impostare i valori dei controlli GUI
	/// indipendentemente dal thread che ne invoca i metodi.
	/// </summary>
	//=========================================================================
	public class SafeGui
	{
		//---------------------------------------------------------------------
		private SafeGui(){}

		#region Public static methods
		//---------------------------------------------------------------------
		public static string ComboBox_GetItemText(ComboBox comboBox)
		{
			if (comboBox.InvokeRequired)
			{
				object[] parameters = { comboBox };
				return (string)comboBox.Invoke(new ComboBox_GetItemText_Delegate(ComboBox_GetItemText), parameters);
			}
			else
			{
				return
					comboBox.Items.Count > 0 ?
					comboBox.GetItemText(comboBox.SelectedItem) :
					string.Empty;
			}
		}
		#endregion

		#region Delegates
		private delegate string ComboBox_GetItemText_Delegate(ComboBox comboBox);
		private delegate void ProgressBarChangeDelegate(ProgressBar progressBar, int maximum, int currentValue);
		private delegate void ProgressBarValueDelegate		(ProgressBar progressBar, int aValue);
		private delegate int ProgressBarGetValueDelegate(ProgressBar progressBar);
		private delegate void ComboBoxFillDelegate(ComboBox comboBox, object[] values);
		private delegate void ComboBoxSelectIndexDelegate	(ComboBox comboBox, int index);
		private delegate void ComboBoxSelectValueDelegate	(ComboBox comboBox, object aValue);
		private delegate void ComboBoxClearDelegate			(ComboBox comboBox);
		private delegate void ButtonManageDelegate			(Button button, string text, bool enable);
		private delegate void ToolBarButtonManageDelegate	(ToolBar toolBar, int index, int imageIndex, string text, string toolTip, bool enable);
		private delegate void ControlEnabledDelegate		(Control aControl, bool enabled);
		private delegate void ControlFocusDelegate			(Control aControl);
		private delegate void ControlVisibleDelegate		(Control aControl, bool visible);
		private delegate void ControlTextDelegate			(Control aControl, string text);
		private delegate void RadioButtonCheckedDelegate	(RadioButton radioButton, bool check);
		private delegate void CheckBoxCheckDelegate			(CheckBox checkBox, bool check);
		private delegate void ComboItemSelectDelegate		(ComboBox combo, int itemToSelect);
		#endregion

		//---------------------------------------------------------------------
		[Obsolete("Use ControlText instead")]
		public static void LabelWrite(Label label, string text)
		{
			ControlText(label, text);
		}

		//---------------------------------------------------------------------
		[Obsolete("Use ControlText instead")]
		public static void TextBoxWrite(TextBox textBox, string text)
		{
			ControlText(textBox, text);
		}

		//---------------------------------------------------------------------
		public static void ControlEnabled(Control aControl, bool enabled)
		{
			if (aControl.InvokeRequired)
			{
				object[] parameters = {aControl, enabled};
				aControl.Invoke(new ControlEnabledDelegate(ControlEnabled), parameters);
			}
			else
				aControl.Enabled = enabled;
		}

		//---------------------------------------------------------------------
		public static void ControlFocus(Control aControl)
		{
			if (aControl.InvokeRequired)
			{
				object[] parameters = {aControl};
				aControl.Invoke(new ControlFocusDelegate(ControlFocus), parameters);
			}
			else
				aControl.Focus();
		}

		//---------------------------------------------------------------------
		public static void ControlVisible(Control aControl, bool visible)
		{
			if (aControl.InvokeRequired)
			{
				object[] parameters = {aControl, visible};
				aControl.Invoke(new ControlVisibleDelegate(ControlVisible), parameters);
			}
			else
				aControl.Visible = visible;
		}

		//---------------------------------------------------------------------
		public static void ControlText(Control aControl, string text)
		{
			if (aControl.InvokeRequired)
			{
				object[] parameters = {aControl, text};
				aControl.Invoke(new ControlTextDelegate(ControlText), parameters);
			}
			else
				aControl.Text = text;
		}

		//---------------------------------------------------------------------
		public static void RadioButtonCheck(RadioButton radioButton, bool check)
		{
			if (radioButton.InvokeRequired)
			{
				object[] parameters = {radioButton, check};
				radioButton.Invoke(new RadioButtonCheckedDelegate(RadioButtonCheck), parameters);
			}
			else
				radioButton.Checked = check;
		}
		
		//---------------------------------------------------------------------
		public static void CheckBoxCheck(CheckBox checkBox, bool check)
		{
			if (checkBox.InvokeRequired)
			{
				object[] parameters = {checkBox, check};
				checkBox.Invoke(new CheckBoxCheckDelegate(CheckBoxCheck), parameters);
			}
			else
				checkBox.Checked = check;
		}

		//---------------------------------------------------------------------
		public static void ComboItemSelect(ComboBox combo, int itemToSelect)
		{
			if (combo.InvokeRequired)
			{
				object[] parameters = {combo, itemToSelect};
				combo.Invoke(new ComboItemSelectDelegate(ComboItemSelect), parameters);
			}
			else
				combo.SelectedIndex = itemToSelect;
		}

		//---------------------------------------------------------------------
		[Obsolete("Use ControlEnabled")]
		public static void FormEnabled(Form aForm, bool enabled)
		{
			ControlEnabled(aForm, enabled);
		}

		//---------------------------------------------------------------------
		private delegate void FormCloseDelegate(Form form);
		static public void FormClose(Form form)
		{
			if (form == null || form.IsDisposed)
				return;
			if (form.InvokeRequired)
			{
				object[] parameters = { form };
				form.Invoke(new FormCloseDelegate(FormClose), parameters);
			}
			else
				form.Close();
		}

		//---------------------------------------------------------------------
		public static void MenuItemEnabled(MenuItem aMenuItem, bool enabled)
		{
			// MenuItem does not derive from component and does not implement
			// the ISynchronizeInvoke interface, so no InvokeRequired and Invoke,
			// but it would need it as is subject to the same problems as the other
			// Windows.Forms stuff, all single apartment threaded...
			// ...let's use a little hack
			if (context == null)
			{
				Debug.Fail("Synchronization context not set, using non thread-safe mode");
				aMenuItem.Enabled = enabled;
			}
			context.Send(
				delegate(object o)
				{
					aMenuItem.Enabled = enabled;
				}, null);

			//NOTA: MenuItem non ha Invoke, magari non ne ha bisogno.
//			if (aMenuItem.InvokeRequired)
//			{
//				object[] parameters = {aMenuItem, enabled};
//				aMenuItem.Invoke(new ControlEnabledDelegate(InternalControlEnabled), parameters);
//			}
//			else
//				aMenuItem.Enabled = enabled;
		}

		//---------------------------------------------------------------------
		public static void ToolStripMenuItemEnabled(ToolStripMenuItem aToolStripMenuItem, bool enabled)
		{
			if (context == null)
			{
				Debug.Fail("Synchronization context not set, using non thread-safe mode");
				aToolStripMenuItem.Enabled = enabled;
			}

			context.Send(delegate(object o) { aToolStripMenuItem.Enabled = enabled; }, null);
		}

		private static System.Threading.SynchronizationContext context; // hack
		public static void SetSynchronizationContext(System.Threading.SynchronizationContext context)
		{
			SafeGui.context = context;
		}

		//---------------------------------------------------------------------
		public static void ProgressBarChange(ProgressBar progressBar, int maximum, int currentValue)
		{

			if (progressBar.InvokeRequired)
			{
				object[] parameters = { progressBar, maximum, currentValue };
				progressBar.Invoke(new ProgressBarChangeDelegate(ProgressBarChange), parameters);
			}
			else
			{
				if (currentValue > maximum)
					currentValue = maximum;
				progressBar.Maximum = maximum;
				progressBar.Value = currentValue;
			}
		}

		//---------------------------------------------------------------------
		public static void ProgressBarValue(ProgressBar progressBar, int aValue)
		{
			if (progressBar.InvokeRequired)
			{
				object[] parameters = { progressBar, aValue };
				progressBar.Invoke(new ProgressBarValueDelegate(ProgressBarValue), parameters);
			}
			else
				progressBar.Value = aValue;
		}

		//---------------------------------------------------------------------
		public static int ProgressBarValue(ProgressBar progressBar)
		{
			if (progressBar.InvokeRequired)
			{
				object[] parameters = { progressBar };
				return (int)progressBar.Invoke(new ProgressBarGetValueDelegate(ProgressBarValue), parameters);
			}
			else
				return progressBar.Value;
		}

		//---------------------------------------------------------------------
		private delegate void ProgressBarPerformStepDelegate(ProgressBar progressBar);
		public static void ProgressBarPerformStep(ProgressBar progressBar)
		{
			if (progressBar.InvokeRequired)
				progressBar.Invoke(new ProgressBarPerformStepDelegate(ProgressBarPerformStep), new object[] { progressBar });
			else
				progressBar.PerformStep();
		}

		//---------------------------------------------------------------------
		[Obsolete("Use ControlVisible instead")]
		public static void ProgressBarVisible(ProgressBar progressBar, bool visible)
		{
			ControlVisible(progressBar, visible);
		}

		//---------------------------------------------------------------------
		public static void ComboBoxFill(ComboBox comboBox, object[] values)
		{
			if (comboBox.InvokeRequired)
			{
				object[] parameters = { comboBox, values };
				comboBox.Invoke(new ComboBoxFillDelegate(ComboBoxFill), parameters);
			}
			else
			{
				comboBox.Items.Clear();
				foreach (object aValue in values)
					comboBox.Items.Add(aValue);
			}
		}

		//---------------------------------------------------------------------
		public static void ComboBoxSelectIndex(ComboBox comboBox, int index)
		{
			if (comboBox.InvokeRequired)
			{
				object[] parameters = {comboBox, index};
				comboBox.Invoke(new ComboBoxSelectIndexDelegate(ComboBoxSelectIndex), parameters);
			}
			else
				comboBox.SelectedIndex = index;
		}

		//---------------------------------------------------------------------
		public static void ComboBoxSelectValue(ComboBox comboBox, object aValue)
		{
			if (comboBox.InvokeRequired)
			{
				object[] parameters = {comboBox, aValue};
				comboBox.Invoke(new ComboBoxSelectValueDelegate(ComboBoxSelectValue), parameters);
			}
			else
				comboBox.SelectedItem = aValue;
		}

		//---------------------------------------------------------------------
		public static void ComboBoxClear(ComboBox comboBox)
		{
			if (comboBox.InvokeRequired)
			{
				object[] parameters = { comboBox };
				comboBox.Invoke(new ComboBoxClearDelegate(ComboBoxClear), parameters);
			}
			else
			{
				comboBox.Items.Clear();
				comboBox.Text = null;
			}
		}

		//---------------------------------------------------------------------
		public static void ButtonManage(Button button, string text, bool enable)
		{
			if (button.InvokeRequired)
			{
				object[] parameters = { button, text, enable };
				button.Invoke(new ButtonManageDelegate(ButtonManage), parameters);
			}
			else
			{
				button.Text = text;
				button.Enabled = enable;
			}
		}

		//---------------------------------------------------------------------
		public static void ToolBarButtonManage(ToolBar toolBar, int index, int imageIndex, string text, string toolTip, bool enable)
		{
			if (toolBar.InvokeRequired)
			{
				object[] parameters = { toolBar, index, imageIndex, text, toolTip, enable };
				toolBar.Invoke(new ToolBarButtonManageDelegate(ToolBarButtonManage), parameters);
			}
			else
			{
				if (index < 0 || toolBar.Buttons.Count <= index) return;
				toolBar.Buttons[index].Enabled = enable;
				if (toolTip != null)
					toolBar.Buttons[index].ToolTipText = toolTip;
				if (text != null)
					toolBar.Buttons[index].Text = text;
				if (imageIndex > -1)
					toolBar.Buttons[index].ImageIndex = imageIndex;
			}
		}

	}

	//=========================================================================
	public class SafeTreeView
	{
		//---------------------------------------------------------------------
		private delegate void AddNodeDelegate(TreeView tree, TreeNode node);

		//---------------------------------------------------------------------
		public static void AddNode(TreeView tree, TreeNode node)
		{
			if (tree.InvokeRequired)
			{
				object[] parameters = {tree, node};
				tree.Invoke(new AddNodeDelegate(AddNode), parameters);
			}
			else
				tree.Nodes.Add(node);
		}

		//---------------------------------------------------------------------
		private delegate void TreeDelegate(TreeView tree);

		//---------------------------------------------------------------------
		public static void BeginUpdate(TreeView tree)
		{
			if (tree.InvokeRequired)
			{
				object[] parameters = {tree};
				tree.Invoke(new TreeDelegate(BeginUpdate), parameters);
			}
			else
				// The BeginUpdate method prevents the control from painting
				// until the EndUpdate method is called.
				tree.BeginUpdate();
		}

		//---------------------------------------------------------------------
		public static void EndUpdate(TreeView tree)
		{
			if (tree.InvokeRequired)
			{
				object[] parameters = {tree};
				tree.Invoke(new TreeDelegate(EndUpdate), parameters);
			}
			else
				tree.EndUpdate();
		}

		//---------------------------------------------------------------------
		public static void Clear(TreeView tree)
		{
			if (tree.InvokeRequired)
			{
				object[] parameters = { tree };
				tree.Invoke(new TreeDelegate(Clear), parameters);
			}
			else
			{
				if (tree == null) return;
				tree.Nodes.Clear();
			}
		}

		//---------------------------------------------------------------------
		private delegate void EnableDelegate(TreeView tree, bool enabled);

		//---------------------------------------------------------------------
		public static void Enable(TreeView tree, bool enabled)
		{
			if (tree.InvokeRequired)
			{
				object[] parameters = {tree, enabled};
				tree.Invoke(new EnableDelegate(Enable), parameters);
			}
			else
				tree.Enabled = enabled;
		}
	}

	//=========================================================================
	public class SafeTreeNode
	{
		//---------------------------------------------------------------------
		private delegate void NodeDelegate(TreeNode node);

		//---------------------------------------------------------------------
		public static void Expand(TreeNode node)
		{
			if (node == null || node.TreeView == null) return;
			if (node.TreeView.InvokeRequired)
			{
				object[] parameters = {node};
				node.TreeView.Invoke(new NodeDelegate(Expand), parameters);
			}
			else
				node.Expand();
		}

		//---------------------------------------------------------------------
		private delegate void SetImageIndexDelegate(TreeNode node, int index);

		//---------------------------------------------------------------------
		public static void SetImageIndex(TreeNode node, int index)
		{
			if (node == null || node.TreeView == null) return;
			if (node.TreeView.InvokeRequired)
			{
				object[] parameters = {node, index};
				node.TreeView.Invoke(new SetImageIndexDelegate(SetImageIndex), parameters);
			}
			else
				node.ImageIndex = index;
		}
		public static void SetSelectedImageIndex(TreeNode node, int index)
		{
			if (node == null || node.TreeView == null) return;
			if (node.TreeView.InvokeRequired)
			{
				object[] parameters = {node, index};
				node.TreeView.Invoke(new SetImageIndexDelegate(SetSelectedImageIndex), parameters);
			}
			else
				node.SelectedImageIndex = index;
		}

		//---------------------------------------------------------------------
		private delegate void AddNodeDelegate(TreeNode node, TreeNode childNode);

		//---------------------------------------------------------------------
		public static void AddNode(TreeNode node, TreeNode childNode)
		{
			if (node == null || node.TreeView == null) return;

			if (node.TreeView.InvokeRequired)
			{
				object[] parameters = {node, childNode};
				node.TreeView.Invoke(new AddNodeDelegate(AddNode), parameters);
			}
			else
				node.Nodes.Add(childNode);
		}

		//---------------------------------------------------------------------
	}

	//=========================================================================
	public class SafeMessageBox
	{
		//---------------------------------------------------------------------
		private static DialogResult dialogResult = DialogResult.No;
		public static DialogResult Show
			(
			Control owner,
			string msg,
			string title,
			MessageBoxButtons messageBoxButtons,
			MessageBoxIcon messageBoxIcon,
			MessageBoxDefaultButton messageBoxDefaultButton
			)
		{
			lock (typeof(SafeMessageBox))
			{
				SafeShow(owner, msg, title, messageBoxButtons, messageBoxIcon, messageBoxDefaultButton);
				return dialogResult; // might be used by other threads, so I lock the class
			}
		}
		private delegate void ShowDelegate
			(
			Control owner,
			string msg,
			string title,
			MessageBoxButtons messageBoxButtons,
			MessageBoxIcon messageBoxIcon,
			MessageBoxDefaultButton messageBoxDefaultButton
			);
		private static void SafeShow
			(
			Control owner,
			string msg,
			string title,
			MessageBoxButtons messageBoxButtons,
			MessageBoxIcon messageBoxIcon,
			MessageBoxDefaultButton messageBoxDefaultButton
			)
		{
			if (owner.InvokeRequired)
			{
				ShowDelegate d = new ShowDelegate(SafeShow);
				owner.Invoke(d, new object[] { owner, msg, title, messageBoxButtons, messageBoxIcon, messageBoxDefaultButton });
			}
			else
			{
				DialogResult result = MessageBox.Show
					(
					owner,
					msg,
					title,
					messageBoxButtons,
					messageBoxIcon,
					messageBoxDefaultButton
					);
				dialogResult = result;
			}
		}
	}

    //=========================================================================
    public class SafeToolStripItem
    {


        //---------------------------------------------------------------------
        private delegate void ToolStripItemTextDelegate(ToolStripItem i, string text);
        //---------------------------------------------------------------------
        public static void Text(ToolStripItem i, string text)
        {
            if (i == null || i.GetCurrentParent() == null)
                return;

            if (i.GetCurrentParent().InvokeRequired)
            {
                object[] parameters = { i, text };
                i.GetCurrentParent().Invoke(new ToolStripItemTextDelegate(Text), parameters);
            }
            else
                i.Text = text;
        }

        //---------------------------------------------------------------------
        private delegate void ToolStripItemEnabledDelegate(ToolStripItem i, bool enabled);
        //---------------------------------------------------------------------
        public static void Enabled(ToolStripItem i, bool enabled)
        {
            if (i == null || i.GetCurrentParent() == null)
                return;

            if (i.GetCurrentParent().InvokeRequired)
            {
                object[] parameters = { i, enabled };
                i.GetCurrentParent().Invoke(new ToolStripItemEnabledDelegate(Enabled), parameters);
            }
            else
                i.Enabled = enabled;
        }
    }


     //=========================================================================
    public class SafeControlProperty
    {
        private delegate void SetControlPropertyThreadSafeDelegate(Control control, string propertyName, object propertyValue);

        public static void Set(Control control, string propertyName, object propertyValue)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetControlPropertyThreadSafeDelegate(Set), new object[] { control, propertyName, propertyValue });
            }
            else
            {
                control.GetType().InvokeMember(propertyName, BindingFlags.SetProperty, null, control, new object[] { propertyValue });
            }
        }
    }
}
