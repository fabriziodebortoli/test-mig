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
  NoOptional: boolean = true;
}
// #endregion

// #region Classi per passaggio parametri al back-end per la cancellazione dei database
/// Si compone di:
/// - oggetto SubscriptionDatabase da eliminare
/// - credenziali di amministrazione di SQL Azure (solo per questo tipo di provider e se voglio eliminare almeno un database)
/// - parametri aggiuntivi relativi all'eliminazione dei contenitori
//============================================================================
export class DeleteDatabaseBodyContent
{
  Database: SubscriptionDatabase;
  AdminCredentials: DatabaseCredentials;
  DeleteParameters: DeleteDatabaseParameters;
}

//============================================================================
export class DeleteDatabaseParameters
{
  DeleteERPDatabase: boolean = false;
  DeleteDMSDatabase: boolean = false;
}
// #endregion