export class Account {

    accountName: string;
    fullName: string;
    password: string;
    notes: string;
    email: string;
    loginFailedCount: number;
    passwordNeverExpires: boolean;
    mustChangePassword: boolean;
    cannotChangePassword: boolean;
    passwordExpirationDate: Date;
    passwordDuration: number;
    disabled:boolean;
    locked:boolean;
    provisioningAdmin: boolean;
    applicationLanguage: string;
    preferredLanguage: string;
    isWindowsAuthentication: boolean;
    expirationDate: Date;
    
    constructor(){
    }
}