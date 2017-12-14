using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Woorm.WoormController;
using Microarea.TaskBuilderNet.Woorm.WoormEngine;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
    /// <summary>
    /// Descrizione di riepilogo per WebForm.
    /// </summary>
    /// ================================================================================
    public class AspNetAskWebForm : Control, INamingContainer
	{
		private AskDialog			askDialog;
		private short				tabIndex = 2;	// 1=ok 2= cancel e quindi gli altri partono da 3
		private const float			lengthFactor = 1.8F;
		
		//--------------------------------------------------------------------------------
		private StringCollection	UserChanged { get { return askDialog.UserChanged; }}
		private ValidationSummary	validationSummary = null;

		private WoormWebControl woormWebControl;
		
		//--------------------------------------------------------------------------------
		Report			Report						{ get { return askDialog.Report; }}
		//--------------------------------------------------------------------------------
		TbReportSession	ReportSession				{ get { return askDialog.Report.ReportSession; }}
		//--------------------------------------------------------------------------------
		Enums			Enums						{ get { return ReportSession.Enums; }}
		
		//--------------------------------------------------------------------------
		protected WoormWebControl	WoormWebControl { get { return woormWebControl; }}
		//--------------------------------------------------------------------------------
		protected RSEngine	StateMachine		
		{ 
			get { return WoormWebControl == null ? null : WoormWebControl.StateMachine; }
			
		}
		//--------------------------------------------------------------------------------
		protected AskDialog		AskDialog			{ get { return askDialog; }}

		
		//--------------------------------------------------------------------------
		public AspNetAskWebForm(AskDialog askDialog, WoormWebControl woormWebControl) : base()
		{
			this.askDialog = askDialog;
			this.woormWebControl = woormWebControl;
		}

		//--------------------------------------------------------------------------
		private string HotLinkPrefix(ReferenceObject.Action action)
		{
			return (action == ReferenceObject.Action.Upper)
				? "HotLinkUpper"
				: "HotLinkLower";
		}
		
		//--------------------------------------------------------------------------
		private string HotLinkID(ReferenceObject.Action action, AskEntry askEntry)
		{
			return HotLinkPrefix(action) + askEntry.Field.PublicName;
		}

		// devo riaggiornare tutti perchè altri control possono avere il dato cambiato
		// senza segnalare l'evento di Changed e poi devo reinviare la form attraverso
		// la macchina a stati lasciandola nello stesso stato
		//--------------------------------------------------------------------------
		private void DataChanged(object sender, EventArgs e)
		{
			if (StateMachine == null)
				return;

			Debug.Assert(StateMachine.CurrentState == State.RenderingForm);
			AssignAllAskData(this);

			WoormWebControl.ControlToFocusID = ((WebControl)sender).ClientID;

			StateMachine.Step();
			WoormWebControl.RebuildControls();
		}	

		// chiamo l'hotlink solo se lo trovo e riesco a costruire una QueryString
		//--------------------------------------------------------------------------
		private void ImageHotLinkClick(object sender, ImageClickEventArgs e)
		{
			if (StateMachine == null)
				return;
			
			Debug.Assert(StateMachine.CurrentState == State.RenderingForm);
			AssignAllAskData(this);

			// Non si può usare la Height perchè ritorna sempre zero si approssima sapendo
			// che l'immagine è stata costruita con 20 pixel. Approssimazione lecita
			ImageButton ct = (ImageButton) sender;
			bool upper = (e.Y <= 10); 

			ReferenceObject.Action action =  upper ? ReferenceObject.Action.Upper : ReferenceObject.Action.Lower;
			if	(askDialog.SetActiveAskEntry(ct.ID.Remove(0, HotLinkPrefix(action).Length), action))
			{
				try
				{
					string radarReport = askDialog.ActiveAskEntry.Hotlink.Prototype.RadarReportName;
					if (!String.IsNullOrEmpty(radarReport))
					{
						StateMachine.ActiveAskDialog = askDialog;
						Debug.Assert(false);
						/*TODOPERASSO chiedere a Silvano
						 * 
						string reportParameters = Helper.FormatParametersForRequest(askDialog.ActiveAskEntry.Hotlink.GetParamsOuterXml(action, askDialog.ActiveAskEntry.Field.Data.ToString()));
						WoormWebControl.InitStateMachine(null, false, true, radarReport, reportParameters);
						WoormWebControl.Filename = radarReport;	*/
					}
					// se non riesco a contattare il server TB allora inibisco la gestione dell'hotlink
					else if (askDialog.ActiveAskEntry.Hotlink.BuildQueryString(askDialog.ActiveAskEntry.Field.Data))
						StateMachine.HtmlPage = HtmlPageType.HotLink;
					else
					{
						StateMachine.Errors.Add(WoormWebControlStrings.QueryStringError);
						StateMachine.HtmlPage = HtmlPageType.Error;
					}				
				}
				catch (TbLoaderClientInterfaceException tbException)
				{
					StateMachine.Errors.Add(WoormWebControlStrings.QueryStringError);
					StateMachine.Errors.Add(tbException.Message);
					StateMachine.HtmlPage = HtmlPageType.Error;
				}				

			}

			StateMachine.Step();
			WoormWebControl.RebuildControls();
		}	

		// riaggiorna il valore di tutti i Field della form su conferma della stessa
		//--------------------------------------------------------------------------
		private void SubmitClick(object sender, EventArgs e)
		{
			if (StateMachine == null)
				return;
			
			Debug.Assert(StateMachine.CurrentState == State.RenderingForm);
			AssignAllAskData(this);

			StateMachine.CurrentState = State.ExecuteAsk;
			StateMachine.Step();
			WoormWebControl.RebuildControls();
		}
		
		//--------------------------------------------------------------------------
		private void CancelClick(object sender, EventArgs e)
		{
			if (StateMachine == null)
				return;

			Debug.Assert(StateMachine.CurrentState == State.RenderingForm);

			StateMachine.CurrentState = State.ExecuteUserBreak;
			StateMachine.Step();
			WoormWebControl.RebuildControls();
		}

		//--------------------------------------------------------------------------
		protected override void CreateChildControls()
		{
			//Tabella esterna a tutto
			Table mainTable = new Table();
			mainTable.Width = Unit.Percentage(100);
			Controls.Add(mainTable);
			
			//1 riga
			TableRow tableRow = new TableRow();
			mainTable.Rows.Add (tableRow);
			
			//Cella che contiene il titolo
			TableCell titleCell = new TableCell();
			titleCell.CssClass = "AskDialogTitle";
			titleCell.HorizontalAlign = HorizontalAlign.Center;
			tableRow.Cells.Add (titleCell);
			
			//Label del titolo
			Label titleLabel		= new Label();
			titleLabel.ID			= "TitleLabel";
			titleLabel.CssClass		= "AskDialogLabel";
			titleLabel.Text			= EncodeForHtml(string.Format(WoormWebControlStrings.AskWebFormTitle, askDialog.LocalizedFormTitle));
			titleCell.Controls.Add(titleLabel);
			
			//Aggiungo una riga vuota per distanziare un pò
			TableRow emptyRow = new TableRow();
			mainTable.Rows.Add (emptyRow);
			TableCell emptyCell = new TableCell();
			emptyRow.Cells.Add(emptyCell);
			LiteralControl space = new LiteralControl("&nbsp;");
			emptyCell.Controls.Add(space);
			TableRow parameterTableRow = new TableRow();
			TableCell parameterTableCell = new TableCell();
			mainTable.Rows.Add (parameterTableRow);
			parameterTableRow.Cells.Add(parameterTableCell);
			parameterTableCell.Controls.Add (CreateParametersTableControls());
		}

		//------------------------------------------------------------------------------
		private string EncodeForHtml(string text)	
		{
			return Microarea.TaskBuilderNet.Woorm.WebControls.Helper.AdjustAmpersand(Microarea.TaskBuilderNet.Woorm.WebControls.Helper.InsertBR(text));
		}

		// siccome i RadioButton di uno stesso gruppo lavorano in sintonia devo gestire 
		// il postback per tutti quelli dello stesso gruppo
		//--------------------------------------------------------------------------

		private Control CreateParametersTableControls()
		{

			Table formTable = new Table();
			formTable.ID = "ask$" + askDialog.FormName;
			formTable.CssClass = "AskDialogParameterTable";
			formTable.Attributes["align"] = "center";
			Controls.Add(formTable);
		
			TableRow titleRow = new TableRow();
			titleRow.CssClass = "AskDialogParameterRow";
			formTable.Controls.Add(titleRow);

			TableCell titleCell = new TableCell();
			titleCell.Text = EncodeForHtml(askDialog.LocalizedFormTitle);
			titleCell.CssClass = "AskDialogParameterCell";
			titleCell.Attributes["align"] = "center";
			titleRow.Controls.Add(titleCell);

			int  groupNo = 0;
			int  entryNo = 0;
			foreach (AskGroup askGroup in askDialog.Groups)
			{
				if (askGroup.Hidden) continue;
				
				groupNo++;
				TableRow boxRow = new TableRow();
				TableCell boxCell = new TableCell();
				formTable.Controls.Add(boxRow);
				boxRow.Controls.Add(boxCell);

				Label groupLabel = new Label();
				groupLabel.Text = EncodeForHtml(askGroup.LocalizedCaption);
				groupLabel.CssClass = "AskDialogGroupLabel";

				Panel groupPanel = new Panel();
				groupPanel.CssClass = "AskDialogGroupPanel";
				
				boxCell.Controls.Add(groupLabel);
				boxCell.Controls.Add(groupPanel);

				Table groupTable = new Table();
				groupTable.CellPadding = 0;
				groupTable.CellSpacing = 2;
				groupPanel.Controls.Add(groupTable);

				foreach (AskEntry askEntry in askGroup.Entries)
				{
					if (askEntry.Hidden) continue;

					entryNo++;

					TableRow groupRow = new TableRow();
					groupTable.Controls.Add(groupRow);

					TableCell groupCell = new TableCell();
					groupRow.Controls.Add(groupCell);

					if (askEntry.Field.Data is bool)
					{
						if (askEntry.ControlStyle == AskStyle.CHECK_BOX_BOOL_STYLE)
							CheckBox(askEntry, groupRow, groupCell);
						else if (askEntry.ControlStyle == AskStyle.RADIO_BUTTON_BOOL_STYLE)
							RadioButton(askEntry, groupRow, groupCell, groupNo);
						else
						{
							// allora si tratta di un EDIT control per il tipo boolean. Devo modificare 
							// la lunghezza perchè il comportamento di default chiede "True/False" e non 
							// "Si/No" e la lunghezza invece è 2
							askEntry.Len = 5;
							TextBox(askEntry, groupTable, groupRow, groupCell);
						}
					}
					else if (askEntry.Field.Data is DataEnum)
					{
						DropDown(askEntry, groupTable, groupRow, groupCell);
					}
					else
					{
						// tutti gli altri tipi li edito con un TextBox
						TextBox(askEntry, groupTable, groupRow, groupCell);
					}
				}
			}
	
			FormButtons(formTable);
			AddValidationSummary(formTable);
			return formTable;
		}

		//--------------------------------------------------------------------------
		private void SetPostBackForAllGroupRadio(Control inner, string groupName)
		{
			// itero su tutti i controls della form sino ad ora aggiunti
			foreach (Control ct in inner.Controls)
			{
				if  ((ct is RadioButton))
				{
					RadioButton rb = (RadioButton) ct;

					// evito quelli che sono già postback e non sono del gruppo
					if (string.Compare(groupName, rb.GroupName, true, CultureInfo.InvariantCulture) == 0)
					{
						rb.AutoPostBack = true;
						rb.CheckedChanged += new EventHandler(this.DataChanged);
					}
				}

				if (ct.Controls.Count > 0)
					SetPostBackForAllGroupRadio(ct, groupName);
			}
		}

		// Verifica se altri Radio del gruppo sono autopostback
		//--------------------------------------------------------------------------
		private bool GroupRadioReferenced(Control inner, string groupName)
		{
			// itero su tutti i controls della form sino ad ora aggiunti
			foreach (Control ct in inner.Controls)
			{
				if  ((ct is RadioButton))
				{
					RadioButton rb = (RadioButton) ct;

					if ((string.Compare(groupName, rb.GroupName, true, CultureInfo.InvariantCulture) == 0) && (rb.AutoPostBack))
						return true;
				}

				if (ct.Controls.Count > 0)
					if (GroupRadioReferenced(ct, groupName))
						return true;
			}

			return false;
		}

		// per allineamento a sinistra si intede allineato alla colonna dei controls
		// e quindi devo mettere due celle nella riga (idem per il radio)
		//--------------------------------------------------------------------------
		private void CheckBox(AskEntry askEntry, TableRow groupRow, TableCell groupCell)
		{
			CheckBox checkBox = new CheckBox();
			if (askEntry.LeftAligned)
			{
				groupCell.Controls.Add(checkBox);
			}
			else
			{
				TableCell rightCell = new TableCell();
				rightCell.Controls.Add(checkBox);
				groupRow.Controls.Add(rightCell);

				Label label = new Label();
				groupCell.Controls.Add(label);
			}
		

			checkBox.TabIndex = tabIndex++;
			checkBox.Text = EncodeForHtml(askEntry.LocalizedCaption);
			checkBox.Enabled = askEntry.Enabled;
			checkBox.CssClass = "AskDialogGenericControl";

			if (askEntry.CaptionPos == Token.LEFT)
				checkBox.TextAlign = TextAlign.Left;

			checkBox.ID = askEntry.Field.PublicName;
			checkBox.Checked = ObjectHelper.CastBool(askEntry.Field.AskData);

			if (askDialog.IsReferenced(askEntry))
			{
				checkBox.CheckedChanged += new EventHandler(this.DataChanged);
				checkBox.AutoPostBack = true;
			}
		}
		
		//--------------------------------------------------------------------------
		private void RadioButton(AskEntry askEntry, TableRow groupRow, TableCell groupCell, int groupNo)
		{
			RadioButton radioBtn = new RadioButton();
			if (askEntry.LeftAligned)
			{
				groupCell.Controls.Add(radioBtn);
			}
			else
			{
				TableCell rightCell = new TableCell();
				rightCell.Controls.Add(radioBtn);
				groupRow.Controls.Add(rightCell);

				Label label = new Label();
				groupCell.Controls.Add(label);
			}

			radioBtn.TabIndex = tabIndex++;
			radioBtn.Text = EncodeForHtml(askEntry.LocalizedCaption);
			radioBtn.GroupName = "radio" + groupNo;
			radioBtn.ID = askEntry.Field.PublicName;
			radioBtn.Checked = ObjectHelper.CastBool(askEntry.Field.AskData);
			radioBtn.Enabled = askEntry.Enabled;
			radioBtn.CssClass = "AskDialogGenericControl";

			if (askEntry.CaptionPos == Token.LEFT)
				radioBtn.TextAlign = TextAlign.Left;

			// nel caso dei Radio devo controllare anche che non sia referenziato uno
			// appartenente allo stesso gruppo
			if (askDialog.IsReferenced(askEntry) || GroupRadioReferenced(this, radioBtn.GroupName))
				SetPostBackForAllGroupRadio(this, radioBtn.GroupName);
		}
		
		//--------------------------------------------------------------------------
		private void DropDown(AskEntry askEntry, Table groupTable, TableRow groupRow, TableCell groupCell)
		{
			DataEnum data = (DataEnum) askEntry.Field.Data;
			EnumItems items = Enums.EnumItems(Enums.TagName(data));
			ArrayList list = new ArrayList();

			TableCell groupCell2 = new TableCell();
			Label label = new Label();
			DropDownList dropDownList = new DropDownList();
			
			// carica gli enumerativi nella lista da passare alla combo e determina quale selezionare
			if (items == null)
				list.Add(WoormWebControlStrings.BadEnum);
			else
				for (int i = 0; i < items.Count; i++) 
				{
					EnumItem ei = items[i];
					if (data.Item == ei.Value) dropDownList.SelectedIndex = i;
					list.Add(ei.LocalizedName);
				}

			label.Text = EncodeForHtml(askEntry.LocalizedCaption);
			label.CssClass = "AskDialogGenericControl";

			dropDownList.TabIndex = tabIndex++;
			dropDownList.ID = askEntry.Field.PublicName;
			dropDownList.Enabled = askEntry.Enabled;
			dropDownList.CssClass = "AskDialogGenericControl";

			dropDownList.DataSource = list;
			dropDownList.DataBind();

			if (askEntry.CaptionPos == Token.TOP)
			{
				TableRow groupRow2 = new TableRow();
				groupTable.Controls.Add(groupRow2);
				groupRow2.Controls.Add(groupCell2);
			}
			else
				groupRow.Controls.Add(groupCell2);


			TableCell c1 = groupCell2;
			TableCell c2 = groupCell;

			if (askEntry.CaptionPos == Token.RIGHT)
			{
				c1 = groupCell;
				c2 = groupCell2;
			}

			c2.Controls.Add(label);
			c1.Controls.Add(dropDownList);

			if (askDialog.IsReferenced(askEntry))
			{
				dropDownList.SelectedIndexChanged += new EventHandler(this.DataChanged);
				dropDownList.AutoPostBack = true;
			}
		}
				
		
		//--------------------------------------------------------------------------
		private void TextBox(AskEntry askEntry, Table groupTable, TableRow groupRow, TableCell groupCell)
		{
			TableCell groupCell2 = new TableCell();
			Label label = new Label();
			TextBox textBox = new TextBox();

			label.Text = EncodeForHtml(askEntry.LocalizedCaption);
			label.CssClass = "AskDialogGenericControl";

			// Il vecchio Woorm C++ mette di default 8 come lunghezza delle date perchè considera l'hanno a due cifre
			// qui per chiarezza lo chiediamo con anno a quattro cifre e quindi aggiungiamo 2 alla lunghezza
			textBox.TabIndex = tabIndex++;
			textBox.MaxLength = (askEntry.Field.Data is DateTime) ? askEntry.Len + 2 : askEntry.Len;
			textBox.Columns = (int)(askEntry.Len * lengthFactor);
			textBox.ID = askEntry.Field.PublicName;
			textBox.Text = askEntry.SetUpperLowerLimit();
			textBox.CssClass = "AskDialogGenericControl";
			textBox.Enabled = askEntry.Enabled;

			if (askEntry.CaptionPos == Token.TOP)
			{
				TableRow groupRow2 = new TableRow();
				groupTable.Controls.Add(groupRow2);
				groupRow2.Controls.Add(groupCell2);
			}
			else
				groupRow.Controls.Add(groupCell2);

			TableCell c1 = groupCell2;
			TableCell c2 = groupCell;

			if (askEntry.CaptionPos == Token.RIGHT)
			{
				c1 = groupCell;
				c2 = groupCell2;
			}

			c2.Controls.Add(label);
			AddTextBox(c1, textBox, askEntry);

			if (askDialog.IsReferenced(askEntry))
			{
				textBox.TextChanged += new EventHandler(this.DataChanged);
				textBox.AutoPostBack = true;
			}
		}
		
		//--------------------------------------------------------------------------
		private bool NeedValidation(AskEntry askEntry)
		{
			switch (askEntry.Field.DataType)
			{
				case "DateTime" :
				case "Byte" :
				case "Int16" :
				case "Int32" :
				case "Int64" : 
				case "Single" : 
				case "Double" : 
					return true;
			}
			return false;
		}
		
		//--------------------------------------------------------------------------
		private bool NeedDateTimeValidation(AskEntry askEntry)
		{
			return (askEntry.Field.DataType == "DateTime");
		}
		
		//--------------------------------------------------------------------------
		private ValidationDataType ValidType(AskEntry askEntry)
		{
			switch (askEntry.Field.DataType)
			{
				case "DateTime" :
					return ValidationDataType.Date;

				case "Byte" :
				case "Int16" :
				case "Int32" :
				case "Int64" : 
					return ValidationDataType.Integer;

				case "Single" : 
				case "Double" : 
					return ValidationDataType.Double;
			}
			return ValidationDataType.String;
		}

		//--------------------------------------------------------------------------
		private void AddTextBox(TableCell parent, TextBox textBox, AskEntry askEntry)
		{
			bool addSummary = false;
			Table t = new Table();
			t.BorderStyle = BorderStyle.None;
			t.CellPadding = 0;
			t.CellSpacing = 2;

			TableRow r1 = new TableRow();
			t.Controls.Add(r1);

			TableCell c1 = new TableCell();
			r1.Controls.Add(c1);
			c1.Controls.Add(textBox);

			if (askEntry.Hotlink != null)
			{
				ImageButton HotLink = new ImageButton();
				HotLink.ID = HotLinkID(ReferenceObject.Action.Upper, askEntry);
				HotLink.ToolTip = askEntry.Hotlink.Prototype.Title + " " + WoormWebControlStrings.HotLinkToolTip;
				HotLink.TabIndex = -1; // no tab order
				HotLink.Enabled = askEntry.Enabled;
				//HotLink.ImageUrl = ImagesHelper.CreateImageAndGetUrl(askEntry.Enabled ? ImageNames.HotLink : ImageNames.DisabledHotLink, WoormWebControl.DefaultReferringType); 
				HotLink.Click += new ImageClickEventHandler(this.ImageHotLinkClick);

				TableCell c2 = new TableCell();
				r1.Controls.Add(c2);
				c2.Controls.Add(HotLink);
			}

			if (NeedValidation(askEntry))
			{
				// validazione dei dati inseriti dall'utente
				CompareValidator validator = new CompareValidator();
				validator.Operator = ValidationCompareOperator.DataTypeCheck;
				validator.Type = ValidType(askEntry);
				validator.ControlToValidate = textBox.ID;
				validator.ErrorMessage = string.Format(WoormWebControlStrings.ValidationError, askEntry.Caption);
				validator.Text = "*";
				validator.TabIndex = -1; // no tab order

				TableCell c3 = new TableCell();
				r1.Controls.Add(c3);
				c3.Controls.Add(validator);
				
				addSummary = true;
			}

			if (NeedDateTimeValidation(askEntry))
			{
				// validazione dei dati inseriti dall'utente
				RangeValidator validator = new RangeValidator();
				validator.Type = ValidationDataType.Date;
				validator.ControlToValidate = textBox.ID;
				validator.MaximumValue = ObjectHelper.MaxDateTime.ToShortDateString();
				validator.MinimumValue = ObjectHelper.MinDateTime.ToShortDateString();
				validator.ErrorMessage = string.Format(WoormWebControlStrings.DateTimeError, askEntry.Caption);
				validator.Text = "*";
				validator.TabIndex = -1; // no tab order

				TableCell c4 = new TableCell();
				r1.Controls.Add(c4);
				c4.Controls.Add(validator);
				
				addSummary = true;
			}

			// se ho delle validazioni e il sommario non è ancora stato creato lo aggiunge alla form
			if (addSummary && validationSummary == null) validationSummary = new ValidationSummary();

			parent.Controls.Add(t);
		}		
		
		//--------------------------------------------------------------------------
		private void AddValidationSummary(Table formTable)
		{
			if (validationSummary == null) return;

			TableRow row = new TableRow();
			formTable.Controls.Add(row);

			TableCell cell = new TableCell();
			row.Controls.Add(cell);

			cell.Controls.Add(validationSummary);

			validationSummary.HeaderText = WoormWebControlStrings.SummaryHeader;
			validationSummary.CssClass = "AskDialogValidationSummary";
		}

		//--------------------------------------------------------------------------
		protected virtual void FormButtons(Table formTable)
		{
			TableRow buttonRow = new TableRow();
			formTable.Controls.Add(buttonRow);

			TableCell buttonCell = new TableCell();
			buttonCell.Attributes["align"] = "center";
			buttonRow.Controls.Add(buttonCell);

			Button Submit = new Button();
			Submit.Text = WoormWebControlStrings.Ok;
			Submit.Width = Unit.Pixel(100);
			Submit.Click += new EventHandler(this.SubmitClick);
			buttonCell.Controls.Add(Submit);

			Button Cancel = new Button();
			Cancel.Text = WoormWebControlStrings.Cancel;
			Cancel.Width = Unit.Pixel(100);
			Cancel.Click += new EventHandler(this.CancelClick);
			buttonCell.Controls.Add(Cancel);
		}		

		//--------------------------------------------------------------------------
		private void AssignAskData(string name, string data)
		{
			Field field = GetFieldByName(name);
			if (field != null)
			{
				object current;
				if (field.DataType == "DataEnum")
				{
					DataEnum template = (DataEnum)field.AskData;
					current = Enums.LocalizedParse(data, template, ReportSession.CompanyCulture);
				}
				else
				{
					current = ObjectHelper.Parse(data, field.AskData);
				}

				if (!ObjectHelper.IsEquals(current, field.AskData) && !UserChanged.Contains(field.Name))
					UserChanged.Add(field.Name);
				
				field.AskData = current;
				field.GroupByData = current;
			}
		}
		
		//--------------------------------------------------------------------------
		private Field GetFieldByName(string name)
		{
			foreach (AskGroup g in AskDialog.Groups)
				foreach (AskEntry e in g.Entries)
					if (e.Field != null && e.Field.Name == name)
						return e.Field;
			return null;
		}

		//--------------------------------------------------------------------------
		protected void AssignFromControlData(Control ct)
		{
			if (ct is TextBox)
			{
				TextBox textBox = ct as TextBox;
				AssignAskData(ct.ID, textBox.Text);
				return;
			}

			if (ct is RadioButton)
			{
				RadioButton radioButton = ct as RadioButton;
				AssignAskData(ct.ID, radioButton.Checked ? ObjectHelper.TrueString : ObjectHelper.FalseString);
				return;
			}

			if (ct is CheckBox)
			{
				CheckBox checkBox = ct as CheckBox;
				AssignAskData(ct.ID, checkBox.Checked ? ObjectHelper.TrueString : ObjectHelper.FalseString);
				return;
			}

			// valida per i DataEnum
			if (ct is DropDownList)
			{
				DropDownList dropDownList = ct as DropDownList;
				dropDownList.CssClass = "AskDialogGenericControl";
				string itemName = dropDownList.SelectedItem.Text;

				AssignAskData(ct.ID, itemName);
				return;
			}
		}
		
		//--------------------------------------------------------------------------
		private void RecursiveAssign(Control inner)
		{
			foreach (Control ct in inner.Controls)
			{
				AssignFromControlData(ct);

				if (ct.Controls.Count > 0)
					RecursiveAssign(ct);
			}
		}
		
		//--------------------------------------------------------------------------
		protected void AssignAllAskData(Control inner)
		{
			// prima assegno i valori inseriti dall'utente
			RecursiveAssign(inner);

			// quindi valuto le espressioni di inizializzazione (che modificano solo i campi NON modificati)
			askDialog.EvalAllInitExpression(null, false);
		}

		//--------------------------------------------------------------------------
		private WebControl GetControlByID(Control parent, string ID)
		{
			foreach (WebControl ct in parent.Controls)
			{
				if (string.Compare(ct.ID, ID, true, CultureInfo.InvariantCulture) == 0) return ct;

				WebControl found = GetControlByID(ct, ID);
				if (found != null) return found;
			}

			return null;
		}

		///<summary>
		///Riassegna il fuoco dopo che un controllo ha causato il postback, per evitare che il fuoco 
		///si perda e vada ad esempio  sulla barra degli indirizzi del browser.
		/// </summary>
		//--------------------------------------------------------------------------
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			//inietta lo script per assegnare il fuoco al campo che ha causato il postback
			if (WoormWebControl != null &&
				!string.IsNullOrEmpty(WoormWebControl.ControlToFocusID) && 
				!this.Page.ClientScript.IsStartupScriptRegistered("SetFocus"))
			{
				string scriptValue = string.Format(
				"var elementToFocus = document.getElementById('{0}');"+
				"if (elementToFocus != null) {{"+
				"elementToFocus.focus();" +
				"}}",
				WoormWebControl.ControlToFocusID);
				Page.ClientScript.RegisterStartupScript(Page.ClientScript.GetType(), "SetFocus", scriptValue, true);
			}
		}
	}
}
