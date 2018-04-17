import { DiagnosticService } from './diagnostic.service';
import { EventEmitter, Injectable } from '@angular/core';
import { Observable } from '../../rxjs.imports';

import { MessageDlgArgs, DiagnosticData, MessageDlgResult, DiagnosticDlgResult } from './../../shared/models/message-dialog.model';
import { ConnectionStatus } from './../../shared/models/connection-status.enum';

import { TaskBuilderService } from './taskbuilder.service';
import { InfoService } from './info.service';
import { HttpService } from './http.service';
import { Logger } from './logger.service';
import { LoadingService } from './../../core/services/loading.service';
import { LocalizationService } from './../../core/services/localization.service';

@Injectable()
export class WebSocketService extends LocalizationService {
    public connection: WebSocket;
    private _socketConnectionStatus = ConnectionStatus.None;

    public error = new EventEmitter<any>();
    public modelData = new EventEmitter<any>();
    public serverCommands = new EventEmitter<any>();
    public windowOpen = new EventEmitter<any>();
    public windowClose = new EventEmitter<any>();
    public runError = new EventEmitter<any>();
    public activationData = new EventEmitter<any>();
    public itemSource = new EventEmitter<any>();
    public itemSourceExtended = new EventEmitter<any>();
    public open = new EventEmitter<any>();
    public close = new EventEmitter<any>();
    public message = new EventEmitter<MessageDlgArgs>();
    public diagnostic = new EventEmitter<DiagnosticData>();
    public buttonsState = new EventEmitter<any>();
    public connectionStatus = new EventEmitter<ConnectionStatus>();
    public windowStrings = new EventEmitter<any>();
    public behaviours = new EventEmitter<any>();
    public existDataCompleted = new EventEmitter<{ found: boolean, mustExist: boolean, value: string, requestId: string }>();

    constructor(
        public infoService: InfoService,
        public httpService: HttpService,
        public logger: Logger,
        public loadingService: LoadingService,
        public diagnosticService: DiagnosticService) {
        super(infoService, httpService);
    }

    setWsConnectionStatus(status: ConnectionStatus) {
        if (this.infoService.isDesktop)
            return;

        this._socketConnectionStatus = status;
        this.connectionStatus.emit(status);
    }

    wsConnect(): void {
        if (this.infoService.isDesktop)
            return;

        const $this = this;

        this.setWsConnectionStatus(ConnectionStatus.Connecting);

        const url = this.infoService.getWsUrl();
        this.logger.debug('WebSocket Connection...', url)

        this.connection = new WebSocket(url);
        this.connection.onmessage = function (e) {
            if (typeof (e.data) === 'string') {
                let obj = null;
                try {
                    obj = JSON.parse(e.data);
                } catch (ex) {
                    $this.logger.error('Invalid json string:\n' + e.data);
                    return;
                }
                try {
                    switch (obj.cmd) {
                        case 'ModelData': $this.modelData.emit(obj.args); break;
                        case 'WindowOpen': $this.windowOpen.emit(obj.args); break;
                        case 'WindowStrings': $this.windowStrings.emit(obj.args); break;
                        case 'ActivationData': $this.activationData.emit(obj.args); break;
                        case 'WindowClose': $this.windowClose.emit(obj.args); break;
                        case 'RunError': $this.runError.emit(obj.args); break;
                        case 'ItemSource': $this.itemSource.emit(obj.args); break;
                        case 'ItemSourceExtended': $this.itemSourceExtended.emit(obj.args); break;
                        case 'ServerCommands': $this.serverCommands.emit(obj.args); break;
                        case 'ExistDataCompleted': $this.existDataCompleted.emit({ found: obj.args['found'], value: obj.args['value'],
                        mustExist: obj.args['mustExist'], requestId: obj.args['requestId'] }); break;
                        // when tbloader has connected to gate, I receive this message; then I can
                        // request the list of opened windows
                        case 'MessageDialog': $this.message.emit(obj.args); break;
                        case 'Diagnostic': $this.diagnostic.emit(obj.args); break;
                        case 'ButtonsState': $this.buttonsState.emit(obj.args); break;
                        case 'Behaviours': $this.behaviours.emit(obj.args); break;

                        default: break;
                    }
                } catch (ex) {
                    $this.logger.error(ex);
                    return;
                }

            }
        };

        this.connection.onerror = (arg) => {
            this.logger.error('WebSocket onError', JSON.stringify(arg));
            this.error.emit(arg);
        };

        this.connection.onopen = (arg) => {
            this.logger.debug("WebSocket Connected", JSON.stringify(arg));

            // sets the name for this client socket
            this.connection.send(JSON.stringify({
                cmd: 'SetClientWebSocketName',
                args:
                    {
                        webSocketName: sessionStorage.getItem('authtoken'),
                        tbLoaderName: this.infoService.getTbLoaderInfo().name
                    }
            }));

            this.setWsConnectionStatus(ConnectionStatus.Connected);

            this.open.emit(arg);

        };

        this.connection.onclose = (arg) => {
            this.logger.debug("WebSocket onClose", JSON.stringify(arg));
            this.setWsConnectionStatus(ConnectionStatus.Disconnected);
            this.close.emit(arg);
        };
    }

