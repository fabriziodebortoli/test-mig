import { Component, Input, Output, EventEmitter, Inject } from '@angular/core';
import { MdDialog } from '@angular/material';
import { AdminAlertDialogComponent } from '../admin-alert-dialog/admin-alert-dialog.component';


@Component({
  selector: 'admin-alert',
  templateUrl: './admin-alert.component.html',
  styleUrls: ['./admin-alert.component.css']
})
export class AdminAlertComponent {

  constructor(public dialog: MdDialog) {}

  openDialog(): void {
    let dialogRef = this.dialog.open(AdminAlertDialogComponent, {
      width: '250px',
      data: { }
    });
  }  
}