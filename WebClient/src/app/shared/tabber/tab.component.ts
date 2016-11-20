import { ComponentInfo } from './../../shared';
import { TabberComponent } from './tabber.component';
import { TbComponent } from './../tb.component';
import { Component, Output, EventEmitter, OnInit, OnDestroy, ComponentRef, Input, ViewChild, ViewContainerRef } from '@angular/core';


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

  @Output() close: EventEmitter<any> = new EventEmitter();

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
    if (this.cmpRef) {
      this.cmpRef.destroy();
    }
    this.tabs.removeTab(this);

  }
}