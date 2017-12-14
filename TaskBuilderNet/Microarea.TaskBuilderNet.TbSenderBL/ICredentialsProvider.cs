using Microarea.TaskBuilderNet.TbSenderBL.PostaLite;
namespace Microarea.TaskBuilderNet.TbSenderBL
{
	public interface ICredentialsProvider
	{
		IUserCredentials GetCredentials(string company);
	}
}
