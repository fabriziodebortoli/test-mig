import { Component, Output, EventEmitter, OnInit, AfterContentInit, Input } from '@angular/core';

import { MenuTabComponent } from './menu-tab/menu-tab.component';

@Component({
  selector: 'tb-menu-tabber',
  templateUrl: './menu-tabber.component.html',
  styleUrls: ['./menu-tabber.component.scss']
})
export class MenuTabberComponent {

  tabs: MenuTabComponent[] = [];
  @Output() selectedTab: EventEmitter<any> = new EventEmitter(true);

  selectTab(tab: MenuTabComponent) {

    if (!this.tabs)
      return;

    this.tabs.forEach((t) => {
      t.active = false;
    });
    tab.active = true;

    this.selectedTab.emit(this.tabs.indexOf(tab));
  }

  addTab(tab: MenuTabComponent) {
    this.tabs.push(tab);
    this.selectTab(tab);
  }

  removeTab(tab: MenuTabComponent) {
    this.tabs.splice(this.tabs.indexOf(tab), 1);
    if (tab.active && this.tabs.length > 0) {
      this.tabs[0].active = true;
      this.selectedTab.emit(0);
    }
  }

}
