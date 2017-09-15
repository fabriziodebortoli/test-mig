import { Http } from '@angular/http';
import { UrlService, HttpService } from '@taskbuilder/core';
import { ComponentService } from '@taskbuilder/core';
import { Logger } from '@taskbuilder/core';
import { Injectable, EventEmitter, Output } from '@angular/core';
import { Subject } from 'rxjs/Subject';

import { EventDataService, InfoService, DocumentService } from '@taskbuilder/core';
import { CommandType, PdfType, SvgType, PngType } from './models';

import { drawDOM, exportPDF, DrawOptions, Group, exportImage, exportSVG } from '@progress/kendo-drawing';
import { saveAs } from '@progress/kendo-file-saver';
import { Subscription } from "rxjs/Subscription";
import { Observable } from 'rxjs/Rx';



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

    @Output() eventNextPage = new EventEmitter<void>();
    @Output() eventFirstPage = new EventEmitter<void>();
    @Output() eventCurrentPage = new EventEmitter<void>();
    @Output() eventSnapshot = new EventEmitter<void>();

    public savingPdf: boolean = false;
    public totalPages: number;
    public firstPageExport: number;
    public lastPageExport: number;
    public pdfState: PdfType = PdfType.NOPDF;
    public svgState: SvgType = SvgType.NOSVG;
    public pngState: PngType = PngType.NOPNG;
    public filePdf = new Group();
    public titleReport: string;

    public user: boolean;


    @Output() rsExportPdf = new EventEmitter<void>();
    @Output() rsExportExcel = new EventEmitter<void>();
    @Output() rsExportDocx = new EventEmitter<void>();

    public exportfile = false;
    public exportpdf = false;
    public exportexcel = false;
    public exportdocx = false;
    public snapshot = false;
    public pdf: string = "PDF";
    public excel: string = "Excel";
    public docx: string = "Docx";

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

    //------SNAPSHOT------------------------------------
    public initiaziedSnapshot() {
        this.eventSnapshot.emit();
        //this.user = allUsers;
    }

    //------EXPORT PDF-----------------------------------
    public initiaziedExport(from: number, to: number) {
        this.firstPageExport = from;
        this.lastPageExport = to;
        if (this.exportpdf)
            this.rsExportPdf.emit();
        if (this.exportexcel)
            this.rsExportExcel.emit();
        if (this.exportdocx)
            this.rsExportDocx.emit();
        this.exportPDF = false;
        this.exportexcel = false;
        this.exportdocx = false;
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
            }).then(() => {
                this.eventFirstPage.emit();
                this.reset();
            });
    }

    //--------------------------------------------------
    public async exportPNG() {
        await drawDOM(document.getElementById('rsLayout'))
            .then((group: Group) => {
                return exportImage(group);
            })
            .then((dataUri) => {
                saveAs(dataUri, this.titleReport + '.png');
                this.pngState = PngType.NOPNG;
            }).then(() => this.eventCurrentPage.emit());

    }

    //--------------------------------------------------
    public async exportSVG() {
        await drawDOM(document.getElementById('rsLayout'))
            .then((group: Group) => {
                return exportSVG(group);
            })
            .then((dataUri) => {
                saveAs(dataUri, this.titleReport + '.svg');
                this.svgState = SvgType.NOSVG;
            }).then(() => this.eventCurrentPage.emit());
    }
}

