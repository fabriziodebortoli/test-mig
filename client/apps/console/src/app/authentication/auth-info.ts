import { Instance } from 'app/model/instance';
import { TokenInfo } from "app/authentication/token-info";
import { Subscription } from "app/model/subscription";
import { ServerUrl } from "app/authentication/server-url";
import { RoleNames } from "app/authentication/auth-helpers";

//--------------------------------------------------------------------------------------------------------
export class AuthorizationProperties{
    jwtEncoded: string;
    accountName: string;
    language: string;
    regionalSettings: string;
    AppSecurityInfo: AppSecurityInfo;
    tokens: Array<TokenInfo>;
    instances: Array<Instance>;
    subscriptions: Array<Subscription>;
    serverUrls: Array<ServerUrl>;
    roles: Array<string>;

    constructor() {
        this.jwtEncoded = "";
        this.accountName = "";
        this.AppSecurityInfo = new AppSecurityInfo();
        this.tokens = new Array<TokenInfo>();
        this.instances = new Array<Instance>();
        this.subscriptions = new Array<Subscription>();
        this.serverUrls = new Array<ServerUrl>();
        this.roles = new Array<string>();        
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
                instance.instanceKey =  p['InstanceKey'];
                instance.description = p['Description'];
                instance.disabled = p['Disabled'];
                instance.origin = p['Origin'];
                instance.tags = p['Tags'];
                instance.underMaintenance = p['UnderMaintenance'];
                this.authorizationProperties.instances.push(instance);
            }
        );
    }

    SetSubscriptions(subscriptions: Array<object>) {
        let subscription: Subscription;
        subscriptions.forEach(
            p => {
                subscription = new Subscription();
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
    
    SetRoles(roles: Array<string>) {
        roles.forEach(
            p => {
                this.authorizationProperties.roles.push(p);
            }
        );
    }

    HasRole(roleName: RoleNames): boolean {
        //@@TODO gestione ruoli: togliere queste due righe quando completa
        if (this.authorizationProperties.roles.length == 0)
            return true;

        if (this.authorizationProperties.roles.length == 0)
        {
            return false;
        }

        let foundElementIndex = this.authorizationProperties.roles.findIndex(p => p == roleName.toString());
        return foundElementIndex >= 0;
    }
}