import { Component, Input, ViewChild, ContentChild, TemplateRef } from '@angular/core';

import { TabStripTabComponent } from '@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip-tab.component';

@Component({
  selector: 'tb-dockpane',
  templateUrl: './dockpane.component.html',
  styleUrls: ['./dockpane.component.scss']
})
export class DockpaneComponent {

  @ContentChild(TemplateRef) templateRef: any;
  @ViewChild(TabStripTabComponent) tabComponent;

  @Input() title: string = '???';
  
  @Input() iconType: string = 'M4';
  @Input() icon: string = 'erp-purchaseorder';

  @Input() activated: boolean = true;

}
