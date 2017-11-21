import { Component, OnInit, Input } from '@angular/core';
import { SubscriptionDatabase } from 'app/model/subscriptionDatabase';
import { ModelService } from '../../services/model.service';
import { OperationResult } from '../../services/operationResult';

@Component({
  selector: 'app-database-upgrade',
  templateUrl: './database-upgrade.component.html',
  styleUrls: ['./database-upgrade.component.css']
})

export class DatabaseUpgradeComponent implements OnInit {

  @Input() model: SubscriptionDatabase;

  isCheckDbRunning: boolean;
  operationsList: OperationResult[];

  canUpdateDb: boolean;
  isUpdateDbRunning: boolean;

  // default data variables
  importDefaultData: boolean = true;
  // lista configurazione dati di default (dovrebbe essere ritornata dall'esterno, in base ai folder presenti nell'installazione
  // per l'ISO stato di attivazione)
  configurations: Array<{ name: string, value: string }> = [
    { name: 'Basic', value: 'Basic' },
    { name: 'Manufacturing-Advanced', value: 'Manufacturing-Advanced' },
    { name: 'Manufacturing-Basic', value: 'Manufacturing-Basic' }
  ];
  selectedConfiguration: { name: string, value: string };
  //

  //-----------------------------------------------------------------------------	
  constructor(private modelService: ModelService) {
  }

  //-----------------------------------------------------------------------------	
  ngOnInit() {

    this.initOperationsList();

    // init configuration dropdown
    this.selectedConfiguration = this.configurations[0];
  }

  //--------------------------------------------------------------------------------------------------------
  initOperationsList() {
    this.operationsList = [];
  }

  //--------------------------------------------------------------------------------------------------------
  onConfigurationChange(configuration) {
    this.selectedConfiguration = configuration.value;
  }

  //--------------------------------------------------------------------------------------------------------
  importDefaultDataChanged(setImportDefaultData: boolean) {
    // assign current variable
    this.importDefaultData = setImportDefaultData;
    // set configurationdropdown readonly
  }

  //--------------------------------------------------------------------------------------------------------
  checkDatabase() {

    // se l'elaborazione e' in esecuzione ritorno
    if (this.isCheckDbRunning)
      return;

    this.canUpdateDb = false;
    this.isCheckDbRunning = true;
    this.initOperationsList();

    let checkDb = this.modelService.checkDatabaseStructure(this.model.SubscriptionKey, this.model).
      subscribe(
      checkDbResult => {

        // l'elaborazione e' terminata
        this.isCheckDbRunning = false;

        this.operationsList = checkDbResult['Content'];

        // se il Code == -1 significa che:
        // - i database sono gia' aggiornati
        // - i database sono privi della TB_DBMark e pertanto sono in uno stato non recuperabile
        // quindi non e' possibile procedere con l'upgrade
        this.canUpdateDb = checkDbResult.Code > -1;

        checkDb.unsubscribe();
      },
      checkDbError => {
        this.isCheckDbRunning = false;
        console.log(checkDbError);
        alert(checkDbError);
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

        // qui dovrei visualizzare il risultato dell'aggiornamento
        // decidere lato back-end quali dettagli ritornare e mostrare all'utente
        //this.operationsList = updateDbResult['Content'];

        updateDb.unsubscribe();
      },
      checkDbError => {
        this.isUpdateDbRunning = false;
        console.log(checkDbError);
        alert(checkDbError);
        this.initOperationsList();
        updateDb.unsubscribe();
      }
      )
  }
}
