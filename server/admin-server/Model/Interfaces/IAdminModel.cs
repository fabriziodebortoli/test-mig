using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model.Interfaces
{
	//================================================================================
	public interface IAdminModel
    {
        void SetDataProvider(IDataProvider dataProvider);
        OperationResult Save();
        IAdminModel Load();
		OperationResult Query(QueryInfo qi);
		bool ExistsOnDB { get; set; }
    }
}
