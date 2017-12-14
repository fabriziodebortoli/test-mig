using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.TbSenderBL.PostaLite;

namespace Microarea.TaskBuilderNet.TbSenderBL.postalite.api
{
	public partial class CreditState : ICreditState { }
	public partial class Estimate : IEstimate { }
	public partial class FileMessage : IFileMessage { }
	public partial class LotInfo : ILotInfo { }
	public partial class LotState : ILotState { }
	public partial class SentLot : ISentLot { }
	public partial class SubscriberInfo : ISubscriberInfo
	{
		//public string CompanyID { get { return "Mago"; } } // TODO lasciare qui???
	}
	public partial class UserCredentials : IUserCredentials { }
	public partial class EstimateCharge : IEstimateCharge { }
}
