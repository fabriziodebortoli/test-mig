import {OperationResult} from '../services/operationResult';
import { RoleNames, RoleLevels } from './auth-helpers';
import {AuthorizationInfo} from './auth-info';

export class UrlGuard {

    public static CanNavigateURL(url:string, authInfo: AuthorizationInfo): OperationResult {

        let opRes:OperationResult = new OperationResult();

        // instances home

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

        // generic subscription operations

        if (url == '') {
            opRes.Message = 'No check-url strategy has been implemented fot this url ' + url;
            opRes.Result = false;
            return opRes;            
        }

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