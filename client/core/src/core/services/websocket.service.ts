import { Observable } from 'rxjs/Rx';
import { EventEmitter, Injectable } from '@angular/core';
import 'rxjs/add/operator/toPromise';


import { CookieService } from 'angular2-cookie/services/cookies.service';

import { MessageDlgArgs, MessageDlgResult } from './../../shared/models';
import { SocketConnectionStatus } from '../../shared/models';

import { LoginSessionService } from './login-session.service';
import { HttpService } from './http.service';
import { UrlService } from './url.service';
// import { CommandService } from './command.service';

import { Logger } from './logger.service';

@Injectable()
export class WebSocketService {
    public loginSessionService: LoginSessionService;

    private connection: WebSocket;
    private _socketConnectionStatus: SocketConnectionStatus = SocketConnectionStatus.None;

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
    public radarQuery: EventEmitter<any> = new EventEmitter();
    public connectionStatus: EventEmitter<SocketConnectionStatus> = new EventEmitter();

    constructor(
        private httpService: HttpService,
        private urlService: UrlService,
        private cookieService: CookieService,
        private logger: Logger) {
    }

    setSocketConnectionStatus(status: SocketConnectionStatus) {

        this._socketConnectionStatus = status;
        this.connectionStatus.emit(status);
    }

    setConnecting() {
        this.setSocketConnectionStatus(SocketConnectionStatus.Connecting);
    }

    wsConnect(): void {
        const $this = this;

        this.setConnecting();

        const url = this.urlService.getWsUrl();
        this.logger.info('wsConnect-url', url)
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
                        case 'RadarQuery': $this.radarQuery.emit(obj.args); break;

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

            this.setSocketConnectionStatus(SocketConnectionStatus.Connected);

            this.open.emit(arg);

        };

        this.connection.onclose = (arg) => {
            this.setSocketConnectionStatus(SocketConnectionStatus.Disconnected);
            this.close.emit(arg);
        };
    }

    wsClose() {
        if (this.connection) {
            this.connection.close();
        }
    }
    checkForOpenConnection(): Observable<boolean> {
        return Observable.create(observer => {
            if (this._socketConnectionStatus === SocketConnectionStatus.Connected) {
                observer.next(true);
                observer.complete();
            } else if (this._socketConnectionStatus === SocketConnectionStatus.Connecting) {
                this.logger.info('Connection not yet avCannot yet use connection, connecting...');
                observer.next(false);
                observer.complete();
            } else {
                const subs = this.loginSessionService.openTbConnectionAsync().subscribe(ret => {
                    subs.unsubscribe();
                    if (ret) {
                        observer.next(true);
                        observer.complete();
                    }
                });
            }
        });
    }
    safeSend(data: any) : Promise<void> {
        return new Promise<void>((resolve, reject) => {
            const subs = this.checkForOpenConnection().subscribe(valid => {
                if (subs) {
                    subs.unsubscribe();
                }
                if (valid) {
                    this.connection.send(JSON.stringify(data));
                    resolve();
                }
                else {
                    this.logger.info("Cannot use web socket, perhaps it is connecting");
                    reject();
                }
            });

        });
    }
    runDocument(ns: String, args: string = ''): Promise<void> {
        const data = { cmd: 'runDocument', ns: ns, sKeyArgs: args };
        return this.safeSend(data);
    }

    doFillListBox(cmpId: String, obj: any): void {
        const data = { cmd: 'doFillListBox', cmpId: cmpId, itemSource: obj.itemSource, hotLink: obj.hotLink };

        this.safeSend(data);
    }
    closeServerComponent(cmpId: string) {
        this.doCommand(cmpId, 'ID_FILE_CLOSE');
    }

    doCommand(cmpId: String, id: String, modelData?: any): void {
        const data = { cmd: 'doCommand', cmpId: cmpId, id: id, model: modelData };
        this.safeSend(data);
    }

    doValueChanged(cmpId: String, id: String, modelData?: any): void {
        const data = { cmd: 'doValueChanged', cmpId: cmpId, id: id, model: modelData };
        this.safeSend(data);
    }

    getRadarQuery(cmpId: String) {
        const data = { cmd: 'getRadarQuery', cmpId: cmpId };
        this.safeSend(data);
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
        this.safeSend(data);

    }

    checkMessageDialog(cmpId: String) {
        const data = { cmd: 'checkMessageDialog', cmpId: cmpId };
        this.safeSend(data);
    }
    doCloseMessageDialog(cmpId: String, result: MessageDlgResult): void {
        const data = { cmd: 'doCloseMessageDialog', cmpId: cmpId, result: result };
        this.safeSend(data);
    }
    setReportResult(cmpId: String, result: any): void {
        const data = { cmd: 'setReportResult', cmpId: cmpId, result: result };
        this.safeSend(data);
    }
}
export class SocketMessage {
    constructor(public name: string, public content: any) {

    }
}
