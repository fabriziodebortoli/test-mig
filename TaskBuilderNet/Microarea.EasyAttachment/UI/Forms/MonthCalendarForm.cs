using System;
using System.Windows.Forms;
using Microarea.EasyAttachment.Components;
using System.Data.SqlTypes;

namespace Microarea.EasyAttachment.UI.Forms
{
	///<summary>
	/// Form che visualizza i controls per identificare un range di date di ricerca
	///</summary>
	//================================================================================
	public partial class MonthCalendarForm : Form
	{
		public event EventHandler<DateRangeEventArgs> OnDateRangeSelected;
        public event EventHandler<ListViewItemSelectionChangedEventArgs> OnDataListViewSelected;
        public event EventHandler AfterFormClosed;

        private DateTime startDate = (DateTime)SqlDateTime.MinValue;
        private DateTime endDate = (DateTime)SqlDateTime.MaxValue;

        public DateTime StartDate { get { return startDate; } }
        public DateTime EndDate { get { return endDate; } }

		//--------------------------------------------------------------------------------
		public MonthCalendarForm()
		{
			InitializeComponent();

			AddItemsToListView();
		}

		///<summary>
		/// Aggiungo alla listview i vari items con le opzioni delle date
		/// Non lo faccio da design perche' voglio localizzare il testo di ogni item
		/// Assegnare esplicitamente il tag agli item della list view in modo da pilotare poi 
		/// correttamente l'assegnazione della date di inizio e fine ricerca (lato Design non lo considera)
		/// </summary>
		//--------------------------------------------------------------------------------
		private void AddItemsToListView()
		{
			ListViewItem lastYear = new ListViewItem(Strings.LastYear);
			lastYear.Tag = Utils.DateRangeType.LAST_YEAR;
			OptionsListView.Items.Add(lastYear);

			ListViewItem currentYear = new ListViewItem(Strings.CurrentYear);
			currentYear.Tag = Utils.DateRangeType.CURRENT_YEAR;
			OptionsListView.Items.Add(currentYear);

			ListViewItem lastSemester = new ListViewItem(Strings.LastSemester);
			lastSemester.Tag = Utils.DateRangeType.LAST_SEMESTER;
			OptionsListView.Items.Add(lastSemester);

			ListViewItem currentSemester = new ListViewItem(Strings.CurrentSemester);
			currentSemester.Tag = Utils.DateRangeType.CURRENT_SEMESTER;
			OptionsListView.Items.Add(currentSemester);

			ListViewItem lastQuarter = new ListViewItem(Strings.LastQuarter);
			lastQuarter.Tag = Utils.DateRangeType.LAST_QUARTER;
			OptionsListView.Items.Add(lastQuarter);

			ListViewItem currentQuarter = new ListViewItem(Strings.CurrentQuarter);
			currentQuarter.Tag = Utils.DateRangeType.CURRENT_QUARTER;
			OptionsListView.Items.Add(currentQuarter);

			ListViewItem lastMonth = new ListViewItem(Strings.LastMonth);
			lastMonth.Tag = Utils.DateRangeType.LAST_MONTH;
			OptionsListView.Items.Add(lastMonth);

			ListViewItem currentMonth = new ListViewItem(Strings.CurrentMonth);
			currentMonth.Tag = Utils.DateRangeType.CURRENT_MONTH;
			OptionsListView.Items.Add(currentMonth);

			ListViewItem lastWeek = new ListViewItem(Strings.LastWeek);
			lastWeek.Tag = Utils.DateRangeType.LAST_WEEK;
			OptionsListView.Items.Add(lastWeek);

			ListViewItem currentWeek = new ListViewItem(Strings.CurrentWeek);
			currentWeek.Tag = Utils.DateRangeType.CURRENT_WEEK;
			OptionsListView.Items.Add(currentWeek);

			ListViewItem yesterday = new ListViewItem(Strings.Yesterday);
			yesterday.Tag = Utils.DateRangeType.YESTERDAY;
			OptionsListView.Items.Add(yesterday);

			ListViewItem clear = new ListViewItem(Strings.ClearSelection);
			clear.Tag = Utils.DateRangeType.ALL_DATES;
			OptionsListView.Items.Add(clear);
		}

		///<summary>
		/// Appena clicco fuori dalla form la chiudo
		///</summary>
		//--------------------------------------------------------------------------------
		private void MonthCalendarForm_Deactivate(object sender, EventArgs e)
		{
			this.Close();
		}

		//--------------------------------------------------------------------------------
		private void MCalendar_DateChanged(object sender, DateRangeEventArgs e)
		{
            startDate = new DateTime(e.Start.Year, e.Start.Month, e.Start.Day, 0, 0, 0, 0);
            endDate = new DateTime(e.End.Year, e.End.Month, e.End.Day, 23, 59, 59, 999);

			if (OnDateRangeSelected != null)
				OnDateRangeSelected(this, e);
		}

