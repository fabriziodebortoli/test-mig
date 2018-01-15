import { AccountInfo } from '../../authentication/account-info';
import { Router, ActivatedRoute } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { AppSubscription } from '../../model/subscription';
import { ModelService } from '../../services/model.service';
import { Observable } from 'rxjs/Observable';
import { OperationResult } from '../../services/operationResult';
import { SubscriptionDatabase } from '../../model/subscriptionDatabase';
import { AuthorizationProperties } from 'app/authentication/auth-info';
import { SubscriptionExternalSource } from '../../model/subscriptionExternalSource';
import { AdminCheckBoxListComponent } from '../../../../../../console/src/components/admin-checkbox-list/admin-checkbox-list.component';

@Component({
  selector: 'app-subscription',
  templateUrl: './subscription.component.html',
  styleUrls: ['./subscription.component.css']
})

export class SubscriptionComponent implements OnInit {

  model: AppSubscription;
  editing: boolean = false;
  databases: SubscriptionDatabase[];
  readingData: boolean;
  underMaintenance: boolean;

  needsMasterDB: boolean;
  needsTestDB: boolean;
  addDatabaseBtnText: string;

  // externalsources
  externalSources: SubscriptionExternalSource[];
  readingExternalSource: boolean;
  externalUnderMaintenance: boolean;
  deleteExternalSourceClicked: boolean = false;
  externalSourceSelectedItem: object;

