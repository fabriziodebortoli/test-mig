import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { DatabaseService } from 'app/services/database.service';
import { SubscriptionDatabase } from 'app/model/subscriptionDatabase';
import { ActivatedRoute } from '@angular/router';
import { ModelService } from 'app/services/model.service';
import { AccountInfo } from 'app/authentication/account-info';
import { DatabaseCredentials } from '../../authentication/credentials';
import { OperationResult } from '../../services/operationResult';
import { DataChannelService } from 'app/services/data-channel.service';
import { ImportExportConsts } from '../components.helper';

@Component({
  selector: 'app-subscription-db-home',
  templateUrl: './subscription-db-home.component.html',
  styleUrls: ['./subscription-db-home.component.css']
})

export class SubscriptionDbHomeComponent implements OnInit, OnDestroy {

  model: SubscriptionDatabase;
  modelTest: SubscriptionDatabase;
  originalModel: SubscriptionDatabase;

  // caricamento informazioni configurazione dei dati di default/esempio
  // da passare poi alle singole tab che hanno le dropdown
  defaultInfo: Object;
  sampleInfo: Object;

  //--------------------------------------------------------------------------------------------------------
  constructor(
    private modelService: ModelService,
    private databaseService: DatabaseService,
    private route: ActivatedRoute,
    private dataChannelService: DataChannelService) {
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

    // in caso di edit viene usato un solo model (anche per il database di test)
    this.model.Name = dbName;

    this.databaseService.needsAskCredentials = false;
  }

  //--------------------------------------------------------------------------------------------------------
  ngAfterContentInit(): void {

    this.loadData();
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnDestroy(): void {
    this.databaseService.testConnectionOK = false;
    this.databaseService.needsAskCredentials = true;
  }

  //--------------------------------------------------------------------------------------------------------
  loadData() {

    this.modelService.getDatabase(this.model.SubscriptionKey, this.model.Name)
      .subscribe(
      res => {
        let databases: SubscriptionDatabase[] = res['Content'];

        if (databases.length == 0)
          return;

        // for each field I have to assign each value!
        this.model.assign(databases[0]);
        // I copy the original model values
        this.originalModel.assign(this.model);

        // precarico localmente le configurazioni per i dati di esempio e di default
        // cosi da propagarle in tutte le tab
        // a regime l'iso stato sara' un'informazione disponibile nella subscription
        this.loadConfigurations('IT');
      },
      err => { alert(err); }
      )
  }

  // load the list of configurations from filesystem
  //-----------------------------------------------------------------------------	
  loadConfigurations(isoCountry: string) {

    let getConfigurations = this.modelService.getConfigurations(this.model.SubscriptionKey, ImportExportConsts.Default, isoCountry).
      subscribe(
      getResult => {

        this.defaultInfo = getResult['Content'];

        let getConfigurations = this.modelService.getConfigurations(this.model.SubscriptionKey, ImportExportConsts.Sample, isoCountry).
          subscribe(
          getResult => {

            this.sampleInfo = getResult['Content'];

            this.dataChannelService.sendMessage();
            getConfigurations.unsubscribe();

            // notify model and configurations are loaded
            this.dataChannelService.sendMessage();
          },
          getError => {
            console.log(getError);
            alert(getError);
            getConfigurations.unsubscribe();
          }
          )
      },
      getError => {
        console.log(getError);
        alert(getError);
        getConfigurations.unsubscribe();
      }
      )
  }
}