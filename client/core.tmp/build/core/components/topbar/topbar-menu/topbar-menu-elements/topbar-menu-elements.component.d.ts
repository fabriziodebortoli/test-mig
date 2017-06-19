import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { ContextMenuItem } from '../../../../../shared';
import { WebSocketService } from '../../../../services/websocket.service';
import { EventDataService } from '../../../../services/eventdata.service';
export declare class TopbarMenuElementsComponent {
    private webSocketService;
    private eventDataService;
    anchorAlign: Align;
    popupAlign: Align;
    private collision;
    anchorAlign2: Align;
    popupAlign2: Align;
    private show;
    private isMouseDown;
    currentItem: ContextMenuItem;
    fontIcon: string;
    menuElements: ContextMenuItem[];
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
