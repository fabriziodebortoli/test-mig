
namespace Microarea.TaskBuilderNet.TbSenderBL.PostaLite
{
	public interface ILotState
	{
		int Id { get; set; }
		int State { get; set; }
		string Description { get; set; }
		int Error { get; set; }
	}
}
