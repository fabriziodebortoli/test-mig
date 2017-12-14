using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microarea.TaskBuilderNet.Core.XmlPersister;

namespace Microarea.TaskBuilderNet.Core.Generic
{

	//	 Form che appare con le sembianze di un menu di contesto, contine una lista con dei range di date 
	//	e permette di selezionare un range tra quelli proposti
	//==============================================================================
	public partial class DateRangeSelector : PopupContainer
	{
		//array delle finestre cliccabili come controlli estremi del range
		private ArrayList wrappedWindows = new ArrayList();
		//data a cui fanno riferimento i ranges
		private DateTime anchorDate = new DateTime();
		//array dei ranges letti da file
		DateRangeList ranges = new DateRangeList();

		//-----------------------------------------------------------------------------
		public DateRangeSelector()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		//-----------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		///<summary>
		///Metodo che imposta la location del control, tenendo conto anche dello spazio
		///disponibile sul monitor per far si che non esca dall'area dello schermo.
		/// </summary>
		//-----------------------------------------------------------------------------
		protected override void SetLocation(IntPtr parsedCtrlHandle)
		{
			//Imposto la posizione della form in modo che non copra i controlli coinvolti nel range(euristica)
			ExternalAPI.Rect parsedCtrlRect = new ExternalAPI.Rect(0, 0, 0, 0);
			ExternalAPI.GetWindowRect(parsedCtrlHandle, ref parsedCtrlRect);
			Location = new Point(parsedCtrlRect.right + 30, parsedCtrlRect.bottom);

			//Trovo lo screen in cui si trova il controllo 
			Screen currentScreen = Screen.FromHandle(parsedCtrlHandle);
			Rectangle rcWorkArea = currentScreen.WorkingArea;

			//se il calendario esce dal monitor a destra, lo mostro sulla sinistra
			if (Right > rcWorkArea.Right)
			{
				Location = new Point(parsedCtrlRect.left - Width - 5, Location.Y);
			}
			//se il calendario esce dal monitor in basso, lo mostro sopra
			if (Bottom > rcWorkArea.Bottom)
			{
				Location = new Point(Location.X, parsedCtrlRect.top - Height);
			}
		}

		//Inizializza e mostra la form
		//-----------------------------------------------------------------------------
		public IntPtr ShowRangeSelector(IntPtr parentHandle, IntPtr clickedCtrlHandle, DateTime applicationDate, string filePath)
		{
			listViewOption.Visible = true;
			this.anchorDate = applicationDate;
			//carica i Ranges hard-coded e legge eventuali altri ranges da file e aggiunge voci opzionali
			PopulateListOption(filePath);
			//Trova la finestra che si suppone sia il secondo  estremo del range e popola l'array di 
			//finestre compatibili come possibile estremo del range
			FindNearestWindow(clickedCtrlHandle);

			//visualizza la finestra
			ShowPopupContainer(parentHandle, clickedCtrlHandle);
			
			listViewOption.Focus();
			return this.Handle;
		}


		//Manda i messaggi al parsed control associato al calendario per comunicare l'avvenuta chiusura della form contentente il 
		//calendario (in questo modo lato c++ pulisce il puntatore)
		//-----------------------------------------------------------------------------
		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
		
			IntPtr WindowFromHandle = GetWindowWrapperFromState(WrapperWindowState.WindowFrom);
			IntPtr WindowToHandle = GetWindowWrapperFromState(WrapperWindowState.WindowTo);

			//forzo il ridisegno delle finestre
			InvalidateWindowRect(WindowToHandle);
			InvalidateWindowRect(WindowFromHandle);

			//detach della finestra originale dal wrapper
			foreach (WindowWrapper wndWrapper in wrappedWindows)
			{
				ExternalAPI.SendMessage(wndWrapper.Handle, ExternalAPI.UM_RANGE_SELECTOR_CLOSED, dataSelected ? (IntPtr)1 : IntPtr.Zero, IntPtr.Zero);
				//sul parsed control secondo estremo dell'intervallo imposto il fuoco
				if (wndWrapper.State == WrapperWindowState.WindowFrom)
					ExternalAPI.SetFocus(wndWrapper.Handle);
				
				wndWrapper.ReleaseHandle();
			}
		}

