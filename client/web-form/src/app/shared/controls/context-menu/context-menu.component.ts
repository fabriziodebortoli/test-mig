import { MenuItem } from './menu-item.model';
import { Component, Input } from '@angular/core';
import { EventDataService } from './../../../core/eventdata.service';
import { WebSocketService } from './../../../core/websocket.service';


@Component({
  selector: 'tb-context-menu',
  templateUrl: './context-menu.component.html',
  styleUrls: ['./context-menu.component.scss']
})
export class ContextMenuComponent {

  private show: boolean = false;

  @Input() contextMenuBinding: any;
  contextMenu: any;

  constructor(private webSocketService: WebSocketService, private eventDataService: EventDataService) {

//   let menuItem = new MenuItem();
//         menuItem.text = 'ciao';
// this.contextMenu.push(menuItem);

    this.webSocketService.contextMenu.subscribe((result) => {
      console.log('result '  + 'a-' + result + '-a');
      this.contextMenu = result.contextMenu;
    });
  }

  public onToggle(): void {
    this.show = !this.show;
  }

  onOpen() {
    this.eventDataService.onContextMenu.emit(this.contextMenuBinding); // idd_pippo_CM
    console.log('onOpenContextMenu'  + 'a' + this.contextMenuBinding + 'a');
  }

}
