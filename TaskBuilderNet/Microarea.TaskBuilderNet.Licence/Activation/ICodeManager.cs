
namespace Microarea.TaskBuilderNet.Licence.Activation
{
	//=========================================================================
	public interface ICodeManager
	{
		bool IsGeneratedCheckGood { get; }
		int GenerateCode();
		int GenerateCheck(int code);
		bool CheckCode(int code);
	}
}
