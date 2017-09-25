export class SubscriptionDatabase {

    InstanceKey: string;
    SubscriptionKey: string;
    Name: string;
    Description: string;
    DBServer: string;
    DBName: string;
    DBOwner: string;
    DBPassword: string;
	UseDMS: boolean = true;
    DMSDBServer: string;
    DMSDBName: string;
    DMSDBOwner: string;
    DMSDBPassword: string;
    Disabled: boolean;
    DatabaseCulture: string;
	IsUnicode: boolean;
    Provider: string;
    Test: boolean;
    UnderMaintenance: boolean;

    constructor() {
    }
}