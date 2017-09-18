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
    
    Server: string;
    Database: string;
    Login: string;
    Password: string;
    
    constructor() {
        this.Server = '';
        this.Database = '';
        this.Login = '';
        this.Password = '';
    }
}