import { Component, Input, ViewChild } from '@angular/core';

import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';

import { ContextMenuItem } from '../../../../../shared';
import { WebSocketService } from '../../../../services/websocket.service';
import { EventDataService } from '../../../../services/eventdata.service';

@Component({
  selector: 'tb-topbar-menu-elements',
  template: "<div id=\"anchor\" #anchor tabindex=\"0\" (blur)=\"closePopupIf()\"> <md-icon (click)=\"onToggle()\" [ngClass]=\"{ 'borderOn': show }\">{{fontIcon}}</md-icon> <kendo-popup *ngIf=\"show\" popupClass='' [anchor]=\"anchor\" (anchorViewportLeave)=\"outView(null)\" (open)=\"onOpen()\" [collision]=\"collision\" [anchorAlign]=\"anchorAlign\" [popupAlign]=\"popupAlign\"> <button kendoButton *ngFor=\"let menuItem of menuElements\" class=\"content\" [disabled]=\"!menuItem?.enabled\" (mouseenter)=\"openSubItems(true, menuItem)\" (mouseleave)=\"openSubItems(false, menuItem)\" (mousedown)=\"setMouseDown()\" (click)=\"doCommand(menuItem)\"> <div #anchor2 class=\"divW\"> <md-icon *ngIf=\"hasSubItems(menuItem)\" class=\"subsIcon\">chevron_left</md-icon> <span [ngClass]=\"{ 'spaceOn': !hasSubItems(menuItem)}\"> {{menuItem?.text}} </span> <md-icon *ngIf=\"menuItem?.checked\" class=\"checkIcon\">check</md-icon>  </div> <kendo-popup *ngIf=\"menuItem?.showMySub\" [anchor]=\"anchor2\" (anchorViewportLeave)=\"outView(menuItem)\" [collision]=\"collision\" [anchorAlign]=\"anchorAlign2\" [popupAlign]=\"popupAlign2\" [popupClass]=\"'content popup'\"> <button kendoButton *ngFor=\"let subItem of menuItem.subItems\" class=\"content\" (click)=\"doCommand(subItem)\" [disabled]=\"!subItem?.enabled\"> <div class=\"divW\"> <md-icon class=\"checkIcon\" *ngIf=\"subItem?.checked\">check</md-icon> <span class=\"spaceOn\"> {{subItem?.text}} </span> </div> </button> </kendo-popup> </button> </kendo-popup> </div>",
  styles: [""]
})
export class TopbarMenuElementsComponent {
  anchorAlign: Align = { horizontal: 'right', vertical: 'bottom' };
  popupAlign: Align = { horizontal: 'right', vertical: 'top' };
  private collision: Collision = { horizontal: 'flip', vertical: 'fit' };
  anchorAlign2: Align = { horizontal: 'left', vertical: 'top' };
  popupAlign2: Align = { horizontal: 'right', vertical: 'top' };
  private show = false;
  private isMouseDown = false;
  currentItem: ContextMenuItem;

  @Input() fontIcon = 'more_vert';
  @Input() menuElements: ContextMenuItem[];
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

  hasSubItems(item: ContextMenuItem) {
    const y = item.subItems;
    return y !== null && y.length > 0;
  }

  openSubItems(open: boolean, item: ContextMenuItem) {
    if (!this.hasSubItems(item) || item === null || item === undefined) {
      return;
    }
    item.showMySub = open;
    this.currentItem = item;
  }

  outView(item: ContextMenuItem) {
    if (item !== null && item !== undefined) {
      item.showMySub = false;
    }

    this.show = false;
    this.currentItem = null;
    this.isMouseDown = false;
  }

}
