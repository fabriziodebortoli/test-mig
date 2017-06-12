export class Account {

    accountId:number;
    accountName:string;
    fullName:string;
    password:string;
    notes:string;
    email:string;
    provisioningAdmin:boolean;
    passwordNeverExpires:boolean;
    mustChangePassword:boolean;
    cannotChangePassword:boolean;
    expiryDateCannotChange:boolean;
    expiryDatePassword:Date
    disabled:boolean;
    locked:boolean;
    applicationLanguage:string;
    preferredLanguage:string;
    isWindowsAuthentication:boolean;

    constructor(){
    }
}