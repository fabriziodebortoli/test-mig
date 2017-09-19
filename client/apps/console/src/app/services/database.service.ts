import { DatabaseCredentials } from './../authentication/credentials';
import { Injectable } from '@angular/core';

@Injectable()
export class DatabaseService {
  
  dbCredentials: DatabaseCredentials;
  needsAskCredentials: boolean;
  testConnectionOK: boolean;
  
  //--------------------------------------------------------------------------------------------------------
  constructor() { 
    this.dbCredentials = new DatabaseCredentials();
    this.needsAskCredentials = true;
    this.testConnectionOK = false;
  }
}
