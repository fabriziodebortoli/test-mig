import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'admin-dialog',
  templateUrl: './admin-dialog.component.html',
  styleUrls: ['./admin-dialog.component.css']
})
export class AdminDialogComponent {

  @Input() title: string;
  @Input() message: string;
  @Input() opened: boolean;
  @Output() onDialogClosed: EventEmitter<any>;
 
  constructor() { 
    this.title = '';
    this.message = '';
    this.opened = false;
    this.onDialogClosed = new EventEmitter<any>();
  }

  public close(status) {
    this.opened = false;
    this.onDialogClosed.emit(true);
  }

}
