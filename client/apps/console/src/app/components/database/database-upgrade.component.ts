import { Component, OnInit, Input } from '@angular/core';
import { SubscriptionDatabase } from 'app/model/subscriptionDatabase';
import { ModelService } from '../../services/model.service';
import { OperationResult } from '../../services/operationResult';
import { DataChannelService } from '../../services/data-channel.service';

@Component({
  selector: 'app-database-upgrade',
  templateUrl: './database-upgrade.component.html',
  styleUrls: ['./database-upgrade.component.css']
})

export class DatabaseUpgradeComponent implements OnInit {

  @Input() model: SubscriptionDatabase;
  @Input() defaultInfo: Object;

  isCheckDbRunning: boolean;
  operationsList: OperationResult[];

  canUpdateDb: boolean;
  isUpdateDbRunning: boolean;
  errorMsg: string;

  // Opendialog variables
  openMsgDialog: boolean = false;
  msgDialog: string;

  // default data variables
  importDefaultData: boolean = true;
  // lista configurazioni per i dati di default
  configurations: Array<{ name: string, value: string }> = [];
  selectedConfiguration: { name: string, value: string };
  //

  //-----------------------------------------------------------------------------	
  constructor(
    private modelService: ModelService,
    private dataChannelService: DataChannelService) {
  }

  //-----------------------------------------------------------------------------	
  ngOnInit() {
    // to initialize the default data configurations in dropdown
    this.dataChannelService.dataChannel.subscribe(
      (res) => {
        this.initConfigurationsDropDown();
      },
      (err) => { }
    );
  }

  //--------------------------------------------------------------------------------------------------------
  initConfigurationsDropDown() {

    let values: Array<string> = [];

    for (let key in this.defaultInfo) {
      if (key === 'IT')
        values = this.defaultInfo[key];
    }

    // inserisco le configurazioni nella dropdown
    values.forEach((item, index) => {
      this.configurations.push({ name: item, value: item })
    });

    this.selectedConfiguration = this.configurations[0];
  }

  //--------------------------------------------------------------------------------------------------------
  onConfigurationChange(configuration) {
    this.selectedConfiguration = configuration.value;
  }

  //--------------------------------------------------------------------------------------------------------
  importDefaultDataChanged(setImportDefaultData: boolean) {
    // assign current variable
    this.importDefaultData = setImportDefaultData;
  }

  //--------------------------------------------------------------------------------------------------------
  checkDatabase() {

    this.errorMsg = '';

    // se l'elaborazione e' in esecuzione ritorno
    if (this.isCheckDbRunning)
      return;

    this.canUpdateDb = false;
    this.isCheckDbRunning = true;
    this.operationsList = [];

    let checkDb = this.modelService.checkDatabaseStructure(this.model.SubscriptionKey, this.model).
      subscribe(
      checkDbResult => {

        // l'elaborazione e' terminata
        this.isCheckDbRunning = false;

        if (checkDbResult.Result) {
          // se il Code != 0 significa che:
          // - i database sono gia' aggiornati
          // - i database sono privi della TB_DBMark e pertanto sono in uno stato non recuperabile
          // quindi non e' possibile procedere con l'upgrade
          // (oppure potrebbe non essere andata a buon fine la connessione ai database della subscription)
          this.canUpdateDb = checkDbResult.Code == 0;

          this.operationsList = checkDbResult['Content'];
        }
        else {
          this.canUpdateDb = checkDbResult.Result;
          this.errorMsg = checkDbResult.Message;
        }

        checkDb.unsubscribe();
      },
      checkDbError => {
        this.isCheckDbRunning = false;
        console.log(checkDbError);
        this.errorMsg = checkDbError;
        checkDb.unsubscribe();
      }
      )
  }

  //--------------------------------------------------------------------------------------------------------
  updateDatabase() {

    // se l'elaborazione e' in esecuzione ritorno
    if (this.isUpdateDbRunning)
      return;

    this.isUpdateDbRunning = true;

    let updateDb = this.modelService.upgradeDatabaseStructure(
      this.model.SubscriptionKey,
      this.importDefaultData ? this.selectedConfiguration.value : '',
      this.model).
      subscribe(
      updateDbResult => {

        // l'elaborazione e' terminata
        this.isUpdateDbRunning = false;
        this.canUpdateDb = false;

        // @@TODO: dovrei visualizzare il risultato dell'aggiornamento
        // decidere lato back-end quali dettagli ritornare e mostrare all'utente
        // this.operationsList = updateDbResult['Content'];
        this.operationsList = [];

        this.msgDialog = 'Operation completed!';
        this.openMsgDialog = true;

        updateDb.unsubscribe();
      },
      checkDbError => {
        this.isUpdateDbRunning = false;
        console.log(checkDbError);
        alert(checkDbError);
        this.operationsList = [];
        updateDb.unsubscribe();
      }
      )
  }
}
