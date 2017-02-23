using System.ComponentModel;
using TaskBuilderNetCore.Interfaces.Model;

namespace TaskBuilderNetCore.Interfaces
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