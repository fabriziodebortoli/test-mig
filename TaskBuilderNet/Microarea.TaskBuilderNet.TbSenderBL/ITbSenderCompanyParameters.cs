using System;
namespace Microarea.TaskBuilderNet.TbSenderBL
{
	public interface ITbSenderCompanyParameters
	{
		int AllotMessagesInterval { get; set; }
		string Company { get; set; }
		bool EveryDay { get; set; }
		DateTime EveryDayAt { get; set; }
		bool Friday { get; set; }
		DateTime GetSentAfterTime(TB_MsgQueue msg);
		bool Monday { get; set; }
		bool Recurring { get; set; }
		int RecurringEveryHours { get; set; }
		DateTime RecurringStartAt { get; set; }
		bool Saturday { get; set; }
		bool Sunday { get; set; }
		bool Thursday { get; set; }
		bool Tuesday { get; set; }
		bool Wednesday { get; set; }
		bool Weekly { get; set; }
		DateTime WeeklyStartAt { get; set; }
	}
}