		//Invia i messaggi ai control selezionati come estremi del range per avvertirli che e' stata
		//effettuata la selezione
		//-----------------------------------------------------------------------------
		private void SetDatesRange(DateTime from, DateTime to, bool selection)
		{
			/*Nota: la delete della memoria allocata per la struttura che contien le data viene fatta dal metodo callbackMethod
			 se la sendMessage va a buon fine*/
			//mando il messaggio alla finestre che deve valorizzarsi con il valore "FROM" del range passando come
			//parametro il puntatore alla struttura che contiene il valore
			DateSelection dateFrom = new DateSelection((Int16)from.Day, (Int16)from.Month, (Int16)from.Year);
			IntPtr ptrFrom = Marshal.AllocCoTaskMem(Marshal.SizeOf(dateFrom));
			Marshal.StructureToPtr(dateFrom, ptrFrom, true);
			ExternalAPI.PostMessage(GetWindowWrapperFromState(WrapperWindowState.WindowFrom), ExternalAPI.UM_RANGE_SELECTOR_SELECTED, ptrFrom, selection ? (IntPtr)1 : IntPtr.Zero);
			//mando il messaggio alla finestre che deve valorizzarsi con il valore "TO" del range passando come
			//parametro il puntatore alla struttura che contiene il valore
			DateSelection dateTo = new DateSelection((Int16)to.Day, (Int16)to.Month, (Int16)to.Year);
			IntPtr ptrTo = Marshal.AllocCoTaskMem(Marshal.SizeOf(dateTo));
			Marshal.StructureToPtr(dateTo, ptrTo, true);
			ExternalAPI.PostMessage(GetWindowWrapperFromState(WrapperWindowState.WindowTo), ExternalAPI.UM_RANGE_SELECTOR_SELECTED, ptrTo, selection ? (IntPtr)1 : IntPtr.Zero);
		}


		//Trova la finestra dello stesso tipo del control cliccato piu' vicina al control stesso
		//-----------------------------------------------------------------------------
		private void FindNearestWindow(IntPtr clickedCtrlHandle)
		{
			ExternalAPI.Rect rectNearest = new ExternalAPI.Rect(0, 0, 0, 0);
			ExternalAPI.Rect rect = new ExternalAPI.Rect(0, 0, 0, 0);
			ExternalAPI.Rect parsedCtrlRect = new ExternalAPI.Rect(0, 0, 0, 0);

			ExternalAPI.GetWindowRect(clickedCtrlHandle, ref parsedCtrlRect);


			//wrappo la finestra cliccata  e la inserisco nell'array delle finestre wrappate per cambiarne
			//il colore di sfondo
			WindowWrapper wrapperFrom = new WindowWrapper(clickedCtrlHandle, WrapperWindowState.WindowFrom);
			wrappedWindows.Add(wrapperFrom);

			IntPtr pWnd = ExternalAPI.GetWindow(clickedCtrlHandle, ExternalAPI.GW_HWNDFIRST);
			IntPtr toCtrlHandle = IntPtr.Zero;
			//ciclo sulle finestre
			while (pWnd != IntPtr.Zero)
			{
				//se sono la finestra cliccata, salto alla prossima
				if (clickedCtrlHandle == pWnd)
				{
					pWnd = ExternalAPI.GetWindow(pWnd, ExternalAPI.GW_HWNDNEXT);
					continue;
				}
				//TODO fare enumerativo
				if (ExternalAPI.SendMessage(pWnd, ExternalAPI.UM_GET_PARSEDCTRL_TYPE, IntPtr.Zero, IntPtr.Zero) == (IntPtr)1
					&&
					ExternalAPI.IsWindowVisible(pWnd)
					&&
					ExternalAPI.IsWindowEnabled(pWnd)
					)
				{
					//wrappo la finestra visibile di tipo datadate e la inserisco nell'array delle finestre wrappate per intercettarne il click
					WindowWrapper wrapper = new WindowWrapper(pWnd, WrapperWindowState.None);
					wrappedWindows.Add(wrapper);

					ExternalAPI.GetWindowRect(pWnd, ref rect);

					if (toCtrlHandle == IntPtr.Zero)
					{
						toCtrlHandle = pWnd;
						ExternalAPI.GetWindowRect(toCtrlHandle, ref rectNearest);
					}
					else
					{
						if (IsNearest(rectNearest, rect, parsedCtrlRect))
						{
							toCtrlHandle = pWnd;
							rectNearest = rect;
						}
					}
				}
				pWnd = ExternalAPI.GetWindow(pWnd, ExternalAPI.GW_HWNDNEXT); ;
			}

			SetWindowWrapperState(toCtrlHandle, WrapperWindowState.WindowTo);

			//forzo il ridisegno della finestra from e to
			InvalidateWindowRect(clickedCtrlHandle);
			InvalidateWindowRect(toCtrlHandle);
		}


