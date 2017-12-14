using System;
//
using Microarea.Library.XmlPersister;

namespace Microarea.Library.CommonDeploymentFunctions.States
{
	/// <summary>
	/// Un'istanza di questa classe rappresenta una fotografia
	/// dello stato dello scheduler.
	/// Tutti i suoi membri sono public r/w perché così facendo si possono
	/// serializzare in formato XML utilizzando XmlSerializer.
	/// </summary>
	[Serializable]
	public class SchedulerState : State
	{
		// Aggiornamenti Download Image
		public bool				Enabled;		// indica se lo Scheduler è attivato
		public EnumIntervalType	Type;			// Daily | Weekly | Monthly
		public uint				Quantity = 1;
		
		// NOTE - XmlSerializer (SOAP) non supporta (alla 1.1 del Framework)
		//		la serializzazione della struct TimeSpan (baco del framork)
		// WORKAROUND: Uso un DateTime.MinValue cui aggiungo il TimeSpan
		//public TimeSpan			StartTime;		// ora del giorno cui iniziare
		// TODOFEDE - make it a r/w property, and, if modified, set LastStartTime = DateTime.MinValue
		public DateTime			StartTime;
		
		public DayOfWeek		WeekDay;		// giorno della settimana (Weekly)
		public int				MonthDay;		// giorno del mese (Monthly)

		//

		public EnumPolicyType	DownloadPolicy		= EnumPolicyType.ServicePack;

		// ultimo fire di evento inizio aggiornamento
		public DateTime LastStartTime = DateTime.MinValue;

		// last time settings have been changed
		public DateTime SettingsChangedTime = DateTime.MinValue;

		//---------------------------------------------------------------------
		protected virtual DateTime Now
		{
			// WARNING: do not remove it: local Now property is overloaded in 
			// order to simulate other dates for Unit Testing tests
			get { return DateTime.Now; }
		}

		//---------------------------------------------------------------------
		public bool HaveToUpdateNow()
		{
			if (!this.Enabled)
				return false;
			DateTime now	= Now;
			DateTime today	= now.Date;
			// WARNING: do not change with DateTime.Now and DateTime.Today,
			// as local Now property is overloaded in order to simulate other
			// dates for Unit Testing tests
			DateTime nextStartTime = GetNextStartTime();
			return 
				today == nextStartTime.Date && 
				now >= nextStartTime;
		}

