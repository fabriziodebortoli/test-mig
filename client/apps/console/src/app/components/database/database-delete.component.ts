import { ModelService } from './../../services/model.service';
import { Component, OnInit, Input } from '@angular/core';
import { SubscriptionDatabase } from 'app/model/subscriptionDatabase';

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
  deleteERPDB: boolean = false;
  deleteDMSDB: boolean = false;

  isReadOnly: boolean = false;

  // opendialog variables
  openDeleteDialog: boolean = false;
  dialogResult: boolean = false;
  msgDelete: string;

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService) { }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {
  }

  //--------------------------------------------------------------------------------------------------------
  ondeleteDatabaseObjects(value: boolean) {
    this.deleteDatabaseObjects = value;
    this.deleteSubscriptionDB = !this.deleteDatabaseObjects;

    this.isReadOnly = value;
    this.deleteERPDB = !this.deleteDatabaseObjects;
    this.deleteDMSDB = !this.deleteDatabaseObjects;
  }

  //--------------------------------------------------------------------------------------------------------
  onDeleteSubscriptionDB(value: boolean) {
    this.deleteSubscriptionDB = value;
    this.deleteDatabaseObjects = !this.deleteSubscriptionDB;
    this.isReadOnly = !value;
  }

  //--------------------------------------------------------------------------------------------------------
  onDeleteERPDB(value: boolean) {
    this.deleteERPDB = value;
  }

  //--------------------------------------------------------------------------------------------------------
  onDeleteDMSDB(value: boolean) {
    this.deleteDMSDB = value;
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
      if (this.deleteERPDB || this.deleteDMSDB)
        message = 'There will be deleted the database containers associated to the subscription and all data will be lost! Do you want to continue?';
      else
        message = 'Only subscription database information will be deleted. Do you want to continue?';
    }

    this.msgDelete = message;
    this.openDeleteDialog = true;
  }

  //--------------------------------------------------------------------------------------------------------
  onCloseDialog() {
    // if 'No' button has been clicked I return
    if (!this.dialogResult)
      return;

    this.isDeleting = true;

    if (this.deleteDatabaseObjects) {
      let deleteOperation = this.modelService.deleteDatabaseObjects(this.model.SubscriptionKey, this.model).
        subscribe(
        deleteResult => {

          alert('Elaboration completed');
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
}
