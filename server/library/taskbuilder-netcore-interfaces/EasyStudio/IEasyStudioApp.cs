using System.Xml;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.EasyStudio.Interfaces
{
	//=========================================================================
	/// <remarks />
	public interface IEasyStudioApp
	{
        string ApplicationName { get; set; }
        /// <remarks />
        string ModuleName { get; }
    }
}
