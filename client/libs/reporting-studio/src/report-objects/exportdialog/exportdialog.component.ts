import { AskdialogService } from './../askdialog/askdialog.service';
import { check } from './../../models/check.model';
import { RsExportService } from './../../rs-export.service';
import { Subscription } from '../../rxjs.imports';
import { Component, Output, EventEmitter, Input, OnChanges, OnDestroy } from '@angular/core';
import { formatNumber } from '@telerik/kendo-intl';

@Component({
    selector: 'rs-exportdialog',
    templateUrl: './exportdialog.component.html',
    styleUrls: ['./exportdialog.component.scss'],
    providers: [AskdialogService]
})

export class ExportdialogComponent implements OnDestroy {
    @Input() check: check;
    subscriptions: Subscription[] = [];
    from: number;
    to: number;
    copy: number;
    inputDisable: boolean = true;
    nameFile: string;

    constructor(public rsExportService: RsExportService, public askDialogService: AskdialogService) {
        let jsonObj: any ={};
        this.from = 1;
        this.to = this.rsExportService.totalPages;
        this.copy = 1;
        this.nameFile = this.rsExportService.titleReport;
        jsonObj.field = {}
        jsonObj.field.name = "multicopy";
        jsonObj.field.id = "idMulticopy";
        jsonObj.field.type = "Boolean";
        jsonObj.field.value = false;

        jsonObj.name = "checkExportDialog";
        jsonObj.caption = "Multi file";
        jsonObj.enabled = false;
        jsonObj.value = false;
        jsonObj.runatserver = false;
        this.check = new check(jsonObj);
    }

    ngOnDestroy() {
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }

    close() {
        this.rsExportService.exportfile = false;
        this.rsExportService.exportexcel = false;
        this.rsExportService.exportpdf = false;
        this.rsExportService.exportdocx = false;
        this.ngOnDestroy();
    }

    startExport() {
        this.rsExportService.currentPDFCopy = 1;
        this.rsExportService.initializedExport(this.from, this.to, this.copy, this.check.value, this.nameFile);
        this.rsExportService.exportfile = false;
    }

    setAllPages() {
        this.from = 1;
        this.to = this.rsExportService.totalPages;
        this.inputDisable = true;
    }

    setRangePages() {
        this.inputDisable = false;
    }

    changeMulticopy(){
        if (this.copy > 1)
            this.check.enabled = true;
        else if(this.copy = 1)
            this.check.enabled = false;
    }
}

