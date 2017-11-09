import { RsExportService } from './../../rs-export.service';
import { Subscription } from '../../rxjs.imports';
import { Component, Output, EventEmitter } from '@angular/core';
import { formatNumber } from '@telerik/kendo-intl';

@Component({
    selector: 'rs-exportdialog',
    templateUrl: './exportdialog.component.html',
    styleUrls: ['./exportdialog.component.scss'],
})

export class ExportdialogComponent {
    subscriptions: Subscription[] = [];
    from: number;
    to: number;
    copy: number;

    multicopy: boolean = true;
    inputDisable: boolean = true;

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
        this.rsExportService.initializedExport(this.from, this.to, this.copy);
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

}

