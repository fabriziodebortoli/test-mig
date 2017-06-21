export class Company {

    companyId: number;
    name: string;
    description: string;
	subscriptionKey: string;
    companyDBServer: string;
    companyDBName: string;
    companyDBOwner: string;
    companyDBPassword: string;
	useDMS: boolean;
    dmsDBServer: string;
    dmsDBName: string;
    dmsDBOwner: string;
    dmsDBPassword: string;
    disabled: boolean;
    databaseCulture: string;
	isUnicode: boolean;
	preferredLanguage: string;
	applicationLanguage: string;
	provider: string;

    constructor() {
    }
}