		//--------------------------------------------------------------------------------
		private void MCalendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            startDate = new DateTime(e.Start.Year, e.Start.Month, e.Start.Day, 0, 0, 0, 0);
            endDate = new DateTime(e.End.Year, e.End.Month, e.End.Day, 23, 59, 59, 999);

			if (OnDateRangeSelected != null)
				OnDateRangeSelected(this, e);
		}

		//--------------------------------------------------------------------------------
		private void MonthCalendarForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (AfterFormClosed != null)
				AfterFormClosed(sender, EventArgs.Empty);
		}

		///<summary>
		/// Dopo che seleziono una voce sulla lista dei tipi di range di data, invio un evento con il 
		/// valore dell'item e chiudo la form
		///</summary>
		//--------------------------------------------------------------------------------
		private void OptionsListView_Click(object sender, EventArgs e)
		{
			ListViewItem currentLvw = new ListViewItem();
			if (OptionsListView.SelectedItems != null &&
				OptionsListView.SelectedItems.Count == 1 &&
				OptionsListView.SelectedItems[0] != null)
				currentLvw = OptionsListView.SelectedItems[0];

            if (currentLvw == null || string.IsNullOrWhiteSpace(currentLvw.Text) || currentLvw.Tag == null)
            {
                startDate = (DateTime)SqlDateTime.MinValue;
                endDate = (DateTime)SqlDateTime.MaxValue;
            }
            else
            {    // mi calcolo gia' la fine della giornata di oggi
                DateTime nowAt23_59 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59, 999);

                switch ((Utils.DateRangeType)(currentLvw.Tag))
                {
                    case Utils.DateRangeType.ALL_DATES:
                        startDate = (DateTime)SqlDateTime.MinValue;
                        endDate = (DateTime)SqlDateTime.MaxValue;
                        break;

                    case Utils.DateRangeType.YESTERDAY:
                        startDate = Utils.GetYesterday();
                        endDate = new DateTime(Utils.GetYesterday().Year, Utils.GetYesterday().Month, Utils.GetYesterday().Day, 23, 59, 59, 999);
                        break;

                    case Utils.DateRangeType.CURRENT_WEEK:
                        startDate = Utils.GetFirstDayOfCurrentWeek(DayOfWeek.Monday);
                        endDate = nowAt23_59;
                        break;

                    case Utils.DateRangeType.LAST_WEEK:
                        DateTime eDate = new DateTime();
                        Utils.GetDatesOfLastWeek(ref startDate, ref eDate);
                        endDate = new DateTime(eDate.Year, eDate.Month, eDate.Day, 23, 59, 59, 999);
                        break;

                    case Utils.DateRangeType.CURRENT_MONTH:
                        startDate = Utils.GetFirstDayOfCurrentMonth();
                        endDate = nowAt23_59;
                        break;

                    case Utils.DateRangeType.LAST_MONTH:
                        startDate = Utils.GetFirstDayOfLastMonth();
                        endDate = Utils.GetLastDayOfLastMonth();
                        break;

                    case Utils.DateRangeType.CURRENT_QUARTER:
                        startDate = Utils.GetStartOfCurrentQuarter();
                        endDate = Utils.GetEndOfCurrentQuarter();
                        break;

                    case Utils.DateRangeType.LAST_QUARTER:
                        startDate = Utils.GetStartOfLastQuarter();
                        endDate = Utils.GetEndOfLastQuarter();
                        break;

                    case Utils.DateRangeType.CURRENT_SEMESTER:
                        startDate = Utils.GetStartOfCurrentSemester();
                        endDate = Utils.GetEndOfCurrentSemester();
                        break;

                    case Utils.DateRangeType.LAST_SEMESTER:
                        startDate = Utils.GetStartOfLastSemester();
                        endDate = Utils.GetEndOfLastSemester();
                        break;

                    case Utils.DateRangeType.CURRENT_YEAR:
                        startDate = Utils.GetFirstDayOfCurrentYear();
                        endDate = nowAt23_59;
                        break;

                    case Utils.DateRangeType.LAST_YEAR:
                        startDate = Utils.GetFirstDayOfLastYear();
                        endDate = Utils.GetLastDayOfLastYear();
                        break;

                    default:
                        startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
                        endDate = nowAt23_59;
                        break;
                }
            }
            
            ListViewItemSelectionChangedEventArgs arg = new ListViewItemSelectionChangedEventArgs(currentLvw, currentLvw.Index, true);
            if (OnDataListViewSelected != null)
                OnDataListViewSelected(this, arg);


			this.Close();
		}
	}
}
