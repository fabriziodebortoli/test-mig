import { Http } from '@angular/http';
import { UrlService, HttpService } from '@taskbuilder/core';
import { ComponentService } from '@taskbuilder/core';
import { Logger } from '@taskbuilder/core';
import { Injectable, EventEmitter, Output } from '@angular/core';
import { Subject } from 'rxjs/Subject';

import { EventDataService, InfoService, DocumentService } from '@taskbuilder/core';
import { CommandType} from './models';

@Injectable()
export class ReportingStudioService extends DocumentService {
    [x: string]: any;
    public pageNum: number = 1;
    public running: boolean = false;
    public runEnabled: boolean = true;
    public showAsk = false;

    private rsServer: string = ''
    websocket: WebSocket;
    public message: Subject<any> = new Subject<string>();

    constructor(
        logger: Logger,
        eventData: EventDataService,
        private cmpService: ComponentService,
        private urlServ: UrlService,
        private httpServ: HttpService,
        protected http: Http,
        infoService: InfoService) {
        super(logger, eventData, infoService);

        this.rsServer = this.urlServ.getWsBaseUrl() + '/rs';
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
}

