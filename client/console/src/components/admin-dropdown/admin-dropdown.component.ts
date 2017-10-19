import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'admin-dropdown',
  templateUrl: './admin-dropdown.component.html',
  styleUrls: ['./admin-dropdown.component.css']
})
export class AdminDropDownComponent {

  @Input() listItems: Array<object>;
  @Input() inputLabel: string;
  @Input() value: string;
  @Input() textField: string;
  @Input() valueField: string;

  constructor() {
    this.listItems = new Array<object>();
    this.inputLabel = '';
    this.value = '';
    this.textField = '';
    this.valueField = '';
  } 
}
