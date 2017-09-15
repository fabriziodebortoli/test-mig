export class SubscriptionDatabase {

    InstanceKey: string;
    SubscriptionKey: string;
    Name: string;
    Description: string;
    DBServer: string;
    DBName: string;
    DBOwner: string;
    DBPassword: string;
	UseDMS: boolean;
    DMSDBServer: string;
    DMSDBName: string;
    DMSDBOwner: string;
    DMSDBPassword: string;
    Disabled: boolean;
    DatabaseCulture: string;
	IsUnicode: boolean;
	Language: string;
	RegionalSettings: string;
    Provider: string;
    Test: boolean;
    UnderMaintenance: boolean;

    constructor() {
    }
}