import { DatabaseInfo } from './databaseInfo';

export class Company {

    companyId: number;
    name: string;
    description: string;
	subscriptionId: number;
    companyDatabaseInfo: DatabaseInfo;
	useDMS: boolean;
    dmsDatabaseInfo: DatabaseInfo;
    disabled: boolean;
    databaseCulture: string;
	isUnicode: boolean;
	preferredLanguage: string;
	applicationLanguage: string;
	provider: string;

    constructor() {
    }
}