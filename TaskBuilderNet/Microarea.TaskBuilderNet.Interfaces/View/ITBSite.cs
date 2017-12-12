using System;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace Microarea.TaskBuilderNet.Interfaces.View
{
	public interface ITBSite : ISite, IDictionaryService, ICloneable
	{
		ITBSite CloneChild(IComponent component, string name);
	}
}
