import { Component, Input } from '@angular/core';

@Component({
  selector: 'admin-list',
  templateUrl: './admin-list.component.html',
  styleUrls: ['./admin-list.component.css']
})
export class AdminListComponent {

  @Input() items: Array<object>;
  @Input() columnNames: Array<string>;

  constructor() { }
}
