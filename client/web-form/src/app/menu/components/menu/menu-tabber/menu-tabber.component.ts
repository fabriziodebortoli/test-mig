import { Component, Output, EventEmitter, OnInit, AfterContentInit, Input } from '@angular/core';
import { TbComponent } from 'tb-shared';
import { MenuTabComponent } from './menu-tab/menu-tab.component';

@Component({
  selector: 'tb-menu-tabber',
  templateUrl: './menu-tabber.component.html',
  styleUrls: ['./menu-tabber.component.scss']
})
export class MenuTabberComponent extends TbComponent {

  tabs: MenuTabComponent[] = [];
  @Output() selectedTab: EventEmitter<any> = new EventEmitter(true);

  selectTab(tab: MenuTabComponent) {

    this.tabs.forEach((t) => {
      t.active = false;
    });
    tab.active = true;

    this.selectedTab.emit(this.tabs.indexOf(tab) - 1);
  }

  addTab(tab: MenuTabComponent) {
    this.tabs.push(tab);
  }

}