    wsClose() {
        if (this.infoService.isDesktop)
            return;

        if (this.connection) {
            this.connection.close();
        }
    }


    safeSend(data: any): Promise<void> {
        return new Promise<void>((resolve, reject) => {
            if (this._socketConnectionStatus === ConnectionStatus.Unavailable) {
                throw this._TB('Backend services unavailable, cannot execute requested operation');
            } else if (this._socketConnectionStatus === ConnectionStatus.Connected) {
                this.connection.send(JSON.stringify(data));
                resolve();
            } else {
                this.loadingService.setLoading(true, this._TB('Please wait, establishing connection with backend services...'))
                const sub = this.connectionStatus.subscribe((status) => {
                    if (status === ConnectionStatus.Connected) {
                        this.connection.send(JSON.stringify(data));
                        resolve();
                        sub.unsubscribe();
                        this.loadingService.setLoading(false);
                    }
                });
            }
        });
    }
    runDocument(ns: String, args: string = ''): Promise<void> {
        const data = { cmd: 'runDocument', ns: ns, sKeyArgs: args };
        return this.safeSend(data);
    }

    openHyperLink(name: string, cmpId: string): Promise<void> {
        const data = { cmd: 'openHyperLink', cmpId: cmpId, name: name };
        return this.safeSend(data);
    }

    existData(name: string, cmpId: string, controlId: string, currentValue: any, currentType: string): Observable<{existData: boolean, value: string}> {
        let requestId = Math.floor(Math.random() * 1000000).toString();
        const data = { cmd: 'queryHyperLink', cmpId: cmpId, controlId: controlId, name: name, value: currentValue, type: currentType , requestId: requestId};
        this.safeSend(data);
        return this.existDataCompleted.filter(x => x.requestId === requestId).map(x => ({existData: !x.mustExist || (x.mustExist && x.found), value: x.value})).take(1);
    }

    doFillListBox(cmpId: String, obj: any): void {
        const data = { cmd: 'doFillListBox', cmpId: cmpId, itemSource: obj.itemSource, hotLink: obj.hotLink, controlId: obj.cmpId };

        this.safeSend(data);
    }

    doCheckListBoxAction(cmpId: String, obj: any): void {
        const data = { cmd: 'doCheckListBoxAction', cmpId: cmpId, itemSource: obj.itemSource, controlId: obj.cmpId, action: obj.action, list: obj.list, itemID: obj.itemID };

        this.safeSend(data);
    }

    closeServerComponent(cmpId: string) {
        this.doCommand(cmpId, 'ID_FILE_CLOSE', '');
    }

    doCommand(cmpId: String, commandId: String, controlId: string, modelData?: any): void {
        const data = { cmd: 'doCommand', cmpId: cmpId, id: commandId, controlId: controlId, data: modelData };
        this.safeSend(data);
    }
    doControlCommand(cmpId: String, id: String, modelData?: any): void {
        const data = { cmd: 'doControlCommand', cmpId: cmpId, id: id, data: modelData };
        this.safeSend(data);
    }
    doClose(cmpId: String, modelData?: any): void {
        const data = { cmd: 'doClose', cmpId: cmpId, data: modelData };
        this.safeSend(data);
    }
    doValueChanged(cmpId: String, id: String, modelData?: any): void {
        const data = { cmd: 'doValueChanged', cmpId: cmpId, id: id, data: modelData };
        this.safeSend(data);
    }

    browseRecord(cmpId: String, tbGuid: string): void {
        const data = { cmd: 'browseRecord', cmpId: cmpId, tbGuid: tbGuid };
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

    getDocumentData(cmpId: String) {
        const data = { cmd: 'getDocumentData', cmpId: cmpId };
        this.safeSend(data);

    }
    getWindowStrings(cmpId: String, culture: string) {
        const data = { cmd: 'getWindowStrings', cmpId: cmpId, culture: culture };
        this.safeSend(data);

    }
    getActivationData(cmpId: String) {
        const data = { cmd: 'getActivationData', cmpId: cmpId };
        this.safeSend(data);

    }
    activateContainer(cmpId: string, id: string, active: boolean, isTileGroup: boolean/*serve per permettere al server di attribuire la giusta numerazione all'id*/) {
        const data = { cmd: 'activateContainer', cmpId: cmpId, id: id, active: active, isTileGroup: isTileGroup };
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
    doCloseDiagnosticDialog(cmpId: String, result: DiagnosticDlgResult): void {
        const data = { cmd: 'doCloseDiagnosticDialog', cmpId: cmpId, result: result };
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
