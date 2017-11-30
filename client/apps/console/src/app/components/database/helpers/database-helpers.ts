import { SubscriptionDatabase } from './../../../model/subscriptionDatabase';
import { DatabaseCredentials } from 'app/authentication/credentials';

//================================================================================
export class ExtendedSubscriptionDatabase {

  AdminCredentials: DatabaseCredentials;
  Database: SubscriptionDatabase;

  //------------------------------------------------------------------------------
  constructor(credentials: DatabaseCredentials, subDatabase: SubscriptionDatabase) {
    this.AdminCredentials = credentials;
    this.Database = subDatabase;
  }
}

// #region Classi per passaggio parametri al back-end per l'importazione dati di default/esempio
//================================================================================
export class ImportDataBodyContent {

  Database: SubscriptionDatabase;
  ImportParameters: ImportDataParameters;
}  

//================================================================================
export class ImportDataParameters {
  OverwriteRecord: boolean = true;
  DeleteTableContext: boolean = false;
}
// #endregion
