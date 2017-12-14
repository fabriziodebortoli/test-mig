
namespace Microarea.TaskBuilderNet.TbHermesBL
{
	public interface ILockerClient
	{
		//string UserName { get; set; }
		//string AuthenticationToken { get; set; }
		bool LockRecord(string companyDBName, string tableName, string lockKey);
		void UnlockRecord(string companyDBName, string tableName, string lockKey);
	}
}
