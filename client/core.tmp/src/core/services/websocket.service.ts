﻿import { EventEmitter, Injectable } from '@angular/core';

import 'rxjs/add/operator/toPromise';

import { MessageDlgArgs, MessageDlgResult } from './../containers/message-dialog/message-dialog.component';
import { CookieService } from 'angular2-cookie/services/cookies.service';

import { HttpService } from './http.service';
import { UrlService } from './url.service';
// import { CommandService } from './command.service';

import { Logger } from './logger.service';

@Injectable()
export class WebSocketService {
    public status = 'Undefined';
    private connection: WebSocket;

    public error: EventEmitter<any> = new EventEmitter();
    public modelData: EventEmitter<any> = new EventEmitter();
    public serverCommands: EventEmitter<any> = new EventEmitter();
    public windowOpen: EventEmitter<any> = new EventEmitter();
    public windowClose: EventEmitter<any> = new EventEmitter();
    public activationData: EventEmitter<any> = new EventEmitter();
    public itemSource: EventEmitter<any> = new EventEmitter();
    public open: EventEmitter<any> = new EventEmitter();
    public close: EventEmitter<any> = new EventEmitter();
    public message: EventEmitter<MessageDlgArgs> = new EventEmitter();
    public buttonsState: EventEmitter<any> = new EventEmitter();


    constructor(
        private httpService: HttpService,
        private urlService: UrlService,
        private cookieService: CookieService,
        private logger: Logger) {
    }

    wsConnect(): void {
        const $this = this;

        const url = this.urlService.getWsUrl();
        this.logger.debug('wsConnecting... ' + url);

        this.connection = new WebSocket(url);
        this.connection.onmessage = function (e) {
            if (typeof (e.data) === 'string') {
                try {
                    const obj = JSON.parse(e.data);

                    switch (obj.cmd) {
                        case 'ModelData': $this.modelData.emit(obj.args); break;
                        case 'WindowOpen': $this.windowOpen.emit(obj.args); break;
                        case 'ActivationData': $this.activationData.emit(obj.args); break;
                        case 'WindowClose': $this.windowClose.emit(obj.args); break;
                        case 'ItemSource': $this.itemSource.emit(obj.args); break;
                        case 'ServerCommands': $this.serverCommands.emit(obj.args); break;
                        // when tbloader has connected to gate, I receive this message; then I can
                        // request the list of opened windows
                        case 'MessageDialog': $this.message.emit(obj.args); break;
                        case 'SetServerWebSocketName': $this.connection.send(JSON.stringify({ cmd: 'getOpenDocuments' })); break;
                        case 'ButtonsState': $this.buttonsState.emit(obj.args); break;
                        default: break;
                    }

                } catch (e) {
                    $this.logger.error('Invalid json string:\n' + e.data);
                }
            }
        };
        this.connection.onerror = (arg) => {
            this.logger.error('wsOnError' + JSON.stringify(arg));
            this.error.emit(arg);
            this.status = 'Error';
        };

        this.connection.onopen = (arg) => {
            // sets the name for this client socket
            this.connection.send(JSON.stringify({
                cmd: 'SetClientWebSocketName',
                args:
                {
                    webSocketName: this.cookieService.get('authtoken'),
                    tbLoaderName: this.cookieService.get('tbloader-name')
                }
            }));

            this.status = 'Open';
            this.open.emit(arg);
        };

        this.connection.onclose = (arg) => {
            this.close.emit(arg);
            this.status = 'Closed';
        };
    }

    wsClose() {
        if (this.connection) {
            this.connection.close();
        }
    }

    doFillListBox(cmpId: String, obj: any): void {
        const data = { cmd: 'doFillListBox', cmpId: cmpId, itemSource: obj.itemSource, hotLink: obj.hotLink };

        this.connection.send(JSON.stringify(data));
    }

    doCommand(cmpId: String, id: String, modelData?: any): void {
        const data = { cmd: 'doCommand', cmpId: cmpId, id: id, model: modelData };
        this.connection.send(JSON.stringify(data));
    }

    doValueChanged(cmpId: String, id: String, modelData?: any): void {
        const data = { cmd: 'doValueChanged', cmpId: cmpId, id: id, model: modelData };
        this.connection.send(JSON.stringify(data));
    }

    /* doValueChanged(cmpId: String, id: String, modelData?: any): void {
         const data = { cmd: 'doValueChanged', cmpId: cmpId, id: id, model: modelData };
         // questo if andrebbe anticipato nel chiamante, se so che non e' azione server side, non devo chiamare servizio websocket
         if (this.commandService.isServerSideCommand(id)) {
             this.connection.send(JSON.stringify(data));
         }
         // else
         // azione solo lato client.
     }*/

    getDocumentData(cmpId: String, modelStructure: any) {
        const data = { cmd: 'getDocumentData', cmpId: cmpId, modelStructure: modelStructure };
        this.connection.send(JSON.stringify(data));
    }
    checkMessageDialog(cmpId: String) {
        const data = { cmd: 'checkMessageDialog', cmpId: cmpId };
        this.connection.send(JSON.stringify(data));
    }
    doCloseMessageDialog(cmpId: String, result: MessageDlgResult): void {
        const data = { cmd: 'doCloseMessageDialog', cmpId: cmpId, result: result };
        this.connection.send(JSON.stringify(data));
    }
    setReportResult(cmpId: String, result: any): void {
        const data = { cmd: 'setReportResult', cmpId: cmpId, result: result };
        this.connection.send(JSON.stringify(data));
    }
}
export class SocketMessage {
    constructor(public name: string, public content: any) {

    }
}