import { Component, Output, EventEmitter, OnInit, OnDestroy, Input } from '@angular/core';

import { TbComponent } from '@taskbuilder/core';
import { MenuTabberComponent } from '../menu-tabber.component';

@Component({
  selector: 'tb-menu-tab',
  template: '',
})
export class MenuTabComponent implements OnDestroy {

  active: boolean;
  @Input() title: string = '...';

  constructor(private tabs: MenuTabberComponent) {
    tabs.addTab(this);
  }

  ngOnDestroy() {
    this.tabs.removeTab(this);
  }

}
