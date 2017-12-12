import { TabStripComponent } from '@progress/kendo-angular-layout';
import { AdminTabComponent } from '../admin-tab/admin-tab.component';
import { Component, ContentChildren, ViewChild, AfterContentInit } from '@angular/core';

const resolvedPromise = Promise.resolve(null); //fancy setTimeout

@Component({
  selector: 'admin-tabs',
  templateUrl: './admin-tabs.component.html',
  styleUrls: ['./admin-tabs.component.css']
})
export class AdminTabsComponent implements AfterContentInit {

  @ContentChildren(AdminTabComponent) tabs;
  @ViewChild(TabStripComponent) tab;

  constructor() {}

  ngAfterContentInit(): void {
    resolvedPromise.then(() => {
      let innerTabs = this.tabs.toArray();
      let internalTabComponents = [];
      for (let i = 0; i < innerTabs.length; i++) {
        if (i === 0) {
          innerTabs[i].tab.selected = true;
        }
        internalTabComponents.push(innerTabs[i].tab);
      }
      this.tab.tabs.reset(internalTabComponents);
      if (innerTabs.length > 0) {
        this.tab.selectTab(0);
      }
    });
  }
}