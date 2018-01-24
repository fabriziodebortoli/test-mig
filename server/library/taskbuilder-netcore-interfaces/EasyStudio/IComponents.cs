using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.EasyStudio.Interfaces
{
	//====================================================================
	public interface IComponents : IDisposable
	{
		IList<IComponent> Components { get; }
		void Add(IComponent component);
		void Remove(IComponent component);
	}
}
