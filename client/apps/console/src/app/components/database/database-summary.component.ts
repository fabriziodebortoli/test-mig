import { OperationResult } from './../../services/operationResult';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ModelService } from '../../services/model.service';
import { AuthorizationProperties } from '../../authentication/auth-info';
import { AccountInfo } from '../../authentication/account-info';

@Component({
  selector: 'app-database-summary',
  templateUrl: './database-summary.component.html',
  styleUrls: ['./database-summary.component.css']
})

export class DatabaseSummaryComponent implements OnInit {

  operationsList: OperationResult[];

  isWorking: boolean;
  subscriptionKey: string;
  instanceKey: string = '';

  name: string = '';
  erpDbName: string = '';
  dmsDbName: string = '';
  dbOwner: string = '';

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private route: ActivatedRoute) { }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {

    this.isWorking = false;
    this.subscriptionKey = this.route.snapshot.queryParams['subscriptionToEdit'];

    let authorizationStored = localStorage.getItem('auth-info');

    if (authorizationStored !== null) {
      let authorizationProperties: AuthorizationProperties = JSON.parse(authorizationStored);

      let currentAccountName = authorizationProperties.accountName;
      // I need the instanceKey where the currentAccount is logged
      let localAccountInfo = localStorage.getItem(currentAccountName);

      if (localAccountInfo != null && localAccountInfo != '') {
        let accountInfo: AccountInfo = JSON.parse(localAccountInfo);
        this.instanceKey = accountInfo.instanceKey;
      }
    }

    this.createOperationsList();
  }

  //--------------------------------------------------------------------------------------------------------
  createOperationsList() {

    this.operationsList = [];

    this.name = this.instanceKey + "_" + this.subscriptionKey + "_Master";
    this.erpDbName = this.instanceKey + "_" + this.subscriptionKey + "_MasterDB";
    this.dmsDbName = this.erpDbName + "DMS";
    this.dbOwner = this.instanceKey + "_" + this.subscriptionKey + "_Admin";

    this.pushOperation('Subscription database name: ' + this.name);
    this.pushOperation('ERP database name: ' + this.erpDbName);
    this.pushOperation('DMS database name: ' + this.dmsDbName);
    this.pushOperation('DBOwner: ' + this.dbOwner);
  }

  //--------------------------------------------------------------------------------------------------------
  pushOperation(message: string) {
    let opRes: OperationResult = new OperationResult();
    opRes.Message = message;
    this.operationsList.push(opRes);
  }

  //--------------------------------------------------------------------------------------------------------
  quickStartDatabase() {

    if (this.subscriptionKey === undefined)
      return;

    this.isWorking = true;

    let subs = this.modelService.quickConfigureDatabase(this.subscriptionKey).
      subscribe(
      result => {
        console.log('*** configureDatabase result: ' + result.Message);

        subs.unsubscribe();
        this.isWorking = false;
      },
      error => {
        console.log('*** configureDatabase error: ' + error);
        subs.unsubscribe();
        this.isWorking = false;
      }
      )
  }
}
