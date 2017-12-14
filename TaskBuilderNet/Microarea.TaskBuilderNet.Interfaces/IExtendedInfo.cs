using System.Collections;

namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface IExtendedInfo : IEnumerable
	{
		//---------------------------------------------------------------------
		void Add(string name, object info);
		//---------------------------------------------------------------------
		string Format(LineSeparator separator);
		//---------------------------------------------------------------------
		IExtendedInfoItem this[int index] { get; set; }
		//---------------------------------------------------------------------
		object this[string name] { get; }
		//---------------------------------------------------------------------
		int Count { get; }
	}
}
