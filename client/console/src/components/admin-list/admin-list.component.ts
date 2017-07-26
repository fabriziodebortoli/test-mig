
import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'admin-list',
  templateUrl: './admin-list.component.html',
  styleUrls: ['./admin-list.component.css']
})
export class AdminListComponent {

  @Input() items: Array<object>;
  @Input() columnNames: Array<string>;
  @Output() onSelectedItem: EventEmitter<object> = new EventEmitter<object>();

  constructor() { }

  goEditMode(item:object) {
    this.onSelectedItem.emit(item);
  }
}
