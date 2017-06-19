import { EventEmitter, Injectable } from '@angular/core';
import 'rxjs/add/operator/toPromise';
import { CookieService } from 'angular2-cookie/services/cookies.service';
import { HttpService } from './http.service';
import { UrlService } from './url.service';
// import { CommandService } from './command.service';
import { Logger } from './logger.service';
export class WebSocketService {
    /**
     * @param {?} httpService
     * @param {?} urlService
     * @param {?} cookieService
     * @param {?} logger
     */
    constructor(httpService, urlService, cookieService, logger) {
        this.httpService = httpService;
        this.urlService = urlService;
        this.cookieService = cookieService;
        this.logger = logger;
        this.status = 'Undefined';
        this.error = new EventEmitter();
        this.modelData = new EventEmitter();
        this.serverCommands = new EventEmitter();
        this.windowOpen = new EventEmitter();
        this.windowClose = new EventEmitter();
        this.activationData = new EventEmitter();
        this.itemSource = new EventEmitter();
        this.open = new EventEmitter();
        this.close = new EventEmitter();
        this.message = new EventEmitter();
        this.buttonsState = new EventEmitter();
    }
    /**
     * @return {?}
     */
    wsConnect() {
        const /** @type {?} */ $this = this;
        const /** @type {?} */ url = this.urlService.getWsUrl();
        this.logger.debug('wsConnecting... ' + url);
        this.connection = new WebSocket(url);
        this.connection.onmessage = function (e) {
            if (typeof (e.data) === 'string') {
                try {
                    const /** @type {?} */ obj = JSON.parse(e.data);
                    switch (obj.cmd) {
                        case 'ModelData':
                            $this.modelData.emit(obj.args);
                            break;
                        case 'WindowOpen':
                            $this.windowOpen.emit(obj.args);
                            break;
                        case 'ActivationData':
                            $this.activationData.emit(obj.args);
                            break;
                        case 'WindowClose':
                            $this.windowClose.emit(obj.args);
                            break;
                        case 'ItemSource':
                            $this.itemSource.emit(obj.args);
                            break;
                        case 'ServerCommands':
                            $this.serverCommands.emit(obj.args);
                            break;
                        // when tbloader has connected to gate, I receive this message; then I can
                        // request the list of opened windows
                        case 'MessageDialog':
                            $this.message.emit(obj.args);
                            break;
                        case 'SetServerWebSocketName':
                            $this.connection.send(JSON.stringify({ cmd: 'getOpenDocuments' }));
                            break;
                        case 'ButtonsState':
                            $this.buttonsState.emit(obj.args);
                            break;
                        default: break;
                    }
                }
                catch (e) {
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
                args: {
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
    /**
     * @return {?}
     */
    wsClose() {
        if (this.connection) {
            this.connection.close();
        }
    }
    /**
     * @param {?} cmpId
     * @param {?} obj
     * @return {?}
     */
    doFillListBox(cmpId, obj) {
        const /** @type {?} */ data = { cmd: 'doFillListBox', cmpId: cmpId, itemSource: obj.itemSource, hotLink: obj.hotLink };
        this.connection.send(JSON.stringify(data));
    }
    /**
     * @param {?} cmpId
     * @param {?} id
     * @param {?=} modelData
     * @return {?}
     */
    doCommand(cmpId, id, modelData) {
        const /** @type {?} */ data = { cmd: 'doCommand', cmpId: cmpId, id: id, model: modelData };
        this.connection.send(JSON.stringify(data));
    }
    /**
     * @param {?} cmpId
     * @param {?} id
     * @param {?=} modelData
     * @return {?}
     */
    doValueChanged(cmpId, id, modelData) {
        const /** @type {?} */ data = { cmd: 'doValueChanged', cmpId: cmpId, id: id, model: modelData };
        this.connection.send(JSON.stringify(data));
    }
    /**
     * @param {?} cmpId
     * @param {?} modelStructure
     * @return {?}
     */
    getDocumentData(cmpId, modelStructure) {
        const /** @type {?} */ data = { cmd: 'getDocumentData', cmpId: cmpId, modelStructure: modelStructure };
        this.connection.send(JSON.stringify(data));
    }
    /**
     * @param {?} cmpId
     * @return {?}
     */
    checkMessageDialog(cmpId) {
        const /** @type {?} */ data = { cmd: 'checkMessageDialog', cmpId: cmpId };
        this.connection.send(JSON.stringify(data));
    }
    /**
     * @param {?} cmpId
     * @param {?} result
     * @return {?}
     */
    doCloseMessageDialog(cmpId, result) {
        const /** @type {?} */ data = { cmd: 'doCloseMessageDialog', cmpId: cmpId, result: result };
        this.connection.send(JSON.stringify(data));
    }
    /**
     * @param {?} cmpId
     * @param {?} result
     * @return {?}
     */
    setReportResult(cmpId, result) {
        const /** @type {?} */ data = { cmd: 'setReportResult', cmpId: cmpId, result: result };
        this.connection.send(JSON.stringify(data));
    }
}
WebSocketService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
WebSocketService.ctorParameters = () => [
    { type: HttpService, },
    { type: UrlService, },
    { type: CookieService, },
    { type: Logger, },
];
function WebSocketService_tsickle_Closure_declarations() {
    /** @type {?} */
    WebSocketService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    WebSocketService.ctorParameters;
    /** @type {?} */
    WebSocketService.prototype.status;
    /** @type {?} */
    WebSocketService.prototype.connection;
    /** @type {?} */
    WebSocketService.prototype.error;
    /** @type {?} */
    WebSocketService.prototype.modelData;
    /** @type {?} */
    WebSocketService.prototype.serverCommands;
    /** @type {?} */
    WebSocketService.prototype.windowOpen;
    /** @type {?} */
    WebSocketService.prototype.windowClose;
    /** @type {?} */
    WebSocketService.prototype.activationData;
    /** @type {?} */
    WebSocketService.prototype.itemSource;
    /** @type {?} */
    WebSocketService.prototype.open;
    /** @type {?} */
    WebSocketService.prototype.close;
    /** @type {?} */
    WebSocketService.prototype.message;
    /** @type {?} */
    WebSocketService.prototype.buttonsState;
    /** @type {?} */
    WebSocketService.prototype.httpService;
    /** @type {?} */
    WebSocketService.prototype.urlService;
    /** @type {?} */
    WebSocketService.prototype.cookieService;
    /** @type {?} */
    WebSocketService.prototype.logger;
}
export class SocketMessage {
    /**
     * @param {?} name
     * @param {?} content
     */
    constructor(name, content) {
        this.name = name;
        this.content = content;
    }
}
function SocketMessage_tsickle_Closure_declarations() {
    /** @type {?} */
    SocketMessage.prototype.name;
    /** @type {?} */
    SocketMessage.prototype.content;
}
