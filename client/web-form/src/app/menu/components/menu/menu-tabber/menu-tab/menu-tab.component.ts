import { Component, Output, EventEmitter, OnInit, OnDestroy, Input } from '@angular/core';
import { MenuTabberComponent } from '../menu-tabber.component';
import { TbComponent } from 'tb-shared';

@Component({
  selector: 'tb-menu-tab',
  template: '',
})
export class MenuTabComponent {

  active: boolean;
  @Input() title: string = '...';

  constructor(private tabs: MenuTabberComponent) {
    tabs.addTab(this);
  }

}
