import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { DatabaseService } from 'app/services/database.service';
import { SubscriptionDatabase } from 'app/model/subscriptionDatabase';
import { ActivatedRoute, Router } from '@angular/router';
import { ModelService } from 'app/services/model.service';
import { AccountInfo } from 'app/authentication/account-info';
import { DatabaseCredentials, ExtendedSubscriptionDatabase } from '../../authentication/credentials';

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
  
  //--------------------------------------------------------------------------------------------------------
  constructor(
    private modelService: ModelService,
    private databaseService: DatabaseService,
    private router: Router,
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
      
      if (this.viewMaster)
      {
        elementT.className= 'myTab'; 
        elementM.className= 'mySelectedTab'; 
      }
      else
      {
        elementM.className= 'myTab';
        elementT.className= 'mySelectedTab'; 
      }
    }
    
    // gestione tab
    //--------------------------------------------------------------------------------------------------------
    ClickTest() {
      this.viewMaster = false;
      var elementT = document.getElementById('labt');
      var elementM = document.getElementById('labm');
      
      if (this.viewMaster)
      {
        elementT.className= 'myTab'; 
        elementM.className= 'mySelectedTab'; 
      }
      else
      {
        elementM.className= 'myTab';
        elementT.className= 'mySelectedTab'; 
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
      this.model.DMSDBServer === '' || this.model.DMSDBName === '' || this.model.DMSDBOwner === '' ) {
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
    
    //--------------------------------------------------------------------------------------------------------
    validateAdminCredentials(subscriptionKey: string, dbCredentials: DatabaseCredentials): boolean {
      
      let test = this.modelService.testConnection(subscriptionKey, dbCredentials).
      subscribe(
        result => {
          if (result.Result) {
            this.databaseService.dbCredentials = dbCredentials;
          }
          else
          alert('Unable to connect! ' + result.Message);
          
          test.unsubscribe();
          return result.Result;
        },
        error => {
          test.unsubscribe();
          return false;
        }
      );
      
      return true;
    }
    
    //--------------------------------------------------------------------------------------------------------
    submitDatabase() {
      
      // first I check input values 
      
      if (!this.validateInput()) 
      return;
      
      let adminLogin: string = prompt("Insert admin credentials login:", "AdminMicroarea");
      let adminPw: string = prompt("Insert admin credentials password:", "");
      
      if (adminLogin === '') {
        alert('Admin login is empty!');
        return;
      }
      
      if (adminLogin === 'AdminMicroarea')
        adminPw = "S1cr04$34!";
      
      // if credentials are valid I test the connection
      
      let subscriptionKey: string = this.model.SubscriptionKey;
      
      let dbCredentials: DatabaseCredentials = new DatabaseCredentials();
      dbCredentials.Provider = this.model.Provider;
      dbCredentials.Server = this.model.DBServer;
      dbCredentials.Login = adminLogin;
      dbCredentials.Password = adminPw;
      
      let test = this.modelService.testConnection(subscriptionKey, dbCredentials).
      subscribe(
        result => {
          
          if (result.Result) {
            let extendedSubDatabase: ExtendedSubscriptionDatabase = new ExtendedSubscriptionDatabase(dbCredentials, this.model);

            let update = this.modelService.updateDatabase(subscriptionKey, extendedSubDatabase).
            subscribe( 
              updateResult => {
                if (!updateResult.Result) {
                  alert(updateResult.Message);
                }
                update.unsubscribe();
                // after save I return to parent page
                this.router.navigate(['/subscription'], { queryParams: { subscriptionToEdit: subscriptionKey } });
              },
              updateError => {
                console.log(updateError);
                alert(updateError);
                update.unsubscribe();
              }
            )
          }
          else
          alert('Unable to connect! ' + result.Message);
          
          test.unsubscribe();
        },
        error => {
          console.log(error);
          alert(error);
          test.unsubscribe();
        }
      );
      
      // salvo prima il model
/*      let subs = this.modelService.saveDatabase(this.model).
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