import { SubscriptionDatabase } from './../../../model/subscriptionDatabase';
import { DatabaseCredentials } from 'app/authentication/credentials';

//================================================================================
export class ExtendedSubscriptionDatabase {

  AdminCredentials: DatabaseCredentials;
  Database: SubscriptionDatabase;

  //--------------------------------------------------------------------------------------------------------
  constructor(credentials: DatabaseCredentials, subDatabase: SubscriptionDatabase) {
    this.AdminCredentials = credentials;
    this.Database = subDatabase;
  }
}