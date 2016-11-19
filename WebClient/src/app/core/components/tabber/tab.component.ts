import { ComponentInfo } from './../../models/component.info';
import { TabberComponent } from './tabber.component';
import { TbComponent } from './../tb.component';
import { Component, OnInit, OnDestroy, ComponentRef, Input, ViewChild, ViewContainerRef } from '@angular/core';


@Component({
  selector: 'tb-tab',
  template: `
    <div [hidden]="!active" class="tabContent">
    <div #tabContent></div>
      <ng-content></ng-content>
    </div>
  `,
  styleUrls: ['./tab.component.css']
})
export class TabComponent implements OnInit, OnDestroy {
  active: boolean;
  cmpRef: ComponentRef<TbComponent>;
  @Input() componentInfo: ComponentInfo;
  @Input() tabTitle: string = '';
  @ViewChild('tabContent', { read: ViewContainerRef }) tabContent: ViewContainerRef;

  constructor(private tabs: TabberComponent) {
    tabs.addTab(this);
  }

  ngOnInit() {
    if (this.componentInfo) {
      this.cmpRef = this.tabContent.createComponent(this.componentInfo.factory);
      this.tabTitle = this.cmpRef.instance.title;
    }
  }

  ngOnDestroy() {
    this.cmpRef.destroy();
    this.tabs.removeTab(this);
  }
}