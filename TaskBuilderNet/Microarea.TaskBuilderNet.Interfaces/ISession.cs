using System;

namespace Microarea.TaskBuilderNet.Interfaces
{
	public interface ISession : IDisposable
	{
        T[] Load<T>(IParameterBuilder parameterBuilder);
		void Save<T>(T obj);
		void Close();
	}


    public interface IParameterBuilder
    {
        Object CreateParameters();
    }
}
