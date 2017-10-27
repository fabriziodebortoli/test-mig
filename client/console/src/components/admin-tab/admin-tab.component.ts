import { Component, Input, ViewChild } from '@angular/core';
import { TabStripTabComponent } from '@progress/kendo-angular-layout';

@Component({
  selector: 'admin-tab',
  templateUrl: './admin-tab.component.html',
  styleUrls: ['./admin-tab.component.css']
})
export class AdminTabComponent {

  @Input() tabTitle: string;
  @ViewChild(TabStripTabComponent) tab;

  constructor() {
    this.tabTitle = '';
  }
}