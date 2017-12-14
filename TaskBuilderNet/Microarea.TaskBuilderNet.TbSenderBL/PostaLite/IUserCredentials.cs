
namespace Microarea.TaskBuilderNet.TbSenderBL.PostaLite
{
	public interface IUserCredentials
	{
		string Login { get; set; }
		string TokenAuth { get; set; }
		int Error { get; set; }
	}
}