		//Metodo che forza il ridisegno della finestra associata all'handle passato come parametro
		//-----------------------------------------------------------------------------
		private bool InvalidateWindowRect(IntPtr Handle)
		{
			return ExternalAPI.InvalidateRect(Handle, IntPtr.Zero, true);
		}

		//Metodo che dice se il rettangolo rectCandidate e' piu vicino al rettangolo rectCtrl rispetto al 
		//rettangolo rectNearest
		//-----------------------------------------------------------------------------
		bool IsNearest(ExternalAPI.Rect rectNearest, ExternalAPI.Rect rectCandidate, ExternalAPI.Rect rectCtrl)
		{
			if (Math.Abs(rectCtrl.top - rectNearest.top) > Math.Abs(rectCtrl.top - rectCandidate.top))
				return true;

			return false;
		}

		//Imposta lo stato del wrapper di finestra se presente nell'array
		//-----------------------------------------------------------------------------
		void SetWindowWrapperState(IntPtr handle, WrapperWindowState state)
		{
			foreach (WindowWrapper windowWrapper in wrappedWindows)
			{
				if (windowWrapper.Handle == handle)
				{
					windowWrapper.State = state;
					return;
				}
			}
		}

		//Restituisce lo stato del wrapper di finestra se presente nell'array
		//-----------------------------------------------------------------------------
		WrapperWindowState GetWindowWrapperState(IntPtr handle)
		{
			foreach (WindowWrapper windowWrapper in wrappedWindows)
			{
				if (windowWrapper.Handle == handle)
					return windowWrapper.State;
			}
			return WrapperWindowState.None;
		}

		// Ritorna l'handle della finestra che e' nello stato passato come parametro
		//-----------------------------------------------------------------------------
		IntPtr GetWindowWrapperFromState(WrapperWindowState state)
		{
			if (state == WrapperWindowState.None)
				return IntPtr.Zero;

			foreach (WindowWrapper windowWrapper in wrappedWindows)
			{
				if (windowWrapper.State == state)
					return windowWrapper.Handle;
			}
			return IntPtr.Zero;
		}

		// Ritorna il wrapper della finestra con handle uguale a quello passato come parametro
		//-----------------------------------------------------------------------------
		WindowWrapper GetWindowWrapperFromHandle(IntPtr handle)
		{
			foreach (WindowWrapper windowWrapper in wrappedWindows)
			{
				if (windowWrapper.Handle == handle)
					return windowWrapper;
			}
			return null;
		}

