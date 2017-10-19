import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'admin-dropdown',
  templateUrl: './admin-dropdown.component.html',
  styleUrls: ['./admin-dropdown.component.css']
})
export class AdminDropDownComponent {

  @Input() listItems: Array<object>;
  @Input() inputLabel: string;
  @Input() textField: string;
  @Input() valueField: string;
  @Input() selectedValue: string;
  @Output() selectedValueChange: EventEmitter<any>;

  currentValue: object;

  constructor() {
    this.currentValue = { name: '', value: ''};
    this.listItems = new Array<object>();
    this.inputLabel = '';
    this.selectedValue = '';
    this.textField = '';
    this.valueField = '';
    this.selectedValueChange = new EventEmitter<any>();
  }

  change(event) {
    this.selectedValue = event['value'];
    this.selectedValueChange.emit(this.selectedValue);
  }

  ngOnChanges() {
    this.currentValue = { name: this.selectedValue, value: this.selectedValue };
  }
}
