import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'admin-dialog',
  templateUrl: './admin-dialog.component.html',
  styleUrls: ['./admin-dialog.component.css']
})
export class AdminDialogComponent {

  @Input() mode: string; // available modes: yesno, message
  @Input() title: string;
  @Input() message: string;
  @Input() opened: boolean;
  @Input() result: boolean;
  @Output() openedChange: EventEmitter<any>;
  @Output() resultChange: EventEmitter<any>;
 
  constructor() { 
    this.mode = '';
    this.title = '';
    this.message = '';
    this.opened = false;
    this.result = false;
    this.resultChange = new EventEmitter<any>();
    this.openedChange = new EventEmitter<any>();
  }

  public close(status) {
    if (this.mode === 'yesno'){
      this.result = status === 'yes';
    }
    this.opened = false;
    this.openedChange.emit(false);
    this.resultChange.emit(this.result);
  }

}
