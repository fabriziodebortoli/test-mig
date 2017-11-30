import { SubscriptionDatabase } from './../model/subscriptionDatabase';

//================================================================================
export class Credentials {
    
    accountName: string;
    password: string;
    
    constructor() {
        this.accountName = '';
        this.password = '';
    }
}

//================================================================================
export class DatabaseCredentials {
    
    Provider: string;
    Server: string;
    Database: string;
    Login: string;
    Password: string;
    
    constructor() {
        this.Provider = '';
        this.Server = '';
        this.Database = '';
        this.Login = '';
        this.Password = '';
    }
}
