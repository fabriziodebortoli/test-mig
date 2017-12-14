namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface IAdvertisementBody
	{
		string Html { get; set; }
		string Link { get; set; }
		ILocalizationBag LocalizationBag { get; set; }
		string Text { get; set; }
	}
}
