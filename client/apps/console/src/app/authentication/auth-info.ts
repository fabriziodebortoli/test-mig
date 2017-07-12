import { TokenInfo } from "app/authentication/token-info";
import { Subscription } from "app/model/subscription";
import { ServerUrl } from "app/authentication/server-url";

export class AuthInfo {

    jwtEncoded: string;
    accountName: string;
    provisioningAdmin: boolean;
    cloudAdmin: boolean;
    preferredLanguage: string;
    applicationLanguage: string;
    tokens: Array<TokenInfo>;
    subscriptions: Array<Subscription>;
    serverUrls: Array<ServerUrl>;
    roles: Array<string>;

    constructor(jwt: string, accountName: string) {
        this.jwtEncoded = jwt;
        this.accountName = accountName;
        this.cloudAdmin = false;
        this.provisioningAdmin = false;
        this.tokens = new Array<TokenInfo>();
        this.subscriptions = new Array<Subscription>();
        this.serverUrls = new Array<ServerUrl>();
        this.roles = new Array<string>();
    }

    SetSubscriptions(subscriptions: Array<object>) {
        let subscription: Subscription;
        subscriptions.forEach(
            p => {
                subscription = new Subscription();
                subscription.subscriptionKey = p['SubscriptionKey'];
                subscription.applicationLanguage = p['ApplicationLanguage'];
                subscription.description = p['Description'];
                subscription.instanceKey = p['InstanceKey'];
                subscription.minDBSizeToWarn = p['MinDBSizeToWarn'];
                subscription.preferredLanguage = p['PreferredLanguage'];
                // @@TODO con Ilaria
                subscription.activationToken = ''; 
                
                this.subscriptions.push(subscription);
            }
        );
    }

    SetServerUrls(serverUrls: Array<object>) {
        let serverUrl: ServerUrl;
        serverUrls.forEach(
            p => {
                serverUrl = new ServerUrl(p['UrlType'], p['Url'], p['AppName']);
                this.serverUrls.push(serverUrl);
            }
        );
    }

    SetTokens(tokens: Array<object>) {
        let token: TokenInfo;
        tokens.forEach(
            p => {
                token = new TokenInfo(p['Token'], p['ExpirationDate'], p['TokenType'])
                this.tokens.push(token);
            }
        );
    }
    
    SetRoles(roles: Array<string>) {
        roles.forEach(
            p => {
                this.roles.push(p);
            }
        );
    }    
}