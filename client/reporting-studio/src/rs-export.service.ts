import { Injectable, EventEmitter, Output } from '@angular/core';

import { drawDOM, exportPDF, DrawOptions, Group, exportImage, exportSVG } from '@progress/kendo-drawing';
import { saveAs } from '@progress/kendo-file-saver';
import { Subscription, Subject, Observable } from './rxjs.imports';

import { PdfType, SvgType, PngType } from './models/export-type.model';
import { ReportingStudioService } from './reporting-studio.service';

@Injectable()
export class RsExportService {
    savingPdf: boolean = false;
    totalPages: number;
    firstPageExport: number;
    lastPageExport: number;
    numberOfCopy: number;
    incrCopy: number;
    multiFile: boolean;
    pdfState: PdfType = PdfType.NOPDF;
    svgState: SvgType = SvgType.NOSVG;
    pngState: PngType = PngType.NOPNG;
    filePdf = new Group();
    titleReport: string;
    layoutId: string;

    currentPDFCopy = 1;

    @Output() eventNextPage = new EventEmitter<void>();
    @Output() eventFirstPage = new EventEmitter<void>();
    @Output() eventCurrentPage = new EventEmitter<void>();
    @Output() rsExportPdf = new EventEmitter<void>();
    @Output() rsExportExcel = new EventEmitter<void>();
    @Output() rsExportDocx = new EventEmitter<void>();
    @Output() eventPageNumber = new EventEmitter<void>();

    exportfile = false;
    exportpdf = false;
    exportexcel = false;
    exportdocx = false;
    pdf: string = "PDF";
    excel: string = "Excel";
    docx: string = "Docx";

    visibleImg: boolean = false;

    constructor(public rsService: ReportingStudioService) { }

    //------EXPORT PDF-----------------------------------
    initializedExport(from: number, to: number, copy: number, multiFile: boolean) {
        this.firstPageExport = from;
        this.lastPageExport = to;
        this.numberOfCopy = copy;
        this.incrCopy = 0;
        this.multiFile = multiFile;
        if (this.exportpdf)
            this.rsExportPdf.emit();
        if (this.exportexcel)
            this.rsExportExcel.emit();
        if (this.exportdocx)
            this.rsExportDocx.emit();
        this.exportpdf = false;
        this.exportexcel = false;
        this.exportdocx = false;
    }

    async appendPDF() {
        await drawDOM(document.getElementById(this.layoutId))
            .then((group: Group) => {
                this.filePdf.append(group);
            })
    }

    async renderPDF() {
        this.incrCopy++;
        if (this.incrCopy < this.numberOfCopy && !this.multiFile) {
            await drawDOM(document.getElementById(this.layoutId))
                .then((group: Group) => {
                    this.filePdf.append(group);
                }).then(() => {
                    this.eventPageNumber.emit();
                    return;
                })
        }
        else {
            await drawDOM(document.getElementById(this.layoutId))
                .then((group: Group) => {
                    this.filePdf.append(group);
                    return exportPDF(this.filePdf, {
                        multiPage: true
                    });
                }).then((dataUri) => {
                    saveAs(dataUri, this.titleReport + '_copy_' + this.incrCopy + '.pdf');

                    /*var doc = document.getElementById('iprint') as HTMLFrameElement;
                    var newWin = window.frames['iprint'];
                    newWin.location = dataUri;

                    doc.onload = function(){
                        

                    }*/
                    
                    //var iframe = document.createElement('iframe');

                    //doc.src = dataUri;

                    /*newWin.addEventListener("loadeddata", function(){
                        console.log("ho caricato il frame");
                        newWin.focus();
                        newWin.print();
                    });*/
                    /*doc.onload = function () {
                        /*var x = window.open();
                        x.document.title = "PRINT";
                    }*/
                    
                    //doc.appendChild(document.body.appendChild(iframe));

                  
                   
                    //iframe.src = dataUri;
                    //document.body.appendChild(iframe);
                    //let iframeWindow = iframe.contentWindow;
                    /*iframeWindow.onload = function () {
                        alert("Local iframe is now loaded.");
                    };*/

                    /*iframe.addEventListener("load", function(){
                        console.log("ho caricato il frame");
                        newWin.focus();
                        newWin.print();
                    });*/

                    
                    

                    this.pdfState = PdfType.NOPDF;
                }).then(() => {
                    this.rsService.reset();
                    this.filePdf = new Group();
                    if (this.incrCopy < this.numberOfCopy && this.multiFile) {
                        this.pdfState = PdfType.PDF;
                        this.eventPageNumber.emit();
                    }
                    else
                        this.eventFirstPage.emit();
                });
        }
    }

    //------EXPORT PNG-----------------------------------
    async exportPNG() {
        await drawDOM(document.getElementById(this.layoutId))
            .then((group: Group) => {
                return exportImage(group);
            })
            .then((dataUri) => {
                saveAs(dataUri, this.titleReport + '.png');
                this.pngState = PngType.NOPNG;
            }).then(() => this.eventCurrentPage.emit());

    }

    //------EXPORT SVG-----------------------------------
    async exportSVG() {
        await drawDOM(document.getElementById(this.layoutId))
            .then((group: Group) => {
                return exportSVG(group);
            })
            .then((dataUri) => {
                saveAs(dataUri, this.titleReport + '.svg');
                this.svgState = SvgType.NOSVG;
            }).then(() => this.eventCurrentPage.emit());
    }

}