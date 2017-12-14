
using Microarea.TaskBuilderNet.TbSenderBL.postalite.api;
namespace Microarea.TaskBuilderNet.TbSenderBL.PostaLite
{
	public interface IPostaLiteService
	{
		IUserCredentials Subscribe(ISubscriberInfo subscriberInfo);
		ICreditState Charge(IUserCredentials credentials, IFileMessage paymentNote, decimal amountCharge, double vat, decimal amountActivation, decimal totalAmount);
		ICreditState GetCreditState(IUserCredentials credentials);
		IEstimate[] GetLotCostEstimate(IUserCredentials credentials, ILotInfo[] lots);
		ISentLot SendLot(IUserCredentials credentials, ILotInfo lot, IFileMessage lotMessage);
		ILotState[] GetLotState(IUserCredentials credentials, int[] idsLot);
		ISubscriberInfo GetSubscriptionInfo(string login, string password, out IUserCredentials credentials);
		string GetCadastralCode(string city, out int error);
		IEstimateCharge GetEstimateCharge(IUserCredentials credentials, decimal amountCharge);
		LegalInfos GetLegalInfos(string language);
	}
}
