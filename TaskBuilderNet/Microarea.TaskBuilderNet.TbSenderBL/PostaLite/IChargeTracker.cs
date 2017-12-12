using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.TaskBuilderNet.TbSenderBL.PostaLite
{
	public interface IChargeTracker
	{
		void Track
			(
			string loginID,
			string userID,
			decimal amountCharge, decimal amountActivation, double vat, decimal totalAmount
			);
		string Url { get; set; }
	}
}
