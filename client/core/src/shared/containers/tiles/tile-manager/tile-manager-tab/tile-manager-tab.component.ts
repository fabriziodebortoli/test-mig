import { Component, Input, TemplateRef, ContentChild, ViewChild, AfterContentInit, ChangeDetectorRef } from '@angular/core';

import { TabStripTabComponent } from '@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip-tab.component';

import { EventDataService } from './../../../../../core/services/eventdata.service';

@Component({
  selector: 'tb-tile-manager-tab',
  templateUrl: './tile-manager-tab.component.html',
  styleUrls: ['./tile-manager-tab.component.scss']
})
export class TileManagerTabComponent implements AfterContentInit {

  subscriptions = [];

  @Input() activated: boolean = true;
  @ContentChild(TemplateRef) templateRef: any;
  @ViewChild(TabStripTabComponent) tabComponent;

  @Input() iconType: string = 'M4';
  @Input() _icon: string = 'erp-purchaseorder';

  @Input()
  set icon(icon: any) {
    this._icon = icon instanceof Object ? icon.value : icon;
  }

  get icon() {
    return this._icon;
  }

  private _title: string;

  public get title() {
    return this._title;
  }

  @Input() public set title(title: any) {
    this._title = title instanceof Object ? title.value.replace("&", "") : title.replace("&", "");
  }

  constructor(
    private eventData: EventDataService,
    private cdr: ChangeDetectorRef
  ) { }

  ngAfterContentInit() {
    this.subscriptions.push(this.eventData.activationChanged.subscribe(() => this.cdr.markForCheck()));
  }

  ngOnDestroy() {
    this.subscriptions.forEach((s) => { s.unsubscribe(); });
  }
}
