import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'admin-textarea',
  templateUrl: './admin-textarea.component.html',
  styleUrls: ['./admin-textarea.component.css']
})
export class AdminTextAreaComponent {

  @Input() inputLabel: string;
  @Input() inputData: string;
  @Input() readOnly: boolean;
  @Output() inputDataChange;

  constructor() { 
    this.inputLabel = '';
    this.inputData = '';
    this.readOnly = false;
    this.inputDataChange = new EventEmitter();
  }

  change(newValue) {
    this.inputData = newValue;
    this.inputDataChange.emit(newValue);
  }
}
