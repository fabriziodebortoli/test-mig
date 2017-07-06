import { ComponentService } from '@taskbuilder/core';
import { Logger } from '@taskbuilder/core';
import { Injectable, EventEmitter, Output } from '@angular/core';
import { Subject } from 'rxjs/Subject';

import { EventDataService } from '@taskbuilder/core';
import { DocumentService } from '@taskbuilder/core';
import { CommandType, PdfType } from './reporting-studio.model';

import { drawDOM, exportPDF, DrawOptions, Group } from '@progress/kendo-drawing';
import { saveAs } from '@progress/kendo-file-saver';
import { Subscription } from "rxjs/Subscription";


@Injectable()
export class ReportingStudioService extends DocumentService {
    public pageNum: number = 1;
    public running: boolean = false;
    public showAsk = false;
    private rsServer: string = ''
    websocket: WebSocket;
    public message: Subject<any> = new Subject<string>();

    @Output() eventNextPage = new EventEmitter<void>();
    @Output() eventFirstPage = new EventEmitter<void>();

    public savingPdf: boolean = false;
    public totalPages: number;
    public pdfState: PdfType = PdfType.NOPDF;
    public filePdf = new Group();
    public titleReport: string;

    constructor(
        logger: Logger,
        eventData: EventDataService,
        private cmpService: ComponentService,
        private urlServ: UrlService) {
        super(logger, eventData);

        this.websocket = new WebSocket(this.rsServer);
        this.websocket.onopen = evt => { this.onOpen(evt) };
        this.websocket.onclose = evt => { this.onClose(evt) };
        this.websocket.onmessage = evt => { this.onMessage(evt) };
        this.websocket.onerror = evt => { this.onError(evt) };
        this.rsServer=this.urlServ.getWsUrl()+ 'rs';
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
        this.cmpService.removeComponentById(this.mainCmpId);
        this.closeConnection();

    }

    reset() {
        this.pageNum = 1;
        this.showAsk = false;
    }

    public async appendPDF() {
        await drawDOM(document.getElementById('rsLayout'))
            .then((group: Group) => {
                this.filePdf.append(group);
            })
    }

    public renderPDF() {
        drawDOM(document.getElementById('rsLayout'))
            .then((group: Group) => {
                this.filePdf.append(group);
                return exportPDF(this.filePdf, {
                    multiPage: true
                });
            })
            .then((dataUri) => {
                saveAs(dataUri, this.titleReport + '.pdf');
                this.pdfState = PdfType.NOPDF;
            }).then(()=> this.eventFirstPage.emit());

    }


}
