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
	language: string;
	regionalSettings: string;
    provider: string;
    test: boolean;

    constructor() {
    }
}