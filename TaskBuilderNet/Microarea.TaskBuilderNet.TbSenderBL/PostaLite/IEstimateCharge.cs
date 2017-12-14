using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.TaskBuilderNet.TbSenderBL.PostaLite
{
	public interface IEstimateCharge
	{
		decimal AmountActivation { get; set; }
		double Vat { get; set; }
		decimal TotalAmount { get; set; }
		string Iban { get; set; }
		string BankName { get; set; }
		string Beneficiary { get; set; }
		int Error { get; set; }
	}
}
