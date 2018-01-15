import { Component, NgModule, trigger, transition, style, animate, state, Input, ViewChild } from '@angular/core';
import { TabStripTabComponent } from '@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip-tab.component';

@Component({
  selector: 'tb-dockpane',
  templateUrl: './dockpane.component.html',
  styleUrls: ['./dockpane.component.scss']
})
export class DockpaneComponent {
  @ViewChild(TabStripTabComponent) tabComponent;
  @Input() title: string = '???';
  @Input() iconType: string = 'M4';
  @Input() icon: string = 'erp-purchaseorder';
  active: boolean;
}
