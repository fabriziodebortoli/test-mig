import { ReportingStudioService } from './reporting-studio.service';
import { Injectable, EventEmitter, Output } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import { PdfType, SvgType, PngType } from './models';

import { drawDOM, exportPDF, DrawOptions, Group, exportImage, exportSVG } from '@progress/kendo-drawing';
import { saveAs } from '@progress/kendo-file-saver';
import { Subscription } from "rxjs/Subscription";
import { Observable } from 'rxjs/Rx';

import { Snapshot } from './report-objects/snapshotdialog/snapshot';



@Injectable()
export class RsExportService {
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
    public nameSnap: string;
    public snapshots: Snapshot[];
    public dateSnap: string;

    @Output() eventNextPage = new EventEmitter<void>();
    @Output() eventFirstPage = new EventEmitter<void>();
    @Output() eventCurrentPage = new EventEmitter<void>();
    @Output() eventSnapshot = new EventEmitter<void>();
    @Output() runSnapshot = new EventEmitter<void>();

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

    constructor(private rsService: ReportingStudioService){}
        



    //------SNAPSHOT------------------------------------
    public initiaziedSnapshot(nameSnapshot, allUsers) {
        this.nameSnap = nameSnapshot;
        this.user = allUsers;
        this.eventSnapshot.emit();
    }

    public startRunSnapshot(name, date, allusers) {
        this.nameSnap = name;
        this.dateSnap = date;
        this.user = allusers;
        this.runSnapshot.emit();
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
        this.exportpdf = false;
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
                this.rsService.reset();
            });
    }

     //------EXPORT PNG-----------------------------------
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

     //------EXPORT SVG-----------------------------------
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