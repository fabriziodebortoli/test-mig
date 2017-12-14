using System.ComponentModel;

namespace Microarea.TaskBuilderNet.Interfaces.EasyBuilder
{
	//=============================================================================
	public interface IEasyBuilderContainer : IContainer
	{
		bool CanCallCreateComponents();
		void CallCreateComponents(); 
		void CreateComponents();
		void ApplyResources();
		void ClearComponents();
		void OnAfterCreateComponents();
		string SerializedType { get; }

		void Add(IComponent component, bool isChanged);

		bool HasComponent(string controlName);
		IComponent GetComponent(string controlName);
	}

	public interface IChangedEventsSource
	{
		IComponent EventSourceComponent { get; }
	}

}
