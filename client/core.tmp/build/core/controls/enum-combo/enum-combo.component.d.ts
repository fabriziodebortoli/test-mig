import { OnChanges, DoCheck, OnDestroy } from '@angular/core';
import { ControlComponent } from './../control.component';
import { EnumsService } from './../../services/enums.service';
import { EventDataService } from './../../services/eventdata.service';
import { WebSocketService } from './../../services/websocket.service';
export declare class EnumComboComponent extends ControlComponent implements OnChanges, DoCheck, OnDestroy {
    private webSocketService;
    private eventDataService;
    private enumsService;
    private tag;
    private items;
    private selectedItem;
    private itemSourceSub;
    itemSource: any;
    width: number;
    constructor(webSocketService: WebSocketService, eventDataService: EventDataService, enumsService: EnumsService);
    fillListBox(): void;
    onChange(): void;
    ngDoCheck(): void;
    ngOnChanges(changes: {}): void;
    ngOnDestroy(): void;
}
