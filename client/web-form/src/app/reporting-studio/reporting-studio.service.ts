import { ComponentService } from './../core/component.service';
import { environment } from './../../environments/environment';
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Subject';

import { Logger } from './../core/logger.service';

import { EventDataService } from './../core/eventdata.service';
import { DocumentService } from './../core/document.service';

@Injectable()
export class ReportingStudioService extends DocumentService {
    public componentId = '';
    public pageNum: number = 1;
    public askPage: string = '';
    public showAsk = false;
    private rsServer: string = environment.baseSocket + 'rs';
    websocket: WebSocket;
    public message: Subject<any> = new Subject<string>();

    constructor(logger: Logger, eventData: EventDataService, private cmpService: ComponentService) {
        super(logger, eventData);

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

    doSendSync(message): boolean {
        this.waitForConnection(() => {
            this.websocket.send(message);
            return true;
        }, 100);

        return false;
    }

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

    writeToScreen(message) {
        console.log(message);
    }

    closeConnection() {
        this.websocket.close();
    }

    close() {
        super.close();
        this.cmpService.removeComponentById(this.componentId);
        this.closeConnection();
    }
}
