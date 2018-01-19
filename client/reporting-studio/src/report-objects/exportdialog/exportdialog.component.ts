import { check } from './../../models/check.model';
import { RsExportService } from './../../rs-export.service';
import { Subscription } from '../../rxjs.imports';
import { Component, Output, EventEmitter, Input, OnChanges, OnDestroy } from '@angular/core';
import { formatNumber } from '@telerik/kendo-intl';

@Component({
    selector: 'rs-exportdialog',
    templateUrl: './exportdialog.component.html',
    styleUrls: ['./exportdialog.component.scss'],
})

export class ExportdialogComponent implements OnDestroy {
    subscriptions: Subscription[] = [];
    from: number;
    to: number;
    copy: number;

    inputDisable: boolean = true;
    multicopyDisable: boolean = true;

    check: boolean = true;
    multicopy: boolean = false;

    constructor(public rsExportService: RsExportService) {
        this.from = 1;
        this.to = this.rsExportService.totalPages;
        this.copy = 1;
    };

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
        this.rsExportService.initializedExport(this.from, this.to, this.copy, this.multicopy);
        this.rsExportService.exportfile = false;
    }

    setMulticopy() {
        this.multicopy = !this.multicopy;
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
            this.multicopyDisable = false;
        else if(this.copy = 1)
            this.multicopyDisable = true;

    }

}

