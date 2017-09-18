import { OperationResult } from '../services/operationResult';
import { RoleNames, RoleLevels } from './auth-helpers';
import { AuthorizationInfo } from './auth-info';

export class UrlGuard {

    //--------------------------------------------------------------------------------------------------------
    public static CanNavigate(url:string, authInfo: AuthorizationInfo): OperationResult {

        let opRes:OperationResult = new OperationResult();

        if (url == '') {
            opRes.Message = 'No check-url strategy has been implemented for this url ' + url;
            opRes.Result = false;
            return opRes;            
        }

        // checking permission by specific component-url

        // checking instances

        if (url.startsWith('/instance?instanceToEdit=')) {

            let instanceKey:string = url.substr(url.lastIndexOf("=") + 1);

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

            let subKey:string = url.substr(url.lastIndexOf("=")+1);

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
            
            let accountName:string = url.substr(url.lastIndexOf("=")+1);

            if (authInfo.VerifyRole(RoleNames.Admin, RoleLevels.Account, accountName)) {
                opRes.Result = true;
                return opRes;
            }

            if (authInfo.VerifyRoleLevel(RoleNames.Admin, RoleLevels.Subscription)) {
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

        if (url == '/subscriptionsHome') {
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

        if (url == '/accountsHome' || url.startsWith('/database')) {
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

        opRes.Message = 'Unknown Url';
        opRes.Result = false;
        return opRes;        
    }

    //--------------------------------------------------------------------------------------------------------
    public static CanNavigateLevel(requiredLevel:string, authInfo: AuthorizationInfo): OperationResult {

        let opRes:OperationResult = new OperationResult();

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