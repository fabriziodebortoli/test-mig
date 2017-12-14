
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
	//==========================================================================================
	public class TBDropDownList : TBTextBox
	{
		public Panel downArrowPanel;
		public Image downArrowImage;

		//--------------------------------------------------------------------------------------
		protected override bool IsStatic { get { return false; } }

		protected override bool IsMultiline { get { return false; } }
		//--------------------------------------------------------------------------------------
		public TBDropDownList()
		{
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			downArrowPanel = new Panel();
			downArrowImage = new Image();
			downArrowImage.ImageUrl = ImagesHelper.CreateImageAndGetUrl("DropDownArrow.png", TBWebFormControl.DefaultReferringType);
			downArrowImage.Height = Unit.Percentage(100);
			base.OnInit(e);
			Page.RegisterRequiresControlState(this);
		
			downArrowPanel.ID = InnerControl.ID + "_ArrowPnl";
			downArrowPanel.TabIndex = -1;
			downArrowPanel.EnableViewState = false;
			downArrowPanel.CssClass = "TBDropDownListPnl";
			downArrowPanel.Controls.Add(downArrowImage);
			ContentTemplateContainer.Controls.Add(downArrowPanel);
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();

			((TextBox)InnerControl).ReadOnly = ((ComboDescription)ControlDescription).ReadOnly && (!IsEnabled);
			
			InnerControl.Attributes["onkeydown"] = string.Format("return tbComboEditKeyDown(event, this,'{0}','{1}','{2}','{3}')", windowId, ClientID, OwnerForm.InnerControl.ClientID, InnerControl.ClientID);
			
			int rad = formControl.IsMacDevice() ? roundCornerRadius * 2 : 0;
			//ripesco il valore di x e y cui e' posizionata la combobox. Potrebbe essere diverso dalle
			//property X e Y se c'e' un childoffest
			int yTextbox = (int)Unit.Parse(InnerControl.Style[HtmlTextWriterStyle.Top]).Value;
			int xTextbox = (int)Unit.Parse(InnerControl.Style[HtmlTextWriterStyle.Left]).Value;

			int textBoxBorderWidth = 2;
			downArrowPanel.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(yTextbox + 1 + rad/8).ToString();
			int btnWidth = 19;

			downArrowPanel.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(textBoxBorderWidth + xTextbox + (int)InnerControl.Width.Value - btnWidth + rad / 2).ToString();
			downArrowPanel.Style[HtmlTextWriterStyle.ZIndex] = (ZIndex + 1).ToString();
			downArrowPanel.Height = Unit.Pixel((int)InnerControl.Height.Value + textBoxBorderWidth);
			downArrowPanel.Width = Unit.Pixel(btnWidth);
			//devo rimuovere o aggiungere l'evento di onclick in base allo stato di abilitazione
			//non posso usare la proprieta' downArrowPanel.Enabled perche' Asp.Net genera il codice html
			//<div disabled="disabled"/>, ma l'attributo disabled non e' supportato dallo standard (in chrome
			//e safari viene giustamente ignorato)

			if (IsEnabled)
			{
				downArrowPanel.Style.Add(HtmlTextWriterStyle.Cursor, "hand");
				//brutto if per differenziare il comportamento della combo su dispositivo Ipad-Iphone
				if ( formControl.IsMacDevice())
					downArrowPanel.Attributes["onclick"] = string.Format("tbDropDownTable('{0}', '{1}')", InnerControl.ClientID, WindowId);		
				else
					downArrowPanel.Attributes["onclick"] = string.Format("tbDropDown('{0}', '{1}')", InnerControl.ClientID, WindowId);	
			}
			else
			{
				downArrowPanel.Attributes.Remove("onclick");
				downArrowPanel.Style.Remove(HtmlTextWriterStyle.Cursor);								
			}
		}

		//--------------------------------------------------------------------------------------
		public override void AddChildControls(WndObjDescription description)
		{
			//does nothing
		}
	}

	//==========================================================================================
	public class TBMSDropDownList : TBTextBox
	{

		public Panel downArrowPanel;
		public Image downArrowImage;

		//--------------------------------------------------------------------------------------
		protected override bool IsStatic { get { return false; } }

		protected override bool IsMultiline { get { return false; } }
		//--------------------------------------------------------------------------------------
		public TBMSDropDownList()
		{
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			downArrowPanel = new Panel();
			downArrowImage = new Image();
			downArrowImage.ImageUrl = ImagesHelper.CreateImageAndGetUrl("DropDownArrow.png", TBWebFormControl.DefaultReferringType);
			downArrowImage.Height = Unit.Percentage(100);
			base.OnInit(e);
			Page.RegisterRequiresControlState(this);
		
			downArrowPanel.ID = InnerControl.ID + "_ArrowPnl";
			downArrowPanel.TabIndex = -1;
			downArrowPanel.EnableViewState = false;
			downArrowPanel.CssClass = "TBDropDownListPnl";
			downArrowPanel.Controls.Add(downArrowImage);
			ContentTemplateContainer.Controls.Add(downArrowPanel);
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();

			string cTxt = ((MSComboDescription)ControlDescription).ComboTxt;

			SetText(cTxt);

			((TextBox)InnerControl).ReadOnly = ((MSComboDescription)ControlDescription).ReadOnly && (!IsEnabled);
			
			InnerControl.Attributes["onkeydown"] = string.Format("return tbComboEditKeyDown(event, this,'{0}','{1}','{2}','{3}')", windowId, ClientID, OwnerForm.InnerControl.ClientID, InnerControl.ClientID);
			
			int rad = formControl.IsMacDevice() ? roundCornerRadius * 2 : 0;
			//ripesco il valore di x e y cui e' posizionata la combobox. Potrebbe essere diverso dalle
			//property X e Y se c'e' un childoffest
			int yTextbox = (int)Unit.Parse(InnerControl.Style[HtmlTextWriterStyle.Top]).Value;
			int xTextbox = (int)Unit.Parse(InnerControl.Style[HtmlTextWriterStyle.Left]).Value;

			int textBoxBorderWidth = 2;
			downArrowPanel.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(yTextbox + 1 + rad/8).ToString();
			int btnWidth = 19;

			downArrowPanel.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(textBoxBorderWidth + xTextbox + (int)InnerControl.Width.Value - btnWidth + rad / 2).ToString();
			downArrowPanel.Style[HtmlTextWriterStyle.ZIndex] = (ZIndex + 1).ToString();
			downArrowPanel.Height = Unit.Pixel((int)InnerControl.Height.Value + textBoxBorderWidth);
			downArrowPanel.Width = Unit.Pixel(btnWidth);
			
			if (IsEnabled)
			{
				downArrowPanel.Style.Add(HtmlTextWriterStyle.Cursor, "hand");
				downArrowPanel.Attributes["onclick"] = string.Format("tbMSDropDown('{0}', '{1}')", InnerControl.ClientID, WindowId);	
			}
			else
			{
				downArrowPanel.Attributes.Remove("onclick");
				downArrowPanel.Style.Remove(HtmlTextWriterStyle.Cursor);								
			}
		}

		//--------------------------------------------------------------------------------------
		public override void AddChildControls(WndObjDescription description)
		{
			//does nothing
		}
	}
	
}