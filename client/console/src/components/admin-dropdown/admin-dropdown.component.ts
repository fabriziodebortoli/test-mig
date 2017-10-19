import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'admin-dropdown',
  templateUrl: './admin-dropdown.component.html',
  styleUrls: ['./admin-dropdown.component.css']
})
export class AdminDropDownComponent {

  public listItems: Array<object> = [];

  constructor() {
    this.listItems = new Array<object>();
  } 
}
