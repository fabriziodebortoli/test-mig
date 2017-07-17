export class SubscriptionDatabase {

    subscriptionKey: string;
    name: string;
    description: string;
    dbServer: string;
    dbName: string;
    dbOwner: string;
    dbPassword: string;
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
    test: boolean;

    constructor() {
    }
}