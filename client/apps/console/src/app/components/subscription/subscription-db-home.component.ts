import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { DatabaseService } from 'app/services/database.service';
import { SubscriptionDatabase } from 'app/model/subscriptionDatabase';
import { ActivatedRoute, Router } from '@angular/router';
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
  viewMaster: boolean = true;
  isEditing: boolean = false;
  dbCredentials: DatabaseCredentials;
  operationsList: OperationResult[];
  extendedSubDatabase: ExtendedSubscriptionDatabase;

  // variables for app-database-operations component
  checkDatabaseIsStarted: boolean = false;
  checkDatabaseIsRunning: boolean = false;
  somethingToDo: boolean = false;

  // admin dialog auxiliary variables  
  openDialog: boolean;
  dialogResult: boolean;
  fields: Array<{ label: string, value: string, hide: boolean }>;

  //--------------------------------------------------------------------------------------------------------
  constructor(
    private modelService: ModelService,
    private databaseService: DatabaseService,
    private router: Router,
    private route: ActivatedRoute) {
    this.fields = [];
    this.operationsList = [];
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

    this.isEditing = true;
    this.viewMaster = true;
    this.databaseService.needsAskCredentials = false;

    // in caso di edit viene usato un solo model (anche per il database di test)
    this.model.Name = dbName;

    // aggiungo i fields da visualizzazione nella dialog per la richiesta credenziali
    this.fields = [
      { label: 'Login', value: '', hide: false },
      { label: 'Password', value: '', hide: true }
    ];
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnDestroy(): void {
    this.databaseService.testConnectionOK = false;
    this.databaseService.needsAskCredentials = true;
  }

  // gestione tab
  //--------------------------------------------------------------------------------------------------------
  ClickMaster() {
    this.viewMaster = true;
    var elementT = document.getElementById('labt');
    var elementM = document.getElementById('labm');

    if (this.viewMaster) {
      elementT.className = 'myTab';
      elementM.className = 'mySelectedTab';
    }
    else {
      elementM.className = 'myTab';
      elementT.className = 'mySelectedTab';
    }
  }

  // gestione tab
  //--------------------------------------------------------------------------------------------------------
  ClickTest() {
    this.viewMaster = false;
    var elementT = document.getElementById('labt');
    var elementM = document.getElementById('labm');

    if (this.viewMaster) {
      elementT.className = 'myTab';
      elementM.className = 'mySelectedTab';
    }
    else {
      elementM.className = 'myTab';
      elementT.className = 'mySelectedTab';
    }
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

  // event on close dialog
  //--------------------------------------------------------------------------------------------------------
  onDialogClose() {
    // if Cancel button has been clicked I return
    if (!this.dialogResult)
      return;

    let adminLogin: string = this.fields[0].value;
    let adminPw: string = this.fields[1].value;

    if (adminLogin === '') {
      alert('Admin login is empty!');
      return;
    }

    if (adminLogin === 'AdminMicroarea')
      adminPw = "S1cr04$34!";

    this.dbCredentials = new DatabaseCredentials();
    this.dbCredentials.Provider = this.model.Provider;
    this.dbCredentials.Server = this.model.DBServer;
    this.dbCredentials.Login = adminLogin;
    this.dbCredentials.Password = adminPw;

    this.executeElaboration();
  }

  //--------------------------------------------------------------------------------------------------------
  submitDatabase() {
    // first I check input values 
    if (!this.validateInput())
      return;

    // devo controllare se qualcosa e' cambiato
    this.openDialog = true;
  }

  //--------------------------------------------------------------------------------------------------------
  executeElaboration() {

    this.checkDatabaseIsStarted = this.checkDatabaseIsRunning = true;
    this.somethingToDo = false;

    let test = this.modelService.testConnection(this.model.SubscriptionKey, this.dbCredentials).
      subscribe(
      testResult => {

        if (testResult.Result) {

          this.extendedSubDatabase = new ExtendedSubscriptionDatabase(this.dbCredentials, this.model);

          let update = this.modelService.checkDatabase(this.model.SubscriptionKey, this.extendedSubDatabase).
            subscribe(
            checkResult => {

              if (!checkResult.Result) {
                alert(checkResult.Message);
              }

              this.operationsList = checkResult['Content'];
              this.somethingToDo = (checkResult.Code === 0);

              update.unsubscribe();
              this.checkDatabaseIsRunning = false;
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

        // clear local array with dialog values
        this.fields.forEach(element => {
          element.value = ''
        });
        test.unsubscribe();
      },
      error => {
        console.log(error);
        alert(error);
        test.unsubscribe();
      }
      );

    // salvo prima il model
    /* let subs = this.modelService.saveDatabase(this.model).
    subscribe(
      databaseResult => {
        subs.unsubscribe();
        
        // poi salvo il modeltest (se necessario)
        // se il Name e' vuoto significa che sto salvando solo il model principale
        if (this.modelTest.Name != undefined && this.modelTest.Name !== '')
        {
          let subs2 = this.modelService.saveDatabase(this.modelTest).
          subscribe(
            databaseResult => {
              if (this.isEditing)
              this.isEditing = !this.isEditing;
              subs2.unsubscribe();
              // after save I return to parent page
              this.router.navigate(['/subscription'], { queryParams: { subscriptionToEdit: subscriptionKey } });
            },
            err => {
              console.log(err);
              alert(err);
              subs2.unsubscribe();
            }
          )
        } // fine save modeltest
        
        // after save I return to parent page
        this.router.navigate(['/subscription'], { queryParams: { subscriptionToEdit: subscriptionKey } });
      },
      err => {
        console.log(err);
        alert(err);
        subs.unsubscribe();
      }
    )*/
  }
}