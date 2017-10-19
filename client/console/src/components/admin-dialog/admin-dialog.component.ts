import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'admin-dialog',
  templateUrl: './admin-dialog.component.html',
  styleUrls: ['./admin-dialog.component.css']
})
export class AdminDialogComponent {

  @Input() dialogTitle: string;
  @Input() status: boolean;

  public opened: boolean;

  constructor() { 
    this.dialogTitle = '';
    this.opened = false;
  }

  public close(status) {
    this.opened = false;
  }

  ngOnChange() {
    
  }
}
