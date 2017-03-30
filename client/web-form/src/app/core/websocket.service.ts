import { EventEmitter, Injectable } from '@angular/core';
import 'rxjs/add/operator/toPromise';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { environment } from './../../environments/environment';

import { HttpService } from './http.service';
import { CommandService } from './command.service'

import { Logger } from 'libclient';

@Injectable()
export class WebSocketService {
    public status: string = 'Undefined';
    private connection: WebSocket;

    public error: EventEmitter<any> = new EventEmitter();
    public dataReady: EventEmitter<any> = new EventEmitter();
    public serverCommandMapReady: EventEmitter<any> = new EventEmitter();
    public windowOpen: EventEmitter<any> = new EventEmitter();
    public windowClose: EventEmitter<any> = new EventEmitter();
    public activationData: EventEmitter<any> = new EventEmitter();
    public itemSource: EventEmitter<any> = new EventEmitter();
    public open: EventEmitter<any> = new EventEmitter();
    public close: EventEmitter<any> = new EventEmitter();

    constructor(private httpService: HttpService,
        private cookieService: CookieService,
        private logger: Logger,
        private commandService: CommandService) {
    }

    wsConnect(): void {
        let $this = this;

        let url = environment.wsBaseUrl;
        this.logger.debug('wsConnecting... ' + url);

        this.connection = new WebSocket(url);
        this.connection.onmessage = function (e) {
            if (typeof (e.data) === 'string') {
                try {
                    let obj = JSON.parse(e.data);

                    switch (obj.cmd) {
                        case 'DataReady': $this.dataReady.emit(obj.args); break;
                        case 'WindowOpen': $this.windowOpen.emit(obj.args); break;
                        case 'ActivationData': $this.activationData.emit(obj.args); break;
                        case 'WindowClose': $this.windowClose.emit(obj.args); break;
                        case 'ItemSource': $this.itemSource.emit(obj.args); break;
                        case 'ServerCommandMapReady': $this.serverCommandMapReady.emit(obj.args); break;
                        //when tbloader has connected to gate, I receive this message; then I can
                        //request the list of opened windows
                        case 'SetServerWebSocketName': $this.connection.send(JSON.stringify({ cmd: 'getOpenDocuments' })); break;
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
            //sets the name for this client socket 
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

     doFillListBox(cmpId: String, itemSource: any): void {
        let data = { cmd: 'doFillListBox', cmpId: cmpId, itemSource: itemSource };
        this.connection.send(JSON.stringify(data));
    }

    doCommand(cmpId: String, id: String, modelData?: any): void {
        let data = { cmd: 'doCommand', cmpId: cmpId, id: id, model: modelData };
        this.connection.send(JSON.stringify(data));
    }

    doValueChanged(cmpId: String, id: String, modelData?: any): void {
        let data = { cmd: 'doValueChanged', cmpId: cmpId, id: id, model: modelData };
        //questo if andrebbe anticipato nel chiamante, se so che non e' azione server side, non devo chiamare servizio websocket
        if (this.commandService.isServerSideCommand(id)) {
            this.connection.send(JSON.stringify(data));
        }
        //else
        //azione solo lato client. 
    }

    getDocumentData(cmpId: String) {
        let data = { cmd: 'getDocumentData', cmpId: cmpId };
        this.connection.send(JSON.stringify(data));
    }
}
export class SocketMessage {
    constructor(public name: string, public content: any) {

    }
}