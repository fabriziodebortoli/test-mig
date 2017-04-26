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
  private show = false;

  contextMenuBinding: MenuItem[];
  contextMenu: MenuItem[];

  constructor(private webSocketService: WebSocketService, private eventDataService: EventDataService) {
    this.webSocketService.contextMenu.subscribe((result) => {
      this.contextMenu = result.contextMenu;
    });
  }

  public onToggle(): void {
    this.show = !this.show;
  }

    public closePopup(): void {
    this.show = false;
  }


  onOpen() {
    this.eventDataService.onContextMenu.emit(this.contextMenuBinding); // idd_pippo_ContextMenu
  }

  public doCommand(menuItem: any) {
    if (!menuItem) { console.log('NOT doCommand for ContextMenu!'); return; }
    this.eventDataService.command.emit(menuItem.id);
    console.log('doCommand OK!');
  }


}
