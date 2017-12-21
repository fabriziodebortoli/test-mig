export class SubscriptionDatabase {

    InstanceKey: string;
    SubscriptionKey: string;
    Name: string;
    Description: string = '';
    DBServer: string;
    DBName: string;
    DBOwner: string;
    DBPassword: string;
	  UseDMS: boolean = true;
    DMSDBServer: string;
    DMSDBName: string;
    DMSDBOwner: string;
    DMSDBPassword: string;
    Disabled: boolean = false;
    DatabaseCulture: string;
	  IsUnicode: boolean = false;
    Provider: string;
    Test: boolean = false;
    UnderMaintenance: boolean = false;

    constructor() {
    }

    assign(subDatabase?: SubscriptionDatabase) {

        this.InstanceKey = subDatabase.InstanceKey;
        this.SubscriptionKey = subDatabase.SubscriptionKey;
        this.Name = subDatabase.Name;
        this.Description = subDatabase.Description;
        this.DBServer = subDatabase.DBServer;
        this.DBName = subDatabase.DBName;
        this.DBOwner = subDatabase.DBOwner
        this.DBPassword = subDatabase.DBPassword;
        this.UseDMS = subDatabase.UseDMS;
        this.DMSDBServer = subDatabase.DMSDBServer;
        this.DMSDBName = subDatabase.DMSDBName;
        this.DMSDBOwner = subDatabase.DMSDBOwner;
        this.DMSDBPassword = subDatabase.DMSDBPassword;
        this.Disabled = subDatabase.Disabled;
        this.DatabaseCulture = subDatabase.DatabaseCulture;
        this.IsUnicode = subDatabase.IsUnicode;
        this.Provider  = subDatabase.Provider;
        this.Test = subDatabase.Test;
        this.UnderMaintenance = subDatabase.UnderMaintenance;
    }

    
}