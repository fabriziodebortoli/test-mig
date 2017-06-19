import { Component, Input, ViewChild } from '@angular/core';

import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';

import { ContextMenuItem } from '../../../shared/models/context-menu-item.model';

import { EventDataService } from '../../../core/services/eventdata.service';
import { WebSocketService } from '../../../core/services/websocket.service';

@Component({
  selector: 'tb-context-menu',
  template: "<div id=\"anchor\" #anchor tabindex=\"0\" (blur)=\"closePopupIf()\"> <md-icon (click)=\"onToggle()\" [ngClass]=\"{ 'borderOn': show }\">{{fontIcon}}</md-icon> <kendo-popup *ngIf=\"show\" popupClass='{{popupClass}}' [anchor]=\"anchor\" (anchorViewportLeave)=\"outView(null)\" (open)=\"onOpen()\" [collision]=\"collision\" [anchorAlign]=\"anchorAlign\" [popupAlign]=\"popupAlign\"> <button kendoButton *ngFor=\"let menuItem of contextMenu\" class=\"content\" [disabled]=\"!menuItem?.enabled\" (mouseenter)=\"openSubItems(true, menuItem)\" (mouseleave)=\"openSubItems(false, menuItem)\" (mousedown)=\"setMouseDown()\" (click)=\"doCommand(menuItem)\"> <div #anchor2 class=\"divW\"> <md-icon *ngIf=\"hasSubItems(menuItem)\" class=\"subsIcon\">chevron_left</md-icon> <span [ngClass]=\"{ 'spaceOn': !hasSubItems(menuItem)}\"> {{menuItem?.text}} </span> <md-icon *ngIf=\"menuItem?.checked\" class=\"checkIcon\">check</md-icon>  </div> <kendo-popup *ngIf=\"menuItem?.showMySub\" [anchor]=\"anchor2\" (anchorViewportLeave)=\"outView(menuItem)\" [collision]=\"collision\" [anchorAlign]=\"anchorAlign2\" [popupAlign]=\"popupAlign2\" [popupClass]=\"'content popup'\"> <button kendoButton *ngFor=\"let subItem of menuItem.subItems\" class=\"content\" (click)=\"doCommand(subItem)\" [disabled]=\"!subItem?.enabled\"> <div class=\"divW\"> <md-icon class=\"checkIcon\" *ngIf=\"subItem?.checked\">check</md-icon> <span class=\"spaceOn\"> {{subItem?.text}} </span> </div> </button> </kendo-popup> </button> </kendo-popup> </div>",
  styles: [".contextMenuClass { width: 5%; margin-right: 0px; margin-left: auto; padding-top: 5px; outline: none; } .material-icons { font-size: 1.3rem; cursor: pointer; flex-direction: row; } .cm-content { display: flex; flex-direction: column; flex-wrap: wrap; width: 200px; height: 24px; } .divW { width: inherit; flex-direction: row; flex-wrap: wrap; text-align: left; font-size: smaller; } .checkIcon { float: right; } .subsIcon { float: left; } .borderOn { border: 1px solid blue; } .spaceOn { padding-left: 24px; } span { vertical-align: sub; } "]
})
export class ContextMenuComponent {
  anchorAlign: Align = { horizontal: 'left', vertical: 'bottom' };
  popupAlign: Align = { horizontal: 'right', vertical: 'top' };
  private collision: Collision = { horizontal: 'flip', vertical: 'fit' };
  anchorAlign2: Align = { horizontal: 'left', vertical: 'top' };
  popupAlign2: Align = { horizontal: 'right', vertical: 'top' };
  private show = false;
  private isMouseDown = false;
  contextMenuBinding: ContextMenuItem[];
  currentItem: ContextMenuItem;

  @Input() fontIcon = 'more_vert';
  @Input() contextMenu: ContextMenuItem[];
  @Input() popupClass = 'content popup';
  @ViewChild('anchor') divFocus: HTMLElement;



  constructor(private webSocketService: WebSocketService, private eventDataService: EventDataService) {
    // SCENARIO 1: RIEMPIRE DA SERVER
    // this.webSocketService.contextMenu.subscribe((result) => {
    //   this.contextMenu = result.contextMenu;
    // });

    // SCENARIO 2: RIEMPITO DA HTML
    this.contextMenu = new Array<ContextMenuItem>();

    const subItems_bis = new Array<ContextMenuItem>();
    const item4 = new ContextMenuItem('solo questo disable unchecked', 'Id4', false, false);
    subItems_bis.push(item4);

    const subItems = new Array<ContextMenuItem>();
    const item1 = new ContextMenuItem('disabled unchecked', 'Id1', false, false);
    const item5 = new ContextMenuItem('enabled checked', 'Id5', true, true);
    const item2 = new ContextMenuItem('has one sub item', 'Id2', true, false, subItems_bis);
    subItems.push(item1, item5);

    const item3 = new ContextMenuItem('has 2 sub items', 'Id3', true, false, subItems);
    this.contextMenu.push(item1, item2, item5, item3);

  }

  onOpen() {
  }

  public doCommand(menuItem: any) {
    if (!menuItem) { console.log('NOT doCommand for ContextMenu!'); return; }
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
