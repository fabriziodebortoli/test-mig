using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
	public class TBMessageControl : TBForm
	{
		IDiagnostic diagnostic;
        string messageFont = "Tahoma";
		Button closeTBMessageControl;
        //--------------------------------------------------------------------------------------
		public TBMessageControl (IDiagnostic diagnostic )
		{
			this.diagnostic = diagnostic;
			closeTBMessageControl = new Button();
		}

        //--------------------------------------------------------------------------------------
		public override string Title { get { return diagnostic.Error ? TBWebFormControlStrings.Error : diagnostic.Warning ? TBWebFormControlStrings.Warning : TBWebFormControlStrings.Information; } }

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			CreateTextBox();
			CreateCloseButton();
			base.OnInit(e);

			EasyLookCustomization.EasyLookCustomSettings easyLookCustomSettings =
				(EasyLookCustomization.EasyLookCustomSettings)Page.Session[EasyLookCustomization.EasyLookCustomSettings.SessionKey];
            if (easyLookCustomSettings != null)
                messageFont = easyLookCustomSettings.FontFamily;

		}

		//--------------------------------------------------------------------------------------
		protected override void AddFocusDummyField()
		{
			//does nothing
		}

		//--------------------------------------------------------------------------------------
		private void CreateCloseButton()
		{
			closeTBMessageControl = new Button();
			closeTBMessageControl.ID = "Message_CloseButton";
			closeTBMessageControl.Text = TBWebFormControlStrings.CloseCaption;
			closeTBMessageControl.Style[HtmlTextWriterStyle.Position] = "absolute";
			closeTBMessageControl.Width = Unit.Pixel(100);
			closeTBMessageControl.Height = Unit.Pixel(25);
			closeTBMessageControl.Style[HtmlTextWriterStyle.Left] = Unit.Pixel((Width - (int)closeTBMessageControl.Width.Value) / 2).ToString();
			closeTBMessageControl.Style[HtmlTextWriterStyle.Top] = Unit.Percentage(91).ToString();
            closeTBMessageControl.Style[HtmlTextWriterStyle.FontFamily] = messageFont;

			InnerControl.Controls.Add(closeTBMessageControl);
		}

		//--------------------------------------------------------------------------------------
		private void CreateTextBox()
		{
			TextBox tb = new TextBox();
			tb.ID = "Message_Text";
			tb.TextMode = TextBoxMode.MultiLine;
			tb.ReadOnly = true;
			tb.Width = Unit.Percentage(80);
			tb.Height = Unit.Percentage(74);
			tb.Style[HtmlTextWriterStyle.Position] = "absolute";
			tb.Style[HtmlTextWriterStyle.Left] = Unit.Percentage(10).ToString();
            tb.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(TitleHeight + 10).ToString();
			//Imposto il colore grigio chiaro in modo che venga visualizzato allo stesso modo da tutti i browser.
			//Ad esempio Safari non visualizza lo sfondo bianco di una textarea readonly 
			tb.Style[HtmlTextWriterStyle.BackgroundColor] = "#EEEEEE";
            tb.Style[HtmlTextWriterStyle.FontFamily] = messageFont;

			foreach (DiagnosticItem item in diagnostic.AllMessages())
				tb.Text += item.FullExplain + Environment.NewLine;

			InnerControl.Controls.Add(tb);
		}

		//--------------------------------------------------------------------------------------
		protected override void AddClosingEvents ()
		{
			// se ho un thread attivo in tbloader con cui colloquiare, lo gestisco tramite Action, (quel che succedera' 
			// poi dipende dall'esito della 
			// altrimenti chiudo semplicemente la finestra del browser
			if (formControl.ThreadAvailable)
			{
				formControl.RegisterControl(closeTBForm, this);
				formControl.RegisterControl(closeTBMessageControl, this);
				//il button esegue il postback in maniera differente dagli altri controlli,e non veniva eseguito
				//l'event handler associato lato server.
				//cosi sul click, simula lato javascript il click della X di chiusura (che e' un ImageButton) 
				closeTBForm.OnClientClick = string.Format("CloseMessageControl('{0}')", closeTBForm.ClientID);
				closeTBMessageControl.OnClientClick = string.Format("CloseMessageControl('{0}')", closeTBMessageControl.ClientID);
			}
			else
			{
				closeTBForm.OnClientClick = "window.close(); return false;";
				closeTBMessageControl.OnClientClick = "window.close(); return false;";
			}
		}

		//--------------------------------------------------------------------------------------
		public void DoClose()
		{
			diagnostic.Clear();
			if (formControl != null)
			{
				formControl.Invalidate();
				formControl.UpdateForms();
			}
		}
		
	}
}
