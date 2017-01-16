using System.ComponentModel;

namespace TaskBuilderNetCore.Interfaces.Model
{
	public interface IModelRoot : IContainer
	{
		bool BelongsToObjectModel(IComponent component);
		IComponent GetComponentByPath(string componentFullPath);
		IComponent GetParentComponentByChildPath(string componentPath);
	}
}
