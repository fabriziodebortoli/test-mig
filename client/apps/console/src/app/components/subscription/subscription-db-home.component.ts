import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { DatabaseService } from 'app/services/database.service';
import { SubscriptionDatabase } from 'app/model/subscriptionDatabase';
import { ActivatedRoute } from '@angular/router';
import { ModelService } from 'app/services/model.service';
import { AccountInfo } from 'app/authentication/account-info';
import { DatabaseCredentials, ExtendedSubscriptionDatabase } from '../../authentication/credentials';
import { OperationResult } from '../../services/operationResult';

@Component({
  selector: 'app-subscription-db-home',
  templateUrl: './subscription-db-home.component.html',
  styleUrls: ['./subscription-db-home.component.css']
})

export class SubscriptionDbHomeComponent implements OnInit, OnDestroy {

  model: SubscriptionDatabase;
  modelTest: SubscriptionDatabase;
  originalModel: SubscriptionDatabase;

  //--------------------------------------------------------------------------------------------------------
  constructor(
    private modelService: ModelService,
    private databaseService: DatabaseService,
    private route: ActivatedRoute) {
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {
    // I read queryparams
    let subscriptionKey: string = this.route.snapshot.queryParams['subscriptionToEdit'];
    if (subscriptionKey === undefined) {
      return;
    }

    let dbName = this.route.snapshot.queryParams['databaseToEdit'];

    // istanzio i due model (master e test)
    this.model = new SubscriptionDatabase();
    this.modelTest = new SubscriptionDatabase();
    this.originalModel = new SubscriptionDatabase();
    
    this.model.SubscriptionKey = this.modelTest.SubscriptionKey = subscriptionKey;

    // I need the instanceKey where the currentAccount is logged
    let localAccountInfo = localStorage.getItem(this.modelService.currentAccountName);
    if (localAccountInfo != null && localAccountInfo != '') {
      let accountInfo: AccountInfo = JSON.parse(localAccountInfo);
      this.model.InstanceKey = this.modelTest.InstanceKey = accountInfo.instanceKey;
    }

    if (dbName === undefined) {
      this.model.Test = false;
      this.modelTest.Test = true;
      return;
    }

    this.databaseService.needsAskCredentials = false;

    // in caso di edit viene usato un solo model (anche per il database di test)
    this.model.Name = dbName;
 }

  //--------------------------------------------------------------------------------------------------------
  ngOnDestroy(): void {
    this.databaseService.testConnectionOK = false;
    this.databaseService.needsAskCredentials = true;
  }
}