		//Inverte gli estremi del range, FROM diventa TO e viceversa
		//-----------------------------------------------------------------------------
		private void InvertSelection()
		{
			foreach (WindowWrapper windowWrapper in wrappedWindows)
			{
				if (windowWrapper.State == WrapperWindowState.WindowFrom)
				{
					windowWrapper.State = WrapperWindowState.WindowTo;
					InvalidateWindowRect(windowWrapper.Handle);
					continue;
				}

				if (windowWrapper.State == WrapperWindowState.WindowTo)
				{
					windowWrapper.State = WrapperWindowState.WindowFrom;
					InvalidateWindowRect(windowWrapper.Handle);
					continue;
				}
			}
		}

		//Popola la listBox con i range di date
		//-----------------------------------------------------------------------------
		private void PopulateListOption(string filePath)
		{
			//Carica i ranges hard-coded
			ranges.LoadDefaultRanges();

			//carica i ranges da file
			ranges.ReadFromXmlFile(filePath);

			ListViewItem listViewItem = null;
			foreach (DateRange dr in ranges)
			{
				listViewItem = listViewOption.Items.Add(dr.description);
				listViewItem.Tag = dr;
			}

			//	TODO scommentare quando funziona editor di range
			//	listViewItem = listViewOption.Items.Add("<Add New Range>");
			//	listViewItem.Tag = null;
			listViewOption.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		//Metodo che visualizza l'anteprima del range selezionato nei parsed control
		//selzionati come estremi del range
		//-----------------------------------------------------------------------------
		private void SetPreviewDateRange()
		{
			DateRange dr = GetListViewSelectedDateRange();
			if (dr != null)
				SetDatesRange(dr.From(anchorDate), dr.To(anchorDate), false);
		}

		//Metodo che sul doppio click o sull'invio sulla listView esegue la selezione delle date e chiude la finestra
		//-----------------------------------------------------------------------------
		public void SetDateRange()
		{
			DateRange dr = GetListViewSelectedDateRange();
			if (dr != null)
			{
				dataSelected = true;
				SetDatesRange(dr.From(anchorDate), dr.To(anchorDate), true);
			}

			Close();
		}

		//Metodo che ritorna il DateRange selezionato
		//-----------------------------------------------------------------------------
		private DateRange GetListViewSelectedDateRange()
		{
			//multiselect = false, solo uno possibile nella selezione
			ListViewItem lvi = listViewOption.SelectedItems.Count == 1 ? listViewOption.SelectedItems[0] : null;
			if (lvi == null)
				return null;

			return lvi.Tag as DateRange;
		}


		//Sulla disattivazione devo chiudere la form, a meno che non abbia cliccato su un controllo
		//per cambiare i controlli coinvolti nel range, oppure sulla listview per selezionare il Range
		//-----------------------------------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
			//catturo il mouse per chiudere la form su un click esterno
			Capture = true;

			base.WndProc(ref m);

			//tasto destro o centrale del mouse fanno chiudere la form
			if (m.Msg == ExternalAPI.WM_MBUTTONDOWN || m.Msg == ExternalAPI.WM_RBUTTONDOWN)
				this.Close();

			if (m.Msg == ExternalAPI.WM_LBUTTONDOWN || m.Msg == ExternalAPI.WM_LBUTTONDBLCLK)
			{
				//se la finestra che si sta cliccando non e' un controllo di quelli selezionabili come
				//estremo del range, chiudo la form
				ExternalAPI.POINT pt;
				pt.x = Cursor.Position.X;
				pt.y = Cursor.Position.Y;

				//Ottengo il wrapper della finestra date le coordinate del click
				IntPtr clickedWnd = ExternalAPI.WindowFromPoint(pt);
				WindowWrapper wrapperWnd = GetWindowWrapperFromHandle(clickedWnd);
				//ottengo il controllo cliccato
				Control c = Control.FromHandle(clickedWnd);

				if (m.Msg == ExternalAPI.WM_LBUTTONDOWN)
				{
					if (wrapperWnd != null)
					{
						//Se e' stata cliccata una finestra selezionabile come estremo del range, aggiorno
						//il range
						UpdateRangeSelection(wrapperWnd);
						return;
					}
					if (clickedWnd != Handle && !this.Contains(c))
					{
						//Se ho cliccato al di fuori di questa form e non su un controllo selezionabile, chiudo la form
						this.Close();
					}
				}
				//se ho fatto doppio click sulla listview, chiamo il metodo che gestisce il doppio click sulla listview
				if (c == listViewOption && m.Msg == ExternalAPI.WM_LBUTTONDBLCLK)
					SetDateRange();
				//propago il click singolo alla listview, in modo che possa internamente aggiornare lo stato di item selezionato
				if (c == listViewOption && m.Msg == ExternalAPI.WM_LBUTTONDOWN)
					ExternalAPI.SendMessage(listViewOption.Handle, m.Msg, m.WParam, m.LParam);
			}
		}

