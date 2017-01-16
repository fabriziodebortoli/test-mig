import { Component, Output, EventEmitter } from '@angular/core';
import { TbComponent } from '../../../';
import { TabComponent } from '../tab/tab.component';

@Component({
  selector: 'tb-tabber',
  templateUrl: './tabber.component.html',
  styleUrls: ['./tabber.component.scss']
})
export class TabberComponent extends TbComponent {

  tabs: TabComponent[] = [];

  @Output() close: EventEmitter<any> = new EventEmitter();
  @Output() selectedTab: EventEmitter<any> = new EventEmitter();

  selectTab(tab: TabComponent) {
    this.tabs.forEach((t) => {
      t.active = false;
    });
    tab.active = true;
    this.selectedTab.emit(this.tabs.indexOf(tab));
  }

  closeTab(tab: TabComponent) {
    this.close.emit(tab);
    tab.close.emit(tab);
  }

  addTab(tab: TabComponent) {
    this.tabs.push(tab);
    this.selectTab(tab);
  }

  removeTab(tab: TabComponent) {
    this.tabs.splice(this.tabs.indexOf(tab), 1);
    if (tab.active && this.tabs.length > 0) {
      this.tabs[0].active = true;
         this.selectedTab.emit(0);
    }
  }

}

