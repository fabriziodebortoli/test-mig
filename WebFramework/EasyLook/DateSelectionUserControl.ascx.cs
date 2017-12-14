using System.Globalization;
using System.Threading;

namespace Microarea.Web.EasyLook
{
	using System;

	/// <summary>
	///		Summary description for DateSelectionUserControl.
	/// </summary>
	public partial class DateSelectionUserControl : System.Web.UI.UserControl
	{
		private string fontName = "Verdana";

		
		#region Web Form Designer generated code

		//------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		//------------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			if (DesignMode)
				return;

			CultureInfo culture = CultureInfo.CreateSpecificCulture(Thread.CurrentThread.CurrentCulture.Name);

			if (!this.IsPostBack)
			{
				FontName = MonthDropDownList.Font.Name;

				if (MonthDropDownList.Items.Count == 0)
				{
					string[] monthNames = culture.DateTimeFormat.MonthNames;
					foreach (string monthName in monthNames)
					{
						if (monthName != null && monthName != String.Empty)
							MonthDropDownList.Items.Add(monthName);
					}
				}
				this.Date = DateTime.Today;
			}
		}
		#endregion

		#region DateSelectionUserControl public properties
		
		//------------------------------------------------------------------------------------
		public DateTime Date
		{
			get
			{
				if 
					(
						YearUpDown.Value < DateTime.MinValue.Year || 
						YearUpDown.Value > DateTime.MaxValue.Year || 
						MonthDropDownList.SelectedIndex == -1					)
					return DateTime.MinValue; 


				return new DateTime(YearUpDown.Value, MonthDropDownList.SelectedIndex + 1, SelectDayCalendar.SelectedDate.Day);
			}
			set
			{
				MonthDropDownList.SelectedIndex = value.Month - 1;
				YearUpDown.Value = value.Year;
				SelectDayCalendar.SelectedDate = SelectDayCalendar.VisibleDate = value;
			}
		}
		 
		//------------------------------------------------------------------------------------
		public string FontName
		{
			get
			{
				return fontName;
			}
			set
			{
				fontName = value;
				MonthDropDownList.Font.Name = fontName;
				YearUpDown.Font.Name = fontName;
				SelectDayCalendar.Font.Name = fontName;
			}
		}
		
		#endregion

		#region DateSelectionUserControl event handlers
		
		//------------------------------------------------------------------------------------
		protected void MonthDropDownList_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (MonthDropDownList.SelectedIndex == -1)
			{
				SelectDayCalendar.SelectedDate = SelectDayCalendar.VisibleDate = DateTime.Today;
				return;
			}
			SelectDayCalendar.SelectedDate = SelectDayCalendar.VisibleDate  = new DateTime(YearUpDown.Value, MonthDropDownList.SelectedIndex + 1, 1);
		}
		
		//------------------------------------------------------------------------------------
		protected void YearUpDown_ValueChanged(object sender, System.EventArgs e)
		{
			if (!Visible)
				return;

			// Determines whether the specified date is a leap day.
			if 
				(
					Thread.CurrentThread.CurrentUICulture.Calendar.IsLeapDay(SelectDayCalendar.SelectedDate.Year, SelectDayCalendar.SelectedDate.Month, SelectDayCalendar.SelectedDate.Day) &&
					!Thread.CurrentThread.CurrentUICulture.Calendar.IsLeapYear(YearUpDown.Value)
				)
				SelectDayCalendar.SelectedDate = SelectDayCalendar.VisibleDate = new DateTime(YearUpDown.Value, SelectDayCalendar.SelectedDate.Month, SelectDayCalendar.SelectedDate.Day-1);
			else
				SelectDayCalendar.SelectedDate = SelectDayCalendar.VisibleDate = new DateTime(YearUpDown.Value, SelectDayCalendar.SelectedDate.Month, SelectDayCalendar.SelectedDate.Day);
		}

		//------------------------------------------------------------------------------------
		protected void SelectDayCalendar_SelectionChanged(object sender, System.EventArgs e)
		{
			if (SelectDayCalendar.SelectedDate.Month != (MonthDropDownList.SelectedIndex + 1))
			{
				MonthDropDownList.SelectedIndex = SelectDayCalendar.SelectedDate.Month - 1;
				SelectDayCalendar.VisibleDate = SelectDayCalendar.SelectedDate;
			}

			if (SelectDayCalendar.SelectedDate.Year != YearUpDown.Value)
				YearUpDown.Value = SelectDayCalendar.SelectedDate.Year;
		}

		#endregion

	}
}
