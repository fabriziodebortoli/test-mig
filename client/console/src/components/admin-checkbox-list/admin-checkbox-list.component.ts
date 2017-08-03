
import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'admin-checkbox-list',
  templateUrl: './admin-checkbox-list.component.html',
  styleUrls: ['./admin-checkbox-list.component.css']
})
export class AdminCheckBoxListComponent {

  @Input() items: Array<object>;
  @Input() columnName: string;

  constructor() { }
}
