using System.ComponentModel;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Interfaces
{
	public interface IDocumentController
	{
		ComponentCollection Components { get; }
		string Name { get; set; }
		string SerializedName { get; }
		string SerializedType { get; }
		void Add(IComponent component);
		void Add(IComponent component, string name);
		INameSpace CustomizationNameSpace { get; }
	}
}