import { DatabaseService } from './../../services/database.service';
import { AccountInfo } from './../../authentication/account-info';
import { ActivatedRoute } from '@angular/router';
import { ModelService } from './../../services/model.service';
import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { SubscriptionDatabase } from '../../model/subscriptionDatabase';
import { Observable } from 'rxjs';
import { OperationResult } from '../../services/operationResult';
import { ExtendedSubscriptionDatabase } from '../../authentication/credentials';
import { DataChannelService } from 'app/services/data-channel.service';

@Component({
  selector: 'app-subscription-database',
  templateUrl: './subscription-database.component.html',
  styleUrls: ['./subscription-database.component.css']
})

export class SubscriptionDatabaseComponent implements OnInit {

  @Input() model: SubscriptionDatabase;
  @Input() originalModel: SubscriptionDatabase;

  editing: boolean = false;
  step: number;

  // opendialog variables
  openMsgDialog: boolean = false;
  msgDialog: string;

  // operationslist to confirm
  operationsList: OperationResult[];
  extendedSubDatabase: ExtendedSubscriptionDatabase;

  // for check elaboration
  checkDbIsStarted: boolean = false;
  checkDbIsRunning: boolean = false;
  checkDbHasErrors: boolean = false;
  saveDbIsRunning: boolean = false;

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
    private route: ActivatedRoute,
    private dataChannelService: DataChannelService) {
    this.step = 1;
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
      this.model.DBPassword = '';
      this.model.DMSDBName = this.model.Name + '_DBDMS';
      this.model.DMSDBOwner = prefix + '_Admin';
      this.model.DMSDBPassword = '';

      // to initialize the provider in dropdown
      this.initProviderValueDropDown();
      return;
    }

    this.editing = true;
    this.databaseService.needsAskCredentials = false;

    // to initialize the provider in dropdown
    this.dataChannelService.dataChannel.subscribe(
      (res) => {
        this.initProviderValueDropDown();
      },
      (err) => {}
    );
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnDestroy() {
    this.dataChannelService.dataChannel.unsubscribe();
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

  // evento intercettato sul click del pulsante Back
  //--------------------------------------------------------------------------------------------------------
  returnToMainData() {
    // reimposto il numero di step = 1
    this.step = 1;
    // su questo booleano si basa *ngif per mostrare i control
    this.checkDbIsStarted = false;
    // svuoto la lista dei msg
    this.operationsList = [];
  }

  // evento intercettato sul click del pulsante Save
  //--------------------------------------------------------------------------------------------------------
  submit() {
    // first I check input values 
    if (!this.validateInput())
      return;

    // se sono in modifica tutti i campi sono readonly, quindi vado a salvare solo l'anagrafica senza check
    if (this.editing) {

      if (!this.onlyMainDataHasChanged()) {

        this.msgDialog = 'No data has changed, so no save operation is needed';
        this.openMsgDialog = true;
        return;
      }
      else
        this.updateDatabase();
    }
    else {
      switch (this.step) {
        case 1:
          this.checkDatabase();
          break;

        case 2:
          this.saveDatabase();
          break;
      }
    }
  }

  // esegue il salvataggio dell'anagrafica senza controlli
  //--------------------------------------------------------------------------------------------------------
  updateDatabase() {

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
  saveDatabase() {

    this.checkDbIsRunning = true; // mostro la gif di elaborazione 
    this.operationsList = []; // pulisco i msg
    this.saveDbIsRunning = true;

    let test = this.modelService.testConnection(this.model.SubscriptionKey, this.databaseService.dbCredentials).
      subscribe(
      testResult => {

        if (testResult.Result) {

          this.extendedSubDatabase = new ExtendedSubscriptionDatabase(this.databaseService.dbCredentials, this.model);

          let update = this.modelService.updateDatabase(this.model.SubscriptionKey, this.extendedSubDatabase).
            subscribe(
            updateResult => {

              this.msgDialog = this.getMessageDialog(updateResult.Result);
              this.openMsgDialog = true;

              this.checkDbIsRunning = false;
              this.saveDbIsRunning = false;

              if (updateResult.Result) {
                this.returnToMainData();
                this.editing = true;
                this.originalModel.assign(this.model);
              }
              else {
                // se il check ha ritornato degli errori mostro nuovamente i controlli con i dati
                this.operationsList = updateResult['Content'];
              }

              update.unsubscribe();
            },
            updateError => {
              console.log(updateError);
              alert(updateError);
              update.unsubscribe();
            }
            )
        }
        else {
          alert('Unable to connect! ' + testResult.Message);
          this.checkDbIsRunning = false;
          this.saveDbIsRunning = false;
        }
        test.unsubscribe();
      },
      error => {
        console.log(error);
        alert(error);
        test.unsubscribe();
      }
      );
  }

  // esegue il check dell'anagrafica CON i vari controlli preventivi sui valori inseriti
  //--------------------------------------------------------------------------------------------------------
  checkDatabase() {

    this.checkDbIsRunning = this.checkDbIsStarted = true;
    this.checkDbHasErrors = false;
    this.step = 2;

    let test = this.modelService.testConnection(this.model.SubscriptionKey, this.databaseService.dbCredentials).
      subscribe(
      testResult => {

        if (testResult.Result) {

          this.extendedSubDatabase = new ExtendedSubscriptionDatabase(this.databaseService.dbCredentials, this.model);

          let checkDb = this.modelService.checkDatabase(this.model.SubscriptionKey, this.extendedSubDatabase).
            subscribe(
            checkDbResult => {

              // lo step rimane a 1 perche' ci sono degli errori irrisolvibili
              this.step = checkDbResult.Result ? 2 : 1;

              // l'elaborazione e' terminata
              this.checkDbIsRunning = false;
              this.checkDbHasErrors = !checkDbResult.Result;

              this.operationsList = checkDbResult['Content'];

              checkDb.unsubscribe();
            },
            checkDbError => {
              console.log(checkDbError);
              alert(checkDbError);
              checkDb.unsubscribe();
            }
            )
        }
        else {
          this.step = 1;
          alert('Unable to connect! ' + testResult.Message);
        }
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
      this.showAlertMessage('Mandatory fields are empty! Check subscription key / database name!');
      return false;
    }

    if (this.model.Provider === '' ||
      this.model.DBServer === '' || this.model.DBName === '' || this.model.DBOwner === '' ||
      this.model.DMSDBServer === '' || this.model.DMSDBName === '' || this.model.DMSDBOwner === '') {
      this.showAlertMessage('Mandatory fields are empty! Check databases information!');
      return false;
    }

    if (this.model.DBServer !== this.model.DMSDBServer) {
      this.showAlertMessage('Both databases must be in the same server!');
      return false;
    }

    if (this.model.DBName === this.model.DMSDBName) {
      this.showAlertMessage('The databases names must be different!');
      return false;
    }

    if (this.model.DBOwner === this.model.DMSDBOwner) {
      if (this.model.DBPassword !== this.model.DMSDBPassword) {
        this.showAlertMessage('Passwords different for same users!');
        return false;
      }
    }

    // per il provider Azure devo controllare se le password rispettano le policy di sicurezza
    if (this.model.Provider === 'SQLAzure') {

      let dbPasswordIsValid = this.meetAzurePasswordPolicy(this.model.DBPassword, this.model.DBOwner);
      let dmsDbPasswordIsValid = (this.model.DBPassword !== this.model.DMSDBPassword)
        ? this.meetAzurePasswordPolicy(this.model.DMSDBPassword, this.model.DMSDBOwner) : dbPasswordIsValid;

      let msg = '';
      if (dbPasswordIsValid && dmsDbPasswordIsValid) {
        return true;
      }
      else {
        if (!dbPasswordIsValid && !dmsDbPasswordIsValid) {
          msg = 'The password specified for both dbowner does not meet the policy requirements';
        }
        else {
          msg = 'The password specified for ' + ((!dbPasswordIsValid) ? 'ERP' : 'DMS') + ' dbowner does not meet the policy requirements';
        }
      }
      if (msg !== '')
        this.showAlertMessage(msg);
      return dbPasswordIsValid && dmsDbPasswordIsValid;
    }

    return true;
  }

  //--------------------------------------------------------------------------------------------------------
  showAlertMessage(message: string) {
    this.msgDialog = message;
    this.openMsgDialog = true;
  }

  //--------------------------------------------------------------------------------------------------------
  meetAzurePasswordPolicy(password: string, user: string): boolean {

    // https://docs.microsoft.com/en-us/sql/relational-databases/security/password-policy

    let forbiddenPasswords = /passw.*|password.*|admin.*|administrator.*|sa.*|sysadmin.*/;

    if (
      password == null || password == undefined || password === '' ||
      password.length < 8 ||
      password.includes(user) ||
      password === user ||
      forbiddenPasswords.test(password)
    )
      return false;

    // Build up the strength of our password
    let numberOfElements = 0;
    numberOfElements = /.*[a-z].*/.test(password) ? ++numberOfElements : numberOfElements;      // Lowercase letters
    numberOfElements = /.*[A-Z].*/.test(password) ? ++numberOfElements : numberOfElements;      // Uppercase letters
    numberOfElements = /.*[0-9].*/.test(password) ? ++numberOfElements : numberOfElements;      // Numbers
    numberOfElements = /[^a-zA-Z0-9]/.test(password) ? ++numberOfElements : numberOfElements;   // Special characters (inc. space)

    return numberOfElements >= 3;
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

  // vecchio codice con test + check + save
  //--------------------------------------------------------------------------------------------------------
  /* oldSaveDatabaseWithCheck() {
 
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
   }*/
}
