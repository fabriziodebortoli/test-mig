import { DatabaseService } from './../../services/database.service';
import { AccountInfo } from './../../authentication/account-info';
import { ActivatedRoute } from '@angular/router';
import { ModelService } from './../../services/model.service';
import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { SubscriptionDatabase } from '../../model/subscriptionDatabase';
import { Observable } from 'rxjs';
import { OperationResult } from '../../services/operationResult';
import { ExtendedSubscriptionDatabase } from '../../authentication/credentials';

@Component({
  selector: 'app-subscription-database',
  templateUrl: './subscription-database.component.html',
  styleUrls: ['./subscription-database.component.css']
})

export class SubscriptionDatabaseComponent implements OnInit {

  @Input() model: SubscriptionDatabase;
  @Input() originalModel: SubscriptionDatabase;

  editing: boolean = false;
  openMsgDialog: boolean = false;
  msgDialog: string;

  operationsList: OperationResult[];
  extendedSubDatabase: ExtendedSubscriptionDatabase;

  elaborationCompletedWithErrors: boolean;
  checkDatabaseIsStarted: boolean = false;

  // dropdown auxiliary variables
  providers: Array<{ name: string, value: string }> = [
    { name: 'SQL Azure', value: 'SQLAzure' },
    { name: 'SQL Server', value: 'SQLServer' }
  ];

  selectedProvider: { name: string, value: string };

