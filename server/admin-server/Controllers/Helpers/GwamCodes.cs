using Microarea.AdminServer.Properties;

namespace Microarea.AdminServer.Controllers.Helpers
{
	//================================================================================
	public class GwamMessageStrings
    {
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
					return Strings.UserUpToDate;
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

				default:
					return Strings.UnknownError;
			}

		}
    }
}
