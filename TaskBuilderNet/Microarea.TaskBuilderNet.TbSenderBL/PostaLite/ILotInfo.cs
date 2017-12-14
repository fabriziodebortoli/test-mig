
using Microarea.TaskBuilderNet.TbSenderBL.postalite.api;
namespace Microarea.TaskBuilderNet.TbSenderBL.PostaLite
{
	public interface ILotInfo
	{
		Delivery TypeDelivery { get; set; }
		Printing TypePrinting { get; set; }
		int TotalPages { get; set; }
		string State { get; set; }
		string ZipCode { get; set; }
		string City { get; set; }
		string County { get; set; }
		string FaxNumber { get; set; }
		string Description { get; set; } // what it is for? who uses/reads it?

		string Mittente_NomeCognomeRagioneSociale { get; set; }
		string Mittente_Indirizzo { get; set; }
		string Mittente_Cap { get; set; }
		string Mittente_Localita { get; set; }
		string Mittente_Provincia { get; set; }
		string Mittente_Email { get; set; }
	}
}
