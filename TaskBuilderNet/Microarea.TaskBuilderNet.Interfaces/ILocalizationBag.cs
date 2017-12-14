namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface ILocalizationBag
	{
		string Days { get; set; }
		string Key { get; set; }
		string ProductName { get; set; }
		long RenewalPeriodTicks { get; set; }
		string UserEmail { get; set; }
	}
}
