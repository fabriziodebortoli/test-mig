import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { ContextMenuItem } from '../../../shared/models/context-menu-item.model';
import { EventDataService } from '../../../core/services/eventdata.service';
import { WebSocketService } from '../../../core/services/websocket.service';
export declare class ContextMenuComponent {
    private webSocketService;
    private eventDataService;
    anchorAlign: Align;
    popupAlign: Align;
    private collision;
    anchorAlign2: Align;
    popupAlign2: Align;
    private show;
    private isMouseDown;
    contextMenuBinding: ContextMenuItem[];
    currentItem: ContextMenuItem;
    fontIcon: string;
    contextMenu: ContextMenuItem[];
    popupClass: string;
    divFocus: HTMLElement;
    constructor(webSocketService: WebSocketService, eventDataService: EventDataService);
    onOpen(): void;
    doCommand(menuItem: any): void;
    onToggle(): void;
    closePopupIf(): void;
    setMouseDown(): void;
    hasSubItems(item: ContextMenuItem): boolean;
    openSubItems(open: boolean, item: ContextMenuItem): void;
    outView(item: ContextMenuItem): void;
}