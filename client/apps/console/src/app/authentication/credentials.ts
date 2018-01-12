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

//================================================================================
export class ChangePasswordInfo {
    
  AccountName: string;
  Password: string;
  NewPassword: string;
  Temporary: boolean;
  
  constructor() {
      this.AccountName = '';
      this.Password = '';
      this.NewPassword = '';
      this.Temporary = false;
  }
}

