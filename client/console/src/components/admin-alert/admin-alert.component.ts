
import { Component, Input, Output, EventEmitter, Inject } from '@angular/core';
import {MdDialog, MdDialogRef, MD_DIALOG_DATA} from '@angular/material';

@Component({
  selector: 'admin-alert',
  templateUrl: './admin-alert.component.html',
  styleUrls: ['./admin-alert.component.css']
})
export class AdminAlertComponent {

  constructor(public dialog: MdDialog) {}

  openDialog(): void {
  }  
}