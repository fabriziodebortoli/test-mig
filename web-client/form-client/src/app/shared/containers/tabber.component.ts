import { TbComponent } from '..';
import { TabComponent } from './tab.component';
import {
  Component, Output, EventEmitter
} from '@angular/core';
@Component({
  selector: 'tb-tabs',
  templateUrl: './tabber.component.html',
  styleUrls: ['./tabber.component.css']
})
export class TabberComponent extends TbComponent {
  tabs: TabComponent[] = [];

  @Output() close: EventEmitter<any> = new EventEmitter();

  selectTab(tab: TabComponent) {
    this.tabs.forEach((t) => {
      t.active = false;
    });
    tab.active = true;
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
    }
  }
}

