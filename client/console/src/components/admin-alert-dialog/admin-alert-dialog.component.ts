import { Component, Inject } from '@angular/core';
import { MdDialog } from '@angular/material';
import { MdDialogRef } from '@angular/material';
import { MD_DIALOG_DATA } from '@angular/material';


@Component({
  selector: 'admin-alert-dialog',
  templateUrl: './admin-alert-dialog.component.html',
  styleUrls: ['./admin-alert-dialog.component.css']
})
export class AdminAlertDialogComponent {

  constructor(public dialogRef: MdDialogRef<AdminAlertDialogComponent>,
    @Inject(MD_DIALOG_DATA) public data: any) {}

    onNoClick(): void {
      this.dialogRef.close();
    } 
}