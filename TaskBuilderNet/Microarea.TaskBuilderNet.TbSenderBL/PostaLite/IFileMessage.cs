
namespace Microarea.TaskBuilderNet.TbSenderBL.PostaLite
{
	public interface IFileMessage
	{
		string FileName { get; set; }
		string FileContentBase64 { get; set; }
	}
}
