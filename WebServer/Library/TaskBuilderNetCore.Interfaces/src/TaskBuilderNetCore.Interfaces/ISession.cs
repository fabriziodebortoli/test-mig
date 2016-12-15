using System;

namespace TaskBuilderNetCore.Interfaces
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
