using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Services.Interfaces
{
	//=========================================================================
	public interface IAdminDataServiceProvider
    {
        IAccount ReadLogin(string userName, string password);
	}
}
