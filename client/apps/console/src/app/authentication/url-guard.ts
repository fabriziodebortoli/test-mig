import { OperationResult } from '../services/operationResult';
import { RoleNames, RoleLevels } from './auth-helpers';
import { AuthorizationInfo } from './auth-info';

export class UrlGuard {

  //--------------------------------------------------------------------------------------------------------
  public static CanNavigate(url: string, authInfo: AuthorizationInfo): OperationResult {

    let opRes: OperationResult = new OperationResult();

    if (url == '') {
      opRes.Message = 'No check-url strategy has been implemented for this url ' + url;
      opRes.Result = false;
      return opRes;
    }

    // checking permission by specific component-url

    // checking test controls page

    if (url == '/testControls') {

      if (!authInfo.VerifyRoleLevel(RoleNames.Admin, RoleLevels.Instance)) {
        opRes.Message = 'You do not have rights to see this content';
        opRes.Result = false;
        return opRes;
      }
      else {
        opRes.Result = true;
        return opRes;
      }
    }

    // checking instances

    if (url.startsWith('/instance?instanceToEdit=')) {

      let instanceKey: string = url.substr(url.lastIndexOf("=") + 1);

      if (!authInfo.VerifyRole(RoleNames.Admin, RoleLevels.Instance, instanceKey)) {
        opRes.Message = 'You do not have rights to edit ' + instanceKey;
        opRes.Result = false;
        return opRes;
      }
      else {
        opRes.Result = true;
        return opRes;
      }
    }

    // checking subscriptions

    if (url.startsWith('/subscription?subscriptionToEdit=')) {

      let subKey: string = url.substr(url.lastIndexOf("=") + 1);

      if (!authInfo.VerifyRole(RoleNames.Admin, RoleLevels.Subscription, subKey)) {
        opRes.Message = 'You do not have rights to edit ' + subKey;
        opRes.Result = false;
        return opRes;
      }
      else {
        opRes.Result = true;
        return opRes;
      }
    }

    // checking accounts

    if (url.startsWith('/account?accountNameToEdit=')) {

      let accountName: string = url.substr(url.lastIndexOf("=") + 1);

      if (authInfo.VerifyRole(RoleNames.Admin, RoleLevels.Account, accountName)) {
        opRes.Result = true;
        return opRes;
      }

      if (authInfo.VerifyRoleLevel(RoleNames.Admin, RoleLevels.Account)) {
        opRes.Result = true;
        return opRes;
      }

      opRes.Message = 'You do not have rights to edit this account ' + accountName;
      opRes.Result = false;
      return opRes;
    }

    if (url == '/instancesHome' || url.startsWith('/instance')) {
      if (!authInfo.VerifyRoleLevel(RoleNames.Admin, RoleLevels.Instance)) {
        opRes.Message = RoleLevels.Instance + ' level missing';
        opRes.Result = false;
        return opRes;
      }
      else {
        opRes.Result = true;
        return opRes;
      }
    }

    if (url == '/subscriptionsHome' || url.startsWith('/subscription')) {
      if (!authInfo.VerifyRoleLevel(RoleNames.Admin, RoleLevels.Subscription)) {
        opRes.Message = RoleLevels.Subscription + ' level missing';
        opRes.Result = false;
        return opRes;
      }
      else {
        opRes.Result = true;
        return opRes;
      }
    }

    if (url == '/accountsHome' || url == '/account') {
      if (!authInfo.VerifyRoleLevel(RoleNames.Admin, RoleLevels.Account)) {
        opRes.Message = RoleLevels.Account + ' level missing';
        opRes.Result = false;
        return opRes;
      }
      else {
        opRes.Result = true;
        return opRes;
      }
    }

    // checking databases (url is like this: /database?subscriptionToEdit=S-ENT&databaseToEdit=EntDbName)

    if (url.startsWith('/database')) {

      // better use try/catch to avoid exception when we play with string indexes!
      try {
        // I search the subscriptionKey in the url
        let startSubKeyIdx: number = url.indexOf("=") + 1;
        let endSubKeyIdx: number = url.indexOf("&");

        let subKey: string = url.substr(startSubKeyIdx, endSubKeyIdx - startSubKeyIdx);

        // then I check the role for this subscription

        if (!authInfo.VerifyRole(RoleNames.Admin, RoleLevels.Subscription, subKey)) {
          opRes.Message = 'You do not have rights to edit ' + subKey;
          opRes.Result = false;
          return opRes;
        }
        else {
          opRes.Result = true;
          return opRes;
        }
      } catch (error) {
        opRes.Message = error;
        opRes.Result = false;
        return opRes;
      }
    }

    opRes.Message = 'Unknown Url';
    opRes.Result = false;
    return opRes;
  }

  //--------------------------------------------------------------------------------------------------------
  public static CanNavigateLevel(requiredLevel: string, authInfo: AuthorizationInfo): OperationResult {

    let opRes: OperationResult = new OperationResult();

    if (!authInfo.VerifyRoleLevel(RoleNames.Admin, requiredLevel)) {
      opRes.Message = requiredLevel + ' level missing';
      opRes.Result = false;
      return opRes;
    }
    else {
      opRes.Result = true;
      return opRes;
    }
  }
}