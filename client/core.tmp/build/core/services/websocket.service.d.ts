import { EventEmitter } from '@angular/core';
import 'rxjs/add/operator/toPromise';
import { MessageDlgArgs, MessageDlgResult } from './../containers/message-dialog/message-dialog.component';
import { CookieService } from 'angular2-cookie/services/cookies.service';
import { HttpService } from './http.service';
import { UrlService } from './url.service';
import { Logger } from './logger.service';
export declare class WebSocketService {
    private httpService;
    private urlService;
    private cookieService;
    private logger;
    status: string;
    private connection;
    error: EventEmitter<any>;
    modelData: EventEmitter<any>;
    serverCommands: EventEmitter<any>;
    windowOpen: EventEmitter<any>;
    windowClose: EventEmitter<any>;
    activationData: EventEmitter<any>;
    itemSource: EventEmitter<any>;
    open: EventEmitter<any>;
    close: EventEmitter<any>;
    message: EventEmitter<MessageDlgArgs>;
    buttonsState: EventEmitter<any>;
    constructor(httpService: HttpService, urlService: UrlService, cookieService: CookieService, logger: Logger);
    wsConnect(): void;
    wsClose(): void;
    doFillListBox(cmpId: String, obj: any): void;
    doCommand(cmpId: String, id: String, modelData?: any): void;
    doValueChanged(cmpId: String, id: String, modelData?: any): void;
    getDocumentData(cmpId: String, modelStructure: any): void;
    checkMessageDialog(cmpId: String): void;
    doCloseMessageDialog(cmpId: String, result: MessageDlgResult): void;
    setReportResult(cmpId: String, result: any): void;
}
export declare class SocketMessage {
    name: string;
    content: any;
    constructor(name: string, content: any);
}
