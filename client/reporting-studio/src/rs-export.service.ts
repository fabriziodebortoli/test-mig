import { Injectable, EventEmitter, Output } from '@angular/core';

import { drawDOM, exportPDF, DrawOptions, Group, exportImage, exportSVG } from '@progress/kendo-drawing';
import { saveAs } from '@progress/kendo-file-saver';
import { Subscription, Subject, Observable } from './rxjs.imports';

import { PdfType, SvgType, PngType } from './models/export-type.model';
import { ReportingStudioService } from './reporting-studio.service';
import { Snapshot } from './report-objects/snapshotdialog/snapshot';

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

    user: boolean;
    nameSnap: string;
    snapshots: Snapshot[];
    dateSnap: string;

    @Output() eventNextPage = new EventEmitter<void>();
    @Output() eventFirstPage = new EventEmitter<void>();
    @Output() eventCurrentPage = new EventEmitter<void>();
    @Output() eventSnapshot = new EventEmitter<void>();
    @Output() runSnapshot = new EventEmitter<void>();

    @Output() rsExportPdf = new EventEmitter<void>();
    @Output() rsExportExcel = new EventEmitter<void>();
    @Output() rsExportDocx = new EventEmitter<void>();
    @Output() eventPageNumber = new EventEmitter<void>();

    exportfile = false;
    exportpdf = false;
    exportexcel = false;
    exportdocx = false;
    snapshot = false;
    pdf: string = "PDF";
    excel: string = "Excel";
    docx: string = "Docx";

    visibleImg: boolean = false;

    constructor(public rsService: ReportingStudioService) { }

    //------SNAPSHOT------------------------------------
    initiaziedSnapshot(nameSnapshot, allUsers) {
        this.nameSnap = nameSnapshot;
        this.user = allUsers;
        this.eventSnapshot.emit();
    }

    startRunSnapshot(name, date, allusers) {
        this.nameSnap = name;
        this.dateSnap = date;
        this.user = allusers;
        this.runSnapshot.emit();
    }

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