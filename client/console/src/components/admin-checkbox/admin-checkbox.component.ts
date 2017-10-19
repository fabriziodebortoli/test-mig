
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'admin-checkbox',
  templateUrl: './admin-checkbox.component.html',
  styleUrls: ['./admin-checkbox.component.css']
})
export class AdminCheckBoxComponent {

  @Input() checkId: string;
  @Input() checkText: string;
  @Input() checkValue: boolean;
  @Input() readOnly: boolean;
  @Output() inputDataChange;
     
  constructor() {
    this.inputDataChange = new EventEmitter();
  }

  onChange(val) {
    this.checkValue = val;
    this.inputDataChange.emit(this.checkValue);
  }  
}
