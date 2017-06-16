import { DatabaseInfo } from './databaseInfo';

export class Company {

    companyId: number;
    name: string;
    description: string;
	subscriptionId: number;
    companyDBServer: string;
    companyDBName: string;
    companyDBOwner: string;
    companyDBPassword: string;
    // companyDatabaseInfo: DatabaseInfo;
	useDMS: boolean;
    // dmsDatabaseInfo: DatabaseInfo;
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