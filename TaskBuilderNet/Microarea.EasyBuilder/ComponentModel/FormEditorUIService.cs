using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyBuilder.ComponentModel
{
	/// <summary>
	/// Implements the IUIService interface.
	/// </summary>
	/// <seealso cref="System.Windows.Forms.Design.IUIService"/>
	//=========================================================================
	public class FormEditorUIService : IUIService
	{
		Control mainForm;
		Hashtable styles;

		/// <summary>
		/// Initializes a new instance of the FormEditorUIService with the given instance
		/// of the FormEditor as main form.
		/// </summary>
		/// <param name="mainForm">The mainForm to be used to show dialogs and forms</param>
		/// <seealso cref="Microarea.EasyBuilder.FormEditor"/>
		//-----------------------------------------------------------------------------
		public FormEditorUIService(Control mainForm)
		{
			this.mainForm = mainForm;

			styles = new Hashtable();
			styles.Add("DialogFont", new Font("Tahoma", 8.25F, FontStyle.Regular));
			styles.Add("HighlightColor", Color.FromArgb(255, 251, 233));
		}

		/// <summary>
		/// Returns a value indicating if a component editor can be showed for the
		/// given component.
		/// </summary>
		/// <param name="component">
		/// The component to evaluate whether to show the component editor or not.
		/// </param>
		/// <remarks>
		/// The return value determines if the "PropertyPages" tab can be showed for
		/// the current property grid.
		/// </remarks>
		/// <seealso cref="System.Windows.Forms.PropertyGrid"/>
		//-----------------------------------------------------------------------------
		public bool CanShowComponentEditor(object component)
		{
			//il valore di ritorno pilota la logica con cui viene mostrato il pulsante "PropertyPages"
			//della property grid.
			return TypeDescriptor.GetEditor(component, typeof(ComponentEditor)) != null;
		}

		/// <summary>
		/// Returns the owner for the dialog to be showed.
		/// </summary>
		/// <remarks>The owner is the FormEditor instance used to create this instance.</remarks>
		/// <seealso cref="System.Windows.Forms.IWin32Window"/>
		//-----------------------------------------------------------------------------
		public IWin32Window GetDialogOwnerWindow()
		{
			return mainForm;
		}

		/// <summary>
		/// Sets the boolean value indicating that there are unsaved changes.
		/// </summary>
		//-----------------------------------------------------------------------------
		public void SetUIDirty()
		{
			IDirtyManager dirtyMng = mainForm as IDirtyManager;
			if (dirtyMng != null)
				dirtyMng.SetDirty(true);
		}

		/// <summary>
		/// Shows the component editor fot the given component as a dialog with the
		/// given owner.
		/// </summary>
		/// <param name="component">The component to show the component editor for</param>
		/// <param name="parent">The parent for the component editor window</param>
		//-----------------------------------------------------------------------------
		public bool ShowComponentEditor(object component, IWin32Window parent)
		{
			return true;
		}

		/// <summary>
		/// Shows the given form as a modal dialog.
		/// </summary>
		/// <param name="form">The form to be showed</param>
		//-----------------------------------------------------------------------------
		public DialogResult ShowDialog(Form form)
		{
			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				return form.ShowDialog(mainForm);
			}
		}

		/// <summary>
		/// Shows a MessageBox with the given exception and message.
		/// </summary>
		/// <param name="ex">The exception describing the error</param>
		/// <param name="message">A message about the error</param>
		/// <seealso cref="System.Windows.Forms.MessageBox"/>
		/// <seealso cref="System.Exception"/>
		//-----------------------------------------------------------------------------
		public void ShowError(Exception ex, string message)
		{
			MessageBox.Show(mainForm, message + Environment.NewLine + Environment.NewLine + ex.ToString());
		}

		/// <summary>
		/// Shows a MessageBox with the given exception.
		/// </summary>
		/// <param name="ex">The exception describing the error</param>
		/// <seealso cref="System.Windows.Forms.MessageBox"/>
		/// <seealso cref="System.Exception"/>
		public void ShowError(Exception ex)
		{
			MessageBox.Show(mainForm, ex.Message);
		}

		/// <summary>
		/// Shows a MessageBox with the given message.
		/// </summary>
		/// <param name="message">A message about the error</param>
		/// <seealso cref="System.Windows.Forms.MessageBox"/>
		//-----------------------------------------------------------------------------
		public void ShowError(string message)
		{
			MessageBox.Show(mainForm, message);
		}

		/// <summary>
		/// Shows a MessageBox with the given message, caption and buttons.
		/// </summary>
		/// <param name="message">A message about the error</param>
		/// <param name="caption">A caption for the dialog</param>
		/// <param name="buttons">Buttons to be showed by the dialog</param>
		/// <seealso cref="System.Windows.Forms.MessageBox"/>
		/// <seealso cref="System.Windows.Forms.MessageBoxButtons"/>
		//-----------------------------------------------------------------------------
		public DialogResult ShowMessage(string message, string caption, MessageBoxButtons buttons)
		{
			return MessageBox.Show(mainForm, message, caption, buttons, MessageBoxIcon.Information);
		}

		/// <summary>
		/// Shows a MessageBox with the given message and caption.
		/// </summary>
		/// <param name="message">A message about the error</param>
		/// <param name="caption">A caption for the dialog</param>
		/// <seealso cref="System.Windows.Forms.MessageBox"/>
		//-----------------------------------------------------------------------------
		public void ShowMessage(string message, string caption)
		{
			MessageBox.Show(mainForm, message, caption);
		}

		/// <summary>
		/// Shows a MessageBox with the given message.
		/// </summary>
		/// <param name="message">A message about the error</param>
		/// <seealso cref="System.Windows.Forms.MessageBox"/>
		//-----------------------------------------------------------------------------
		public void ShowMessage(string message)
		{
			MessageBox.Show(mainForm, message);
		}

		/// <summary>
		/// Shows the tool window identified by the given guid
		/// </summary>
		/// <seealso cref="System.Guid"/>
		//-----------------------------------------------------------------------------
		public bool ShowToolWindow(Guid toolWindow)
		{
			return true;
		}

		/// <summary>
		/// Gets a collection of styles for the dialog to be showed.
		/// </summary>
		//-----------------------------------------------------------------------------
		public IDictionary Styles
		{
			get
			{
				return styles;
			}
		}
    }

    /// <remarks />
    public static class IUIServiceExtensions
    {
        /// <remarks />
        public static void ShowModalMessageBox(this IUIService @this, Action messageBoxAction)
        {
            System.Windows.Forms.Form activeForm = System.Windows.Forms.Form.ActiveForm;
            if (activeForm == null)
            {
                foreach (System.Windows.Forms.Form form in System.Windows.Forms.Application.OpenForms)
                {
                    activeForm = form;
                    if (activeForm != null)
                    {
                        break;
                    }
                }
            }
            System.Diagnostics.Debug.Assert(activeForm != null);

            activeForm.Invoke(new Action(
                ()
                =>
                {
                    messageBoxAction();
                }
                ));
        }
    }
}
