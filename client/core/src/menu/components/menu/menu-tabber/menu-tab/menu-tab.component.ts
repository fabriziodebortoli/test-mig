import { Component, Output, EventEmitter, OnInit, OnDestroy, Input } from '@angular/core';

import { MenuTabberComponent } from '../menu-tabber.component';

@Component({
  selector: 'tb-menu-tab',
  template: '',
})
export class MenuTabComponent implements OnDestroy {

  active: boolean;
  @Input() title: string = '...';

  constructor(public tabs: MenuTabberComponent) {
    tabs.addTab(this);
  }

  ngOnDestroy() {
    this.tabs.removeTab(this);
  }

}
