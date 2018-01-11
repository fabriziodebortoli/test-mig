import { Component, Input, TemplateRef, ContentChild, ViewChild } from '@angular/core';
import { TabStripTabComponent } from '@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip-tab.component';

@Component({
  selector: 'tb-tile-manager-tab',
  templateUrl: './tile-manager-tab.component.html',
  styleUrls: ['./tile-manager-tab.component.scss']
})
export class TileManagerTabComponent {
  
  @ContentChild(TemplateRef) templateRef: any;
  @ViewChild(TabStripTabComponent) tabComponent;
    
  @Input() iconType: string = 'M4';
  @Input() icon: string = 'erp-purchaseorder';
  private _title: string;

  public get title(): string {
    return this._title;
  }

  @Input() public set title(value: string) {
    this._title = value.replace("&", "");
  }
  
  constructor() { }
}