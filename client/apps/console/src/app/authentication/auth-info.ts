import { Instance } from 'app/model/instance';
import { TokenInfo } from "app/authentication/token-info";
import { AppSubscription } from "app/model/subscription";
import { ServerUrl } from "app/authentication/server-url";
import { RoleNames } from "app/authentication/auth-helpers";
import { AccountRole } from "app/model/accountrole";

//--------------------------------------------------------------------------------------------------------
export class AuthorizationProperties{
    jwtEncoded: string;
    accountName: string;
    language: string;
    regionalSettings: string;
    AppSecurityInfo: AppSecurityInfo;
    tokens: Array<TokenInfo>;
    instances: Array<Instance>;
    subscriptions: Array<AppSubscription>;
    serverUrls: Array<ServerUrl>;
    roles: Array<AccountRole>;

    constructor() {
        this.jwtEncoded = "";
        this.accountName = "";
        this.AppSecurityInfo = new AppSecurityInfo();
        this.tokens = new Array<TokenInfo>();
        this.instances = new Array<Instance>();
        this.subscriptions = new Array<AppSubscription>();
        this.serverUrls = new Array<ServerUrl>();
        this.roles = new Array<AccountRole>();        
    }
}

//--------------------------------------------------------------------------------------------------------
export class AppSecurityInfo {
    AppId: string;
    SecurityValue: string;

    constructor() {
        this.AppId = '';
        this.SecurityValue = '';
    }
}

//--------------------------------------------------------------------------------------------------------
export class AuthorizationInfo {
    authorizationProperties: AuthorizationProperties;

    constructor(jwt: string, accountName: string) {
        this.authorizationProperties = new AuthorizationProperties();
        this.authorizationProperties.jwtEncoded = jwt;
        this.authorizationProperties.accountName = accountName;
    }

    SetSecurityValues(asi: AppSecurityInfo) {
        this.authorizationProperties.AppSecurityInfo.AppId = asi.AppId; 
        this.authorizationProperties.AppSecurityInfo.SecurityValue = asi.SecurityValue; 
    }

    SetInstances(instances: Array<object>) {
        let instance: Instance;
        instances.forEach(
            p => {
                instance = new Instance();
                instance.InstanceKey =  p['InstanceKey'];
                instance.Description = p['Description'];
                instance.Disabled = p['Disabled'];
                instance.Origin = p['Origin'];
                instance.Tags = p['Tags'];
                instance.UnderMaintenance = p['UnderMaintenance'];
                this.authorizationProperties.instances.push(instance);
            }
        );
    }

    SetSubscriptions(subscriptions: Array<object>) {
        let subscription: AppSubscription;
        subscriptions.forEach(
            p => {
                subscription = new AppSubscription();
                subscription.SubscriptionKey = p['SubscriptionKey'];
                subscription.Description = p['Description'];
                subscription.MinDBSizeToWarn = p['MinDBSizeToWarn'];
                subscription.Language = p['language'];
                subscription.RegionalSettings = p['RegionalSettings'];
                subscription.UnderMaintenance = p['UnderMaintenance'];
                // @@TODO con Ilaria
                subscription.ActivationToken = ''; 
                
                this.authorizationProperties.subscriptions.push(subscription);
            }
        );
    }

    SetServerUrls(serverUrls: Array<object>) {
        let serverUrl: ServerUrl;
        serverUrls.forEach(
            p => {
                serverUrl = new ServerUrl(p['UrlType'], p['Url'], p['AppName']);
                this.authorizationProperties.serverUrls.push(serverUrl);
            }
        );
    }

    SetTokens(tokens: Array<object>) {
        let token: TokenInfo;
        tokens.forEach(
            p => {
                token = new TokenInfo(p['Token'], p['ExpirationDate'], p['TokenType'])
                this.authorizationProperties.tokens.push(token);
            }
        );
    }
    
    SetRoles(roles: Array<AccountRole>) {
        roles.forEach(
            p => {
                this.authorizationProperties.roles.push(p);
            }
        );
    }

    HasRoleName(roleName: string): boolean {
        
        if (this.authorizationProperties.roles.length == 0)
        {
            return false;
        }

        let foundElementIndex = this.authorizationProperties.roles.findIndex(p => p.RoleName.toUpperCase() == roleName.toUpperCase());
        return foundElementIndex >= 0;
    }

    VerifyRole(roleName:string, level: string, entityKey:string): boolean {
        
        if (this.authorizationProperties.roles.length == 0)
        {
            return false;
        }

        let foundElementIndex = this.authorizationProperties.roles.findIndex(
            (p => p.RoleName.toUpperCase() == roleName.toUpperCase()) &&
            (p => p.Level.toUpperCase() == level.toUpperCase()) &&
            (p => p.EntityKey.toUpperCase() == entityKey.toUpperCase())
        );

        return foundElementIndex >= 0;
    }

    VerifyRoleLevel(roleName:string, level: string): boolean {

        if (this.authorizationProperties.roles.length == 0)
        {
            return false;
        }

        let foundElementIndex = this.authorizationProperties.roles.findIndex(
            (p => p.RoleName.toUpperCase() == roleName.toUpperCase()) &&
            (p => p.Level.toUpperCase() == level.toUpperCase())
        );

        return foundElementIndex >= 0;
    }
}