using System.Linq;

namespace Microarea.TaskBuilderNet.TbSenderBL
{
	public partial class MA_Company
	{
		//-------------------------------------------------------------------------------
		public static MA_Company Find(string company)
		{
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			{
				return db.MA_Company.First();
			}
		}
	}
}
