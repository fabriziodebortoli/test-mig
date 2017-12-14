using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	public enum WrapperWindowState { None, WindowTo, WindowFrom }
	public enum Period { Day, Month, PartialMonth, Quarter, Year, PartialYear, FiscalYear, PartialFiscalYear }
	public enum PeriodType { From, To, Contained }


	// Form che appare con le sembianze di un menu di contesto e permette di selezionare un range tra quelli proposti
	//==============================================================================
	public partial class PopupContainer : Form
	{
		//finestra wrapper dell'owner del controllo cliccato
		protected NativeWindow wrapperWindow = null;
		//indica se e' stato effettuato il doppio click sul range per impostarlo ai parsed control
		protected bool dataSelected = false;

		//-----------------------------------------------------------------------------
		public PopupContainer()
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
			if (wrapperWindow != null)
				wrapperWindow.ReleaseHandle();
			base.Dispose(disposing);
		}

		//-----------------------------------------------------------------------------
		protected void ShowPopupContainer(IntPtr parentHandle, IntPtr clickedCtrlHandle)
		{
			//inizializzo il wrapper della finestra c++ che contiene i controlli "range"
			//e' la finestra papa' del controllo cliccato
			wrapperWindow = new NativeWindow();
			wrapperWindow.AssignHandle(clickedCtrlHandle);
			//posiziona il controllo sullo schermo
			SetLocation(clickedCtrlHandle);
			this.StartPosition = FormStartPosition.Manual;
			//evita che la form appaia nella taskbar di Windows 
			this.ShowInTaskbar = false;
			//mostro la finestra che permette di selezionare il range di date
			this.Show(wrapperWindow);
		}

		//Su deattivazione deve chiudersi
		//-----------------------------------------------------------------------------
		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);
			this.Close();
		}

		///<summary>
		///Metodo che imposta la location del control, tenendo conto anche dello spazio
		///disponibile sul monitor per far si che non esca dall'area dello schermo.
		/// </summary>
		//-----------------------------------------------------------------------------
		protected virtual void SetLocation(IntPtr parsedCtrlHandle)
		{
		}

		//Struttura usata per memorizzare le date in una zona di memoria, per predisporle alla  lettura
		//dal parte del parsed control lato C++ (TENERE ALLINEATA con struct DateSelection in \Framework\TbGenlib\Parsedt.cpp )
		[StructLayout(LayoutKind.Sequential)]
		protected struct DateSelection
		{
			Int16 day;
			Int16 month;
			Int16 year;

			//---------------------------------------------------------------------
			public DateSelection(Int16 day, Int16 month, Int16 year)
			{
				this.day = day;
				this.month = month;
				this.year = year;
			}
		}
	}
}
