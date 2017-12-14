using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Validation;
using Microarea.TaskBuilderNet.Forms.Controls;
using Microarea.TaskBuilderNet.Forms.Properties;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.Validation;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.TaskBuilderNet.Forms
{
	internal enum HotLinkUIStyle { None, Button };

	//================================================================================================================
	sealed class TBWinHotLinkExtender : TBWinExtender, ITBHotLinkUIProvider
	{
		HotLinkUIStyle uiStyle;

		MHotLink hotLink;

		UIClickablePanel upperButton;
		UIClickablePanel lowerButton;
		UIPanel panel;
		UISingleSelectionDropDownList combo;

    	TBValidationProvider validationProvider;
		public ITBValidationProvider ValidationProvider { get { return validationProvider; } }

		//----------------------------------------------------------------------------
		public TBWinHotLinkExtender(TBWFCUIControl tbCUI, HotLinkUIStyle uiStyle = HotLinkUIStyle.Button)
			: base(tbCUI)
		{
			hotLink = Data != null ? Data.CurrentHotLink as MHotLink : null;
			combo = Controller.Control as UISingleSelectionDropDownList;
			if (combo != null)
			{
				combo.UIDropDownOpening += new EventHandler<EventArgs>(combo_DropDown);
				combo.UIDropDownClosed += new EventHandler<EventArgs>(combo_DropDownClosed);
				combo.DisplayMember = "Description";
				combo.ValueMember = "Key";
			}

			this.uiStyle = uiStyle;

			validationProvider = new TBValidationProvider();
			validationProvider.ValidationModes = ValidationModes.FocusChange;
			validationProvider.Validating += new EventHandler<ValidationEventArgs>(Validate);
		}

		//----------------------------------------------------------------------------
		public override void OnDataReadOnlyChanged(bool newReadOnly)
		{
			base.OnDataReadOnlyChanged(newReadOnly);

			if (panel != null)
			{
				panel.Enabled = !newReadOnly;
			}
		}

		//----------------------------------------------------------------------------
		public override void OnFormModeChanged(FormModeType newFormMode)
		{
			base.OnFormModeChanged(newFormMode);

			if (panel == null)
				return;

			panel.Enabled = newFormMode == FormModeType.Edit || newFormMode == FormModeType.New;
		}

		//----------------------------------------------------------------------------
		public override IUIControl CreateExtenderUIControl()
		{
			// todo diagnostica
			if (Data == null || hotLink == null || Controller == null || Extendee == null)
				return null;

			if (this.uiStyle == HotLinkUIStyle.Button)
			{
				Control control = Controller.Control as Control;

				panel = new UIPanel();
				panel.Size = new Size(20, 20);
				panel.TabStop = false;
				panel.Visible = control.Visible;

				upperButton = new UIClickablePanel();
				panel.Controls.Add(upperButton);

				lowerButton = new UIClickablePanel();
				panel.Controls.Add(lowerButton);

				upperButton.Size = new Size(20, 10);
				upperButton.Click += new EventHandler(UpperButton_Click);
				upperButton.TabStop = false;
				upperButton.BackgroundImage = Resources.Hotlink_Upper;
				
				lowerButton.Size = new Size(20, 10);
				lowerButton.Click += new EventHandler(LowerButton_Click);
				lowerButton.BackgroundImage = Resources.Hotlink_Lower;
				lowerButton.TabStop = false;

				upperButton.Location = new Point(0, 0);
				lowerButton.Location = new Point(0, upperButton.Bottom);

				return panel;
			}

			return null;
		}

		//-------------------------------------------------------------------------
		public override void DestroyExtenderUIControl()
		{
			if (upperButton != null && !upperButton.IsDisposed)
			{
				upperButton.Dispose();
				upperButton = null;
			}

			if (lowerButton != null && !lowerButton.IsDisposed)
			{
				lowerButton.Dispose();
				lowerButton = null;
			}
			if (panel != null && !panel.IsDisposed)
			{
				panel.Dispose();
				panel = null;
			}
		}

		//----------------------------------------------------------------------------
		public override IList<IMenuItemGeneric> GetContextMenuItems()
		{
			IList<IMenuItemGeneric> items = base.GetContextMenuItems();
			if (hotLink == null)
				return items;

			MenuItemClickable item = new MenuItemClickable
			(
				Resources.CodeRadarString,
				"CodeRadar",
				UpperButton_Click,
				() => { return true; },
				() => { return true; },
				Keys.F8
			);
			items.Add(item);


			item = new MenuItemClickable
			(
				Resources.DescriptionRadarString,
				"DescriptionRadar",
				LowerButton_Click,
				() => { return true; },
				() => { return true; },
				Keys.F9
			);
			items.Add(item);

			item = new MenuItemClickable
			(
				Resources.OpenLinkString,
				"OpenLink",
				OpenLink_Click,
				() => { return true; },
				() => { return true; }
			);
			items.Add(item);

			item = new MenuItemClickable
			(
				Resources.AddOnFlyString,
				"AddOnFly",
				AddOnFlyRadar_Click,
				() => { return true; },
				() => { return true; }
			);
			items.Add(item);

			foreach (MHotLinkSearch current in hotLink.Searches)
			{
				if (current.AssociatedButton != MHotLinkSearch.ButtonAssociation.None)
					continue;

				string text = current.Description.IsNullOrEmpty() ? hotLink.Name : current.Description;

				item = new MenuItemClickable
				(
					text,
					text,
					tempItem_Click,
					() => { return true; },
					() => { return true; },
					Keys.None,
					current
				);
				items.Add(item);
			}

			return items;
		}

		//-------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				DestroyExtenderUIControl();

				if (validationProvider != null)
				{
					validationProvider.Validating -= new EventHandler<ValidationEventArgs>(Validate);
					validationProvider.Dispose();
					validationProvider = null;
				}

				if (combo != null)
				{
					combo.UIDropDownOpening -= new EventHandler<EventArgs>(combo_DropDown);
					combo.UIDropDownClosed -= new EventHandler<EventArgs>(combo_DropDownClosed);
					combo = null;
				}
			}
		}

		//----------------------------------------------------------------------------
		void Validate(object sender, ValidationEventArgs e)
		{
			if (Data == null || hotLink == null)
				return;

			MDataObj currentValue = MDataObj.Create(Data.DataType) as MDataObj;
			currentValue.Value = e.Value;

			// dato esistente, tutto ok
			if (hotLink.ExistData(currentValue) || !hotLink.DataMustExist)
			{
				Controller.SetModified();
				hotLink.ClearRunningMode();
				Data.Value = currentValue.Value;

				return;
			}

			// eventuale stato di errore
			if (
				!hotLink.CanAddOnFly ||
				MessageBox.Show(
					string.Format(System.Globalization.CultureInfo.InvariantCulture, "Data {0} not found.\r\n Do you want to enter it now?", currentValue.FormatData()),
					"Add On Fly",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1,
					MessageBoxOptions.DefaultDesktopOnly
					) == DialogResult.No
				)
			{
				e.Info.Validated = false;
				e.Info.Message = "Il dato non esiste!!!!";

				hotLink.ClearRunningMode();

				return;
			}

			IMAbstractFormDoc document = hotLink.ExecuteAddOnFly(currentValue, false);
			document.DocumentClosed += new EventHandler<EventArgs>(document_DocumentClosed);
			document.DocumentSaved += new EventHandler<EventArgs>(document_DocumentSaved);

			e.Info.Validated = false;

			// dato esistente, tutto ok
			hotLink.ClearRunningMode();
		}

		//----------------------------------------------------------------------------
		void combo_DropDownClosed(object sender, EventArgs e)
		{
			if (combo == null || combo.SelectedIndex < 0)
			{
				return;
			}
			combo.DisplayMember = "Key";
		}

		//----------------------------------------------------------------------------
		void combo_DropDown(object sender, EventArgs e)
		{
			if (combo == null)
			{
				return;
			}

			combo.DisplayMember = "Description";
			combo.DataSource = hotLink.SearchComboQueryData(-1);
		}

		//----------------------------------------------------------------------------
		void tempItem_Click(object sender, EventArgs e)
		{
            if (hotLink == null)
                return;

			IUIMenuItem menuItem = sender as IUIMenuItem;
			if (menuItem == null)
				return;

			MHotLinkSearch search = menuItem.Tag as MHotLinkSearch;
			if (search == null)
				return;

			Controller.UIValueToDataObj(Data);

			hotLink.SearchOnLink(search);
		}

		//----------------------------------------------------------------------------
		void AddOnFlyRadar_Click(object sender, EventArgs e)
		{
		}

		//----------------------------------------------------------------------------
		void OpenLink_Click(object sender, EventArgs e)
		{
		}

		//----------------------------------------------------------------------------
		void UpperButton_Click(object sender, EventArgs e)
		{
            if (hotLink != null)
            {
				Controller.UIValueToDataObj(Data);
                hotLink.SearchOnLinkUpper();
            }
		}

		//----------------------------------------------------------------------------
		void LowerButton_Click(object sender, EventArgs e)
		{
            if (hotLink != null)
            {
				Controller.UIValueToDataObj(Data);
                hotLink.SearchOnLinkLower();
            }
		}

        //-------------------------------------------------------------------------
        void document_DocumentSaved(object sender, EventArgs e)
        {
            IDataObj recField = hotLink.Record.GetData(hotLink.Searches.ByKey.FieldName);
	        if (recField == null)
	        {
		        return;
	        }
			Controller.SetModified();
        }

        //-------------------------------------------------------------------------
        void document_DocumentClosed(object sender, EventArgs e)
		{
			MAbstractFormDoc document = sender as MAbstractFormDoc;
			if (document == null)
				return;

			document.DocumentClosed -= new EventHandler<EventArgs>(document_DocumentClosed);
			document.DocumentSaved -= new EventHandler<EventArgs>(document_DocumentSaved);
        }
    }
}
