using Microarea.AdminServer.Properties;

namespace Microarea.AdminServer.Controllers.Helpers
{
	/// <summary>
	/// QUESTI CODICI DEVONO ESSERE ALLINEATI CON QUELLI DELL'ENUMERATIVO GWAMCodes
	/// DEFINITO NEL GWAM back-end
	/// </summary>
	//================================================================================
	public class GwamMessageStrings
    {
		//-----------------------------------------------------------------------------	
		public static string GetString(int code)
        {
            switch (code)
            {
                case 0:
                    return Strings.OK;
				case 1:
					return Strings.Undefined;
				case 2:
					return Strings.AccountNameCannotBeEmpty;
				case 3:
					return Strings.InvalidAccountName;
				case 4:
					return Strings.NoSubscriptionsAvailable;
				case 5:
					return Strings.InstanceNotValid;
				case 6:
					return Strings.DataUpToDate;
				case 7:
					return Strings.UserLoaded;
				case 8:
					return Strings.AuthorizationHeaderMissing;
				case 9:
					return Strings.MissingAppId;
				case 10:
					return Strings.UnknownApplicationID;
				case 11:
					return Strings.InvalidCredentials;
				case 12:
					return Strings.SubscriptionKeyEmpty;
				case 13:
					return Strings.EmptyCredentials;
				case 14:
					return Strings.InternalError;
				case 15:
					return Strings.ExceptionOccurred;
				case 16:
					return Strings.TicksCannotBeEmpty;
				case 17:
					return Strings.UserIsExpired;
				case 18:
					return Strings.UserDisabled; 
				case 19:
					return Strings.UserLocked; 
				case 20:
					return Strings.InstanceKeyEmpty;
				case 21:
					return Strings.InvalidToken;
				case 22:
					return Strings.SuspectedToken;
				case 23:
					return Strings.MissingToken;
                case 24:
                    return Strings.WrongRecoveryCode;
				case 25:
					return Strings.UnknownAccount;
				case 26:
					return Strings.NoInstancesAvailable;
				case 27:
					return Strings.EmptyParameters;
                case 28:
                    return Strings.UnknownModelName;        
                case 29:
                    return Strings.EmptyModelName;
                case 30:
					return Strings.MissingRole;
				case 31:
					return Strings.ValidToken;
                case 32:
                    return Strings.DataToUpdate;
                default:
					return Strings.UnknownError;
			}
		}
    }
}
