using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebCtrLs = System.Web.UI.WebControls;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Woorm.WoormController;
using Microarea.TaskBuilderNet.Woorm.WoormEngine;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;
using Microarea.TaskBuilderNet.Woorm.WoormWebControl;

using TbWebCtrLs = Microarea.TaskBuilderNet.Woorm.TBWebFormControl;

using RSjson;

[assembly: WebResource(AspNetRender.ScriptUrl, "text/javascript")]

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{

    /// <summary>
    /// Controllo che renderizza testi verticali usando SVG
    /// </summary>
    /// ================================================================================
    public class VerticalTextControl: WebControl
	{
		string textValue;
		Rectangle rect;
		int align;
		FontElement font;
		Color textColor;

		//--------------------------------------------------------------------------
		public VerticalTextControl(string textValue, Rectangle rect, int align, FontElement font, Color textColor)
		{
			this.textValue = textValue;
			this.rect = rect;
			this.align = align;
			this.font = font;
			this.textColor = textColor;
		}
	
		//--------------------------------------------------------------------------
		protected override void RenderContents(HtmlTextWriter writer)
		{
			base.RenderContents(writer);
			
			if ((align & (BaseObjConsts.DT_EX_90 | BaseObjConsts.DT_EX_270)) == 0)
					return; //non e' un testo ruotato
			
			if (textValue.Length == 0)
					return;
			
			int angle = (align & BaseObjConsts.DT_EX_90) != 0 ? -90 : -270;
			
			
			writer.WriteLine("<svg height='100%' width='100%' style='background-color:transparent;'>");
			WriteSvgContents(writer, angle);
			writer.WriteLine("</svg>");
		}

		//--------------------------------------------------------------------------
		protected virtual void WriteSvgContents(HtmlTextWriter writer, int angle)
		{
			int xTextPos; 
			int yTextPos;
			string textAnchor; 	
			string textDecoration = "";
			
			if ((FontStyle.Strikeout & font.FontStyle) == FontStyle.Strikeout)
				textDecoration = "text-decoration='line-through' ";
			if ((FontStyle.Underline & font.FontStyle) == FontStyle.Underline)
				textDecoration = "text-decoration='underline' ";
			
			if ((FontStyle.Italic & font.FontStyle) == FontStyle.Italic)
				textDecoration +=  "font-style='italic' ";
			if ((FontStyle.Italic & font.FontStyle) == FontStyle.Italic)
				textDecoration +=  "font-weight='bold' ";	
			 
			textValue = textValue.Replace("\\n", "\r\n");
			bool isMultiline = Helper.IsMultilineString(textValue, align);
			string[] lines = {};
			int numLines = 0;
			int textHeight = font.Size;
			if (isMultiline)
			{
				lines = Helper.SplitMultilineString(textValue);
				numLines = lines.Length;
			}

			if (angle == -270)
			{
				//guardando il primo carattere scritto, rappresenta la x del suo punto in basso a sx, l'underline nell'esempio tra parentesi (_Testo da scrivere)
				xTextPos = 0;
				yTextPos = 0;
				textAnchor = "start";
				
				if((align & BaseObjConsts.DT_LEFT) == BaseObjConsts.DT_LEFT && isMultiline)
				{
					xTextPos =  textHeight * (numLines - 1);
				}

				if ((align & BaseObjConsts.DT_RIGHT) == BaseObjConsts.DT_RIGHT)
					xTextPos = rect.Width - textHeight;
				
				if ((align & BaseObjConsts.DT_CENTER) == BaseObjConsts.DT_CENTER)
				{
					if (isMultiline)
					{
						xTextPos =  (rect.Width + textHeight * (numLines - 1)) / 2; //equivale a rect.Width / 2 + textHeight * numLines / 2 - textHeight / 2;
					}
					else
					{
						xTextPos =  (rect.Width - textHeight) / 2; //equivale a  rect.Width / 2 - textHeight / 2;
					}
				}

				//Allineamento verticale
				if ((align & BaseObjConsts.DT_VCENTER) == BaseObjConsts.DT_VCENTER)
				{
					yTextPos =  rect.Height / 2;
					textAnchor = "middle";
				}
				if ((align & BaseObjConsts.DT_BOTTOM) == BaseObjConsts.DT_BOTTOM)
				{
					yTextPos =  rect.Height;
					textAnchor = "end";
				}
			}
			else
			{
				//guardando il primo carattere scritto, rappresenta la x del suo punto in basso a sx, l'underline nell'esempio tra parentesi (_Testo da scrivere)
				xTextPos = textHeight;
				yTextPos = 0;
				textAnchor = "end";

				if ((align & BaseObjConsts.DT_RIGHT) == BaseObjConsts.DT_RIGHT)
				{
					if (isMultiline)
						xTextPos = rect.Width - textHeight * (numLines - 1);
					else
						xTextPos = rect.Width;
				}
				else if ((align & BaseObjConsts.DT_CENTER) == BaseObjConsts.DT_CENTER)
				{
					if (isMultiline)
						xTextPos = rect.Width / 2 - textHeight * numLines / 2 + textHeight / 2;
					else
						xTextPos =  rect.Width / 2 + textHeight / 2;
				}
				//Allineamento verticale
				if ((align & BaseObjConsts.DT_VCENTER) == BaseObjConsts.DT_VCENTER)
				{
					yTextPos =  rect.Height / 2;
					textAnchor = "middle";
				}
				if ((align & BaseObjConsts.DT_BOTTOM) == BaseObjConsts.DT_BOTTOM)
				{
					yTextPos =  rect.Height;
					textAnchor = "start";
				}
			}
			if (isMultiline)
			{	
				writer.WriteLine(string.Format("<g transform='translate({0},{1})'>", xTextPos, yTextPos));
				writer.WriteLine(string.Format("<g transform='rotate({0})'>", angle));
				for (int i = 0; i < lines.Length; i++)
				{
					writer.WriteLine(string.Format("<text text-anchor='{0}'  {1} font-family='{2}' font-size='{3}' fill='{4}' > {5} </text>", textAnchor, textDecoration, font.FaceName,font.Size, ColorTranslator.ToHtml(textColor), lines[i]));
					writer.WriteLine(string.Format("<g transform='translate({0},{1})'>", 0, textHeight));
				}
			}
			else
			{
				writer.WriteLine(string.Format("<g transform='translate({0},{1})'>", xTextPos, yTextPos));
				writer.WriteLine(string.Format("<text text-anchor='{0}' transform='rotate({1})' {2} font-family='{3}' font-size='{4}' fill='{5}' > {6} </text>", textAnchor, angle, textDecoration, font.FaceName, font.Size, ColorTranslator.ToHtml(textColor), textValue));
			}
			writer.WriteLine("</g>");
		}
	}



	/// <summary>
	/// Descrizione di riepilogo per WoormWebControl.
	/// </summary>
	/// ================================================================================
	public class AspNetRender : Control, INamingContainer
	{
		internal const string ScriptUrl = "Microarea.TaskBuilderNet.Woorm.WoormWebControl.WoormViewerClientControl.js";
		internal const string InitScript = "initScript";
		int zOrder = 100;
		int offset = 3;
		private const int toolbarHeight = 30;
		private WoormDocument woorm;
		private WoormWebControl woormWebControl;
		private bool showToolBar = true;
		private bool useUpdatePanel = true;

		private string reportControllerSessionTag;
		private ConditionalUpdatePanel updPnlViewerControl;
		private ConditionalUpdatePanel updPanelWait;
		private ConditionalUpdatePanel updPageCount;
																 
		//--------------------------------------------------------------------------
		private string nbsp(string s) 
		{ 
			if (s == null || s == string.Empty)
				return @"&nbsp;";
			return s;
		}		

		//--------------------------------------------------------------------------
		public RdeReader		RdeReader		{ get { return woorm.RdeReader; }}
		public WoormWebControl	WoormWebControl { get { return woormWebControl; } }
		public RSEngine		StateMachine	{ get { return WoormWebControl == null ? null : WoormWebControl.StateMachine; }}
		public WoormDocument	Woorm			{ get { return woorm; }	set { woorm = value; }}
		public TbReportSession	Session			{ get { return woorm.ReportSession; }}
		public ReportEngine		ReportEngine	{ get { return StateMachine.Report.Engine; }}

		public bool				ShowToolBar				{ get { return showToolBar; } set { showToolBar = value; } }
		public bool				UseUpdatePanel			{ get { return useUpdatePanel; } set { useUpdatePanel = value; } }

		public bool ShowSaveForUserButton { get { return StateMachine.Woorm.CanSaveForUser || StateMachine.Woorm.CanSaveForAllUsers; } }

		//tutte le pagine sono pronte se sono in esecuzione snapshot (RSEngine.Report == null) o se l'rdeWriter ha finito di
		//scrivere in esecuzione (booleano  RdeReader.AllPagesReady)
		public bool AllPagesReady { get { return StateMachine == null || StateMachine.Report == null || RdeReader.AllPagesReady; } }

		//------------------------------------------------------------------------------
		public AspNetRender(WoormDocument woorm, string stateMachineSessionTag, WoormWebControl woormWebControl)
		{
			this.woorm = woorm;
			this.woormWebControl = woormWebControl;
			this.reportControllerSessionTag = stateMachineSessionTag;
		}

		//------------------------------------------------------------------------------
		private string ReportNameOnly
		{
			get
			{
				if (Session.UserInfo != null)
				{
					INameSpace ns = Session.UserInfo.PathFinder.GetNamespaceFromPath(woorm.Filename);
					return ns.Report.Replace(NameSolverStrings.WrmExtension, "");
				}
				return "";
			}
		}

	
		//------------------------------------------------------------------------------
		private string px(int size) 
		{
			return size.ToString("0px");
		}

		//--------------------------------------------------------------------------
		protected void LoadPage(PageType page)	
		{ 
			Woorm.LoadPage(page); 

			// forza la ricreazione dei control per ricostruirli con i dati appena caricati
			// altrimenti mostrerebbero quelli presenti alla prima creazione(postback)
			ChildControlsCreated = false;
			this.EnsureChildControls();
			//aggiorna l'intero controllo
			updPnlViewerControl.Update();
		}

		//--------------------------------------------------------------------------
		protected void LoadPage(int pageNumber)
		{
			Woorm.LoadPage(pageNumber);

			// forza la ricreazione dei control per ricostruirli con i dati appena caricati
			// altrimenti mostrerebbero quelli presenti alla prima creazione(postback)
			ChildControlsCreated = false;
			this.EnsureChildControls();
			//aggiorna l'intero controllo
			updPnlViewerControl.Update();
		}

		#region toolbar generica
		//--------------------------------------------------------------------------
		protected void FirstClick	(object sender, ImageClickEventArgs e)	
		{
			LoadPage(PageType.First); 
		}

		//--------------------------------------------------------------------------
		protected void LastClick	(object sender, ImageClickEventArgs e)	
		{ 
			if (AllPagesReady) 
				LoadPage(PageType.Last); 
		}

		//--------------------------------------------------------------------------
		protected void PrevClick	(object sender, ImageClickEventArgs e)	
		{ 
			LoadPage(PageType.Prev); 
		}

		//--------------------------------------------------------------------------
		protected void NextClick	(object sender, ImageClickEventArgs e)	
		{ 
			if (RdeReader.IsPageReady()) 
				LoadPage(PageType.Next); 
		}

		//--------------------------------------------------------------------------
		protected void StopClick(object sender, ImageClickEventArgs e)
		{
			if (!AllPagesReady)
			{
				ReportEngine.StopEngine(WoormViewerStrings.ExecutionStopped);
				woorm.ReportSession.StoppedByUser = true;
				StateMachine.ExtractionThread.Join();
				StateMachine.CurrentState = State.UserInterrupted;
				StateMachine.Step();
				// forza la ricreazione dei control per ricostruirli con i dati appena caricati
				// altrimenti mostrerebbero quelli presenti alla prima creazione(postback)
				ChildControlsCreated = false;
				this.EnsureChildControls();
			}
		}

		//--------------------------------------------------------------------------
		protected void SaveForUserClick(object sender, ImageClickEventArgs e)
		{ 
			if (StateMachine != null)
			{
				StateMachine.CurrentState = State.RunPersister;
				StateMachine.Step();
				WoormWebControl.RebuildControls();
				WoormWebControl.MainUpdatePnl.Update();
			}
		}
		
		//--------------------------------------------------------------------------
		protected void ButtonToolbar(WebCtrLs.TableRow row,string imageName,string text,ImageClickEventHandler anEvent)
		{
			ButtonToolbar(row, imageName, text, anEvent, "window.close();");
		}
		//--------------------------------------------------------------------------
		protected void ButtonToolbar(WebCtrLs.TableRow row,string imageName,string text,ImageClickEventHandler anEvent,string javaScriptEvent)
		{
			WebCtrLs.TableCell firstCell = new WebCtrLs.TableCell();
			ImageButton btn = new ImageButton();
			
			firstCell.EnableViewState = false;

			btn.ID = "Button" + imageName;
			btn.ToolTip = text;
			btn.TabIndex = -1; // no tab order
			//btn.ImageUrl = ImagesHelper.CreateImageAndGetUrl(imageName, WoormWebControl.DefaultReferringType);

			btn.AlternateText= imageName;

			// se non passo l'evento allora inserisco javascript per chiudere il browser
			if (anEvent != null) btn.Click += anEvent; else 	btn.Attributes.Add("onclick", javaScriptEvent);

			row.Controls.Add(firstCell);
			firstCell.Controls.Add(btn);
		}
		//--------------------------------------------------------------------------
		protected void Toolbar(WebCtrLs.Panel outerPanel)
		{
			WebCtrLs.Panel panel = new WebCtrLs.Panel();
			panel.EnableViewState = false;
			outerPanel.Controls.Add(panel);
				
			WebCtrLs.Table table = new System.Web.UI.WebControls.Table();
			table.EnableViewState = false;
			table.CellSpacing = table.CellPadding = 0;
			table.BorderStyle = BorderStyle.None;
			panel.Controls.Add(table);
			
			WebCtrLs.TableRow row = new WebCtrLs.TableRow();
			table.Controls.Add(row);
			row.EnableViewState = false;

			row.Height = Unit.Pixel(toolbarHeight);
			row.CssClass = "ReportToolbar";

			WebCtrLs.TableCell cell = new WebCtrLs.TableCell();
			row.Controls.Add(cell);
			cell.EnableViewState = false;

			// la toolbar vera e propria è un'altra tabella le cui singole celle sono i bottoni
			WebCtrLs.Table table2 = new WebCtrLs.Table();
			cell.Controls.Add(table2);
			table2.EnableViewState = false;

			table2.Height = Unit.Percentage(100);
			table2.BorderWidth = Unit.Pixel(0);
			table2.CellSpacing = 0;
			table2.CellPadding = 0;

			WebCtrLs.TableRow row2 = new WebCtrLs.TableRow();
			table2.Controls.Add(row2);
			row2.EnableViewState = false;

			ButtonToolbar(row2, ImageNames.PrevPage, WoormWebControlStrings.Previous, new ImageClickEventHandler(this.PrevClick));
			ButtonToolbar(row2, ImageNames.NextPage, WoormWebControlStrings.Next, new ImageClickEventHandler(this.NextClick));
			ButtonToolbar(row2, ImageNames.FirstPage, WoormWebControlStrings.First, new ImageClickEventHandler(this.FirstClick));
			ButtonToolbar(row2, ImageNames.LastPage, WoormWebControlStrings.Last,  AllPagesReady ? new ImageClickEventHandler(this.LastClick) : null, GetWaitScript());

			if (AllPagesReady || woorm.ReportSession.StoppedByUser)
				ButtonToolbar(row2,ImageNames.Run,WoormWebControlStrings.Run,null,string.Format("parent.location.href = '{0}';",Page.Request.Url.PathAndQuery));
			else
				RunExecButtonToolbar(row2);
			
			
			if (ShowSaveForUserButton)
				ButtonToolbar(row2,ImageNames.SaveForUser,WoormWebControlStrings.SaveForCurrentUser, AllPagesReady ? new ImageClickEventHandler(this.SaveForUserClick) : null, GetWaitScript());
                
            //----
			PDFPrintToolbarButton(row2);
            XLSPrintToolbarButton(row2);

            //---- Print
			string path = CalculateRelativeUrl(Page);
				
			string jsRadarEvent = AllPagesReady 
								?	string.Format(
								"javascript:linkDocument('{0}?{1}&Print={2}', '', 'true'); return false;",
								path,
								this.Page.Request.QueryString,
								this.reportControllerSessionTag)
								:	GetWaitScript();
				
				
			ButtonToolbar(row2,
				ImageNames.PrintHtml,
				WoormWebControlStrings.PrintHtml,
				null,
				AllPagesReady ? jsRadarEvent : GetWaitScript()
				);
            //----
			
			ButtonToolbar(row2, ImageNames.Exit, WoormWebControlStrings.Exit, null);

			Spacer(row2) ;
			
			ReportStatusInfos(row2) ;
		}

		//--------------------------------------------------------------------------
		private void RunExecButtonToolbar(TableRow row)
		{
			WebCtrLs.TableCell firstCell = new WebCtrLs.TableCell();
			ImageButton btn = new ImageButton();

			firstCell.EnableViewState = false;

			btn.ID = "Button" + ImageNames.RunExec;
			btn.ToolTip = WoormWebControlStrings.Stop;
			btn.TabIndex = -1; // no tab order
			//btn.ImageUrl = ImagesHelper.CreateImageAndGetUrl(ImageNames.RunExec,WoormWebControl.DefaultReferringType);

			btn.AlternateText= ImageNames.RunExec;

			btn.Click += new ImageClickEventHandler(this.StopClick); 
			btn.Attributes.Add("onclick", "stopPing();");

			row.Controls.Add(firstCell);
			firstCell.Controls.Add(btn);
		}

		///<summary>
		///Ritorna lo script js che fa l'alert del messaggio localizzato di attesa 
		///</summary>
		//------------------------------------------------------------------------------
		private string GetWaitScript()
		{
			return string.Format("alert('{0}')", WoormWebControlStrings.WaitingRunEngineMessage);
		}

		///<summary>
		///Aggiunge il bottone per la stampa Pdf
		///</summary>
		//------------------------------------------------------------------------------
		private void PDFPrintToolbarButton(WebCtrLs.TableRow row)
		{
             PrintToolbarButton(row, ImageNames.PrintPdf, WoormWebControlStrings.PrintPdf, "TBPdfResource.axd");
		}

        ///<summary>
        ///Aggiunge il bottone per la stampa Xls
        ///</summary>
        //------------------------------------------------------------------------------
        private void XLSPrintToolbarButton(WebCtrLs.TableRow row)
        {
            PrintToolbarButton(row, ImageNames.PrintXls, WoormWebControlStrings.PrintXls, "TBXlsResource.axd");
        }

        ///<summary>
        ///Aggiunge un bottone per una stampa 
        ///</summary>
        //------------------------------------------------------------------------------
        private void PrintToolbarButton(WebCtrLs.TableRow row, string btnImageName, string btnToolTip, string provider)
        {
            WebCtrLs.TableCell firstCell = new WebCtrLs.TableCell();
            firstCell.EnableViewState = false;

            HyperLink btn = new HyperLink();

            btn.ID = "Button" + btnImageName;
            btn.ToolTip = btnToolTip;
            btn.TabIndex = -1; // no tab order
            //btn.ImageUrl = ImagesHelper.CreateImageAndGetUrl(btnImageName, WoormWebControl.DefaultReferringType);

            if (AllPagesReady)
            {
                btn.NavigateUrl = "~/"+ provider +"?SessionTag=" + this.reportControllerSessionTag;
                btn.Target = "_blank";
            }
            else
            {
                btn.Attributes.Add("onclick", GetWaitScript());
            }

            btn.Text = btnImageName;

            row.Controls.Add(firstCell);
            firstCell.Controls.Add(btn);
        }	

		#endregion generica

		#region toolbar ReportStatusInfo
		//------------------------------------------------------------------------------
		public void Spacer(TableRow row)
		{
			WebCtrLs.TableCell spacerCell = new WebCtrLs.TableCell();
			spacerCell.EnableViewState = false;
			spacerCell.Width = Unit.Pixel(150);
			spacerCell.Text	= "&nbsp";

			row.Controls.Add(spacerCell);
		}

		//------------------------------------------------------------------------------
		public void WriteReportStatusInfos(TableCell control, string text)
		{
			WriteStyleReportStatusInfos(control);
			control.Text						= text;
		}

		//------------------------------------------------------------------------------
		public void WriteStyleReportStatusInfos(WebControl control)
		{
			control.EnableViewState = false;
			control.CssClass = "StatusInfo";
			if (control.Enabled)
			{
				control.CssClass += " Enabled";
			}
		}

		//------------------------------------------------------------------------------
		public string PositionInfo 
		{ 
			get
			{
				if (this.woorm is RDEWoormDocument) return "Snapshot";

				return Session.UserInfo.PathFinder.IsCustomPath(woorm.Filename) 
					? NameSolverStrings.Custom 
					: NameSolverStrings.Standard;
			}
		}

		// Nome report, Custom/Standard, Page counters
		//------------------------------------------------------------------------------
		public void ReportStatusInfos(TableRow row) 
		{ 
			WebCtrLs.Table infoTable = new WebCtrLs.Table();
			WebCtrLs.TableRow infoRow = new WebCtrLs.TableRow();

			infoTable.BorderWidth = Unit.Pixel(2);
			infoTable.CellSpacing = 0;
			infoTable.CellPadding = 3;

			WebCtrLs.TableCell dataCell = new WebCtrLs.TableCell();
			WebCtrLs.TableCell reportNameCell = new WebCtrLs.TableCell();
			WebCtrLs.TableCell whereCell = new WebCtrLs.TableCell();
			WebCtrLs.TableCell userCell = new WebCtrLs.TableCell();
			WebCtrLs.TableCell pageCountersCell = new WebCtrLs.TableCell();

			WriteReportStatusInfos(dataCell,Session.UserInfo.ApplicationDate.ToShortDateString());
			WriteReportStatusInfos(reportNameCell,ReportNameOnly);
			WriteReportStatusInfos(whereCell,PositionInfo);
			WriteReportStatusInfos(userCell,Session.UserInfo.LoginManager.UserName);
			WriteStyleReportStatusInfos(pageCountersCell);
			//Numero pagine
			updPageCount = new ConditionalUpdatePanel(UseUpdatePanel);
			updPageCount.UpdateMode = UpdatePanelUpdateMode.Always;
			pageCountersCell.Controls.Add(updPageCount);
			WebCtrLs.Label label = new WebCtrLs.Label();
			DropDownList ddlPageNumber = new DropDownList();
			ddlPageNumber.Font.Bold = label.Font.Bold							= true;
			ddlPageNumber.Style["Font-Family"] = label.Style["Font-Family"]		= "Verdana";
			ddlPageNumber.Style["Font-Size"] = label.Style["Font-Size"]			= px(14);
			//abilito il controllo per selezionare le pagine solo quando sono tutte disponibili
			ddlPageNumber.Enabled = AllPagesReady;
			label.Text						= string.Format("/{0}", AllPagesReady ? RdeReader.TotalPages.ToString() : "*");

			int maxPage = AllPagesReady ? RdeReader.TotalPages : RdeReader.CurrentPage;
			for (int i = 1; i <= maxPage; i++)
			{
				ddlPageNumber.Items.Add(i.ToString());
			}

			ddlPageNumber.SelectedIndex = RdeReader.CurrentPage - 1; //0-based
			ddlPageNumber.AutoPostBack = true;
			ddlPageNumber.SelectedIndexChanged += new EventHandler(ddlPageNumber_SelectedIndexChanged);
			
			updPageCount.ContentTemplateContainer.Controls.Add(ddlPageNumber);
			
			updPageCount.ContentTemplateContainer.Controls.Add(label);

			infoTable.Controls.Add(infoRow);
			infoRow.Controls.Add(dataCell);
			infoRow.Controls.Add(reportNameCell);
			infoRow.Controls.Add(whereCell);
			infoRow.Controls.Add(userCell);
			infoRow.Controls.Add(pageCountersCell);

			infoRow.EnableViewState = false;
			infoTable.EnableViewState = false;

			// la aggiungo alla tabella della toolbar
			WebCtrLs.TableCell infoCell = new WebCtrLs.TableCell();
			infoCell.Controls.Add(infoTable);
			row.Controls.Add(infoCell);
		}


		//------------------------------------------------------------------------------
		void ddlPageNumber_SelectedIndexChanged(object sender,EventArgs e)
		{
			DropDownList ddl = (DropDownList)sender;
			if (AllPagesReady)
			{
				try{
					int pageNo = int.Parse(ddl.SelectedValue);
					LoadPage(pageNo);
				}
				catch(Exception)
				{
				}
			}
		}
		#endregion

		


		//------------------------------------------------------------------------------
		private int FontSize(string fontStyleName)
		{
			FontElement fe = woorm.GetFontElement(fontStyleName);
			return (fe == null) ? 0 : fe.Size; 
		}

		//------------------------------------------------------------------------------
		public void AdjustOverloadingLabel(Rectangle inflated, FieldRect obj)
		{
			if	(
					(obj.Value.Align & (BaseObjConsts.DT_VCENTER | BaseObjConsts.DT_EX_VCENTER_LABEL)) == (BaseObjConsts.DT_VCENTER | BaseObjConsts.DT_EX_VCENTER_LABEL) &&
					obj.Label.Text != null && obj.Label.Text.Length > 0
				)
				inflated = InflateInternalRect(inflated, obj.Label, obj.Value);
		}

		//------------------------------------------------------------------------------
		public Rectangle InflateInternalRect(Rectangle rect, RSjson.Label labelObj, RSjson.WoormValue valueObj)
		{
			int labelFontSize = FontSize(labelObj.FontStyleName);
			int valueFontSize = FontSize(valueObj.FontStyleName);

			int height = rect.Height;
			int y = rect.Y; 

			if (rect.Height - labelFontSize < valueFontSize)
			{
				if (rect.Height > valueFontSize + offset)
					height = valueFontSize + 3;
				y = rect.Y + (rect.Height - height); 
			}
			else
			{
				height = rect.Height - labelFontSize;
				y = rect.Y + labelFontSize; 
			}

			Rectangle inflated = new Rectangle(rect.X, y, rect.Width, height);
			return inflated;
		}

		

		//------------------------------------------------------------------------------
		private string InsertBR(string text)	
		{
			text = Microarea.TaskBuilderNet.Woorm.WebControls.Helper.InsertBR(text);
			// Devo rimpiazzare anche i \\n, operche il parser del lexan parsa i \n restituendo un doppio \\
			// Vedere il metodo public bool Parse (WoormParser lex) dove parsa il titolo	
			// (if (lex.LookAhead(Token.TEXTSTRING) && !lex.ParseString(out title)) ) di woorm.woormviewer.column
			return text.Replace("\\n","<br/>");
		}

		
		//------------------------------------------------------------------------------
		public void WriteTextFile(WebCtrLs.Label control, string textFilename)	
		{
			control.Text = woorm.ReadTextFile(textFilename);
		}

		//------------------------------------------------------------------------------
		public void WriteImage(Panel panel, FieldRect obj) 
		{
			string sourceFilePath = woorm.GetFilename(obj.Value.FormattedData,NameSpaceObjectType.Image);
			string ext = System.IO.Path.GetExtension(sourceFilePath);
			FileProvider fp = new FileProvider(woorm,ext);
			string destPath = fp.GenericTmpFile;	//nome di file temporaneo sotto la web folder esposta

			if (!File.Exists(sourceFilePath))
				return;

			//copio il file sotto webfolder esposta (necessario per problemi di permessi)
			File.Copy(sourceFilePath,destPath,true);

			Panel fieldRectPanel = new Panel();
			panel.Controls.Add(fieldRectPanel);

			WebCtrLs.Image image = new WebCtrLs.Image();
			
			//gestione allineamento
			Panel verticalCenteringDiv = new Panel();
			verticalCenteringDiv.Controls.Add(image);
			fieldRectPanel.Controls.Add(verticalCenteringDiv);
			fieldRectPanel.Style.Add(HtmlTextWriterStyle.Display,"table");
			verticalCenteringDiv.Style.Add(HtmlTextWriterStyle.Display,"table-cell");

			if ((obj.Value.Align & BaseObjConsts.DT_RIGHT) == BaseObjConsts.DT_RIGHT)
				fieldRectPanel.Style.Add(HtmlTextWriterStyle.TextAlign,"right");
			else if ((obj.Value.Align & BaseObjConsts.DT_CENTER) == BaseObjConsts.DT_CENTER)
				fieldRectPanel.Style.Add(HtmlTextWriterStyle.TextAlign,"center");
			else if ((obj.Value.Align & BaseObjConsts.DT_LEFT) == BaseObjConsts.DT_LEFT)
				fieldRectPanel.Style.Add(HtmlTextWriterStyle.TextAlign,"left");

			if ((obj.Value.Align & BaseObjConsts.DT_VCENTER) == BaseObjConsts.DT_VCENTER)
				verticalCenteringDiv.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");
			else if ((obj.Value.Align & BaseObjConsts.DT_BOTTOM) == BaseObjConsts.DT_BOTTOM)
				verticalCenteringDiv.Style.Add(HtmlTextWriterStyle.VerticalAlign,"bottom");
			else if ((obj.Value.Align & BaseObjConsts.DT_EX_VCENTER_LABEL) == 0)
				verticalCenteringDiv.Style.Add(HtmlTextWriterStyle.VerticalAlign,"top");

			// costruisce una rettagolo inflatando dei bordi
			Rectangle inflated = Helper.Inflate(obj.BaseRectangle, obj.Borders, obj.BorderPen);

			fieldRectPanel.EnableViewState = false;
			fieldRectPanel.BackColor = Color.Transparent;
			fieldRectPanel.Height	= Unit.Pixel(inflated.Size.Height);
			fieldRectPanel.Width		= Unit.Pixel(inflated.Size.Width);
	
			image.ImageUrl	= "~\\" + fp.GenericTmpFileRelPath;
			if (obj.ShowProportional)
			{
				Unit width,height;
				if (GetProportionalImageSize(Unit.Pixel(inflated.Size.Width),Unit.Pixel(inflated.Size.Height),destPath,out width,out height))
				{
					image.Width = width;
					image.Height = height;
				}
			}
			else
			{
				image.Height	= Unit.Pixel(obj.BaseRectangle.Size.Height);
				image.Width		= Unit.Pixel(obj.BaseRectangle.Size.Width);
			}

			WriteStyleZOrder		(fieldRectPanel);
			WriteStyleOrigin		(fieldRectPanel, inflated.Location);
			WriteStyleBorders		(fieldRectPanel, obj.Borders ,obj.BorderPen);
			WriteStyleAbsoluteHidden(fieldRectPanel);
		}


		#region Style settings
		//------------------------------------------------------------------------------
		private void WriteStyleOrigin(WebControl control, Point origin) 
		{ 
			control.Style["Top"]		= px(origin.Y);
			control.Style["Left"]		= px(origin.X);
		}

		//------------------------------------------------------------------------------
		public void WriteStyleScroll(WebControl control)	
		{ 
			control.Style["Overflow"] = "auto";
		}

		//------------------------------------------------------------------------------
		public void WriteStyleColor(WebControl control, Color fore, Color bkgn)	
		{ 
			control.BackColor = bkgn;
			control.ForeColor = fore;
		}

		//------------------------------------------------------------------------------
		public void WriteStyleZOrder(WebControl control)	
		{ 
			control.Style["Z-Index"] = zOrder.ToString();
			zOrder++;
		}

		/// <summary>
		/// Scrive il font corretto andandolo a cercare attraverso il suo nome, prima
		/// nella tabella dei fonts del documento e se non lo trova lo cerca nella tabella
		/// dei fonts della applicazione che essendo statica o esiste già o viene creata
		/// in questo momento.
		/// Nota: ignora l'attributo colore perchè usa il foregroud del rettangolo di appartenenza
		/// </summary>
		/// <param name="fontStyleName"></param>
		//------------------------------------------------------------------------------
		private void WriteStyleFont(WebControl control, string fontStyleName) 
		{
			FontElement fe = woorm.GetFontElement(fontStyleName);
			if (fe == null) return;

			control.Font.Italic		= (FontStyle.Italic & fe.FontStyle) == FontStyle.Italic;
			control.Font.Bold		= (FontStyle.Bold & fe.FontStyle) == FontStyle.Bold;
			control.Font.Strikeout	= (FontStyle.Strikeout & fe.FontStyle) == FontStyle.Strikeout;
			control.Font.Underline	= (FontStyle.Underline & fe.FontStyle) == FontStyle.Underline;

			control.Style["Font-Family"]	= fe.FaceName; 
			control.Style["Font-Size"]		= px(fe.Size); 
		}


		///<summary>
		///Metodo che scrive i bordi del webControl tenedo conto del Pen
		///</summary>
		//------------------------------------------------------------------------------
		public void WriteStyleBorders(WebControl control, Borders borders, BorderPen pen)	
		{	
			string solid = "solid";
			string none = "none";
	
			control.Style["Border-Top-Style"]		= borders.Top ? solid : none;
			control.Style["Border-Right-Style"]		= borders.Right ? solid : none;
			control.Style["Border-Left-Style"]		= borders.Left ? solid : none;
			control.Style["Border-Bottom-Style"]	= borders.Bottom ? solid : none;

			control.BorderWidth = Unit.Pixel(pen.Width);
			control.BorderColor = pen.Color;
		}

		///<summary>
		///Metodo che scrive i bordi del webControl tenedo conto dei Pen che vengono usati per disegnarli (un pen per i bordi top-left-right, 
		///e uno per il bordo inferiore: usato perche il separatore di riga(che coincide con il bordo inferiore della cella) puo essere diverso 
		///dagli altri bordi)
		/// </summary>
		//------------------------------------------------------------------------------
		public void WriteStyleBorders(WebControl control,Borders borders,BorderPen pen, BorderPen rowSepPen)
		{
			WriteStyleBorders(control, borders, pen);

			//personalizzo il bordo inferiore usando il Pen del RowSeparator
			control.Style["Border-Bottom-Width"]	=	Unit.Pixel(rowSepPen.Width).ToString();
			control.Style["Border-Bottom-Color"]	=	ColorTranslator.ToHtml(rowSepPen.Color);
		}


		//------------------------------------------------------------------------------
		public void WriteStyleAbsolute(WebControl control)	
		{
			control.Style[HtmlTextWriterStyle.Position] = "absolute";
		}
	

		//------------------------------------------------------------------------------
		public void WriteStyleHidden(WebControl control)	
		{
			control.Style["Overflow"] = "hidden";
		}
		
		//------------------------------------------------------------------------------
		public void WriteStyleBorderWidth0(WebControl control)	
		{
			control.Style["border-width"] = "0px";
		}

		//------------------------------------------------------------------------------
		private void WriteStylePosition(WebControl control, Rectangle rect) 
		{ 
			control.Style["Top"]	= px(rect.Top);
			control.Style["Left"]	= px(rect.Left);
			control.Style["Width"]	= px(rect.Width);
			control.Style["Height"] = px(rect.Height);
		}

		//------------------------------------------------------------------------------
		private void WriteStyleTextAlign(WebControl control, int alignEx) 
		{
			string s = "";

			if ((alignEx & BaseObjConsts.DT_RIGHT) == BaseObjConsts.DT_RIGHT)		
                s = "right";
			else if ((alignEx & BaseObjConsts.DT_CENTER) == BaseObjConsts.DT_CENTER)	
                s = "center";

			if (s.Length > 0) 
                control.Style["Text-Align"] = s;
		}

        //------------------------------------------------------------------------------
        private void WriteAlign(TableCell cell, int alignEx)
        {
            cell.VerticalAlign = V_Align(alignEx);
            cell.HorizontalAlign = H_Align(alignEx);
        }

		//------------------------------------------------------------------------------
		private HorizontalAlign H_Align(int alignEx) 
		{ 
			HorizontalAlign s =  HorizontalAlign.Left;

			if ((alignEx & BaseObjConsts.DT_RIGHT) == BaseObjConsts.DT_RIGHT)		
                s = HorizontalAlign.Right;
			else if ((alignEx & BaseObjConsts.DT_CENTER) == BaseObjConsts.DT_CENTER)	
                s = HorizontalAlign.Center;

			return s;
		}

		//------------------------------------------------------------------------------
		private VerticalAlign V_Align(int alignEx) 
		{
            VerticalAlign s = VerticalAlign.Top;

			if ((alignEx & BaseObjConsts.DT_VCENTER) == BaseObjConsts.DT_VCENTER)	
                s = VerticalAlign.Middle;
			else if ((alignEx & BaseObjConsts.DT_BOTTOM) == BaseObjConsts.DT_BOTTOM)	
                s = VerticalAlign.Bottom;

			return s;
		}

		//------------------------------------------------------------------------------
		public void WriteStyleAbsoluteHidden(WebControl control)	
		{ 
			control.Style["Position"]	= "absolute";
			control.Style["Overflow"]	= "hidden";
		}
		
        //------------------------------------------------------------------------------
		public void WriteStyleNoWrap(WebControl control)	
		{ 
			control.Style["white-space"]= "nowrap";
		}

		//------------------------------------------------------------------------------
		public void WriteStyleClip(WebControl control, Rectangle rect)	
		{ 
			control.Style["Clip"] = string.Format("rect(0px,{0}px,{1}px,0px)", rect.Width, rect.Height);
		}

		#endregion



		#region Oggetti singoli
		//------------------------------------------------------------------------------
		public void UnknowHtml(Panel panel) 
		{ 
			WebCtrLs.Label label = new WebCtrLs.Label();
			panel.Controls.Add(label);

			label.EnableViewState = false;
			label.BackColor = Color.Yellow;
			label.ForeColor = Color.Red;
			label.Text = WoormWebControlStrings.UnknownObject;
			WriteStyleZOrder(label);
		}

		//------------------------------------------------------------------------------
		public void SqrRectHtml(Panel panel, SqrRect obj) 
		{
			// valuata dal motore del viewer durante il parse.
			if (obj.IsHidden) return;

			//disegno eventuale ombra
			WriteDropShadow(panel, obj.BaseRectangle, obj.DropShadowHeight, obj.DropShadowColor);

			WebCtrLs.Label label = new WebCtrLs.Label();
			panel.Controls.Add(label);

			label.EnableViewState = false;
			label.BackColor = obj.BkgColor;
            label.ToolTip = obj.GetTooltip;

			WriteStyleZOrder		(label);
			WriteStylePosition(label, obj.BaseRectangle.InflateForPosition(obj.Borders, obj.BorderPen));
			WriteStyleBorders		(label, obj.Borders, obj.BorderPen);
			WriteStyleAbsoluteHidden(label);
		}

		//------------------------------------------------------------------------------
		public void TextRectHtml(Panel panel, TextRect obj) 
		{ 
			// valuata dal motore del viewer durante il parse.
			if (obj.IsHidden) return;

			//disegno eventuale ombra
			WriteDropShadow(panel, obj.BaseRectangle, obj.DropShadowHeight, obj.DropShadowColor);

			WebControl webCtrl;
			Rectangle rect = obj.BaseRectangle.InflateForPosition(obj.Borders,obj.BorderPen); //rimpicciolisco il rettangolo per fare spazio ai bordi

			//renderizzazione field rect con testo ruotato
			if ((obj.Label.Align & (BaseObjConsts.DT_EX_90 | BaseObjConsts.DT_EX_270)) != 0)
            {
				Panel fieldPanel = new Panel();
				WriteStyleZOrder			(fieldPanel);
				WriteStyleOrigin			(fieldPanel, rect.Location);
				WriteStylePosition          (fieldPanel, obj.BaseRectangle.InflateForPosition(obj.Borders, obj.BorderPen));
				WriteStyleBorders			(fieldPanel, obj.Borders, obj.BorderPen);
				WriteStyleAbsoluteHidden	(fieldPanel);
				WriteStyleColor				(fieldPanel, woorm.TrueColor(BoxType.Text, obj.TextColor, obj.Label.FontStyleName), obj.BkgColor);
				FontElement fe = woorm.GetFontElement(obj.Label.FontStyleName);
		
				VerticalTextControl vtc = new VerticalTextControl(obj.Label.Text, rect, obj.Label.Align, fe,  woorm.TrueColor(BoxType.Text, obj.TextColor, obj.Label.FontStyleName));
				WriteStyleOrigin(vtc, rect.Location);
				fieldPanel.Controls.Add(vtc);
				panel.Controls.Add(fieldPanel);

				return;
			}

			if ((obj.Label.Align & BaseObjConsts.DT_EX_FIELD_SET) != 0)
			{
				TbWebCtrLs.TBFieldSet label = new TbWebCtrLs.TBFieldSet();
				label.Caption = InsertBR(obj.LocalizedText);

				if ((obj.Label.Align & BaseObjConsts.DT_CENTER) != 0)
					label.Align = TbWebCtrLs.TBFieldSet.AlignStyle.Center;
				else if ((obj.Label.Align & BaseObjConsts.DT_RIGHT) != 0)
					label.Align = TbWebCtrLs.TBFieldSet.AlignStyle.Right;

				webCtrl = label;
			}
			else
			{
				//Costruisco una tabella trasparente per gestire l'allineamento verticale
				WebCtrLs.Table table = new WebCtrLs.Table();
				table.EnableViewState = false;
				table.CellSpacing = 0;
				table.CellPadding = 0;
				table.BorderWidth = Unit.Pixel(0);
				table.Style.Add(HtmlTextWriterStyle.BorderCollapse,"separate");
				WriteStyleAbsoluteHidden(table);

				WebCtrLs.TableRow row = new WebCtrLs.TableRow();
				row.EnableViewState = false;
				table.Controls.Add(row);

				WebCtrLs.TableCell cell = new WebCtrLs.TableCell();
				row.Controls.Add(cell);
				row.EnableViewState = false;

				WriteStyleFont(cell,obj.Label.FontStyleName);
				WriteStyleColor(cell,woorm.TrueColor(BoxType.Text,obj.TextColor,obj.Label.FontStyleName),Color.Transparent);
				WriteStyleScroll(cell);

				cell.EnableViewState = false;

                WriteAlign(cell, obj.Label.Align);

                cell.Text = InsertBR(obj.LocalizedText);

                webCtrl = table;
				//le table html sono renderizzate con i bordi interni, quindi non devo modificare il rettangolo per fare spazio ai bordi
				rect = obj.BaseRectangle;
            }

            webCtrl.EnableViewState = false;
            webCtrl.ToolTip = obj.GetTooltip;

            panel.Controls.Add(webCtrl);

			WriteStyleZOrder(webCtrl);
			WriteStylePosition(webCtrl, rect);

 			WriteStyleTextAlign(webCtrl, obj.Label.Align);

			WriteStyleFont(webCtrl, obj.Label.FontStyleName);
			WriteStyleBorders(webCtrl, obj.Borders, obj.BorderPen);

			WriteStyleColor(webCtrl, woorm.TrueColor(BoxType.Text, obj.TextColor, obj.Label.FontStyleName), obj.BkgColor);
			WriteStyleAbsoluteHidden(webCtrl);
		}

		//------------------------------------------------------------------------------
		public void GraphRectHtml(Panel panel, GraphRect obj) 
		{
			// valuata dal motore del viewer durante il parse.
			if (obj.IsHidden) return;

			//disegno eventuale ombra
			WriteDropShadow(panel, obj.BaseRectangle, obj.DropShadowHeight, obj.DropShadowColor);

			Panel graphRectHtml = new Panel();
			panel.Controls.Add(graphRectHtml);

			WebCtrLs.Image image = new WebCtrLs.Image();
			graphRectHtml.Controls.Add(image);

			graphRectHtml.EnableViewState = false;
			graphRectHtml.BackColor = obj.BkgColor;
			graphRectHtml.ToolTip   = obj.GetTooltip;
			graphRectHtml.Height	= Unit.Pixel(obj.BaseRectangle.Size.Height);
			graphRectHtml.Width		= Unit.Pixel(obj.BaseRectangle.Size.Width);

			string sourceFilePath = File.Exists(obj.ImageFileName) ? 
                                    obj.ImageFileName :
                                    woorm.GetFilename(obj.ImageFileName, NameSpaceObjectType.Image);
			if (File.Exists(sourceFilePath))		//copio il file sotto webfolder esposta (necessario per problemi d permessi)
			{
			    string ext = System.IO.Path.GetExtension(sourceFilePath);
			    FileProvider fp = new FileProvider(woorm, ext);
			    string destPath = fp.GenericTmpFile;	//nome di file temporaneo sotto la web folder esposta
				File.Copy(sourceFilePath, destPath, true);

				if (obj.ShowProportional)
				{
					Unit width,height;
					if (GetProportionalImageSize(Unit.Pixel(obj.BaseRectangle.Size.Width),Unit.Pixel(obj.BaseRectangle.Size.Height),destPath,out width,out height))
					{
						image.Width = width;
						image.Height = height;
					}
				}
				else
				{
					image.Height	= Unit.Pixel(obj.BaseRectangle.Size.Height);
					image.Width		= Unit.Pixel(obj.BaseRectangle.Size.Width);
				}

				image.ImageUrl	= "~\\" + fp.GenericTmpFileRelPath;
			}

			WriteStyleZOrder		(graphRectHtml);
			WriteStyleOrigin		(graphRectHtml,obj.BaseRectangle.Location);
			WriteStyleBorders		(graphRectHtml,obj.Borders,obj.BorderPen);
			WriteStyleAbsoluteHidden(graphRectHtml);
		}

		//------------------------------------------------------------------------------
		public void WriteGenericImage(TableCell cell, string imageUrl, bool showProportional, string imageFilePath) 
		{ 
			WebCtrLs.Image image = new WebCtrLs.Image();
			cell.Controls.Add(image);
			
			if (showProportional)
			{
				Unit width, height;
				if (GetProportionalImageSize(cell.Width,cell.Height,imageFilePath,out width,out height))
				{
					image.Width = width;
					image.Height = height;
				}
			}

			image.EnableViewState = false;
			image.BackColor = Color.Transparent;
			image.ImageUrl	= imageUrl;
		}

		///<summary>
		/// Metodo che dato un web control contenitore  e dato un path di un'immagine, restituisce le dimensioni dell'immagine tali che 
		/// sia contenuta nel controllo e mantenga un'aspetto proporzionale
		/// </summary>
		//------------------------------------------------------------------------------
		private static bool GetProportionalImageSize(Unit containerWidth, Unit containerHeight, string imageFilePath, out Unit width, out Unit height)
		{
			width = containerWidth;
			height = containerHeight;
			try
			{
				using (System.Drawing.Image img = Bitmap.FromFile(imageFilePath))
				{
					if (img != null)
					{
						//calcolo le dimensioni dell'immagine in modo che sfrutti al massimo lo spazio del contenitore
						double hRatio = containerWidth.Value/(double)(img.Width);
						double vRatio = containerHeight.Value/(double)(img.Height);

						if (hRatio > vRatio)
						{
							width = Unit.Pixel((int)(img.Width * vRatio));
							height = Unit.Pixel((int)(img.Height * vRatio));
							return true;
						}
						else
						{
							width = Unit.Pixel((int)(img.Width * hRatio));
							height = Unit.Pixel((int)(img.Height * hRatio));
							return true;
						}
					}
				}
			}
			catch (Exception)
			{
				return false;
			}
			return false;
		}

		//------------------------------------------------------------------------------
		public void FileRectHtml(Panel panel, FileRect obj) 
		{ 
			// valuata dal motore del viewer durante il parse.
			if (obj.IsHidden) return;

			//disegno eventuale ombra
			WriteDropShadow(panel, obj.BaseRectangle, obj.DropShadowHeight, obj.DropShadowColor);

			WebCtrLs.Label label = new WebCtrLs.Label();
			panel.Controls.Add(label);

			label.EnableViewState = false;
            label.ToolTip = obj.GetTooltip;

			WriteTextFile				(label, obj.Text);
			WriteStyleZOrder			(label);
			WriteStylePosition(label, obj.BaseRectangle.InflateForPosition(obj.Borders, obj.BorderPen));
			WriteStyleTextAlign			(label, obj.Label.Align);
			WriteStyleFont				(label, obj.Label.FontStyleName);
			WriteStyleBorders			(label, obj.Borders, obj.BorderPen);
			WriteStyleColor				(label, woorm.TrueColor(BoxType.Text, obj.Label.TextColor, obj.Label.FontStyleName), obj.Label.BkgColor);
			WriteStyleAbsoluteHidden	(label);
			WriteStyleScroll			(label);
		}

		//------------------------------------------------------------------------------
		public WebControl WriteFieldLabel(Panel panel, FieldRect obj, bool isTransparent) 
		{ 
			WebControl webCtrl;

			if ((obj.Label.Align & BaseObjConsts.DT_EX_FIELD_SET) != 0)
			{
				TbWebCtrLs.TBFieldSet label = new TbWebCtrLs.TBFieldSet();
				label.Caption = obj.LocalizedText;
				if ((obj.Label.Align & BaseObjConsts.DT_CENTER) != 0)
					label.Align = TbWebCtrLs.TBFieldSet.AlignStyle.Center;
				else if ((obj.Label.Align & BaseObjConsts.DT_RIGHT) != 0)
					label.Align = TbWebCtrLs.TBFieldSet.AlignStyle.Right;

				webCtrl = label;
		   }
		   else
		   {
				WebCtrLs.Label label = new WebCtrLs.Label();
				label.Text = obj.LocalizedText;

                webCtrl = label;
           }

			webCtrl.EnableViewState = false;
			panel.Controls.Add(webCtrl);

			WriteStyleZOrder			(webCtrl);
			WriteStylePosition          (webCtrl, obj.BaseRectangle.InflateForPosition(obj.Borders, obj.BorderPen));
			WriteStyleFont				(webCtrl, obj.Label.FontStyleName);
			WriteStyleBorders			(webCtrl, obj.Borders, obj.BorderPen);
			
			Color bkgColor = isTransparent ? Color.Transparent : obj.BkgColor;
			WriteStyleColor				(webCtrl, woorm.TrueColor(BoxType.Text, obj.LabelTextColor, obj.Label.FontStyleName), bkgColor);
			WriteStyleAbsoluteHidden	(webCtrl);
			WriteStyleNoWrap			(webCtrl);

            return webCtrl;
		}

		//------------------------------------------------------------------------------
		public void FieldRectHtml(Panel panel, FieldRect obj) 
		{ 
			// valutata dal motore del viewer durante il parse.
			if (obj.IsHidden) return;

			//disegno eventuale ombra
			WriteDropShadow(panel, obj.BaseRectangle, obj.DropShadowHeight, obj.DropShadowColor);

			if (obj.IsImage) 
			{
				WriteImage(panel, obj);
				return;
			}

			// costruisce una tabella trasparente con una sola cella inflatando dei bordi
			Rectangle inflated = Helper.Inflate(obj.BaseRectangle, obj.Borders, obj.BorderPen);
			Point pLocation = inflated.Location;

			//renderizzazione field rect con testo ruotato
			if ((obj.Value.Align & (BaseObjConsts.DT_EX_90 | BaseObjConsts.DT_EX_270)) != 0)
            {
				Panel fieldPanel = new Panel();
				WriteStyleZOrder			(fieldPanel);
				WriteStyleOrigin			(fieldPanel, pLocation);
				WriteStylePosition          (fieldPanel, obj.BaseRectangle.InflateForPosition(obj.Borders, obj.BorderPen));
				WriteStyleBorders			(fieldPanel, obj.Borders, obj.BorderPen);
				WriteStyleAbsoluteHidden	(fieldPanel);
				WriteStyleColor				(fieldPanel, woorm.TrueColor(BoxType.Text, obj.TextColor, obj.Value.FontStyleName), obj.BkgColor);
				FontElement fe = woorm.GetFontElement(obj.Value.FontStyleName);
		
				VerticalTextControl vtc = new VerticalTextControl(obj.Value.FormattedData, inflated, obj.Value.Align, fe,  woorm.TrueColor(BoxType.Text, obj.TextColor, obj.Value.FontStyleName));
				WriteStyleOrigin(vtc, pLocation);
				fieldPanel.Controls.Add(vtc);
				panel.Controls.Add(fieldPanel);

				WebControl webLabel1 = WriteFieldLabel(panel, obj, true);
				WebCtrLs.Table table1 = new WebCtrLs.Table();

				if (webLabel1 is TbWebCtrLs.TBFieldSet)
				{
					webLabel1.Controls.Add(table1);
					pLocation = new Point(0,0);
				}
				else
				{
					panel.Controls.Add(table1);
				}
				return;
			}	
			
			WebControl webLabel = WriteFieldLabel(panel, obj, false);
			WebCtrLs.Table table = new WebCtrLs.Table();

            if (webLabel is TbWebCtrLs.TBFieldSet)
            {
                webLabel.Controls.Add(table);
                pLocation = new Point(0,0);
            }
            else
            {
                panel.Controls.Add(table);
            }

			table.EnableViewState = false;
			table.CellSpacing = 0;
			table.CellPadding = 0;
			table.BorderWidth = Unit.Pixel(0);

			AdjustOverloadingLabel(inflated, obj);

			WriteStyleZOrder			(table);
			WriteStyleOrigin			(table, pLocation);
			WriteStyleClip				(table, inflated);
			WriteStyleAbsoluteHidden	(table);

			table.Height = inflated.Height;
			table.Width = inflated.Width;

			WebCtrLs.TableRow row = new WebCtrLs.TableRow();
			row.EnableViewState = false;
			table.Controls.Add(row);

			WebCtrLs.TableCell cell = new WebCtrLs.TableCell();
			row.Controls.Add(cell);
			row.EnableViewState = false;

			WriteStyleFont		(cell, obj.Value.FontStyleName);
			WriteStyleColor		(cell, woorm.TrueColor(BoxType.Text, obj.TextColor, obj.Value.FontStyleName), Color.Transparent);
			WriteStyleScroll	(cell);

			cell.EnableViewState = false;
			cell.Height = Unit.Pixel(inflated.Height);
			cell.Width = Unit.Pixel(inflated.Width);
            //DEBUG cell.BackColor = Color.Aquamarine;

            WriteAlign(cell, obj.Value.Align);

            cell.ToolTip = obj.GetTooltip;

			if (obj.IsTextFile)
			{
				WebCtrLs.Label label = new WebCtrLs.Label();
				cell.Controls.Add(label);

				label.EnableViewState = false;
				label.Width = Unit.Percentage(100);
				label.Height = Unit.Percentage(100);
				label.Text = woorm.ReadTextFile(obj.Value.FormattedData);
				WriteStyleScroll(label);
			}
            else if (obj.IsBarCode && obj.Value.FormattedData != null && obj.Value.FormattedData != string.Empty)
            {
                BarCodeViewer bcv = new BarCodeViewer(woorm, obj.BarCode);
                string humanText = string.Empty;
                if (obj.BarCode != null && obj.BarCode.HumanTextAlias > 0)
                    humanText = woorm.GetFormattedDataFromAlias(obj.BarCode.HumanTextAlias);

                string barCodeFile = bcv.GetBarcodeImageFile(obj.Value, woorm.GetFontElement(obj.Value.FontStyleName), obj.InsideRect, humanText);
                if (!string.IsNullOrEmpty(barCodeFile))
                    WriteGenericImage(cell, barCodeFile, obj.ShowProportional, bcv.GenericTmpFile);

                return;
            }
            else
            {
                if (obj.HasFormatStyleExpr && obj.Value.RDEData != null)
                {
                    string formatStyleName = obj.DynamicFormatStyleName;
                    if (formatStyleName.Length > 0)
                    {
                        obj.Value.FormattedData = FormatFromSoapData(formatStyleName, obj.InternalID, obj.Value.RDEData);
                    }
                }

                cell.Text = nbsp(InsertBR(obj.Value.FormattedData));
            }

			if (obj.Value.FormattedData != null && obj.Value.FormattedData.Length > 0)
				SetConnection(cell, obj.InternalID, -1, true);
		}
		#endregion

		#region Tabella usando celle singole
		// agisce solo non in absolute positioning. il Clip in tabella avviene solo se absolute

		//------------------------------------------------------------------------------
		public TableCell WriteSingleCell
			(
			Panel		panel,
			Rectangle	cellRect,
			Borders		borders,
			BorderPen	borderPen,
			Color		foreColor, 
			Color		bkgColor,
			string		fontStyleName,
			int			align,
			string		text,
			bool		hide,
            string      tooltip
			) 
		{
			return WriteSingleCell
			(
				panel,
				cellRect, borders, borderPen,
				foreColor, bkgColor, fontStyleName,
				align, text,
				hide, 
                tooltip,
				null, null
			);  
		}  
 

		//------------------------------------------------------------------------------
		private string GetClickEvent(ConnectionLink conn, string strNsReport, int atRowNumber, string sourceValue, Color colorBrowsed, out string parameters)
		{	
			parameters = Helper.FormatParametersForRequest(conn.GetArgumentsOuterXml(Woorm, atRowNumber, sourceValue));

			string path = CalculateRelativeUrl(Page);
            string sColor = HtmlUtility.ToHtml(colorBrowsed);
            
			return string.Format("this.style.color = '{0}'; return linkDocument('{1}?namespace={2}', '{3}');",
               sColor,
 				path,
				HttpUtility.UrlEncode(strNsReport),
				parameters
				);
		}

		//------------------------------------------------------------------------------
		private static string CalculateRelativeUrl (Page page)
		{
			string root = page.MapPath("~");
			string path = page.Request.PhysicalPath.Substring(root.Length);
			return path;
		}   
     
		//------------------------------------------------------------------------------
		protected virtual void SetConnection(TableCell cell, int alias, int atRowNumber, bool enabled)
		{     
			ConnectionLink conn = Woorm.Connections.GetConnectionOnAlias(alias, Woorm, atRowNumber);
			string navigateURL = string.Empty;

			if (conn != null && conn.Valid )
			{
				//Già fatto! woorm.SynchronizeSymbolTable(atRowNumber);
				switch(conn.ConnectionType)
				{
					case ConnectionLinkType.Report:
					case ConnectionLinkType.ReportByAlias:
					{
						string strNSreport = conn.Namespace;
						if (conn.ConnectionType == ConnectionLinkType.ReportByAlias)
						{
							Variable v = RdeReader.SymbolTable.Find(strNSreport);
							if (v != null &&v.Data != null)
								strNSreport = v.Data.ToString(); 
						}
					
                        //----
                        bool bSpecific = true;
                        FontElement feb = woorm.GetFontElement(cell.Font.Name + "_Hyperlink_Browsed");
                        if (feb == null)
                            feb = woorm.GetFontElement(cell.Font.Name + "_Hyperlink");
                        if (feb == null)
                        {
                            bSpecific = false;
                            feb = woorm.GetFontElement("<Hyperlink_Browsed>");
                        }
                        Color colorBrowsed = feb != null ? feb.Color : Color.FromArgb(128, 0, 128);
                        
                        //----
						string parameters;
						string clickEvent = GetClickEvent(conn, strNSreport, atRowNumber, cell.Text, colorBrowsed, out parameters);
                        
                        if (enabled)
                        {
                            cell.Attributes.Add("onclick", clickEvent);
                            cell.Style["cursor"] = "pointer";
                        }

                        //----
						string key = Helper.GetConnectionKey(strNSreport, parameters);
                        if (Page.Session[key] == null)
                            Page.Session[key] = false;
 
                        bool browsed = ((bool)Page.Session[key]);
                            
                        //----
                        FontElement fe = woorm.GetFontElement(cell.Font.Name + (browsed ? "_Hyperlink_Browsed" : "_Hyperlink"));
                        if (fe == null && browsed)
                            fe = woorm.GetFontElement(cell.Font.Name + "_Hyperlink");
                        if (fe == null)
                        {
                            bSpecific = false;
                            fe = woorm.GetFontElement(browsed ? "<Hyperlink_Browsed>" : "<Hyperlink>");
                        }

                        if (fe == null)
                        {
						    cell.Font.Underline = true;
                            cell.ForeColor = browsed ? Color.FromArgb(128, 0, 128) : Color.Blue; 
                        }
                        else
                        {
                            cell.Font.Underline = (FontStyle.Underline & fe.FontStyle) == FontStyle.Underline;
                            cell.ForeColor = fe.Color; 

                            if (bSpecific)
                            {
                                cell.Font.Italic = (FontStyle.Italic & fe.FontStyle) == FontStyle.Italic;
                                cell.Font.Bold = (FontStyle.Bold & fe.FontStyle) == FontStyle.Bold;
                                cell.Font.Strikeout = (FontStyle.Strikeout & fe.FontStyle) == FontStyle.Strikeout;
  
                                cell.Style["Font-Family"] = fe.FaceName;
                                cell.Style["Font-Size"] = px(fe.Size);
                            }
                        }
                        if (!enabled)
                            cell.Font.Underline = false;
						break;
					}
					case ConnectionLinkType.URL:
					case ConnectionLinkType.URLByAlias:
					{
						navigateURL = conn.GetNavigateUrl(atRowNumber);
		
						WebCtrLs.HyperLink hyperLink= new HyperLink();
						hyperLink.Text = cell.Text;
						if (conn.ConnectionSubType != ConnectionLinkSubType.CallTo)
							hyperLink.Target = "_blank";
						hyperLink.NavigateUrl = navigateURL;
						cell.Controls.Add(hyperLink);
						break;
					}
				}
			}
		}
	
		//------------------------------------------------------------------------------
		protected virtual void SetHotLinkConnection(TableCell cell)
		{
			LinkButton hotLinkControl = new LinkButton();
			hotLinkControl.Text = cell.Text;
			cell.Controls.Add(hotLinkControl);
			hotLinkControl.Click += new EventHandler(WoormWebControl.RadarButtonSelectionClick);
		}

		//------------------------------------------------------------------------------
		public TableCell WriteSingleCell
			(
			Panel		panel,
			Rectangle	cellRect,
			Borders		borders,
			BorderPen	borderPen,
			Color		foreColor, 
			Color		bkgColor,
			string		fontStyleName,
			int			align,
			string		text,
			bool		hide,
            string      tooltip,
			Column		column,		// info per barcode
			Cell		columnCell	// info per barcode
			) 
		{ 
			if (hide) return null;

			BorderPen bottomPen = borderPen;
			if (columnCell!= null && !columnCell.IsLastRow && column != null && column.Table.Borders.RowSepPen != null)
			{
				bottomPen = column.Table.Borders.RowSepPen;
			}

			Rectangle inflated = Helper.Inflate(cellRect, borders, borderPen, bottomPen);

			WebCtrLs.Panel callPanel = new WebCtrLs.Panel();
			 
			WriteStyleZOrder		(callPanel);
			WriteStylePosition		(callPanel, cellRect.InflateForPosition(borders, borderPen, bottomPen));
			WriteStyleClip			(callPanel, cellRect);

			callPanel.EnableViewState = false;

			WriteStyleBorders	(callPanel, borders, borderPen, bottomPen);

			WriteStyleColor		(callPanel,foreColor,bkgColor);
			WriteStyleAbsolute	(callPanel);
			WriteStyleHidden	(callPanel);
		
			//------------------------------------------------------- la tabella
			WebCtrLs.Table table = new WebCtrLs.Table();
			
			table.EnableViewState = false;
			table.CellSpacing = 0;
			table.CellPadding = 0;

			WriteStyleZOrder			(table);
			WriteStylePosition			(table, inflated);
			WriteStyleClip				(table, inflated);

			table.Height = inflated.Height;
			table.Width = inflated.Width;

			WriteStyleAbsolute		(table);
			WriteStyleHidden		(table);
			WriteStyleBorderWidth0	(table);
			
			//-------------------------------------------------------- la riga
			WebCtrLs.TableRow row = new WebCtrLs.TableRow();
			row.EnableViewState = false;
			table.Controls.Add(row);

			//-------------------------------------------------------  la cella
			WebCtrLs.TableCell cell = new WebCtrLs.TableCell();
 			row.Controls.Add(cell);
			row.EnableViewState = false;

			WriteStyleFont(cell, fontStyleName);
			WriteStyleColor	(cell, foreColor, Color.Transparent);

			cell.EnableViewState = false;
			cell.Height = Unit.Pixel(inflated.Height);
			cell.Width = Unit.Pixel(inflated.Width);

			WriteAlign(cell, align); 

            cell.ToolTip = tooltip;

			if (column != null && columnCell != null && column.IsBarCode && columnCell.Value.FormattedData != string.Empty)
			{
				BarCodeViewer bcv = new BarCodeViewer(woorm, column.BarCode);
				string humanText = string.Empty;
				if (column.BarCode != null && column.BarCode.HumanTextAlias > 0)
					humanText = woorm.GetFormattedDataFromAlias (column.BarCode.HumanTextAlias, columnCell.AtRowNumber);

				string barCodeFile = bcv.GetBarcodeImageFile(columnCell.Value, woorm.GetFontElement(fontStyleName), cellRect, humanText);
				if (barCodeFile != string.Empty)
					WriteGenericImage(cell,barCodeFile,column.ShowProportional, bcv.GenericTmpFile);
				else
					cell.Text = text;			
			}
			else if (column != null && columnCell != null && column.ShowAsBitmap)
			{
				string sourceFilePath = woorm.GetFilename(text, NameSpaceObjectType.Image);
				string ext = System.IO.Path.GetExtension(sourceFilePath);
				FileProvider fp = new FileProvider(woorm, ext);
				string destPath = fp.GenericTmpFile;	//nome di file temporaneo sotto la web folder esposta
								
				if (File.Exists(sourceFilePath))		//copio il file sotto webfolder esposta (necessario per problemi d permessi)
				{
					File.Copy(sourceFilePath,destPath,true);
					WriteGenericImage(cell,"~\\" + fp.GenericTmpFileRelPath,column.ShowProportional,destPath); //non e' un barCode ma immagine generica
				}
				else
					cell.Text = text;		
			}
			else
			{	
				cell.Text = InsertBR(text);
			}
			panel.Controls.Add(callPanel);
			panel.Controls.Add(table);

			return cell;
		}

		//------------------------------------------------------------------------------
		public void WriteTableTitleHtml(Panel panel, RSjson.Table obj) 
		{ 
			Borders borders = new Borders
			(
				obj.Borders.TableTitleTop,
				obj.Borders.TableTitleLeft,
				false,
				obj.Borders.TableTitleRight
			);

			WriteSingleCell
			(
				panel, obj.TitleRect,
				borders, obj.TitlePen,
				woorm.TrueColor(BoxType.TableTitle, obj.Title.TextColor, obj.Title.FontStyleName),
				obj.Transparent ? Color.Transparent : obj.Title.BkgColor,
				obj.Title.FontStyleName,
				obj.Title.Align, obj.LocalizedText,
				obj.HideTableTitle,
                string.Empty
			);
		}

		//------------------------------------------------------------------------------
		public void WriteColumnsTitleHtml(Panel panel, RSjson.Table obj) 
		{ 
			bool first = true;
			int lastColumn = obj.LastVisibleColumn();

			for (int col = 0; col <= lastColumn; col++)
			{
				Column column = (Column)obj.Columns[col];
				bool showTitle = ((obj.HideTableTitle && obj.HideColumnsTitle) || obj.HideColumnsTitle);
				bool last = col == lastColumn;

				if (!column.IsHidden)
				{
					Borders borders = new Borders
					(
						obj.Borders.ColumnTitleTop && !showTitle,
						first && obj.Borders.ColumnTitleLeft && !showTitle,
						obj.Borders.BodyTop,
						!showTitle &&
						(
						(last && obj.Borders.ColumnTitleRight) ||
						(!last && obj.Borders.ColumnSeparator && obj.Borders.ColumnTitleSeparator)
						)
					);

					WriteSingleCell
					(
						panel, column.ColumnTitleRect,
						borders, column.ColumnTitlePen,
						woorm.TrueColor(BoxType.ColumnTitle, column.TitleTextColor, column.Title.FontStyleName),
						column.TitleBkgColor,
						column.Title.FontStyleName,
						column.Title.Align, column.LocalizedText,
						showTitle,
                        column.TitleTooltip
					);
					first = false;
				}
			}
		}

        //---------------------------------------------------------------------------
        CultureInfo GetCollateCultureFromId(ushort id)
        {
            Variable v = woorm.SymbolTable.FindById(id);
            return v == null ? CultureInfo.InvariantCulture : v.CollateCulture;
        }

        ///<summary>
        ///Riformatta dinamicamente il dato 
        /// </summary>
        string FormatFromSoapData(string formatStyleName, ushort ID, object data)
        {
           CultureInfo collateCulture = GetCollateCultureFromId(ID);
           //vedi RDEReader
           return woorm.FormatStyles.FormatFromSoapData(formatStyleName, data, woorm.Namespace, collateCulture);
        }

		///<summary>
		///Metodo che disegna il corpo della tabella
		/// </summary>
		//------------------------------------------------------------------------------
		public void WriteTableBodyHtml(Panel panel, RSjson.Table obj) 
		{
			//predispone la table per la modalita di Easyview dinamica (nel caso sia presente)
			obj.InitEasyview();

			int lastColumn = obj.LastVisibleColumn();
			for (int row = 0; row < obj.RowNumber; row++)
			{
				bool firstCol = true;
				for (int col = 0; col <= lastColumn; col++)
				{
					Column column = (Column)obj.Columns[col];
					if (!column.IsHidden)
					{
                        if (row == 0)
                            column.PreviousValue = null;

						Cell cell = (Cell)column.Cells[row];
						bool lastCol = col == lastColumn;
						cell.AtRowNumber = row;

                        if (cell.HasFormatStyleExpr && cell.Value.RDEData != null)
                        {
                            string formatStyleName = cell.ValueFormatStyleName;
                            if (formatStyleName.Length > 0)
                            {
                                cell.Value.FormattedData = FormatFromSoapData(formatStyleName, column.InternalID, cell.Value.RDEData);
                            }
                        }

						Borders borders = new Borders
							(
							    false,
							    firstCol && obj.Borders.BodyLeft,
							    obj.HasBottomBorderAtCell(cell),
							    (!lastCol && obj.Borders.ColumnSeparator) || (lastCol && obj.Borders.BodyRight)
							);
                        Borders cellBorders = borders;
                        if (cell.HasCellBordersExpr)
                        {
                            cellBorders = cell.ValueCellBorders(borders);
                        }

                       string fontStyleName = string.Empty;
                       if (!cell.SubTotal && cell.HasTextFontStyleExpr)
                       {
                            fontStyleName = cell.ValueTextFontStyleName;
                        }
                       if (fontStyleName.Length == 0)
                            fontStyleName = cell.SubTotal
							    ? column.SubTotal.FontStyleName
                                : cell.Value.FontStyleName;

						Color fore	= cell.SubTotal
							? woorm.TrueColor(BoxType.SubTotal, cell.ValueSubTotTextColor,  fontStyleName)
							: woorm.TrueColor(BoxType.Cell,     cell.ValueTextColor,        fontStyleName);

						Color bkg	= cell.SubTotal 
							?  
							    cell.GetValueSubTotBkgColor (obj.UseColorEasyview(row) ? obj.EasyviewColor : cell.column.SubTotal.BkgColor)
							: 
                                cell.GetValueBkgColor       (obj.UseColorEasyview(row) ? obj.EasyviewColor : cell.DefaultBkgColor);

						if (obj.FiscalEnd && row >= obj.CurrentRow)
						{
							fore = Color.SlateGray;
							bkg = Color.DimGray;
						}

						if (cell.SubTotal)
						{
                            column.PreviousValue = null;

							WriteSingleCell
							(
								panel, 
                                cell.RectCell,
								cellBorders, column.ColumnPen, 
								fore, bkg,
								fontStyleName,
								column.SubTotal.Align, 
								nbsp(cell.Value.FormattedData),
								false,
                                cell.Tooltip,
								column, cell
							);
						}
						else
						{
							TableCell tableCell = WriteSingleCell
							(
								panel, 
                                cell.RectCell,
                                cellBorders, column.ColumnPen, 
								fore, bkg,
								fontStyleName,
								cell.Value.Align,
								nbsp(cell.FormattedDataForWrite),
								false,
                                cell.Tooltip,
								column, cell
							);

							if (
                                tableCell != null && column != null && 
                                cell != null && cell.Value != null &&
                                cell.Value.FormattedData != null && cell.Value.FormattedData.Length > 0
                                )
								SetConnection(tableCell, column.InternalID, row, !cell.Value.CellTail);
						}

						firstCol = false;
					}
				}

				obj.EasyViewNextRow(row);
			}
		}

		//------------------------------------------------------------------------------
		public void WriteTableTotalsHtml(Panel panel, RSjson.Table obj) 
		{ 
			int lastColumn = obj.LastVisibleColumn();
			for (int col = 0; col <= lastColumn; col++)
			{
				Column column = (Column)obj.Columns[col];

				if (!column.IsHidden)
				{
					TotalCell total = column.TotalCell;
					bool first = (col == 0);
					bool last = (col == obj.ColumnNumber - 1);

					int nextVisibleColumn = obj.NextVisibleColumn(col);
					bool nextColumnHasTotal =
						(
						(col < lastColumn) && 
						(nextVisibleColumn >= 0) && 
						obj.HasTotal(nextVisibleColumn)
						);

					BorderPen pen = total.TotalPen; 
					Borders borders;
					if (column.ShowTotal)
					{
						borders = new  Borders
							(
							false,
							first,
							obj.Borders.TotalBottom,
							obj.Borders.TotalRight
							);
					}
					else
					{	// serve per scrivere il bordo del successivo totale
						borders = new Borders
							(
							false,
							false,
							false,
							!last && nextColumnHasTotal && obj.Borders.TotalLeft
							);
						//disegno il bordo sx del prossimo totale con il suo Pen e non con quello della cella corrente
						//(allinemento con comportamento di woorm c++)
						if (col + 1 <= lastColumn)
						{
							Column nextColumn = (Column)obj.Columns[col+1];
							pen = nextColumn.TotalCell.TotalPen;
						}
					}

					Color fore = woorm.TrueColor(BoxType.Total, total.TotalTextColor, total.Value.FontStyleName);
					Color bkg = !column.ShowTotal || obj.Transparent ? Color.Transparent : total.TotalBkgColor;

                    if (total.HasFormatStyleExpr && total.Value.RDEData != null)
                    {
                        string formatStyleName = total.ValueFormatStyleName;
                        if (formatStyleName.Length > 0)
                        {
                            total.Value.FormattedData = FormatFromSoapData(formatStyleName, column.InternalID, total.Value.RDEData);
                        }
                    }

					WriteSingleCell
					(
						panel, 
                        total.RectCell,
						borders, pen, 
						fore, bkg, 
						total.Value.FontStyleName,
						total.Value.Align, 
                        nbsp(column.ShowTotal ? total.Value.FormattedData : ""),
						false,
                        string.Empty
					);			
				}
			}
		}

		//------------------------------------------------------------------------------
		public void TableHtml(Panel panel, RSjson.Table obj) 
		{
            // valutata dal motore del viewer durante il parse.
            if (obj.IsHidden) return;

			WriteTableTitleHtml(panel, obj);
            //TODO gestione Width Expression - anticipato parsing per il TbLocalizer
            //TODO debuggare con WidthExpression
            WriteColumnsTitleHtml(panel, obj);
			WriteTableBodyHtml(panel, obj);
			WriteTableTotalsHtml(panel, obj);

			//disegno eventuale ombra
			WriteDropShadow(panel, obj.BaseCellsRect, obj.DropShadowHeight, obj.DropShadowColor);
		}

		///<summary>
		///Metodo statico che dato un rettongolo di un oggetto, e il suo panel contenitore, disegna due panel in basso e a 
		///destra per dare l'effetto ombra all'oggetto
		///</summary>
		//------------------------------------------------------------------------------
		private static void WriteDropShadow(Panel panel, Rectangle rectangleObj, int shadowHeight, Color color)
		{
			//se c'e' ombra chiamo il metodo che la disegna
			if (shadowHeight <= 0)
				return;

			Panel bottomShadow = new Panel();
			bottomShadow.Style.Add(HtmlTextWriterStyle.Position, "absolute");
			bottomShadow.Style.Add(HtmlTextWriterStyle.Left,Unit.Pixel(rectangleObj.Left + shadowHeight).ToString());
			bottomShadow.Style.Add(HtmlTextWriterStyle.Top, Unit.Pixel(rectangleObj.Bottom).ToString());
			bottomShadow.Width = rectangleObj.Width;
			bottomShadow.Height = shadowHeight;
			bottomShadow.BackColor = color;
			panel.Controls.Add(bottomShadow);

			Panel rightShadow = new Panel();
			rightShadow.Style.Add(HtmlTextWriterStyle.Position,"absolute");
			rightShadow.Style.Add(HtmlTextWriterStyle.Left,Unit.Pixel(rectangleObj.Right).ToString());
			rightShadow.Style.Add(HtmlTextWriterStyle.Top,Unit.Pixel(rectangleObj.Top + shadowHeight).ToString());
			rightShadow.Width = shadowHeight;
			rightShadow.Height = rectangleObj.Height;
			rightShadow.BackColor = color;
			panel.Controls.Add(rightShadow);
		}

		#endregion

		//--------------------------------------------------------------------------
		protected Panel ReportHeader(WebCtrLs.Table table)
		{
			WebCtrLs.TableRow row = new WebCtrLs.TableRow();
			row.EnableViewState = false;
			table.Controls.Add(row);

			WebCtrLs.TableCell cell = new WebCtrLs.TableCell();
			cell.EnableViewState = false;
			row.Controls.Add(cell);

			WebCtrLs.Panel panel = new WebCtrLs.Panel();
			panel.EnableViewState = false;
			cell.Controls.Add(panel);

			SetReportHeaderStyle(panel);
			
			return panel;
		}

		//--------------------------------------------------------------------------
		protected virtual void SetReportHeaderStyle(WebControl control)
		{
			control.Height = Unit.Pixel(UnitConvertions.MUtoLP(woorm.PageInfo.DmPaperLength, UnitConvertions.MeasureUnits.CM, 100, 5));
			control.Width = Unit.Pixel(UnitConvertions.MUtoLP(woorm.PageInfo.DmPaperWidth, UnitConvertions.MeasureUnits.CM, 100, 5));
		}

	
		//--------------------------------------------------------------------------
		protected void Report(WebCtrLs.Panel outerPanel,int feedbackPanelHeight)
		{
			Panel reportPanel = new Panel();
			reportPanel.Height = Unit.Percentage(100);
			reportPanel.Width = Unit.Percentage(100);
			
			//per evitare la doppia scrollbar lo metto visibile
			//e' la pagina contenitore nel body che si occupa di mettere le scrollbar
			reportPanel.Style[HtmlTextWriterStyle.Overflow] = "visible";
			reportPanel.Style["position"] = "absolute";
			reportPanel.Style["top"] = Unit.Pixel(toolbarHeight + feedbackPanelHeight).ToString();

			outerPanel.Controls.Add(reportPanel);

			WebCtrLs.Table table = new WebCtrLs.Table();
			table.EnableViewState = false;
			table.CellSpacing = table.CellPadding = 0;
			table.BorderStyle = BorderStyle.None;
			reportPanel.Controls.Add(table);

			Panel panel = ReportHeader(table);

			// incollo al panel l'eventuale immagine di sfondo
			if (woorm.Options.BkgnBitmap != null && woorm.Options.BkgnBitmap.Length != 0)
			{
				string sourceFilePath = woorm.GetFilename(woorm.Options.BkgnBitmap, NameSpaceObjectType.Image);
				string ext = System.IO.Path.GetExtension(sourceFilePath);
				FileProvider fp = new FileProvider(woorm, ext);
				string destPath = fp.GenericTmpFile;	//nome di file temporaneo sotto la web folder esposta
								
				if (File.Exists(sourceFilePath))		//copio il file sotto webfolder esposta (necessario per problemi d permessi)
				{
					File.Copy(sourceFilePath, destPath, true);
					panel.BackImageUrl = "~\\" + fp.GenericTmpFileRelPath;
					panel.Style["background-repeat"] = "no-repeat";
					//panel.Style["background-attachment"] = "fixed";
					panel.Style["background-position"] = string.Format("{0}px {1}px", woorm.Options.BitmapOrigin.X, woorm.Options.BitmapOrigin.Y);
				}
				 
			}

            RenderBaseObjList(panel, woorm.Objects);
		}

        private void RenderBaseObjList(Panel panel, BaseObjList list)
        {
            foreach (BaseObj obj in list)
            {
                switch (obj.GetType().Name)
                {
                    case "FieldRect": FieldRectHtml(panel, (FieldRect)obj); break;
                    case "TextRect": TextRectHtml(panel, (TextRect)obj); break;
                    case "Table":
						TableHtml(panel, (RSjson.Table)obj);
                        break;
                    case "GraphRect": GraphRectHtml(panel, (GraphRect)obj); break;
                    case "FileRect": FileRectHtml(panel, (FileRect)obj); break;
                    case "SqrRect": SqrRectHtml(panel, (SqrRect)obj); break;

                    case "Repeater":
                        {
                            RepeaterHtml(panel, (WoormViewer.Repeater)obj); 
                            break;
                        }

                    case "MetafileRect": /* non supportato */ break;
                    default:
                        UnknowHtml(panel);
                        break;
                }
            }
        }

        private void RepeaterHtml(Panel panel, WoormViewer.Repeater repeater)
        {
            SqrRectHtml(panel, repeater);

            foreach (BaseObjList list in repeater.Rows)
            {
                RenderBaseObjList(panel, list);
            }
        }

		//------------------------------------------------------------------------------
		private const string cr  = "\r\n";

		//------------------------------------------------------------------------------
		private string MessageScript
		{
			get
			{
				int msgNo = woorm.Messages.Count;
				if (msgNo == 0) 
					return null;

				string messages = "";
				for (int i = 0; i < msgNo; i++)
				    messages += string.Format("alert('{0}')\r\n", woorm.Messages[i]);

				return messages;
			}
		} 
		
 
		//--------------------------------------------------------------------------
		protected override void CreateChildControls()
		{
			WebCtrLs.Panel panel = new WebCtrLs.Panel();
			panel.EnableViewState = false;
			panel.BorderWidth = Unit.Pixel(0);

			updPnlViewerControl = new ConditionalUpdatePanel(UseUpdatePanel);
			updPnlViewerControl.ID = "report";
			updPnlViewerControl.UpdateMode = UpdatePanelUpdateMode.Conditional;
			updPnlViewerControl.ChildrenAsTriggers = false;
			Controls.Add(updPnlViewerControl);
			updPnlViewerControl.ContentTemplateContainer.Controls.Add(panel);

			//Chiamo il metodo di disegno del report all'interno di un try-catch perche da qualche parte potrebbero essere 
			//sollevate eccezioni (es. nel disegno del barcode), che vanno qui trappate per spedire al client il codice html con 
			//il messaggio di errore
			try
			{
				//used to persist report init parameters across round trips
				if (Session.ReportParameters != null && Session.ReportParameters.Length > 0)
				{
					string htmlText = string.Format("<input type='hidden' value = '{0}' name = '{1}'/>",
						Helper.FormatParametersForRequest(Session.ReportParameters),
						Microarea.TaskBuilderNet.Woorm.WebControls.Helper.DocumentParametersControlName
						);
					Controls.Add(new LiteralControl(htmlText));			
				}

				//carico la pagina per forzare anche il controllo del file totPages e capire quindi se il motore
				//ha terminato l'esecuzione
				Woorm.LoadPage(RdeReader.CurrentPage);

				if (showToolBar)
					Toolbar(panel);

				//registro script per l'esecuzione degli script "dinamici"
				string scriptName = "execInitScript";
				if (!this.Page.ClientScript.IsStartupScriptRegistered(scriptName) && updPageCount != null)
				{
					//Registro la funzione che esegue gli script se presenti
					string execInitScript =		string.Format("function execInitScript() {{"
													+			"var btn =  $get('{0}'); "
													+			"if (btn == null) return;"
													+			"var script = btn.attributes['{1}'];"
													+			"if (script)"
													+			" eval(script.value);}}", updPageCount.ClientID, InitScript);

					ScriptManager.RegisterStartupScript(this,GetType(),scriptName,execInitScript,true);
				}

				int feedbackPanelHeight = 0;
				if (!AllPagesReady)
				{
					CreateFeedbackPanel(ref feedbackPanelHeight);
				}
				else
				{
					AddInitScript("stopPing();");
				}

				if (RdeReader.IsPageReady() && (RdeReader.CurrentPage != Woorm.ReportSession.PageRendered || AllPagesReady))
				{
					Woorm.ReportSession.PageRendered = RdeReader.CurrentPage;
					string messageScript = MessageScript;
					if (!string.IsNullOrWhiteSpace(messageScript))
						AddInitScript(messageScript);
				
					updPnlViewerControl.Update();	
				}
			
				Report(panel,feedbackPanelHeight);
			}
			catch (Exception ex)
			{
				//rimuove tutti i controlli eventualmente aggiunti fino ad ora
				panel.Controls.Clear();
				//se c'e' un'eccezione devo smettere di fare ping, il visualizzatore si ferma su messaggio errore
				AddInitScript("stopPing();");
				ShowExceptionControl excControl = new ShowExceptionControl(ex);
				panel.Controls.Add(excControl);
				updPnlViewerControl.Update();
				if (updPanelWait != null)
					updPanelWait.Update();
			}
		}

		//--------------------------------------------------------------------------
		private void AddInitScript(string script)
		{
			//metto in un attributo "initScript" il javascript da eseguire (nome atrtibuto deve essere allineato con quello nel 
			//file TaskBuilderNet\Microarea.TaskBuilderNet.Woorm\WoormWebControl\WoormViewerClientControl.js  )
			if (updPageCount != null)
				updPageCount.Attributes.Add(InitScript, script);
		}
		
		//--------------------------------------------------------------------------
		private void CreateFeedbackPanel(ref int feedbackPanelHeight)
		{
			feedbackPanelHeight = 20;
			Panel panelWait = new Panel();
			panelWait.Style.Add(HtmlTextWriterStyle.ZIndex,"1000");
			panelWait.Style.Add(HtmlTextWriterStyle.Position,"absolute");
			panelWait.Style.Add(HtmlTextWriterStyle.Height, Unit.Pixel(feedbackPanelHeight).ToString());

			updPanelWait = new ConditionalUpdatePanel(UseUpdatePanel);
			updPanelWait.ID = "wait";
			updPanelWait.UpdateMode = UpdatePanelUpdateMode.Conditional;
			updPanelWait.Update();
			updPanelWait.ContentTemplateContainer.Controls.Add(panelWait);
			updPnlViewerControl.ContentTemplateContainer.Controls.Add(updPanelWait);

			//bottone dummy usato per registrare gli script da eseguire come suo attributo e per
			//fare il postback della pagina
			WebCtrLs.Button btnDummy = null;		//bottone dummy usato per fare il postback
			btnDummy = new Button();
			btnDummy.ID = "dummyPingBtn";
			btnDummy.Height = 0;
			btnDummy.Style.Add(HtmlTextWriterStyle.Visibility,"hidden");
			panelWait.Controls.Add(btnDummy);


			System.Web.UI.WebControls.Label label = new System.Web.UI.WebControls.Label();
			panelWait.Controls.Add(label);

			//se e' stato interrotto dall'utente
			if (woorm.ReportSession.StoppedByUser)
			{
				label.Text = WoormViewerStrings.ExecutionStopped;
				updPnlViewerControl.Update();
				updPanelWait.Update();
			}
			else if (StateMachine.Report != null)
			{
				label.Text = ".....Working.......  Page: " + ReportEngine.OutChannel.PageNumber.ToString();
				//se ho almeno una tabella, controllo lo stato di avanzamento della prima tabella
				if (ReportEngine.RepSymTable.DisplayTables.Count > 0)
				{
					label.Text += "  Row: " + ((DisplayTable)ReportEngine.RepSymTable.DisplayTables[0]).CurrentRow;
				}
			}

			if (!this.Page.ClientScript.IsStartupScriptRegistered("ping"))
			{
				//Registro la chiamata alla funzione js che fa partire il ping per dare feedback all'utente
				//parte da 0.5 secondi il primo ping (il secondo param, 500) poi rallenta sino a 4 secondi
				//Questo per non rallentare la visualizzazione dei report dall'esecuzione breve
				string pingScript = string.Format("ping('{0}', 500);", btnDummy.ClientID);
				ScriptManager.RegisterStartupScript(this, GetType(),"ping", pingScript, true);
			}
		}  		
	}

	// ================================================================================
	public class PrintViewerControl : AspNetRender
	{
		//------------------------------------------------------------------------------
		public PrintViewerControl(WoormDocument woorm, string stateMachineSessionTag, WoormWebControl woormWebControl)
			: base(woorm, stateMachineSessionTag, woormWebControl)
		{
			this.ShowToolBar = false;
		}

		//--------------------------------------------------------------------------
		protected override void CreateChildControls()
		{
			int current = Woorm.RdeReader.CurrentPage;

			Controls.Add(new LiteralControl("<SCRIPT> window.onload = doPrint; function doPrint() {window.print();}</SCRIPT>"));
			Controls.Add(new LiteralControl("<style type='text/css'>.screen	{height: 1px; border: 1px solid #dddddd; width: 100%; padding: 0px; margin: 0px; } </style> <style type='text/css' media='print'> .screen	{display: none;}</style>"));
			for (int i = 1; i <= Woorm.RdeReader.TotalPages; i++)
			{
				Woorm.LoadPage(i);

				base.CreateChildControls();
				if (i < Woorm.RdeReader.TotalPages)
				{
					Controls.Add(new LiteralControl("<hr class='screen' />"));
					Controls.Add(new LiteralControl("<div style='visibility: hidden; page-break-before: always; padding: 0px; margin: 0px; border: 0px;'></div>"));
				}
			}

			Woorm.RdeReader.CurrentPage = current;
		}

		//--------------------------------------------------------------------------
		protected override void SetReportHeaderStyle(WebControl control)
		{
			control.Style["Overflow"] = "hidden";
			control.Style["Clip"] = "auto";
		}

		//--------------------------------------------------------------------------
		protected override void SetConnection(TableCell cell, int alias, int atRowNumber, bool enabled)
		{
			// does nothing: no available link report when printing!
		}

	}


	/// <summary>
	/// Controllo che mostra le eccezioni all'utente
	/// </summary>
	/// ================================================================================
	public class ShowExceptionControl : Panel
	{
		Exception exception;

		//------------------------------------------------------------------------------
		public ShowExceptionControl(Exception ex)
		{
			if (ex.InnerException != null)
				exception = ex.InnerException;
			else
				exception = ex;
		}

		//------------------------------------------------------------------------------
		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			WebCtrLs.Image errorImg = new WebCtrLs.Image();
			//errorImg.ImageUrl = ImagesHelper.CreateImageAndGetUrl(ImageNames.ErrorIcon, WoormWebControl.DefaultReferringType);
			Controls.Add(errorImg);

			WebCtrLs.Button CloseButton = new WebCtrLs.Button();
			WebCtrLs.Button DetailButton = new WebCtrLs.Button();
			WebCtrLs.Label DetailLabel = new WebCtrLs.Label();
			WebCtrLs.Label DetailLabelDescription = new WebCtrLs.Label();
			WebCtrLs.Label StackTrace = new WebCtrLs.Label();
			WebCtrLs.Label StackTraceDescription = new WebCtrLs.Label();

			DetailButton.Text = WoormWebControlStrings.Details;
			DetailLabel.Text = WoormWebControlStrings.DetailsLabel;
			StackTrace.Text = WoormWebControlStrings.Stack;
			CloseButton.Text = WoormWebControlStrings.Close;

			string ErrorLabelDescription;
			if (exception == null)
			{
				DetailButton.Visible = false;
				ErrorLabelDescription = WoormWebControlStrings.UnknownError;
				return;
			}
			ErrorLabelDescription = Microarea.TaskBuilderNet.Woorm.WebControls.Helper.InsertBR(exception.Message);
			string details = GetDetail(exception);

			DetailLabelDescription.Visible = true;
			DetailLabelDescription.Text = Microarea.TaskBuilderNet.Woorm.WebControls.Helper.InsertBR(details);
			DetailLabel.Visible = true;

			StackTraceDescription.Text = Microarea.TaskBuilderNet.Woorm.WebControls.Helper.InsertBR(exception.StackTrace);

			//Prima riga
			WebCtrLs.Table table = new WebCtrLs.Table();
			table.Style.Add(HtmlTextWriterStyle.MarginTop,"30px");
			TableRow firstRow = new TableRow();
			table.Rows.Add(firstRow);
			TableCell imgCell = new TableCell();
			imgCell.HorizontalAlign = WebCtrLs.HorizontalAlign.Center;
			imgCell.RowSpan = 2;
			imgCell.Controls.Add(errorImg);
			firstRow.Cells.Add(imgCell);
			
			TableCell ErrorCell = new TableCell();
			ErrorCell.Text = WoormWebControlStrings.ErrorLabelText;
			firstRow.Cells.Add(ErrorCell);
			
			//Seconda riga
			TableRow secondRow = new TableRow();
			table.Rows.Add(secondRow);
			TableCell ErrorDescriptionCell = new TableCell();
			ErrorDescriptionCell.Text = ErrorLabelDescription;
			secondRow.Cells.Add(ErrorDescriptionCell);

			//terza riga
			TableRow thirdRow = new TableRow();
			table.Rows.Add(thirdRow);
			TableCell DetailsButtonCell = new TableCell();
			DetailsButtonCell.HorizontalAlign = WebCtrLs.HorizontalAlign.Right;
			thirdRow.Cells.Add(DetailsButtonCell);
			DetailButton.Style.Add(HtmlTextWriterStyle.Margin,"20px");
			DetailsButtonCell.Controls.Add(DetailButton);
			TableCell CloseButtonCell = new TableCell();
			thirdRow.Cells.Add(CloseButtonCell);
			CloseButton.Style.Add(HtmlTextWriterStyle.Margin,"20px");
			CloseButtonCell.HorizontalAlign = WebCtrLs.HorizontalAlign.Center;
			CloseButtonCell.Controls.Add(CloseButton);

			table.BorderWidth = 1;
			table.BorderColor = Color.Gray;
			table.HorizontalAlign = WebCtrLs.HorizontalAlign.Center;
			Controls.Add(table);

			Panel stackPanel = new Panel();
			stackPanel.Style.Add(HtmlTextWriterStyle.Visibility,"hidden");
			WebCtrLs.Table tableStack = new WebCtrLs.Table();
			stackPanel.Controls.Add(tableStack);
			
			TableRow trDetailLabel = new TableRow();
			TableCell tdDetailLabel = new TableCell();
			tdDetailLabel.Controls.Add(DetailLabel);
			trDetailLabel.Cells.Add(tdDetailLabel);
			tableStack.Rows.Add(trDetailLabel);

			TableRow trDetailLabelDescription = new TableRow();
			TableCell tdDetailLabelDescription = new TableCell();
			tdDetailLabelDescription.Controls.Add(DetailLabelDescription);
			trDetailLabelDescription.Cells.Add(tdDetailLabelDescription);
			tableStack.Rows.Add(trDetailLabelDescription);

			TableRow trStackTrace = new TableRow();
			TableCell tdStackTrace = new TableCell();
			tdStackTrace.Controls.Add(StackTrace);
			trStackTrace.Cells.Add(tdStackTrace);
			tableStack.Rows.Add(trStackTrace);

			TableRow trStackTraceDescription = new TableRow();
			TableCell tdStackTraceDescription = new TableCell();
			tdStackTraceDescription.Controls.Add(StackTraceDescription);
			trStackTraceDescription.Cells.Add(tdStackTraceDescription);
			tableStack.Rows.Add(trStackTraceDescription);
	
			Controls.Add(stackPanel);
			
			CloseButton.OnClientClick = "window.close();return false;";
			DetailButton.OnClientClick = string.Format(	"var panel = $get('{0}');" +
														"if (panel == null) return;" +
														"if (panel.style.visibility == 'hidden')" +
														" panel.style.visibility = 'visible';" +
														"else"+
														" panel.style.visibility = 'hidden';" +
														"return false;"
														, stackPanel.ClientID);
			
		}

		//--------------------------------------------------------------------------------
		private string GetDetail(Exception ex)
		{
			StringBuilder sb = new StringBuilder();
			if (ex is IDetailedException)
			{
				string details = ((IDetailedException)ex).Details;
				if (details.Length > 0)
					sb.AppendLine(details);
			}

			if (ex.InnerException != null)
			{
				sb.AppendLine(ex.InnerException.Message);
				string details = GetDetail(ex.InnerException);
				if (details.Length > 0)
					sb.AppendLine(details);
			}
			return sb.ToString();
		}
	}
}
