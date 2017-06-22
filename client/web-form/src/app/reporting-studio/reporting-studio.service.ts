import { ComponentService } from './../core/component.service';
import { Logger } from './../core/logger.service';
import { environment } from './../../environments/environment';
import { Injectable, EventEmitter, Output } from '@angular/core';
import { Subject } from 'rxjs/Subject';

import { EventDataService } from './../core/eventdata.service';
import { DocumentService } from './../core/document.service';
import { CommandType, PdfType } from './reporting-studio.model';

import { drawDOM, exportPDF, DrawOptions, Group } from '@progress/kendo-drawing';
import { saveAs } from '@progress/kendo-file-saver';


@Injectable()
export class ReportingStudioService extends DocumentService {
    public pageNum: number = 1;
    public running: boolean = false;
    public showAsk = false;
    private rsServer: string = environment.baseSocket + 'rs';
    websocket: WebSocket;
    public message: Subject<any> = new Subject<string>();

    @Output() eventDownload = new EventEmitter<void>();

    public savingPdf: boolean = false;
    public totalPages: number;
    public pdfState: PdfType = PdfType.NOPDF;
    public filePdf = new Group();
    public titleReport: string;

    constructor(
        logger: Logger,
        eventData: EventDataService,
        private cmpService: ComponentService) {
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
        this.cmpService.removeComponentById(this.mainCmpId);
        this.closeConnection();

    }

    reset() {
        this.pageNum = 1;
        this.showAsk = false;
    }

    loopPdfPage(title: string) {
        this.titleReport = title;
        if (this.pdfState === PdfType.SAVINGPDF) {
            if (this.pageNum === this.totalPages) {
                this.renderPDF();
                this.pdfState = PdfType.NOPDF;
            }
            else {
                this.eventDownload.emit();
            }
        }
    }

    public appendPDF() {
        drawDOM(document.getElementById('rsLayout'))
            .then((group: Group) => {
                this.filePdf.append(group);
                this.loopPdfPage(this.titleReport);
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
            });
        this.eventDownload.emit();
    }


}
