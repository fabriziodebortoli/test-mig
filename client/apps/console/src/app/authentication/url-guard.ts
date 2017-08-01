import {OperationResult} from '../services/operationResult';
import { RoleNames, RoleLevels } from './auth-helpers';
import {AuthorizationInfo} from './auth-info';

export class UrlGuard {

    public static CanNavigate(url:string, authInfo: AuthorizationInfo): OperationResult {

        let opRes:OperationResult = new OperationResult();

        // checking permission by specific component-url

        // checking instances

        if (url == '/instancesHome') {
            if (!authInfo.VerifyRole(RoleNames.Admin, RoleLevels.Instance, "*")) {
                opRes.Message = RoleNames.Admin + ' role missing';
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

        if (url == '') {
            opRes.Message = 'No check-url strategy has been implemented fot this url ' + url;
            opRes.Result = false;
            return opRes;            
        }

        // checking permission by specific level

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