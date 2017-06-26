import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { MenuItem } from './../../../context-menu/menu-item.model';
import { Component, Input, ViewChild } from '@angular/core';
import { EventDataService } from '@taskbuilder/core';
import { WebSocketService } from '@taskbuilder/core';

@Component({
  selector: 'tb-topbar-menu-elements',
  templateUrl: './topbar-menu-elements.component.html',
  styleUrls: ['./topbar-menu-elements.component.scss']
})
export class TopbarMenuElementsComponent {
  anchorAlign: Align = { horizontal: 'right', vertical: 'bottom' };
  popupAlign: Align = { horizontal: 'right', vertical: 'top' };
  private collision: Collision = { horizontal: 'flip', vertical: 'fit' };
  anchorAlign2: Align = { horizontal: 'left', vertical: 'top' };
  popupAlign2: Align = { horizontal: 'right', vertical: 'top' };
  private show = false;
  private isMouseDown = false;
  currentItem: MenuItem;

  @Input() fontIcon = 'more_vert';
  @Input() menuElements: MenuItem[];
  @ViewChild('anchor') divFocus: HTMLElement;

  constructor(private webSocketService: WebSocketService, private eventDataService: EventDataService) {
  }

  onOpen() {
  }

  public doCommand(menuItem: any) {
    if (!menuItem) { console.log('NOT doCommand for menuElements!'); return; }
    if (this.hasSubItems(menuItem)) { return; }
    this.eventDataService.command.emit(menuItem.id);
    console.log('doCommand OK!');
    this.onToggle();
  }

  ///////////////////////////////////////////////////////////////////////////////////

  public onToggle(): void {
    this.show = !this.show;
    if (!this.show && this.currentItem !== null && this.currentItem !== undefined) {
      this.currentItem.showMySub = false;
    }
  }

  public closePopupIf(): void {
    if (this.isMouseDown) {
      this.isMouseDown = false;
      document.getElementById('anchor').focus();
      return;
    }
    this.outView(this.currentItem);
  }

  setMouseDown() {
    this.isMouseDown = true;
  }

  hasSubItems(item: MenuItem) {
    const y = item.subItems;
    return y !== null && y.length > 0;
  }

  openSubItems(open: boolean, item: MenuItem) {
    if (!this.hasSubItems(item) || item === null || item === undefined) {
      return;
    }
    item.showMySub = open;
    this.currentItem = item;
  }

  outView(item: MenuItem) {
    if (item !== null && item !== undefined) {
      item.showMySub = false;
    }

    this.show = false;
    this.currentItem = null;
    this.isMouseDown = false;
  }

}