  // DeleteSourceDialog variables
  openDeleteSourceDialog: boolean = false;
  deleteSourceDialogResult: boolean = false;
  msgDeleteSource: string;

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router, private route: ActivatedRoute) {
    this.model = new AppSubscription();
    this.databases = [];
    this.underMaintenance = false;

    this.externalSources = [];
    this.externalUnderMaintenance = false;
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {

    let subscriptionKey: string = this.route.snapshot.queryParams['subscriptionToEdit'];

    if (subscriptionKey === undefined || subscriptionKey === '')
      return;

    this.editing = true;
    this.readingData = true;
    this.readingExternalSource = true;

    // first I load the subscription 

    let accountName: string;
    let authorizationStored = localStorage.getItem('auth-info');

    if (authorizationStored !== null) {
      let authorizationProperties: AuthorizationProperties = JSON.parse(authorizationStored);
      accountName = authorizationProperties.accountName;
    }

    let localAccountInfo = localStorage.getItem(accountName);
    let instanceKey: string = '';

    if (localAccountInfo != null && localAccountInfo != '') {
      let accountInfo: AccountInfo = JSON.parse(localAccountInfo);
      instanceKey = accountInfo.instanceKey;
    }

    this.modelService.getSubscriptions(accountName, instanceKey, subscriptionKey)
      .subscribe(
      res => {
        let subscriptions: AppSubscription[] = res['Content'];

        if (subscriptions.length == 0) {
          this.readingData = false;
          return;
        }

        this.model = subscriptions[0];

        // then I load the databases of selected subscription

        this.modelService.getDatabases(subscriptionKey)
          .subscribe(
          res => {

            // carico la lista dei db dal provisioning
            let allDatabases: SubscriptionDatabase[] = res['Content'];

            // riempio due strutture separate per tipo
            let masterDatabases: SubscriptionDatabase[] = [];
            let testDatabases: SubscriptionDatabase[] = [];
            allDatabases.forEach((db) => {
              if (db.Test)
                testDatabases.push(db);
              else masterDatabases.push(db);
            });

            // aggiungo alla lista globale solo i primi database che trovo 
            // (se ne possono avere solo due, uno di produzione ed uno di test)
            this.needsTestDB = (testDatabases.length === 0);
            if (!this.needsTestDB)
              this.databases.push(testDatabases[0]);

            this.needsMasterDB = (masterDatabases.length === 0);
            if (!this.needsMasterDB)
              this.databases.push(masterDatabases[0]);

            // cambio il testo del button a seconda del db che devo creare
            this.addDatabaseBtnText = 'Configure ' + (this.needsTestDB ? 'TEST' : 'MASTER') + ' database';

            this.readingData = false;

            // se almeno un database e' in manutenzione visualizzo lo spinner
            for (var index = 0; index < this.databases.length; index++) {

              var db = this.databases[index];
              if (db.UnderMaintenance) {
                this.underMaintenance = true;
                break;
              }
            }
          },
          err => {
            alert(err);
            this.readingData = false;
          }
          )

        // then I load the external sources of selected subscription

        this.modelService.getExternalSources(subscriptionKey)
          .subscribe(
          res => {

            // carico la lista dei database esterni
            this.externalSources = res['Content'];

            this.readingExternalSource = false;

            // se almeno un externalsource e' in manutenzione visualizzo lo spinner
            for (var index = 0; index < this.externalSources.length; index++) {

              var source = this.externalSources[index];
              if (source.UnderMaintenance) {
                this.externalUnderMaintenance = true;
                break;
              }
            }
          },
          err => {
            alert(err);
            this.readingExternalSource = false;
          }
          )
      },
      err => {
        alert(err);
        this.readingData = false;
        this.readingExternalSource = false;
      }
      )
  }

  //--------------------------------------------------------------------------------------------------------
  submitSubscription() {
    if (this.model.SubscriptionKey == undefined) {
      alert('Mandatory fields are empty! Check subscription key!');
      return;
    }

    let subscriptionOperation: Observable<OperationResult> = this.modelService.saveSubscription(this.model);

    let subs = subscriptionOperation.subscribe(
      subscriptionResult => {
        this.model = new AppSubscription();
        if (this.editing)
          this.editing = !this.editing;

        subs.unsubscribe();
        // after save I return to parent page
        this.router.navigateByUrl('/subscriptionsHome');
      },
      err => {
        console.log(err);
        alert(err);
        subs.unsubscribe();
      }
    )
  }

  // e.g. url: localhost:10344/database?subscriptionToEdit=S-ENT&databaseToEdit=I-M4-ENT_S-ENT_Master
  //--------------------------------------------------------------------------------------------------------
  openDatabase(item: object) {
    // route to edit an existing database, I add in the existing query string the database name
    this.router.navigate(['/database'], { queryParams: { databaseToEdit: item['Name'] }, queryParamsHandling: "merge" });
  }

  // e.g. url: localhost:10344/database/configuration?subscriptionToEdit=S-ENT
  //--------------------------------------------------------------------------------------------------------
  configureDatabase() {
    // route to configure database (when no subscription database exist)
    this.router.navigate(['/database/configuration'], { queryParamsHandling: "preserve" });
  }

  // e.g. url: localhost:10344/subscription?subscriptionToEdit=S-ENT&test=false
  //--------------------------------------------------------------------------------------------------------
  addDatabase() {
    // route to add new database (when I want to add a master or test database)
    this.router.navigate(['/database'], { queryParams: { test: !this.needsTestDB }, queryParamsHandling: "merge" });
  }

  // e.g. url: localhost:10344/externalsource?subscriptionToEdit=S-ENT&sourceToEdit=AWS
  //--------------------------------------------------------------------------------------------------------
  openExternalSource(item: object) {

    // per differenziare il click generico sulla lista: se si tratta di una delete o di una open
    if (this.deleteExternalSourceClicked) {

      this.externalSourceSelectedItem = item;

      this.openDeleteSourceDialog = true;
      this.msgDeleteSource = 'Are you sure to delete ' + item['Source'] + ' external source?';

      return;
    }

    // route to edit an existing externalsource, I add in the existing query string the source name
    this.router.navigate(['/externalsource'], { queryParams: { sourceToEdit: item['Source'] }, queryParamsHandling: "merge" });
  }

  // e.g. url: localhost:10344/externalsource?subscriptionToEdit=S-ENT&test=false
  //--------------------------------------------------------------------------------------------------------
  addExternalSource() {
    // route to add new externalsource
    this.router.navigate(['/externalsource'], { queryParamsHandling: "preserve" });
  }

  // sto eliminando una riga dalla lista degli external sources
  //--------------------------------------------------------------------------------------------------------
  deleteExternalSource() {
    this.deleteExternalSourceClicked = true;
  }

  // evento sulla chiusura della dialog di conferma cancellazione external source
  //--------------------------------------------------------------------------------------------------------
  onCloseDeleteSourceDialog() {

    this.deleteExternalSourceClicked = false;

    // if 'No' button has been clicked I return
    if (!this.deleteSourceDialogResult)
      return;

    // faccio un reverse loop ed elimino la riga corrispondente dalla lista
    for (var i = this.externalSources.length - 1; i >= 0; i--) {
      var source = this.externalSources[i];
      if (source.Source === this.externalSourceSelectedItem['Source']) {
        this.externalSources.splice(i, 1);
        break;
      }
    }

    // chiamo l'API per eseguire la delete sul database
    let queryDelete = this.modelService.queryAdminDelete(
      this.model.SubscriptionKey,
      'SUBSCRIPTIONEXTERNALSOURCES',
      {
        MatchingFields: {
          InstanceKey: this.externalSourceSelectedItem['InstanceKey'],
          SubscriptionKey: this.externalSourceSelectedItem['SubscriptionKey'],
          Source: this.externalSourceSelectedItem['Source']
        }
      }).subscribe(
      deleteResult => {
        queryDelete.unsubscribe();
      },
      deleteError => {
        console.log(deleteError);
        alert(deleteError);
        queryDelete.unsubscribe();
      }
      )
  }
}
