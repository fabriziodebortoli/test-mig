import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { ContextMenuDirective } from './../directives/context-menu.directive';
import { MenuItem } from './menu-item.model';
import { Component, Input, ViewChild } from '@angular/core';
import { EventDataService } from './../../core/eventdata.service';
import { WebSocketService } from '@taskbuilder/core';


@Component({
  selector: 'tb-context-menu',
  templateUrl: './context-menu.component.html',
  styleUrls: ['./context-menu.component.scss']
})
export class ContextMenuComponent {
  anchorAlign: Align = { horizontal: 'left', vertical: 'bottom' };
  popupAlign: Align = { horizontal: 'right', vertical: 'top' };
  private collision: Collision = { horizontal: 'flip', vertical: 'fit' };
  anchorAlign2: Align = { horizontal: 'left', vertical: 'top' };
  popupAlign2: Align = { horizontal: 'right', vertical: 'top' };
  private show = false;
  private isMouseDown = false;
  contextMenuBinding: MenuItem[];
  currentItem: MenuItem;

  @Input() fontIcon = 'more_vert';
  @Input() contextMenu: MenuItem[];
  @Input() popupClass = 'content popup';
  @ViewChild('anchor') divFocus: HTMLElement;



  constructor(private webSocketService: WebSocketService, private eventDataService: EventDataService) {
    // SCENARIO 1: RIEMPIRE DA SERVER
    // this.webSocketService.contextMenu.subscribe((result) => {
    //   this.contextMenu = result.contextMenu;
    // });

    // SCENARIO 2: RIEMPITO DA HTML
    this.contextMenu = new Array<MenuItem>();

    const subItems_bis = new Array<MenuItem>();
    const item4 = new MenuItem('solo questo disable unchecked', 'Id4', false, false);
    subItems_bis.push(item4);

    const subItems = new Array<MenuItem>();
    const item1 = new MenuItem('disabled unchecked', 'Id1', false, false);
    const item5 = new MenuItem('enabled checked', 'Id5', true, true);
    const item2 = new MenuItem('has one sub item', 'Id2', true, false, subItems_bis);
    subItems.push(item1, item5);

    const item3 = new MenuItem('has 2 sub items', 'Id3', true, false, subItems);
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
