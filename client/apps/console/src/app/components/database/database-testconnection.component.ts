import { SubscriptionDatabase } from './../../model/subscriptionDatabase';
import { DatabaseService } from './../../services/database.service';
import { ActivatedRoute } from '@angular/router';
import { Component, OnInit, Input } from '@angular/core';
import { DatabaseCredentials } from '../../authentication/credentials';
import { ModelService } from 'app/services/model.service';
import { DatabaseProvider } from '../components.helper';

@Component({
  selector: 'app-database-testconnection',
  templateUrl: './database-testconnection.component.html',
  styleUrls: ['./database-testconnection.component.css']
})

export class DatabaseTestconnectionComponent implements OnInit {

  isWorking: boolean;
  dbCredentials: DatabaseCredentials;
  subscriptionKey: string;

  // opendialog variables
  openMsgDialog: boolean = false;
  msgDialog: string;

  // dropdown auxiliary variables
  providers: Array<{ name: string, value: string }> = [
    { name: 'SQL Azure', value: DatabaseProvider.SQLAZURE },
    { name: 'SQL Server', value: DatabaseProvider.SQLSERVER }
  ];

  selectedProvider: { name: string, value: string } = { name: '', value: '' };

  //--------------------------------------------------------------------------------------------------------
  constructor(
    private modelService: ModelService,
    private databaseService: DatabaseService,
    private route: ActivatedRoute) {
    this.isWorking = false;
    this.dbCredentials = new DatabaseCredentials();
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {
    this.subscriptionKey = this.route.snapshot.queryParams['subscriptionToEdit'];
  }

  //--------------------------------------------------------------------------------------------------------
  onKeyUp(event) {
    // if I press Enter I call testConnection method
    if (event.keyCode == 13) {
      this.testConnection();
    }
  }

  //--------------------------------------------------------------------------------------------------------
  testConnection() {

    this.dbCredentials.Provider = this.selectedProvider.value;

    if (this.dbCredentials.Provider == '' || this.dbCredentials.Server == '' || this.dbCredentials.Login == '') {
      this.msgDialog = 'Check your credentials first!';
      this.openMsgDialog = true;
      return;
    }

    this.isWorking = true;

    let subs = this.modelService.testConnection(this.subscriptionKey, this.dbCredentials).
      subscribe(
      result => {
        if (result.Result) {
          this.databaseService.dbCredentials = this.dbCredentials;
          this.databaseService.testConnectionOK = true;
        }
        else {
          this.msgDialog = 'Unable to connect! ' + result.Message;
          this.openMsgDialog = true;
        }

        this.isWorking = false;
        subs.unsubscribe();
      },
      error => {
        this.isWorking = false;
        subs.unsubscribe();
      }
      );
  }
}