import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { ContextMenuDirective } from './../directives/context-menu.directive';
import { MenuItem } from './menu-item.model';
import { Component, Input, ViewChild } from '@angular/core';
import { EventDataService } from './../../core/eventdata.service';
import { WebSocketService } from './../../core/websocket.service';


@Component({
  selector: 'tb-context-menu',
  templateUrl: './context-menu.component.html',
  styleUrls: ['./context-menu.component.scss']
})
export class ContextMenuComponent {
  anchorAlign: Align = { horizontal: 'left', vertical: 'bottom' };
  popupAlign: Align = { horizontal: 'right', vertical: 'top' };

  anchorAlign2: Align = { horizontal: 'right', vertical: 'center' };
  popupAlign2: Align = { horizontal: 'left', vertical: 'top' };
  private show = false;
  private showSubItems = false;
  private isMouseDown = false;
  @ViewChild('anchor') divFocus: HTMLElement;

  contextMenuBinding: MenuItem[];
  contextMenu: MenuItem[];

  constructor(private webSocketService: WebSocketService, private eventDataService: EventDataService) {
    // SCENARIO 1: RIEMPIRE DA SERVER
    // this.webSocketService.contextMenu.subscribe((result) => {
    //   this.contextMenu = result.contextMenu;
    // });

    // SCENARIO 2: RIEMPITO DA HTML
    this.contextMenu = new Array<MenuItem>();
    const subItems = new Array<MenuItem>();
    const item1 = new MenuItem('id1', 'TextId1', true, false);
    const item2 = new MenuItem('id', 'roby', false, false);
    subItems.push(item1, item2);
    const item3 = new MenuItem("id2", "TextId2", true, false, subItems);
    this.contextMenu.push(item1, item2, item3 );

  }

  public onToggle(): void {
    this.show = !this.show;
    if(!this.show)
     this.showSubItems = false;
  }

  public closePopupIf(): void {
    if (this.isMouseDown) {
      this.isMouseDown = false;
      document.getElementById('anchor').focus();
      return;
    }
    this.show = false;
    this.showSubItems = false;
  }

  setMouseDown() {
    this.isMouseDown = true;
  }

  onOpen() {
    this.eventDataService.onContextMenu.emit(this.contextMenuBinding); // idd_pippo_ContextMenu
  }

  public doCommand(menuItem: any) {
    if (!menuItem) { console.log('NOT doCommand for ContextMenu!'); return; }
    if (this.hasSubItems(menuItem)) { return; }
    this.eventDataService.command.emit(menuItem.id);
    console.log('doCommand OK!');
    this.onToggle();
  }

  hasSubItems(item: MenuItem) {
    const y = item.subItems;
    return y !== null && y.length > 0;
  }

  openSubItems(open: boolean, item: MenuItem) {
    if(!this.hasSubItems(item))
      return;
    this.showSubItems = open;
  }

  outView() {
    this.show = false;
    this.showSubItems = false;
    this.isMouseDown = false;
   // return true;
  }
  anchorViewportLeave() {
    this.closePopupIf();
  }


}
