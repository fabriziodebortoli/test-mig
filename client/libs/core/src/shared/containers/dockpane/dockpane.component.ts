import { Component, Input, ViewChild, ContentChild, TemplateRef, HostBinding } from '@angular/core';

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
  
  @Input() _icon: string = 'erp-purchaseorder';

    @Input()
    set icon(icon: any) {
        this._icon = icon instanceof Object ? icon.value : icon;
    }

    get icon() {
        return this._icon;
    }

  @Input() activated:boolean = true;

}
