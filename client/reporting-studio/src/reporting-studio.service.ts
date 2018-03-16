import { Injectable, EventEmitter, Output } from '@angular/core';
import { Http } from '@angular/http';

import { EventDataService, DocumentService, ComponentService, TbComponentServiceParams } from '@taskbuilder/core';

import { Subject } from './rxjs.imports';
import { CommandType } from './models/command-type.model';

@Injectable()
export class ReportingStudioService extends DocumentService {
    [x: string]: any;
    pageNum: number = 1;
    runEnabled: boolean = true;
    showAsk = false;
    isSnapshot: boolean = false;
    showSnapshotDialog = false;
    nameSnap: string = "";
    allUsers: string = "";
    namespace: string = "";

    rsServer: string = ''
    websocket: WebSocket;
    message: Subject<any> = new Subject<string>();

    @Output() eventSnapshot = new EventEmitter<void>();

    constructor(
        params: TbComponentServiceParams,
        eventData: EventDataService,
        public cmpService: ComponentService,
        public http: Http
    ) {
        super(params, eventData);

        this.rsServer = this.infoService.getWsBaseUrl() + '/rs';
        this.websocket = new WebSocket(this.rsServer);
        this.websocket.onopen = evt => { this.onOpen(evt) };
        this.websocket.onclose = evt => { this.onClose(evt) };
        this.websocket.onmessage = evt => { this.onMessage(evt) };
        this.websocket.onerror = evt => { this.onError(evt) };
    }

    onOpen(evt: any) {
        this.writeToScreen('CONNECTED');
    }

    onClose(evt) {
        this.writeToScreen('DISCONNECTED');
    }

    onMessage(evt) {
        this.message.next(evt.data);
    }

    onError(evt) {
        this.writeToScreen(evt.data);
    }

    doSend(message: string) {
        this.waitForConnection(() => {
            this.websocket.send(message);
        }, 100);
    }

    //--------------------------------------------------
    doSendSync(message): boolean {
        this.waitForConnection(() => {
            this.websocket.send(message);
            return true;
        }, 100);
        return false;
    }

    //--------------------------------------------------
    waitForConnection(callback, interval) {
        if (this.websocket.readyState === 1) {
            callback();
        } else {
            let that = this;
            setTimeout(function () {
                that.waitForConnection(callback, interval);
            }, interval);
        }
    };

    //--------------------------------------------------
    writeToScreen(message) {
        console.log(message);
    }

    //--------------------------------------------------
    closeConnection() {
        this.websocket.close();
    }

    //--------------------------------------------------
    close() {
        super.close();
        this.cmpService.removeComponentById(this.mainCmpId);
        this.closeConnection();
    }

    //--------------------------------------------------
    reset() {
        this.pageNum = 1;
        this.showAsk = false;
    }

    //--------------------------------------------------
    generateId(): string {
        let result = '';
        let chars = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ';
        for (var i = 10; i > 0; --i) result += chars[Math.floor(Math.random() * chars.length)];
        return result;
    }

    //--------------------------------------------------
    initiaziedSnapshot(nameSnapshot, allUsers) {
        this.nameSnap = nameSnapshot;
        this.allUsers = allUsers;
        this.eventSnapshot.emit();
    }
}