		/// <summary>
		/// Calculates the next scheduled time based on scheduling options
		/// and previous firing time
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public DateTime GetNextStartTime()
		{
			SchedulerState scheduler = this;
			if (!scheduler.Enabled)
				return DateTime.MinValue;

			// init date value
			DateTime now	= Now;
			DateTime today	= now.Date;
			// WARNING: do not change with DateTime.Now and DateTime.Today,
			// as local Now property is overloaded in order to simulate other
			// dates for Unit Testing tests

			if (today < scheduler.LastStartTime.Date) // should never happen
				return DateTime.MinValue;

			TimeSpan scheduledTime = scheduler.StartTime.Subtract(DateTime.MinValue);

			DateTime nextStartTime = DateTime.MinValue;
			DateTime scheduledDay;
			DateTime expected;
			int missingDays = 0;
			int daysPast = 0;

			switch (scheduler.Type)
			{
				case EnumIntervalType.Days :
					scheduledDay = today;

					if (scheduler.LastStartTime != DateTime.MinValue)
					{
						daysPast = ((TimeSpan)(today - scheduler.LastStartTime.Date)).Days;
						int qty = (int)(scheduler.Quantity);
						missingDays = qty >= daysPast ? qty - daysPast : daysPast % qty;
						if (missingDays == 0)
						{
							// should be today at asked time, even if asked time is over:
							// being DateTime.Now > nextTime, the process will be fired anyway
							nextStartTime = today.Add(scheduledTime);
							break;
						}
						scheduledDay = today.AddDays(missingDays);
					}

					expected = scheduledDay.Add(scheduledTime);
					if (now > expected && SettingsChangedTime > expected)
						// tomorrow at scheduled time
						nextStartTime = expected.AddDays(scheduler.Quantity);
					else
						// today at scheduled time
						nextStartTime = expected;
					break;

				case EnumIntervalType.Weeks :
					scheduledDay = today;

					if (scheduler.LastStartTime != DateTime.MinValue)
					{
						daysPast = ((TimeSpan)(today - scheduler.LastStartTime.Date)).Days;
						int rangeWeekDays = (int)scheduler.Quantity * 7;
						missingDays = rangeWeekDays >= daysPast ? rangeWeekDays - daysPast : daysPast % rangeWeekDays;
						if (missingDays == 0)
						{
							// should be today at asked time, even if asked time is over:
							// being DateTime.Now > nextTime, the process will be fired anyway
							nextStartTime = today.Add(scheduledTime);
							break;
						}
						scheduledDay = today.AddDays(missingDays);
					}

					if (today.DayOfWeek > scheduler.WeekDay)
					{
						// next week at scheduled day and time
						int diffDays =  (int)today.DayOfWeek - (int)scheduler.WeekDay;
						nextStartTime = today.AddDays(-diffDays).AddDays(7 * scheduler.Quantity).Add(scheduledTime);
					}
					else if (today.DayOfWeek == scheduler.WeekDay)
					{
						expected = scheduledDay.Add(scheduledTime);
						if (now > expected && SettingsChangedTime > expected)
							// next scheduled week at scheduled day and time
							nextStartTime = expected.AddDays(7 * scheduler.Quantity);
						else
							// today at scheduled day and time
							nextStartTime = expected;
					}
					else if (today.DayOfWeek < scheduler.WeekDay)
					{
						// this week at scheduled day and time
						int diffDays = (int)scheduler.WeekDay - (int)today.DayOfWeek;
						nextStartTime = today.AddDays(diffDays).Add(scheduledTime);
					}
					break;
				
				case EnumIntervalType.Months :
					scheduledDay = today;

					if (scheduler.LastStartTime != DateTime.MinValue)
					{
						int monthsPast;
						if (today.Year == scheduler.LastStartTime.Year)
							monthsPast = today.Month - scheduler.LastStartTime.Month;
						else
						{
							monthsPast = 
								12 - scheduler.LastStartTime.Month + today.Month +
								12 * (today.Year - scheduler.LastStartTime.Year - 1);
						}
						int qty = (int)(scheduler.Quantity);
						int missingMonths = qty >= monthsPast ? qty - monthsPast : monthsPast % qty;

						// assumption: if previous date exists, (scheduler.LastStartTime.Day == this.MonthDay)
						scheduledDay = new DateTime(today.Year, today.Month, this.MonthDay).AddMonths(missingMonths);
						if (today.Year == scheduledDay.Year && today.Month == scheduledDay.Month &&
							today.Day > this.MonthDay)
							scheduledDay = scheduledDay.AddMonths(qty);

						nextStartTime = scheduledDay.Add(scheduledTime);
						break;
					}

					if (today.Day > scheduler.MonthDay)
					{
						// next month at scheduled day and time
						int daysSince1st = today.Day - 1;
						DateTime nextFirst = new DateTime(today.Year, today.Month, 1).AddMonths((int)scheduler.Quantity);
						int daysInNextMonth = DateTime.DaysInMonth(nextFirst.Year, nextFirst.Month);
						if (daysInNextMonth < scheduler.MonthDay)
							nextStartTime = new DateTime(nextFirst.Year, nextFirst.Month, daysInNextMonth).Add(scheduledTime);
						else
							nextStartTime = new DateTime(nextFirst.Year, nextFirst.Month, scheduler.MonthDay).Add(scheduledTime);
					}
					else if (today.Day == scheduler.MonthDay)
					{
						expected = scheduledDay.Add(scheduledTime);
						if (now > expected && SettingsChangedTime > expected)
							// next month at scheduled day and time
							nextStartTime = expected.AddMonths((int)scheduler.Quantity);
						else
							// today at scheduled day and time
							nextStartTime = expected;
					}
					else if (today.Day < scheduler.MonthDay)
					{
						// NOTE - not every month has 31 days!
						int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
						if (daysInMonth < scheduler.MonthDay)
							// this month on last month day at scheduled time
							nextStartTime = new DateTime(scheduledDay.Year, scheduledDay.Month, daysInMonth).Add(scheduledTime);
						else
							// this month at scheduled day and time
							nextStartTime = new DateTime(scheduledDay.Year, scheduledDay.Month, scheduler.MonthDay).Add(scheduledTime);
					}
					break;
			}

			return nextStartTime;
		}
	}

	/// <summary>
	/// Tipologia di intervallo impostato per lo scheduler.
	/// Il qualificatore di accesso deve essere tale per cui sia visibile
	/// da SchedulerState.
	/// </summary>
	///========================================================================
	public enum EnumIntervalType { Days, Weeks, Months };

	/// <summary>
	/// Tipo di politica di download/upgrade basata sulla release version
	/// dell'immagine installata sul client.
	/// </summary>
	///========================================================================
	public enum EnumPolicyType
	{
		ServicePack,	// download/upgrade concesso solo per a livello di SP
		Minor,			// download/upgrade concesso per cambio di Minor release
		Major			// download/upgrade concesso per cambio di Major release
	};
}