using System.Data;

namespace Microarea.AdminServer.Model.Interfaces
{
	public interface IModelObject
    {
		IModelObject Fetch(IDataReader reader);
		string GetKey();
	}
}
