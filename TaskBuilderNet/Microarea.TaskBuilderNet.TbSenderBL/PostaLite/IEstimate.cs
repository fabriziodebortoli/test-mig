
namespace Microarea.TaskBuilderNet.TbSenderBL.PostaLite
{
	public interface IEstimate
	{
		//int IdZone { get; set; }
		string DescriptionZone { get; set; }
		bool Incongruous { get; set; }
		decimal Total { get; set; }
		decimal AmountPrint { get; set; }
		decimal AmountPostage { get; set; }
		int Error { get; set; }
	}
}
