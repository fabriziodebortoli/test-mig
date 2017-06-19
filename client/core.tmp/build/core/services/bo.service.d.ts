import { Observable } from 'rxjs/Rx';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { WebSocketService } from './websocket.service';
import { BOHelperService } from './bohelper.service';
export declare class BOService extends DocumentService {
    private webSocketService;
    boHelperService: BOHelperService;
    serverSideCommandMap: any[];
    modelStructure: {};
    subscriptions: any[];
    boClients: BOClient[];
    constructor(webSocketService: WebSocketService, boHelperService: BOHelperService, eventData: EventDataService);
    getPatchedData(): any;
    init(cmpId: string): void;
    dispose(): void;
    close(): void;
    isServerSideCommand(idCommand: string): boolean;
    registerModelField(owner: string, name: string): void;
    doCommand(id: string): void;
    doChange(id: string): void;
    onCommand(id: string): boolean | Observable<boolean>;
    onChange(id: string): boolean | Observable<boolean>;
    doEvent(index: number, eventName: string, id: string, observer: any): void;
}
export declare class BOClient {
    protected boService: BOService;
    constructor(boService: BOService);
    init(): void;
    onCommand(id: string): Observable<boolean>;
    onChange(id: string): Observable<boolean>;
}
