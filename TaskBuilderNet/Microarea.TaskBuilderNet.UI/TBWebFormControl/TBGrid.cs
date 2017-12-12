using System;
using System.Drawing;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
	/// <summary>
	///  Classe che rappresenta il controllo Web che renderizza l'intero bodyedit (tabella + bottoni status bar + eventuali altri bottoni (tree bodyedit))
	/// </summary>
	//=====================================================================================
	class TBGridContainer : TBPanel
	{
		public int bodyEditBtnSide = 17;
		public int buttonsContainerRightWidth = 120;
		public int rowIndicatorWidth = 67;

		//Layout della statusbar del body edit
		//	________________________________________________________________________________
		// |buttonsContainerLeft|  [scrollerContainer] |  rowIndicator|buttonsContainerRight|
		// |--------------------------------------------------------------------------------|

		Panel buttonsContainerLeft; //Barra inferiore del body edit a sx che contiene tutti i bottoni, tranne quelli indicati nel commento sotto
		Panel buttonsContainerRight; //Barra inferiore del body edit a dx che contiene i bottoni "vai alla 1 riga"  "vai ultima riga" e la stringa "rigacorrente/righe totali"
		Label rowIndicator; //visualizza numero di riga selezionata del bodyedit, e' contenuto nel pannello "buttonsContainerRight"
		Panel scrollerContainer; //contiene i bottoni per avanzare o retrocedere di pagina nel caso di bodyedit paginato
	
		//--------------------------------------------------------------------------------------
		public Panel ButtonsContainerLeft
		{
			get { return buttonsContainerLeft; }
		}

		//--------------------------------------------------------------------------------------
		public Panel ButtonsContainerRight
		{
			get { return buttonsContainerRight; }
		}

		//--------------------------------------------------------------------------------------
		public Panel ScrollerContainer
		{
			get { return scrollerContainer; }
		}

		//--------------------------------------------------------------------------------------
		internal override bool Focusable { get { return false; } }

		//--------------------------------------------------------------------------------------
		public TBGridContainer()
			: base()
		{
		}
		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			buttonsContainerLeft = new Panel();
			buttonsContainerRight = new Panel();
			rowIndicator = new Label();
			scrollerContainer = new Panel(); 
			
			base.OnInit(e);

			buttonsContainerLeft.ID = ID + "btnsContainer";
			buttonsContainerRight.Controls.Add(rowIndicator);
			panel.Controls.Add(buttonsContainerLeft);
			panel.Controls.Add(buttonsContainerRight);
			panel.Controls.Add(scrollerContainer);
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			rowIndicator.Text = ((WndBodyDescription)ControlDescription).RowsIndicator;
			rowIndicator.Width = Unit.Pixel(rowIndicatorWidth);
			rowIndicator.Style[HtmlTextWriterStyle.Position] = "absolute";
			rowIndicator.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(2).ToString();

			//i contenitori dei bottoni della status bar del bodyedit vanno tutti posizionati alla stessa altezza
			int containersTop = Height - bodyEditBtnSide;

			//posiziono il pannello contenitore dei bottoni del body edit in basso(bodyEditButtonHeight pixel altezza del bottone)
			buttonsContainerLeft.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(containersTop).ToString();
			//2 e'l'offset del tbgrid rispetto a gridcontainer (sempre uguale in c++, non modificabile) 
			buttonsContainerLeft.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(2/*bordi*/).ToString();
			buttonsContainerLeft.Style[HtmlTextWriterStyle.Position] = "absolute";
			buttonsContainerLeft.Style[HtmlTextWriterStyle.ZIndex] = (ZIndex + 1).ToString();

			scrollerContainer.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(containersTop).ToString();
			scrollerContainer.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(Width/2).ToString();
			scrollerContainer.Style[HtmlTextWriterStyle.Position] = "absolute";
			scrollerContainer.Style[HtmlTextWriterStyle.ZIndex] = (ZIndex + 1).ToString();

			buttonsContainerRight.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(containersTop).ToString();
			buttonsContainerRight.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(Width + 2/*bordi*/ - buttonsContainerRightWidth).ToString();
			buttonsContainerRight.Style[HtmlTextWriterStyle.Position] = "absolute";
			buttonsContainerRight.Style[HtmlTextWriterStyle.ZIndex] = (ZIndex + 1).ToString();
		}

		//--------------------------------------------------------------------------------------
		private static bool IsBodyEditButton(WndObjDescription d)
		{
			return 
                d.Type == WndObjDescription.WndObjType.BeButton || 
                d.Type == WndObjDescription.WndObjType.BeButtonRight;
		}
		//--------------------------------------------------------------------------------------
		public static bool IsBodyEditButtonRight(WndObjDescription d)
		{
			return d.Type == WndObjDescription.WndObjType.BeButtonRight;
		}

		//--------------------------------------------------------------------------------------
		public override void AddChildControls(WndObjDescription description)
		{
			//memorizzano la posizione dei bottoni all'interno della staus bar sx e dx del body edit
			int posBtnLeft = 0;
			int posBtnRight = 0;

			foreach (WndObjDescription d in description.Childs)
			{
				if (IsBodyEditButton(d))
				{
                    TBWebControl wc = d.CreateControl(this);
					if (wc != null)
					{
						TBBodyEditButton beButton = (TBBodyEditButton)wc;
						if (IsBodyEditButtonRight(d))
						{
							beButton.Position = posBtnRight++;
							buttonsContainerRight.Controls.Add(wc);
						}
						else
						{
							beButton.Position = posBtnLeft++;
							buttonsContainerLeft.Controls.Add(wc);
						}
						addedControls.Add(wc);
						wc.AddChildControls(d);
					}
				}
				else
					AddControl(d);
			}
		}
	}

	/// <summary>
	///  Classe che rappresenta il controllo Web che renderizza la tabella (griglia) del bodyedit
	/// </summary>
	//=====================================================================================
	class TBGridTable : TBScrollPanel
	{
		private int rowHeight = 17;		//inizializzo l'altezza di default a 17, nel caso non ci fossero righe viene usato questo valore per il disegno della griglia vuota
		private int rightColBound = 0;
		private int maxZIndex = 0;
	
		TBWebControl firstContentCell = null;
		TBWebControl lastContentCell = null;
		TBWebControl lastCell = null;
		
		Table gridTable = null;
		TableHeaderRow headerRow = null;
		TableRow trInsertNewRow = null;


		//--------------------------------------------------------------------------------------
		public int RowHeight{ 
								get { return rowHeight; }
								set { rowHeight = value; }
							}
		//--------------------------------------------------------------------------------------
		internal override bool Focusable { get { return false; } }
		//--------------------------------------------------------------------------------------
		public int MaxZIndex { get { return maxZIndex; } }
		//--------------------------------------------------------------------------------------
		public TBGridContainer TableContainer { get { return ParentTBWebControl as TBGridContainer; } }

		//--------------------------------------------------------------------------------------
		public TBGridTable()
		{
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			InitContent();

			base.OnInit(e);
			InnerControl.TabIndex = -1;
		}

		//--------------------------------------------------------------------------------------
		private void InitContent()
		{
			rightColBound = 0;
			firstContentCell = null;
			lastContentCell = null;
			lastCell = null;
		}

		//--------------------------------------------------------------------------------------
		public override void UpdateFromControlDescription(WndObjDescription description)
		{
			InitContent();
			base.UpdateFromControlDescription(description);
		}
		
		//--------------------------------------------------------------------------------------
		public override void AddChildControls(WndObjDescription description)
		{
			headerRow = new TableHeaderRow();
			gridTable = new Table();
			gridTable.CssClass = "TBGridInnerTable";
			gridTable.Rows.Add(headerRow);
			addedControls.Add(gridTable);
			panel.Controls.Add(gridTable);
			
			//creo le righe necessarie per visualizzare tabella bodyedit (n righe)
			for (int i = 0; i < ((WndBodyTableDescription)ControlDescription).Rows; i++)
			{
				TableRow tr = new TableRow();
				gridTable.Rows.Add(tr);
			}
			foreach (WndObjDescription d in description.Childs)
			{
				TBWebControl cell = AddBodyEditTableControl(d);
				maxZIndex = Math.Max(maxZIndex, cell.ZIndex);
				if (cell is TBGridCell)
				{
					if (firstContentCell == null)
						lastCell = cell;
					lastContentCell = cell;
				}
				else
				{
					rightColBound = Math.Max(rightColBound, cell.Width + cell.X);
				}
			}
			//imposto la larghezza della tabella = alla somma della larghezza delle colonne
			//(questo perche' in alcuni bodyedit la somma delle larghezze delle colonne e' minore della larghezza del bodyedit,
			//rimane uno spazio vuoto
			gridTable.Width = rightColBound;
			//metodo che aggiunge i bottoni per scorrere tra le pagine del body edit nel caso di 
			//body edit paginato
			AddPageScrollControls((WndBodyDescription)(TableContainer.ControlDescription));
		
			//Aggiunge la riga sensibile al click(e focus) per l'aggiunta di una nuova riga + le righe vuote
			//di rimempimento della tabella
			AddGridFillerRows();
		}


		///<summary>
		///Metodo che restituisce true se il TbWebControl passato e' una cella del bodyedit 
		///(sia cella del corpo, che cella header di colonna)
		/// </summary>
		//--------------------------------------------------------------------------------------
		private static bool IsGridCell(TBWebControl wc)
		{
			return (wc is TBGridCell || wc is TBGridColTitle);
		}

		///<summary>
		///Metodo che aggiunge le celle del body edit (sono dei DIV) all'interno della tabella html che 
		///visualizza il body edit
		/// </summary>
		//--------------------------------------------------------------------------------------
		private void InsertInTable(TBWebControl wc)
		{
			if (gridTable == null)
				return;

			int nRow = ((WndBodyElementDescription)wc.ControlDescription).Row;
			int nCol = ((WndBodyElementDescription)wc.ControlDescription).Col;
			
			if (nRow == -1) //e' una cella header
			{
				for (int i = headerRow.Cells.Count - 1;  i < nCol; i++) //devo aggiungere le celle perche non ancora creata quella che mi serve
				{
					TableHeaderCell tc = new TableHeaderCell();
					headerRow.Cells.Add(tc);
				}
				TableHeaderCell thcToInsert = (TableHeaderCell)headerRow.Cells[nCol];
				thcToInsert.Controls.Add(wc);
				return;
			}
			// e' una cella del corpo
			nRow++; //incremento di 1 nRow perche' nella tabella htm la riga 0 e' occupata dall'header, nel body edit c++ la riga 0 e' la prima 
					// con i dati
			
			//body edit "paginato", devo normalizzare l'indice delle righe su base 0
			if (((WndBodyDescription)TableContainer.ControlDescription).PrevRows)
				nRow =  nRow - ((WndBodyTableDescription)ControlDescription).WebRowStart;

			TableRow trToInsert = gridTable.Rows[nRow];

			for (int i = trToInsert.Cells.Count - 1; i < nCol; i++) //devo aggiungere le celle perche non ancora creata quella che mi serve
			{
				TableCell tc = new TableCell();
				trToInsert.Cells.Add(tc);
			}
			TableCell tcToInsert = trToInsert.Cells[nCol];
			tcToInsert.Controls.Add(wc);
		}
		
		
		///<summary>
		///Metodo che aggiunge le celle del body edit (sono dei DIV) all'interno della tabella html che 
		///visualizza il body edit, chiamando il metodo InsertInTable. (aggiunge sia celle di header che celle di corpo)
		///Nel caso ci fossero controlli figli che non sono celle le aggiunge direttamente come figli del panel che contiene 
		///la tabella html
		/// </summary>
		//--------------------------------------------------------------------------------------
		private TBWebControl AddBodyEditTableControl(WndObjDescription d)
		{
			TBWebControl wc = d.CreateControl(this);
			if (wc != null)
			{
				if (IsGridCell(wc))
					InsertInTable(wc); //devo posizionarlo nella tabella
				else
					panel.Controls.Add(wc);  //non dovrebbe mai passare di qui: Puo' avere figli che non siano una cella? 
				
				addedControls.Add(wc);
				wc.AddChildControls(d);
			}
			return wc;
		}

		///<summary>
		///Metodo che aggiunge i bottoni per muoversi sulle pagine del body edit. Sono aggiunti
		///nel caso di bodyedit che hanno molte righe( maggiore della costante WEB_PAGE_SIZE definita in
		///\TaskBuilder\Framework\TbGes\BODYEDIT.CPP), e quindi sono paginati
		/// </summary>
		//--------------------------------------------------------------------------------------
		private void AddPageScrollControls(WndBodyDescription wndBodyDescription)
		{
			TableContainer.ScrollerContainer.Controls.Clear();
			if (wndBodyDescription.PrevRows)
			{
				ImageButton prev = new ImageButton();
				prev.ID = "prev" + ID;
				prev.ImageUrl = ImagesHelper.CreateImageAndGetUrl("PrevPage.png", TBWebFormControl.DefaultReferringType);
				prev.ToolTip = TBWebFormControlStrings.PreviousRows;
				prev.CssClass = "TBGridScroller";
				prev.OnClientClick = string.Format("tbMoveBodyPage('{0}', false);", ClientID);
				TableContainer.ScrollerContainer.Controls.Add(prev);
			}

			if (wndBodyDescription.NextRows)
			{
				ImageButton next = new ImageButton();
				next.ID = "next" + ID;
				next.ImageUrl = ImagesHelper.CreateImageAndGetUrl("NextPage.png", TBWebFormControl.DefaultReferringType);
				next.ToolTip = TBWebFormControlStrings.NextRows;
				next.CssClass = "TBGridScroller";
				next.OnClientClick = string.Format("tbMoveBodyPage('{0}', true);", ClientID);
				TableContainer.ScrollerContainer.Controls.Add(next);
			}
		}

		///<summary>
		/// Metodo che aggiunge le righe vuote del body edit, per dargli un aspetto tabellare.
		/// Sulla prima riga vuoto aggiunge l'evento per gestire l'aggiunta della nuova riga sul 
		/// click o quando prende il focus
		/// </summary>
		//--------------------------------------------------------------------------------------
		private void AddGridFillerRows()
		{
			trInsertNewRow = new TableRow();
			int top = 0;
			if (lastCell != null)
				top = lastCell.Y + lastCell.Height;
			int height = Math.Max(Height - top - 24 /*24: space for scrollbar*/ - 2/*border*/, 20);
			
			//Aggiungo la prima riga sensibile al click per aggiungiere nuove righe al body edit e 
			//Aggiungo le righe di riempimento solo nel caso di bodyedit non paginato, oppure sul bodyedit 
			//paginato nell'ultima pagina
			if (!((WndBodyDescription)TableContainer.ControlDescription).NextRows)
			{
				//inserisco tante celle quante sono le colonne
				int nCols = gridTable.Rows[0].Cells.Count;

				for (int i = 0; i < nCols; i++)
				{
					TableCell tc = new TableCell();
					tc.Height = RowHeight;

					TBGridNewRowCell cellNewRow = new TBGridNewRowCell(ClientID, i);
					//imposto la larghezza della cella di riempimento uguale alla larghezza della colonna cui appartiene
					if (gridTable.Rows[0] != null && gridTable.Rows[0].Cells[i] != null && gridTable.Rows[0].Cells[i].Controls[0] != null)
					{
						TBGridColTitle colTitle = gridTable.Rows[0].Cells[i].Controls[0] as TBGridColTitle;
						if (colTitle != null)
						{
							cellNewRow.Width = cellNewRow.cellFocusable.Width = colTitle.ColWidth;
						}
					}	
					tc.Controls.Add(cellNewRow);
					trInsertNewRow.Cells.Add(tc);
				}
				gridTable.Rows.Add(trInsertNewRow);
			
				int nRowToAdd = ((RowHeight != 0) ? height / RowHeight : 15) - 1; //-1 perche' la riga "filler" e' sempre aggiunta
				TableRow dummyRow;
				for (int j = 0; j < nRowToAdd; j++)
				{
					dummyRow = new TableRow();
					for (int i = 0; i < nCols; i++)
					{
						TableCell tc = new TableCell();
						tc.Height = RowHeight;
						dummyRow.Cells.Add(tc);
					}
					gridTable.Rows.Add(dummyRow);
				}
			}
			SetActionHandler();
		}

		//--------------------------------------------------------------------------------------
		public int TableHeight()
		{
			int top = 0;
			if (lastCell != null)
				top = lastCell.Y + lastCell.Height;

			return Math.Max(Height - top - 24 /*24: space for scrollbar*/ - 2/*border*/, 20);
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			SetActionHandler();
			//ATTENZIONE: si allarga di bodyEditBtnSide pixel perche e' lo spazio guadagnato spostando i bottoni laterali in basso
			InnerControl.Width = Unit.Pixel(Width + TableContainer.bodyEditBtnSide); 
		}

		//--------------------------------------------------------------------------------------
		private void SetActionHandler()
		{
			if ( trInsertNewRow== null)
				return;

			trInsertNewRow.Enabled = !(((WndBodyTableDescription)ControlDescription).ReadOnly);
		}
	}

	///<summary>
	/// Classe che rappresenta il controllo che renderizza le cella del body edit che permette l'inserimento di una nuova riga
	/// sul click o sulla presa di fuoco
	/// </summary>
	//==========================================================================================
	class TBGridNewRowCell : Panel
	{
		public TextBox cellFocusable = null;
		
		//--------------------------------------------------------------------------------------
		public TBGridNewRowCell(string ClientID, int i)
		{
			cellFocusable = new TextBox();
			cellFocusable.Attributes["onfocus"] = string.Format("tbNewRow($get('{0}'), {1});", ClientID, i);
			CssClass = "TBGridFiller";
			cellFocusable.CssClass = "TBGridCellText";
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			Controls.Add(cellFocusable);
			base.OnInit(e);
		}

	}

	///<summary>
	/// Classe che rappresenta il controllo che renderizza la cella header di una colonna del body edit 
	/// </summary>
	//==========================================================================================
	class TBGridColTitle : TBPanel
	{
		Label label;
		Panel resizeCol;
		Unit colWidth; //larghezza effettiva a video della colonna (tenendo conto del bordo)

		//--------------------------------------------------------------------------------------
		public Unit ColWidth { get { return colWidth; } }
		//--------------------------------------------------------------------------------------
		public TBGridTable Table { get { return parentTBWebControl as TBGridTable; } }
		//--------------------------------------------------------------------------------------
		protected override short BorderWidth { get { return 1; } }

		//--------------------------------------------------------------------------------------
		public override WebControl InnerControl
		{
			get	{return label;}
		}
		
		//--------------------------------------------------------------------------------------
		public TBGridColTitle()
		{
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			label = new Label();
			resizeCol = new Panel();
			base.OnInit(e);
			label.TabIndex = -1;
			panel.Controls.Add(label);
			panel.Controls.Add(resizeCol);
			resizeCol.Attributes["onmousedown"] = string.Format("dragTbGridResizeCol(event, '{0}', '{1}', '{2}', {3})", InnerControl.ClientID, Table.TableHeight(), Table.ClientID, Table.Width);	
		}


		//devo assegnare solo larghezza e altezza, la posizione sara relativa (0,0) all'interno della cella (td)
		//--------------------------------------------------------------------------------------
		protected override void SetControlPosition()
		{
			InnerControl.Style[HtmlTextWriterStyle.Position] = "relative";
			int width =  Math.Max(0,Width - 2 * BorderWidth);
			colWidth = width;
			InnerControl.Width = Unit.Pixel(width);
            InnerControl.Height = Unit.Pixel(Math.Max(0,Height - 2 * BorderWidth));
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			//lato c++ hanno simulato una colonna nascosta del body edit mettendo larghezza zero
			// allora la nascondo (es. in Balances/Reclassifications/Reclassification Schemas)
			if ((Width - 2 * BorderWidth) <= 0)
				this.Visible = false;
			label.Text = ControlDescription.HtmlTextAttribute;
			//imposto il tooltip, utile per le colonne strette in cui il testo non
			//e' completamente visibile
			label.ToolTip = ControlDescription.HtmlTextAttribute;
			resizeCol.Height = InnerControl.Height;
			resizeCol.Width = Unit.Pixel(4);
			resizeCol.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(Width - 4).ToString();
			resizeCol.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(0).ToString();
			resizeCol.Style[HtmlTextWriterStyle.Position] = "absolute";
			resizeCol.Style["z-index"] = (ZIndex + 1).ToString();
			resizeCol.Style[HtmlTextWriterStyle.Cursor] = "w-resize";
		}

		//--------------------------------------------------------------------------------------
		protected override void Render(HtmlTextWriter writer)
		{
			//Assegno la dimensione del div che rappresenta l'updatepanel di TbGridcell per far si che la cella di tabella che contiene la cella si adatti al contenuto
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Position, "relative");
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Width, label.Width.ToString());
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Height, label.Height.ToString());
			base.Render(writer);
		}
	}

	///<summary>
	/// Classe che rappresenta il controllo che renderizza la singola cella del body edit 
	/// </summary>
	//==========================================================================================
	class TBGridCell : TBPanel
	{
		protected TextBox textBox = null;
		//--------------------------------------------------------------------------------------
		protected override string InitFunctionName { get { return "alignParentZIndex"; } }
		//--------------------------------------------------------------------------------------
		public TBGridContainer TableContainer { get { return Table.TableContainer; } }
		//--------------------------------------------------------------------------------------
		public TBGridTable Table { get { return parentTBWebControl as TBGridTable; } }
		//--------------------------------------------------------------------------------------
		public override int ZIndex { get { return ActiveCell ? Table.MaxZIndex + 20 : base.ZIndex; } }
		//--------------------------------------------------------------------------------------
		public bool ActiveCell { get { return ((WndBodyElementDescription)ControlDescription).ActiveCell; } }
		//--------------------------------------------------------------------------------------
		public TBGridCell()
		{
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			textBox = new TextBox();
			panel.Controls.Add(textBox);
			
			base.OnInit(e);
			
			textBox.ID = this.ID + "_txt";
			formControl.RegisterControl(textBox, this);
		
			RegisterInitControlScript();
		}

		//devo assegnare solo larghezza e altezza, la posizione sara relativa (0,0) all'interno della cella (td)
		//--------------------------------------------------------------------------------------
		protected override void SetControlPosition()
		{
			InnerControl.Style[HtmlTextWriterStyle.Position] = "relative";
			Table.RowHeight = Height;
			//Width e Height non possono essere negative, nel caso di valore negativo le imposto a 0
			InnerControl.Width = Unit.Pixel(Math.Max(0, Width - 2 * BorderWidth));
            InnerControl.Height = Unit.Pixel(Math.Max(0, Table.RowHeight - 2 * BorderWidth));
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			
			WndBodyElementDescription description = (WndBodyElementDescription)ControlDescription;
			if (description.Childs.Count > 0)
			{
				textBox.Visible = false;
				return;
			}

			textBox.Visible = true;

			StringBuilder sb = new StringBuilder();

			sb.Append("TBGridCellText ");
			
			if (IsEnabled)
			{
				textBox.ReadOnly = false;
				textBox.TabIndex = 0; 
				textBox.Attributes["onclick"] = "";
				textBox.Attributes["onfocus"] = string.Format("tbMove(this, '{0}')", this.ClientID);
			}
			else
			{
				panel.Enabled = true;
				textBox.ReadOnly = true;
				textBox.TabIndex = -1;
				textBox.Attributes["onfocus"] = "";
				sb.Append("Disabled ");
				if (description.IsHyperLink && !string.IsNullOrEmpty(description.HtmlTextAttribute))
				{
					textBox.Attributes["onclick"] = string.Format("DoHyperLink('{0}')", ClientID);
					sb.Append(" HyperLink ");
				}
				else
					textBox.Attributes["onclick"] = string.Format("SelectRow('{0}')", ClientID);
			}

			if (!description.IsCheckBox)
			{
				textBox.ToolTip = textBox.Text = description.HtmlTextAttribute;
			}
			else
			{
				//Se e' flaggata la checkbox(CheckBoxValue = true), metto l'immagine del flag come sfondo
				if (description.CheckBoxValue)
				{
					textBox.Style.Add(HtmlTextWriterStyle.BackgroundImage, ImagesHelper.CreateImageAndGetUrl("CheckMark.png", TBWebFormControl.DefaultReferringType));
					sb.Append(" Check");
				}
				else
				{
					textBox.Style.Remove(HtmlTextWriterStyle.BackgroundImage);
				}
			}

			textBox.Width = Unit.Pixel(Width);
			textBox.Height = Unit.Pixel(Height - 2);
			textBox.Style[HtmlTextWriterStyle.Left] = "0px";
			textBox.Style[HtmlTextWriterStyle.Top] = "0px";
			if (description.AlignCenter)
				textBox.Style[HtmlTextWriterStyle.TextAlign] = "center";
			else if (description.AlignRight)
				textBox.Style[HtmlTextWriterStyle.TextAlign] = "right";


			//Se sono la riga attiva (selezionata) imposto il colore azzurro come sfondo
			//(che indica la selezione come in Mago GDI)
			if (description.ActiveRow)
			{
				textBox.Style[HtmlTextWriterStyle.BackgroundColor] = "#99CCFF";
				textBox.Style[HtmlTextWriterStyle.Color] = "#000000";
			}
			else
			{
				//Se la riga non e' attiva rimuovo i colori assegnati da codice
				//(ereditera eventuali colorazioni definite nel css, esempio il blu per il 
				//testo degli hyperlink)
				textBox.Style.Remove(HtmlTextWriterStyle.BackgroundColor);
				textBox.Style.Remove(HtmlTextWriterStyle.Color);
				//Nel caso il body edit abbia una personalizzazione di colore, le imposto
				if (description.BackgroundColor != Color.Empty)
					textBox.BackColor = description.BackgroundColor;
				if (description.TextColor != Color.Empty)
					textBox.ForeColor = description.TextColor;
			}

			textBox.CssClass = sb.ToString();

			textBox.Style["z-index"] = ZIndex.ToString();	
		}

		//--------------------------------------------------------------------------------------
		protected override void Render(HtmlTextWriter writer)
		{
			//Assegno la dimensione del div che rappresenta l'updatepanel di TbGridcell per far si che la cella di tabella che contiene la cella si adatti al contenuto
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Position, "relative");
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Width, Unit.Pixel(Width).ToString());
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Height, Unit.Pixel(Table.RowHeight).ToString());
			base.Render(writer);
		}
	}
	
	///<summary>
	/// Classe che rappresenta il controllo che renderizza la cella del body edit ad albero
	/// Se e' un nodo con figli ha i tasti per fare expand/collapse.
	/// </summary>
	//==========================================================================================
	class TBGridTreeCell : TBGridCell
	{
		ImageButton toggleNode;

		protected override void OnInit(EventArgs e)
		{
			toggleNode = new ImageButton();
			toggleNode.ID = "toggle" + ClientID;
			panel.Controls.Add(toggleNode);
			base.OnInit(e);
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();

			WndBodyTreeElementDescription description = (WndBodyTreeElementDescription)ControlDescription;
	
			if (description.HasChild)
			{
				//Metodo che riposiziona il testo e l'immagine per fare collapse/expand del nodo
				//in base al livello in cui si trova il nodo
				RepositionControls();

				//associo la funzione js sul click che fara partire l'azione di expand/collapse del nodo
				toggleNode.OnClientClick = string.Format("ToggleExpandNode('{0}')", ClientID);
				
				if (description.IsExpanded)
					toggleNode.ImageUrl = ImagesHelper.CreateImageAndGetUrl("CollapsedNodeIcon.png", TBWebFormControl.DefaultReferringType);
				else
					toggleNode.ImageUrl = ImagesHelper.CreateImageAndGetUrl("ExpandedNodeIcon.png", TBWebFormControl.DefaultReferringType);
			}
			toggleNode.Visible = description.HasChild;
		}

		///<summary>
		///Metodo che riposiziona il testo e l'immagine per fare collapse/expand del nodo
		///in base al livello in cui si trova il nodo
		/// </summary>
		//--------------------------------------------------------------------------------------
		private void RepositionControls()
		{
			//lato del quadrato del bottone expand/collapse nodo
			int toggleNodeSide = 11;
			//spaziatura tra icona e testo
			int spacing = 2;

			toggleNode.Height = Unit.Pixel(toggleNodeSide);
			toggleNode.Width = Unit.Pixel(toggleNodeSide);
			
			//x a cui posizionare l'icona di collapse/expand, dipende dal livello del nodo
			//(sottraggo 1 in modo che i nodi di primo livello non subiscano spostamento)
			int toggleNodeXOffset = (((WndBodyTreeElementDescription)ControlDescription).Level - 1) * (toggleNodeSide + spacing);
			toggleNode.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(toggleNodeXOffset).ToString();

			//x a cui posizionare l'icona di collapse/expand, dipende dal livell odel nodo
			int toggleNodeYOffset = (int)((Height - toggleNodeSide) / 2);
			toggleNode.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(toggleNodeYOffset).ToString();
			
			toggleNode.Style[HtmlTextWriterStyle.Position] = "absolute";
			toggleNode.Style["z-index"] = ZIndex.ToString();
			toggleNode.Style[HtmlTextWriterStyle.Cursor] = "hand";

			//sposto la textbox a destra dello spazio usato per visualizzare il bottone expand/collapse nodo
			//e la restringo
			int textXOffset = toggleNodeXOffset + toggleNodeSide + spacing; 
			textBox.Style[HtmlTextWriterStyle.Position] = "relative";
			textBox.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(textXOffset).ToString();
			textBox.Width = Unit.Pixel(Width - textXOffset);
		}
	}


	///<summary>
	/// Classe utilizzata per renderizzare un bottone del body edit (type = BEbutton)
	/// I bottoni del body edit nella visualizzazione Web devono essere posizionati in basso 
	/// sotto la "griglia" del body edit e non si deve quindi tenere conto 
	/// della loro posizione nel bodyedit C++ renderizzato sul server TaskBuilder
	/// </summary>

	//==========================================================================================
	class TBBodyEditButton : TBButton
	{
		//memorizza la posizione del bottone all'interno della status bar cui appartiene (sx e dx del body edit)
		int position;
		//--------------------------------------------------------------------------------------
		public int Position
		{
			get { return position; }
			set { position = value; }
		}

		//i bottoni sotto la griglia del body edit sono immagini ma non devono allargarsi sul mouseover
		//per evitare fenomeni di sfarfallio dovuto a comparsa di scrollbar
		//--------------------------------------------------------------------------------------
		protected override ButtonType GetButtonType
		{
			get { return ButtonType.ImageButton; }
		}

		//Restituisce il Controllo web che rappresenta il Body edit che contiene questo bottone
		//--------------------------------------------------------------------------------------
		public TBGridContainer TableContainer { get { return ParentTBWebControl as TBGridContainer; } }

		//--------------------------------------------------------------------------------------
		protected override void SetControlPosition()
		{
			//I bottoni di sinistra sono posizionati in un contenitore (il Panel "buttonsContainerLeft") quelli di destra in un altro
			//(Panel "buttonsContainerRight").Qui i contenitori sono gia' posizionati correttamente, devo posizionare solo i bottoni in modo 
			//relativo rispetto ai contenitori. Il loro top quindi sara' 0.
			//Il left invece va calcolato tenendo conto della Position(intesa come indice) del bottone

			//posizionamento del bottone all'interno del suo contenitore (status bar(pannello) destro o sinistro del bodyedit) in base alla posizione
			InnerControl.Style[HtmlTextWriterStyle.Left] = 
                (TBGridContainer.IsBodyEditButtonRight(ControlDescription))
						? Unit.Pixel(TableContainer.rowIndicatorWidth + Position * TableContainer.bodyEditBtnSide + TableContainer.bodyEditBtnSide).ToString()
						: Unit.Pixel(Position * TableContainer.bodyEditBtnSide).ToString();
			InnerControl.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(0).ToString();
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();

			InnerControl.CssClass += " NotFocusable";
			InnerControl.TabIndex = -1;
		}
	}
}
