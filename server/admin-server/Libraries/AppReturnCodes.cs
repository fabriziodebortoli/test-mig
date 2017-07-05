﻿namespace Microarea.AdminServer.Libraries
{
	//--------------------------------------------------------------------------------
	public enum AppReturnCodes
	{
		OK,
		ExceptionOccurred,
		Undefined,
		AccountNameCannotBeEmpty,
		InvalidAccountName,
		NoSubscriptionsAvailable,
		InstanceNotValid,
		UserUpToDate,
		UserLoaded,
		AuthorizationHeaderMissing,
		InvalidCredentials,
		SubscriptionKeyEmpty,
		EmptyCredentials,
		InternalError,
		UserIsExpired,
		UserDisabled,
		UserLocked,
		InstanceKeyEmpty,
		InvalidToken,
		SuspectedToken,
		MissingToken
	}
}
