import { ModelService } from './../../services/model.service';
import { Component, OnInit, Input } from '@angular/core';
import { SubscriptionDatabase } from 'app/model/subscriptionDatabase';
import { DeleteDatabaseBodyContent, DeleteDatabaseParameters } from 'app/components/database/helpers/database-helpers';
import { Router } from '@angular/router';

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

  isReadOnly: boolean = false;

  // opendialog variables
  openDeleteDialog: boolean = false;
  dialogResult: boolean = false;
  msgDelete: string;

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router) { 
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {
    this.deleteParams = new DeleteDatabaseParameters();
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

    this.msgDelete = message;
    this.openDeleteDialog = true;
  }

  //--------------------------------------------------------------------------------------------------------
  onCloseDeleteDialog() {
    // if 'No' button has been clicked I return
    if (!this.dialogResult)
      return;

    this.isDeleting = true;

    // I delete only the objects in ERP database
    if (this.deleteDatabaseObjects) {
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

    // se voglio eliminare i contenitori del database 
    // e sono in Azure devo richiedere le credenziali di amministrazione
    if (this.deleteSubscriptionDB) {

      let deleteBodyContent: DeleteDatabaseBodyContent = new DeleteDatabaseBodyContent();
      deleteBodyContent.Database = this.model;
      deleteBodyContent.DeleteParameters = this.deleteParams;

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

      // I return to subscription homepage and meanwhile databases will be deleted
      this.router.navigate(['/subscription'], { queryParams: { subscriptionToEdit: this.model.SubscriptionKey } });
    }
  }
}
