import { TbComponent } from '../../../';
import { TabberComponent } from '../tabber/tabber.component';
import { Component, Output, EventEmitter, OnInit, OnDestroy, Input } from '@angular/core';

@Component({
  selector: 'tb-tab',
  templateUrl: './tab.component.html',
  styleUrls: ['./tab.component.scss']
})
export class TabComponent extends TbComponent implements OnInit, OnDestroy {

  active: boolean;

  @Input() tabTitle: string = 'tabTitle';
  @Input() showCloseButton: boolean = true;

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
