
import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'admin-checkbox',
  templateUrl: './admin-checkbox.component.html',
  styleUrls: ['./admin-checkbox.component.css']
})
export class AdminCheckBoxComponent {

  @Input() checkBoxId: string;
  @Input() checkValue: boolean;
  @Output() onSelectedCheck: EventEmitter<object> = new EventEmitter<object>();

  constructor() { }

  goEditMode(item:object) {
    this.onSelectedCheck.emit(item);
  }
}
