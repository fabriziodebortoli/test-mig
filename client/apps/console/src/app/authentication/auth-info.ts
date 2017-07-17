import { TokenInfo } from "app/authentication/token-info";
import { Subscription } from "app/model/subscription";
import { ServerUrl } from "app/authentication/server-url";
import { RoleNames } from "app/authentication/auth-helpers";

export class AuthorizationProperties{

    jwtEncoded: string;
    accountName: string;
    preferredLanguage: string;
    applicationLanguage: string;
    tokens: Array<TokenInfo>;
    subscriptions: Array<Subscription>;
    serverUrls: Array<ServerUrl>;
    roles: Array<string>;

    constructor() {
        this.jwtEncoded = "";
        this.accountName = "";
        this.tokens = new Array<TokenInfo>();
        this.subscriptions = new Array<Subscription>();
        this.serverUrls = new Array<ServerUrl>();
        this.roles = new Array<string>();        
    }
}

export class AuthorizationInfo {

    authorizationProperties: AuthorizationProperties;

    constructor(jwt: string, accountName: string) {
        this.authorizationProperties = new AuthorizationProperties();
        this.authorizationProperties.jwtEncoded = jwt;
        this.authorizationProperties.accountName = accountName;
    }

    SetSubscriptions(subscriptions: Array<object>) {
        let subscription: Subscription;
        subscriptions.forEach(
            p => {
                subscription = new Subscription();
                subscription.subscriptionKey = p['SubscriptionKey'];
                subscription.description = p['Description'];
                subscription.minDBSizeToWarn = p['MinDBSizeToWarn'];
                subscription.preferredLanguage = p['PreferredLanguage'];
                subscription.applicationLanguage = p['ApplicationLanguage'];
                subscription.underMaintenance = p['UnderMaintenance'];
                // @@TODO con Ilaria
                subscription.activationToken = ''; 
                
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
        if (this.authorizationProperties.roles.length == 0)
        {
            return false;
        }

        let foundElementIndex = this.authorizationProperties.roles.findIndex(p => p == roleName.toString());
        return foundElementIndex >= 0;
    }
}