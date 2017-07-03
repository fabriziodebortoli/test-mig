using Microarea.AdminServer.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Controllers.Helpers
{
   
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
                    return Strings.SubscriptionKeyEmpty;
                    
            }
                return Strings.UnknownError;

        }
        }

    /* //-----------------------------------------------------------------------------	
     public enum GwamCodes
     {
        0 OK,
        1 Undefined,
        2 AccountNameCannotBeEmpty,
        3 InvalidAccountName,
        4 NoSubscriptionsAvailable,
        5 InstanceNotValid,
        6 UserUpToDate,
        7 UserLoaded,
        8 AuthorizationHeaderMissing,
        9 MissingAppId,
        10 UnknownApplicationID,
        11 InvalidCredentials,
        12 SubscriptionKeyEmpty,
        13 EmptyCredentials,
        14 InternalError,
        15 ExceptionOccurred
        \gwam-app\server\gwam-app\Controllers\Helpers\GwamMessageCodes.cs
     }*/

}
