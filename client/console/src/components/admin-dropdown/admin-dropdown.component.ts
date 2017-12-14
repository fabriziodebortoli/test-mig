import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'admin-dropdown',
  templateUrl: './admin-dropdown.component.html',
  styleUrls: ['./admin-dropdown.component.css']
})
export class AdminDropDownComponent {

  @Input() listItems: Array<{ name: string, value: string }>;
  @Input() inputLabel: string;
  @Input() textField: string;
  @Input() valueField: string;
  @Input() selectedValue: { name: string, value: string};
  @Input() readOnly: boolean;
  @Output() selectedValueChange: EventEmitter<any>;

  constructor() {
    this.selectedValue = { name: '', value: ''};
    this.listItems = new Array<{ name: string, value: string }>();
    this.inputLabel = '';
    this.textField = '';
    this.valueField = '';
    this.selectedValueChange = new EventEmitter<any>();
    this.readOnly = false;
  }

  change(event) {
    this.selectedValue = event;
    this.selectedValueChange.emit(this.selectedValue);
  }
}
