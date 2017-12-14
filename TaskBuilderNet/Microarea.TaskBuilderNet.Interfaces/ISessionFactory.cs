using System;

namespace Microarea.TaskBuilderNet.Interfaces
{
	public interface ISessionFactory : IDisposable
	{
		ISession OpenSession();
		void Close();
	}
}
