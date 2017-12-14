using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Core.Generic
{

	// Form che appare con le sembianze di un menu di contesto, contiene il controllo calendario e permette di selezionare 
	// una data o con il click del mouse o con la tastiera
	//==============================================================================
	public partial class Calendar : PopupContainer
	{
		private IntPtr clickedControlHandle = IntPtr.Zero;
        private IntPtr ptrDate = IntPtr.Zero;

		//-----------------------------------------------------------------------------
		public Calendar()
		{
			InitializeComponent();
		}

		///<summary>
		///Metodo che imposta la location del control, tenendo conto anche dello spazio
		///disponibile sul monitor per far si che non esca dall'area dello schermo.
		/// </summary>
		//-----------------------------------------------------------------------------
		protected override void SetLocation(IntPtr parsedCtrlHandle)
		{
			//calcolo il rettangolo del parsed control associato
			ExternalAPI.Rect parsedCtrlRect = new ExternalAPI.Rect(0, 0, 0, 0);
			ExternalAPI.GetWindowRect(parsedCtrlHandle, ref parsedCtrlRect);
			
			//Posizione il calendario sotto il parsed control associato
			Location = new Point(parsedCtrlRect.left, parsedCtrlRect.bottom);
			
			//Trovo lo screen in cui si trova il controllo 
			Screen currentScreen = Screen.FromHandle(parsedCtrlHandle);
			Rectangle rcWorkArea = currentScreen.WorkingArea;

			//Controllo se il calendario esce dallo schermo
			//Se sborda in basso lo posiziono sopra
			if (this.Bottom > rcWorkArea.Bottom)
			{
				Location = new Point(Location.X, parsedCtrlRect.top - Height);
			}
			// se sborda a destra o sinistra lo traslo in orizzontale
			if (this.Left < rcWorkArea.Left)
			{
				Location = new Point(Location.X + rcWorkArea.Left - this.Left, Location.Y);
			}
			if (this.Right > rcWorkArea.Right)
			{
				Location = new Point(Location.X + rcWorkArea.Right - this.Right, Location.Y);
			}
		}

		//-----------------------------------------------------------------------------
		protected override void OnShown(EventArgs e)
		{
			//An.19417
			base.OnShown(e);
			Size = monthCalendar.Size;
		}

		//Inizializza e mostra la form
		//-----------------------------------------------------------------------------
		public IntPtr ShowMonthCalendar(IntPtr parentHandle, IntPtr clickedCtrlHandle, DateTime anchorDate, DateTime selectedDate)
		{
			clickedControlHandle = clickedCtrlHandle;
			//imposto la data odierna
			monthCalendar.TodayDate = anchorDate;
			monthCalendar.SetDate(selectedDate);
			ShowPopupContainer(parentHandle, clickedCtrlHandle);

			return this.Handle;
		}

		//Manda i messaggi al parsed control associato al calendario per comunicare l'avvenuta chiusura della form contentente il 
		//calendario (in questo modo lato c++ pulisce il puntatore al wrapper della finestra managed)
		//-----------------------------------------------------------------------------
		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			// bug fix per gestire correttamente il value changed (se veniva fatta una messagebox nella valuechanged questa non compariva)
			if (ptrDate != IntPtr.Zero)
				ExternalAPI.PostMessage(clickedControlHandle, ExternalAPI.UM_RANGE_SELECTOR_SELECTED, ptrDate, (IntPtr)1);

            ExternalAPI.PostMessage(clickedControlHandle, ExternalAPI.UM_RANGE_SELECTOR_CLOSED, (IntPtr)1, IntPtr.Zero);
        }

		//Sulla selezione della data da calendario manda il messaggio al parsed control perche si aggiorni e si chiude
		//-----------------------------------------------------------------------------
		private void monthCalendar_DateSelected(object sender, DateRangeEventArgs e)
		{
			/*Nota: la delete della memoria allocata per la struttura che contiene la data viene fatta dal metodo callbackMethod
			 se la sendMessage va a buon fine*/

			//mando il messaggio alla finestre che deve valorizzarsi con il valore "FROM" del range passando come
			//parametro il puntatore alla struttura che contiene il valore
			DateSelection date = new DateSelection((Int16)e.Start.Day, (Int16)e.Start.Month, (Int16)e.Start.Year);
			ptrDate = Marshal.AllocCoTaskMem(Marshal.SizeOf(date));
			Marshal.StructureToPtr(date, ptrDate, true);

			// bug fix per gestire correttamente il value changed (se veniva fatta una messagebox nella valuechanged questa non compariva)
			ExternalAPI.PostMessage(clickedControlHandle, ExternalAPI.WM_ACTIVATE, (IntPtr)ExternalAPI.WA_ACTIVE, clickedControlHandle); ;
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
			if (monthCalendar != null)
			{
				monthCalendar.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
