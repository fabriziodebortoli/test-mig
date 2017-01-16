using System;

namespace TaskBuilderNetCore.Interfaces
{
	public interface ISessionFactory : IDisposable
	{
		ISession OpenSession();
		void Close();
	}
}