		//Metodo che dato il wrapper della finestra cliccata aggiorna lo stato del range nel'array delle finestre wrapped
		//-----------------------------------------------------------------------------------------
		private void UpdateRangeSelection(WindowWrapper clickedWrapperWnd)
		{
			//se e' stato cliccato in controllo FROM, faccio scattare l'evento per invertire la selezione
			if (clickedWrapperWnd.State == WrapperWindowState.WindowFrom)
			{
				InvertSelection();
				return;
			}
			//se e' stato cliccato un altro controllo, lo imposto come controllo TO
			//e faccio scattare l'evento per resettare lo stato del controllo TO precedente
			if (clickedWrapperWnd.State == WrapperWindowState.None)
			{
				clickedWrapperWnd.State = WrapperWindowState.WindowTo;

				ChangeToRange(clickedWrapperWnd.Handle);

				ExternalAPI.InvalidateRect(clickedWrapperWnd.Handle, IntPtr.Zero, true);
			}
		}

		//Quando cambio la finestra di destinazione con il click del mouse, resetto lo stato della vecchia
		//finestra di destinazione e ne forzo il ridisegno
		//-----------------------------------------------------------------------------
		void ChangeToRange(IntPtr handle)
		{
			foreach (WindowWrapper w in wrappedWindows)
			{
				if (w.State == WrapperWindowState.WindowTo && w.Handle != handle)
				{
					w.State = WrapperWindowState.None;
					//avverto il parsed control che non e' piu' un estremo del range, quindi se aveva un eventuale valore impostato in anteprima
					//lo perdera' e lo reimpostera' con il suo originale
					ExternalAPI.SendMessage(w.Handle, ExternalAPI.UM_RANGE_SELECTOR_CLOSED, dataSelected ? (IntPtr)1 : IntPtr.Zero, IntPtr.Zero);
					InvalidateWindowRect(w.Handle);
					return;
				}
			}
		}

