import { TbComponent } from '..';
import { TabberComponent } from './tabber.component';
import { Component, Output, EventEmitter, OnInit, OnDestroy, Input } from '@angular/core';

@Component({
  selector: 'tb-tab',
  template: `
    <div [hidden]="!active" class="tabContent">
      <ng-content></ng-content>
    </div>
  `,
  styleUrls: ['./tab.component.css']
})
export class TabComponent extends TbComponent implements OnInit, OnDestroy {
  active: boolean;
  @Input() tabTitle: string = 'tabTitle';

  @Output() close: EventEmitter<any> = new EventEmitter();

  constructor(private tabs: TabberComponent) {
    super();
    console.log("tabTitle", this.tabTitle);
    tabs.addTab(this);
  }

  ngOnInit() {
  }

  ngOnDestroy() {
    this.tabs.removeTab(this);
  }
}