  //--------------------------------------------------------------------------------------------------------
  constructor(
    private modelService: ModelService,
    private databaseService: DatabaseService,
    private route: ActivatedRoute) {
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {

    // I prefill input data if is the first configuration
    if (this.model.Name === undefined || this.model.Name === '') {
      let suffix = this.model.Test ? '_Test' : '_Master'
      let prefix = this.model.InstanceKey + '_' + this.model.SubscriptionKey;
      this.model.Name = prefix + suffix;
      this.model.Provider = this.databaseService.dbCredentials.Provider;
      this.model.DBName = this.model.Name + '_DB';
      this.model.DBOwner = prefix + '_Admin';
      this.model.DMSDBName = this.model.Name + '_DBDMS';
      this.model.DMSDBOwner = prefix + '_Admin';
      // to initialize the provider in dropdown
      this.initProviderValueDropDown();
      return;
    }

    this.editing = true;
    this.databaseService.needsAskCredentials = false;

    // I load the database information only if Name is filled

    this.modelService.getDatabase(this.model.SubscriptionKey, this.model.Name)
      .subscribe(
      res => {
        let databases: SubscriptionDatabase[] = res['Content'];

        if (databases.length == 0)
          return;

        // for each field we have to assign the value!
        this.model.assign(databases[0]);
        // to initialize the provider in dropdown
        this.initProviderValueDropDown();
        // I copy the original model values
        this.originalModel.assign(this.model);
      },
      err => { alert(err); }
      )
  }

  //--------------------------------------------------------------------------------------------------------
  initProviderValueDropDown() {
    let currentProvider = this.providers.find(p => p.value === this.model.Provider);
    this.selectedProvider = currentProvider;
  }

  //--------------------------------------------------------------------------------------------------------
  onProviderValueChange(providerValue) {
    this.model.Provider = providerValue.value;
  }

  //--------------------------------------------------------------------------------------------------------
  getMessageDialog(result: boolean): string {
    return result ? 'Operation successfully completed' : 'Operation ended with errors';
  }

  //--------------------------------------------------------------------------------------------------------
  save() {
    // first I check input values 
    if (!this.validateInput())
      return;

    if (this.editing) {
      // se sono in modifica tutti i campi sono readonly, quindi vado a salvare solo l'anagrafica senza check
      if (!this.onlyMainDataHasChanged()) {
        this.msgDialog = 'No data has changed, so no save operation is needed';
        this.openMsgDialog = true;
        return;
      }
      else {
        this.saveDatabase();
      }
    }
    else {
      // se sto inserendo i dati manualmente devo eseguire il check preventivo di tutti i dati
      this.saveDatabaseWithCheck();
    }
  }

  // esegue il salvataggio dell'anagrafica senza controlli
  //--------------------------------------------------------------------------------------------------------
  saveDatabase() {

    let runUpdate = this.modelService.saveDatabase(this.model).
      subscribe(
      runUpdateResult => {

        this.msgDialog = this.getMessageDialog(runUpdateResult.Result);
        this.openMsgDialog = true;

        runUpdate.unsubscribe();
      },
      runUpdateError => {
        console.log(runUpdateError);
        alert(runUpdateError);
        runUpdate.unsubscribe();
      }
      )
  }

  // esegue il salvataggio dell'anagrafica CON i vari controlli preventivi sui valori inseriti
  //--------------------------------------------------------------------------------------------------------
  saveDatabaseWithCheck() {

    this.checkDatabaseIsStarted = true;
    this.elaborationCompletedWithErrors = false;

    let test = this.modelService.testConnection(this.model.SubscriptionKey, this.databaseService.dbCredentials).
      subscribe(
      testResult => {

        if (testResult.Result) {

          this.extendedSubDatabase = new ExtendedSubscriptionDatabase(this.databaseService.dbCredentials, this.model);

          let update = this.modelService.checkDatabase(this.model.SubscriptionKey, this.extendedSubDatabase).
            subscribe(
            checkResult => {

              if (checkResult.Result) {
                // se il check va a buon fine procedo con il vero e proprio update
                let runUpdate = this.modelService.updateDatabase(this.model.SubscriptionKey, this.extendedSubDatabase).
                  subscribe(
                  runUpdateResult => {

                    if (!runUpdateResult.Result) {
                      this.elaborationCompletedWithErrors = true;
                      // qui dovrei avere una lista di operazioni fallite da visualizzare
                    }

                    this.msgDialog = this.getMessageDialog(runUpdateResult.Result);
                    this.openMsgDialog = true;

                    this.checkDatabaseIsStarted = false;

                    runUpdate.unsubscribe();
                  },
                  runUpdateError => {
                    console.log(runUpdateError);
                    alert(runUpdateError);
                    runUpdate.unsubscribe();
                  }
                  )
              }

              if (!checkResult.Result) {
                // se il check ha ritornato degli errori mostro nuovamente i controlli con i dati
                this.checkDatabaseIsStarted = false;
                this.elaborationCompletedWithErrors = true;
              }

              this.operationsList = checkResult['Content'];

              update.unsubscribe();
            },
            updateError => {
              console.log(updateError);
              alert(updateError);
              update.unsubscribe();
            }
            )
        }
        else
          alert('Unable to connect! ' + testResult.Message);

        test.unsubscribe();
      },
      error => {
        console.log(error);
        alert(error);
        test.unsubscribe();
      }
      );
  }

  //--------------------------------------------------------------------------------------------------------
  validateInput(): boolean {
    if (this.model.SubscriptionKey == undefined || this.model.Name == undefined) {
      alert('Mandatory fields are empty! Check subscription key / database name!');
      return false;
    }

    if (this.model.Provider === '' ||
      this.model.DBServer === '' || this.model.DBName === '' || this.model.DBOwner === '' ||
      this.model.DMSDBServer === '' || this.model.DMSDBName === '' || this.model.DMSDBOwner === '') {
      alert('Mandatory fields are empty! Check databases information!');
      return false;
    }

    if (this.model.DBServer !== this.model.DMSDBServer) {
      alert('Both databases must be in the same server!');
      return false;
    }

    if (this.model.DBName === this.model.DMSDBName) {
      alert('The databases names must be different!');
      return false;
    }

    if (this.model.DBOwner === this.model.DMSDBOwner) {
      if (this.model.DBPassword !== this.model.DMSDBPassword) {
        alert('Passwords different for same users!');
        return false;
      }
    }

    return true;
  }

  // ritorna true solo se solo i pochi dati anagrafici sono variati (cosi si procede con un semplice update senza controlli)
  //--------------------------------------------------------------------------------------------------------
  onlyMainDataHasChanged(): boolean {

    if (this.originalModel.Description !== this.model.Description ||
      this.originalModel.Disabled !== this.model.Disabled) {

      if (this.originalModel.Provider === this.model.Provider &&
        this.originalModel.IsUnicode === this.model.IsUnicode &&
        this.originalModel.DBServer === this.model.DBServer &&
        this.originalModel.DBName === this.model.DBName &&
        this.originalModel.DBOwner === this.model.DBOwner &&
        this.originalModel.DBPassword === this.model.DBPassword &&
        this.originalModel.DMSDBServer === this.model.DMSDBServer &&
        this.originalModel.DMSDBName === this.model.DMSDBName &&
        this.originalModel.DMSDBOwner === this.model.DMSDBOwner &&
        this.originalModel.DMSDBPassword === this.model.DMSDBPassword
      )
        return true;
    }
    return false;
  }
}
