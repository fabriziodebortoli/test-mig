
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'admin-checkbox',
  templateUrl: './admin-checkbox.component.html',
  styleUrls: ['./admin-checkbox.component.css']
})
export class AdminCheckBoxComponent {

  @Input() checkText: string;
  @Input() checkValue: boolean;
  @Input() readOnly: boolean;
  @Output() inputDataChange: EventEmitter<any> = new EventEmitter<any>();
     
  constructor() {
    
  }

  onChange(val) {
    this.checkValue = val;
    this.inputDataChange.emit(this.checkValue);
  }  
}
