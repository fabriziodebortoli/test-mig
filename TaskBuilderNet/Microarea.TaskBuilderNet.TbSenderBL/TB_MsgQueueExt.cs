using System.Collections.Generic;
using System.Linq;
using Microarea.TaskBuilderNet.TbSenderBL.postalite.api;

namespace Microarea.TaskBuilderNet.TbSenderBL
{
	public partial class TB_MsgQueue
	{
		//-------------------------------------------------------------------------------
		public Delivery EnumDeliveryType
		{
			get { return (Delivery)this.DeliveryType; }
			set { this.DeliveryType = (int)value; }
		}

		//-------------------------------------------------------------------------------------
		public static List<TB_MsgQueue> GetMessages(string company)
		{
			List<TB_MsgQueue> list = new List<TB_MsgQueue>();
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			{
				//if (db == null)
				//    return null;
				var items = from p in db.TB_MsgQueue select p;
				list.AddRange(items);
			}
			return list;
		}

		//-------------------------------------------------------------------------------------
		public static void SaveNewMessage(TB_MsgQueue msg, string company)
		{
			using (var db = ConnectionHelper.GetPostaLiteIntegrationEntities(company))
			{
				db.AddToTB_MsgQueue(msg);
				db.SaveChanges();
			}
		}

		//-------------------------------------------------------------------------------------
		public static TB_MsgQueue CreateMessage()
		{
			TB_MsgQueue msg = new TB_MsgQueue()
			{
				//TBCreated = DateTime.Now,
				//TBModified = DateTime.Now,
				//TBCreatedID = 1,
				//TBModifiedID = 1
			};
			return msg;
		}
	}
}