		///<summary>
		///Metodo che intercetta la pressione del tasto invio sulla listview:
		///e seleziona in modo definitivo il range di date
		/// </summary>
		//-----------------------------------------------------------------------------
		private void listViewOption_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyValue == (int)Keys.Return)
				SetDateRange();
		}
		///<summary>
		//Metodo che sulla selezione di un elemento della listView visualizza l'anteprima del range selezionato
		/// </summary>
		//-----------------------------------------------------------------------------
		private void listViewOption_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetPreviewDateRange();
		}
	}

	//Classe che wrappa una finestra dato un handle, per intercettarne i messaggi 
	//==============================================================================
	public class WindowWrapper : NativeWindow, IDisposable
	{
		private const float selectionLineThick = 3;		//spessore della linea disegnata per evidenziare il parsed control
		private const int borderThick = ((int)selectionLineThick - 2); //spessore  da lasciare nel disegno della linea in basso e a sinistra
		
		private Rectangle rectangle;					//rettangolo della client area della finestra wrappata
		private WrapperWindowState state;				//stato della finestra wrappata
		private Pen bluePen;
		private Pen redPen;
		
		//-----------------------------------------------------------------------------
		internal WrapperWindowState State
		{
			get { return state; }
			set { state = value; }
		}

		//-----------------------------------------------------------------------------
		public WindowWrapper()
		{
			//Costruttore vuoto, per favore non cancellare
		}

		//-----------------------------------------------------------------------------
		public WindowWrapper(IntPtr WindowHandle, WrapperWindowState State)
		{
			//inizializzo i Pen
			bluePen = new Pen(Color.Blue, selectionLineThick);
			redPen = new Pen(Color.Red, selectionLineThick);
			
			state = State;
			ExternalAPI.Rect rect = new ExternalAPI.Rect(0, 0, 0, 0);
			ExternalAPI.GetClientRect(WindowHandle, ref rect);
			rectangle = new Rectangle(0, 0, rect.right - rect.left - borderThick, rect.bottom - rect.top - borderThick);
			AssignHandle(WindowHandle);
		}

		//-----------------------------------------------------------------------------
		public void Dispose()
		{
			ReleaseHandle();

			bluePen.Dispose();
			redPen.Dispose();
		}

		//-----------------------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
			//per i controlli selezionati come FROM e TO effettuo delel operazioni grafiche per evidenziarlo 
			if (m.Msg == ExternalAPI.WM_PAINT)
			{
				base.WndProc(ref m);
				if (state != WrapperWindowState.None)
				{
					using (Graphics g = Graphics.FromHwnd(Handle))
					{
						Color customColor = Color.Transparent;

						switch (state)
						{
							case WrapperWindowState.WindowFrom:
								{
									g.DrawRectangle(bluePen, rectangle);
									break;
								}
							case WrapperWindowState.WindowTo:
								{
									g.DrawRectangle(redPen, rectangle);
									break;
								}
						}
					}
				}
				return;
			}
			base.WndProc(ref m);
		}
	}


	//Classe che rappresenta un range di date, oltre alle 2 date limite ha un campo descrizione
	//==============================================================================
	[Serializable]
	public class DateRange: State
	{
		[XmlAttribute]
		public string name = "";
		[XmlAttribute]
		public string description = "";
		[XmlElement(ElementName = "Period")]
		public Period period;
		[XmlElement(ElementName = "Type")]
		public PeriodType type;
		[XmlElement(ElementName = "Offset")]
		public int offset = 0;
		
		
		//-----------------------------------------------------------------------------
		public DateTime From(DateTime anchorDate)
		{
			DateTime from = anchorDate;
			switch (type)
			{
				case PeriodType.From:
					{
						from = anchorDate;
						break;
					}
				case PeriodType.To:
					{
						from = SubtractPeriodFromDate(anchorDate);
						break;
					}
				case PeriodType.Contained:
					{
						from = PeriodFrom(anchorDate);
						break;
					}
			}

			if (offset != 0)
				from = ApplyOffsetFrom(from);

			return from;
		}

		//-----------------------------------------------------------------------------
		private DateTime ApplyOffsetFrom(DateTime date)
		{
			DateTime newDate;
			switch (period)
			{
				case Period.Day:
					{
						return new DateTime(date.Year, date.Month, date.Subtract(new TimeSpan(offset, 0, 0, 0)).Day);
					}
				case Period.Month:
					{
						newDate = date.AddMonths(offset);
						return new DateTime(newDate.Year, newDate.Month, newDate.Day);
					}
				case Period.PartialMonth:
					break;
				case Period.Quarter:
					break;
				case Period.Year:
					{
						newDate = date.AddYears(offset);
						return new DateTime(newDate.Year, newDate.Month, newDate.Day);
					}
				case Period.PartialYear:
					break;
				case Period.FiscalYear:
					break;
				case Period.PartialFiscalYear:
					break;
			}
			return date;
		}

		//-----------------------------------------------------------------------------
		private DateTime ApplyOffsetTo(DateTime date)
		{
			DateTime newDate;
			switch (period)	
			{
				case Period.Day:
					{
						return new DateTime(date.Year, date.Month, date.Subtract(new TimeSpan(offset, 0, 0, 0)).Day);
					}
				case Period.Month:
					{
						newDate = date.AddMonths(offset);
						return new DateTime(newDate.Year, newDate.Month, DateTime.DaysInMonth(newDate.Year, newDate.Month));
					}
				case Period.PartialMonth:
					break;
				case Period.Quarter:
					break;
				case Period.Year:
					{
						newDate = date.AddYears(offset);
						return new DateTime(newDate.Year, newDate.Month, newDate.Day);
					}
					
				case Period.PartialYear:
					break;
				case Period.FiscalYear:
					break;
				case Period.PartialFiscalYear:
					break;
			}
			return date;
		}


		//-----------------------------------------------------------------------------
		public DateTime To(DateTime anchorDate)
		{
			DateTime to = anchorDate;
			switch (type)
			{
				case PeriodType.To:
					{
						to = anchorDate;
						break;
					}
				case PeriodType.From:
					{
						to = AddPeriodToDate(anchorDate);
						break;
					}
				case PeriodType.Contained:
					{
						to = PeriodTo(anchorDate);
						break;
					}
			}
			if (offset != 0)
				to = ApplyOffsetTo(to);

			return to;
		}

		//-----------------------------------------------------------------------------
		private DateTime PeriodFrom(DateTime date)
		{
			switch (period)
			{
				case Period.Day:
					return date;
				case Period.Month:
					return new DateTime(date.Year, date.Month, 1);
				case Period.Quarter:
					return BeginQuarterFromDate(date);
				case Period.Year:
					return new DateTime(date.Year, 1, 1); ;

				//case FISCALYEAR:
			}
			return date;
			
		}

		//-----------------------------------------------------------------------------
		private DateTime PeriodTo(DateTime date)
		{
			switch (period)
			{
				case Period.Day:
					return date;
				case Period.Month:
					return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
				case Period.Quarter:
					return EndQuarterFromDate(date);
				case Period.Year:
					return new DateTime(date.Year, 12, 31);

				//case FISCALYEAR:

			}
			return date;
		}

		//-----------------------------------------------------------------------------
		private DateTime BeginQuarterFromDate(DateTime date)
		{
			DateTime quarter1From = new DateTime(date.Year, 1, 1);
			DateTime quarter2From = new DateTime(date.Year, 4, 1);
			DateTime quarter3From = new DateTime(date.Year, 7, 1);
			DateTime quarter4From = new DateTime(date.Year, 10, 1);

			if (date.CompareTo(quarter4From) != -1) //date is later quarter4From 
				return quarter4From;
			if (date.CompareTo(quarter3From) != -1) //date is later quarter3From 
				return quarter3From;
			if (date.CompareTo(quarter2From) != -1) //date is later quarter2From 
				return quarter2From;

			return quarter1From;
		}
		//-----------------------------------------------------------------------------
		private DateTime EndQuarterFromDate(DateTime date)
		{ 
			DateTime quarter1To = new DateTime(date.Year, 3, DateTime.DaysInMonth(date.Year, 3));
			DateTime quarter2To = new DateTime(date.Year, 6, DateTime.DaysInMonth(date.Year, 6));
			DateTime quarter3To = new DateTime(date.Year, 9, DateTime.DaysInMonth(date.Year, 9));
			DateTime quarter4To = new DateTime(date.Year, 12, DateTime.DaysInMonth(date.Year, 12));

			if (date.CompareTo(quarter1To) != 1) //date is earlier quarter1From 
				return quarter1To;
			if (date.CompareTo(quarter2To) != 1) //date is earlier quarter2From 
				return quarter2To;
			if (date.CompareTo(quarter3To) != 1) //date is earlier quarter3From 
				return quarter3To;

			return quarter4To;
		}

		//-----------------------------------------------------------------------------
		private DateTime AddPeriodToDate(DateTime date)
		{
			switch (period)
			{
				case Period.Day:
					return date.AddDays(1);
				case Period.Month:
					return date.AddMonths(1);
				case Period.PartialMonth:
					return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
				case Period.Quarter:
					return date.AddMonths(3);
				case Period.Year:
					return date.AddYears(1);
				case Period.PartialYear:
					return new DateTime(date.Year, 12, 31);
				//case FISCALYEAR:
				//case PARTIALFISCALYEAR :

			}
			return date;
		}
		//-----------------------------------------------------------------------------
		private DateTime SubtractPeriodFromDate(DateTime date)
		{
			switch (period)
			{
				case Period.Day:
					return date.Subtract(new TimeSpan(1, 0, 0, 0));
				case Period.Month:
					return date.Subtract(new TimeSpan(31, 0, 0, 0));
				case Period.PartialMonth:
					return new DateTime(date.Year, date.Month, 1);
				case Period.Quarter:
					return date.Subtract(new TimeSpan(93, 0, 0, 0));
				case Period.Year:
					return new DateTime(date.Year - 1, date.Month, date.Day);
				case Period.PartialYear:
					return new DateTime(date.Year, 1, 1);

				//case FISCALYEAR:
				//case PARTIALFISCALYEAR :

			}
			return date;
		}

		//-----------------------------------------------------------------------------
		public DateRange()
		{
		}

		//-----------------------------------------------------------------------------
		public DateRange(string name, string description, Period period, PeriodType type, int offset)
		{
			this.name = name;
			this.description = description;
			this.period = period;
			this.type = type;
			this.offset = offset;
		}
	}

	[Serializable]
	public class DateRangeList : List<DateRange>
	{
		//-----------------------------------------------------------------------------
		public DateRangeList()
		{
		}

		//Carica i ranges hard-coded
		//-----------------------------------------------------------------------------
		public void LoadDefaultRanges()
		{
			DateRange dr = new DateRange("Today", DateRangeStrings.Today, Period.Day, PeriodType.Contained, 0);
			Add(dr);
			
			dr = new DateRange("CurrentMonth", DateRangeStrings.CurrentMonth, Period.Month, PeriodType.Contained, 0);
			Add(dr);

			dr = new DateRange("CurrentYear", DateRangeStrings.CurrentYear, Period.Year, PeriodType.Contained, 0);
			Add(dr);

			dr = new DateRange("DateToThisMonth", DateRangeStrings.DateToThisMonth, Period.PartialMonth, PeriodType.From, 0);
			Add(dr);

			dr = new DateRange("ThisMonthToDate", DateRangeStrings.ThisMonthToDate, Period.PartialMonth, PeriodType.To, 0);
			Add(dr);

			dr = new DateRange("DateToThisYear", DateRangeStrings.DateToThisYear, Period.PartialYear, PeriodType.From, 0);
			Add(dr);

			dr = new DateRange("ThisYearToDate", DateRangeStrings.ThisYearToDate, Period.PartialYear, PeriodType.To, 0);
			Add(dr);

			dr = new DateRange("LastMonth", DateRangeStrings.LastMonth, Period.Month, PeriodType.Contained, -1);
			Add(dr);

			dr = new DateRange("LastYear", DateRangeStrings.LastYear, Period.Year, PeriodType.Contained, -1);
			Add(dr);	
		}

		//Legge i range da file xml passato come parametro
		//-----------------------------------------------------------------------------
		public bool ReadFromXmlFile(string filePath)
		{
			if (!File.Exists(filePath))
				return false;

			DateRangeList dateRangeFromFile = new DateRangeList();

			XmlSerializer dateRangeSerializer = new XmlSerializer(typeof(DateRangeList));
			try
			{
				//leggo  da file su un daterangelist ausiliario
				using (StreamReader sw = new StreamReader(filePath))
				{
					dateRangeFromFile = dateRangeSerializer.Deserialize(sw) as DateRangeList;
				}

				//travaso dal datalist ausiliario
				foreach (DateRange dr in dateRangeFromFile)
					Add(dr);
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.ToString());
				return false;
			}
			return true;
		}
	}
}

