import { OnChanges, DoCheck, OnDestroy } from '@angular/core';
import { ControlComponent } from './../control.component';
import { EventDataService } from './../../services/eventdata.service';
import { WebSocketService } from './../../services/websocket.service';
export declare class ComboComponent extends ControlComponent implements OnChanges, DoCheck, OnDestroy {
    private webSocketService;
    private eventDataService;
    private items;
    private selectedItem;
    private itemSourceSub;
    itemSource: any;
    hotLink: any;
    width: number;
    constructor(webSocketService: WebSocketService, eventDataService: EventDataService);
    fillListBox(): void;
    onChange(change: any): void;
    ngDoCheck(): void;
    ngOnChanges(changes: {}): void;
    ngOnDestroy(): void;
}
