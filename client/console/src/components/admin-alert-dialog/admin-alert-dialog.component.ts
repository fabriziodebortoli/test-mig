import { Component, Inject } from '@angular/core';
import { MatDialog } from '@angular/material';
import { MatDialogRef } from '@angular/material';
import { MAT_DIALOG_DATA } from '@angular/material';


@Component({
  selector: 'admin-alert-dialog',
  templateUrl: './admin-alert-dialog.component.html',
  styleUrls: ['./admin-alert-dialog.component.css']
})
export class AdminAlertDialogComponent {

  constructor(public dialogRef: MatDialogRef<AdminAlertDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any) {}

    onNoClick(): void {
      this.dialogRef.close();
    } 
}