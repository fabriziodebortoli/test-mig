export class Account {

    AccountName: string;
    FullName: string;
    Password: string;
    Notes: string;
    Email: string;
    LoginFailedCount: number;
    PasswordNeverExpires: boolean;
    MustChangePassword: boolean;
    CannotChangePassword: boolean;
    PasswordExpirationDate: Date;
    PasswordDuration: number;
    Disabled:boolean;
    Locked:boolean;
    ProvisioningAdmin: boolean;
    CloudAdmin: boolean;
    RegionalSettings: string;
    Language: string;
    IsWindowsAuthentication: boolean;
    ExpirationDate: Date;
    ParentAccount: string;
    Confirmed: boolean;
    
    constructor(){
    }
}