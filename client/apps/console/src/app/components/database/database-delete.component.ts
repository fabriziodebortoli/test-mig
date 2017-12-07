import { ModelService } from './../../services/model.service';
import { Component, OnInit, Input } from '@angular/core';
import { SubscriptionDatabase } from 'app/model/subscriptionDatabase';
import { DeleteDatabaseBodyContent, DeleteDatabaseParameters } from 'app/components/database/helpers/database-helpers';
import { Router } from '@angular/router';
import { DatabaseCredentials } from '../../authentication/credentials';
import { DatabaseProvider } from '../components.helper';

@Component({
  selector: 'app-database-delete',
  templateUrl: './database-delete.component.html',
  styleUrls: ['./database-delete.component.css']
})

export class DatabaseDeleteComponent implements OnInit {

  @Input() model: SubscriptionDatabase;

  isDeleting: boolean = false;

  deleteDatabaseObjects: boolean = false;
  deleteSubscriptionDB: boolean = true;

  // options
  deleteParams: DeleteDatabaseParameters;
  adminCredentials: DatabaseCredentials;

  isReadOnly: boolean = false;

  // DeleteOpenDialog variables
  openDeleteDialog: boolean = false;
  deleteDialogResult: boolean = false;
  msgDelete: string;

  // CredentialsOpenDialog variables
  openCredentialsDialog: boolean = false;
  credentialsDialogResult: boolean = false;
  credentialsFields: Array<{ label: string, value: string, hide: boolean }>;

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router) {
    this.credentialsFields = [];
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {
    this.deleteParams = new DeleteDatabaseParameters();

    // aggiungo i fields da visualizzazione nella dialog per la richiesta credenziali Azure
    this.credentialsFields = [
      { label: 'Login', value: '', hide: false },
      { label: 'Password', value: '', hide: true }
    ];
  }

  //--------------------------------------------------------------------------------------------------------
  ondeleteDatabaseObjects(value: boolean) {
    this.deleteDatabaseObjects = value;
    this.deleteSubscriptionDB = !this.deleteDatabaseObjects;

    this.isReadOnly = value;
    this.deleteParams.DeleteERPDatabase = !this.deleteDatabaseObjects;
    this.deleteParams.DeleteDMSDatabase = !this.deleteDatabaseObjects;
  }

  //--------------------------------------------------------------------------------------------------------
  onDeleteSubscriptionDB(value: boolean) {
    this.deleteSubscriptionDB = value;
    this.deleteDatabaseObjects = !this.deleteSubscriptionDB;
    this.isReadOnly = !value;
  }

  //--------------------------------------------------------------------------------------------------------
  onDeleteERPDB(value: boolean) {
    this.deleteParams.DeleteERPDatabase = value;
  }

  //--------------------------------------------------------------------------------------------------------
  onDeleteDMSDB(value: boolean) {
    this.deleteParams.DeleteDMSDatabase = value;
  }

  //--------------------------------------------------------------------------------------------------------
  delete() {
    // se sto gia' eseguendo mi fermo
    if (this.isDeleting)
      return;

    this.showMessage();
  }

  // primo messaggio mostrato dopo aver cliccato su Delete
  //--------------------------------------------------------------------------------------------------------
  showMessage() {
    let message: string;

    if (this.deleteDatabaseObjects)
      message = 'All non-system database objects (such as tables, procedures and views) will be deleted. Do you want to continue?';
    else {
      if (this.deleteParams.DeleteERPDatabase || this.deleteParams.DeleteDMSDatabase)
        message = 'There will be deleted the database containers associated to the subscription and all data will be lost! Do you want to continue?';
      else
        message = 'Only subscription database information will be deleted. Do you want to continue?';
    }

    // apro la dialog con la conferma operazioni
    this.msgDelete = message;
    this.openDeleteDialog = true;
  }

  // evento sulla chiusura della dialog di conferma operazioni
  //--------------------------------------------------------------------------------------------------------
  onCloseDeleteDialog() {
    // if 'No' button has been clicked I return
    if (!this.deleteDialogResult)
      return;

    // I delete only the objects in ERP database
    if (this.deleteDatabaseObjects) {

      this.execDeleteDatabaseObjects();
    }

    if (this.deleteSubscriptionDB) {

      // se voglio eliminare i contenitori del database e sono in Azure 
      // devo prima richiedere le credenziali di amministrazione tramite l'apposita dialog
      if (this.model.Provider === DatabaseProvider.SQLAZURE &&
        (this.deleteParams.DeleteERPDatabase || this.deleteParams.DeleteDMSDatabase)) {
        // apro la dialog
        this.openCredentialsDialog = true;
        return;
      }

      this.execDeleteDatabase();

      // I return to subscription homepage and meanwhile databases will be deleted
      this.router.navigate(['/subscription'], { queryParams: { subscriptionToEdit: this.model.SubscriptionKey } });
    }
  }

  // evento sulla chiusura della dialog di richiesta credenziali amministrazione
  //--------------------------------------------------------------------------------------------------------
  onCloseCredentialsDialog() {
    // if 'No' button has been clicked I return
    if (!this.credentialsDialogResult)
      return;

    //test admin connection first!
    let adminLogin: string = this.credentialsFields[0].value;
    let adminPw: string = this.credentialsFields[1].value;

    if (adminLogin === '') {
      alert('Admin login is empty!');
      return;
    }

    this.adminCredentials = new DatabaseCredentials();
    this.adminCredentials.Provider = this.model.Provider;
    this.adminCredentials.Server = this.model.DBServer;
    this.adminCredentials.Login = adminLogin;
    this.adminCredentials.Password = adminPw;

    let test = this.modelService.testConnection(this.model.SubscriptionKey, this.adminCredentials).
      subscribe(
      testResult => {

        if (testResult.Result) {

          this.execDeleteDatabase();

          // I return to subscription homepage and meanwhile databases will be deleted
          this.router.navigate(['/subscription'], { queryParams: { subscriptionToEdit: this.model.SubscriptionKey } });
        }
        else
          alert('Unable to connect! ' + testResult.Message);

        // clear local array with dialog values
        this.credentialsFields.forEach(element => { element.value = '' });
        test.unsubscribe();
      },
      error => {
        console.log(error);
        alert(error);
        test.unsubscribe();
      }
      );
  }

  // richiama il metodo di cancellazione dei soli oggetti del database
  //--------------------------------------------------------------------------------------------------------
  execDeleteDatabaseObjects() {

    this.isDeleting = true;

    let deleteOperation = this.modelService.deleteDatabaseObjects(this.model.SubscriptionKey, this.model).
      subscribe(
      deleteResult => {

        alert('Delete operation successfully completed');
        this.isDeleting = false;
        deleteOperation.unsubscribe();
      },
      deleteError => {
        this.isDeleting = false;
        deleteOperation.unsubscribe();
        alert(deleteError);
      }
      )
  }

  // richiama il metodo di cancellazione della riga nella SubscriptionDatabase e degli eventuali contenitori dei database
  //--------------------------------------------------------------------------------------------------------
  execDeleteDatabase() {

    this.isDeleting = true;

    let deleteBodyContent: DeleteDatabaseBodyContent = new DeleteDatabaseBodyContent();
    deleteBodyContent.Database = this.model;
    deleteBodyContent.DeleteParameters = this.deleteParams;
    deleteBodyContent.AdminCredentials = this.adminCredentials;

    let deleteOperation = this.modelService.deleteDatabase(this.model.SubscriptionKey, deleteBodyContent).
      subscribe(
      deleteResult => {

        alert('Delete operation successfully completed');
        this.isDeleting = false;
        deleteOperation.unsubscribe();
      },
      deleteError => {
        this.isDeleting = false;
        deleteOperation.unsubscribe();
        alert(deleteError);
      }
      )
  }
}
