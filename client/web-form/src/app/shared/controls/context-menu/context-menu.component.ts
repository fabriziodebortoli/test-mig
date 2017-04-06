import { MenuItem } from './menu-item.model';
import { Component, Input } from '@angular/core';


@Component({
  selector: 'tb-context-menu',
  templateUrl: './context-menu.component.html',
  styleUrls: ['./context-menu.component.scss']
})
export class ContextMenuComponent {

  private show: boolean = false;

  // @Input() contextMenu: MenuItem[] = [];

  constructor() {

  }

  public onToggle(): void {
    this.show = !this.show;
  }
}
