import { RsExportService } from './../../rs-export.service';
import { Subscription } from 'rxjs/Subscription';
import { Component, Output, EventEmitter } from '@angular/core';
import { formatNumber } from '@telerik/kendo-intl';

@Component({
    selector: 'rs-exportdialog',
    templateUrl: './exportdialog.component.html',
    styleUrls: ['./exportdialog.component.scss'],
})

export class ExportdialogComponent {
    subscriptions: Subscription[] = [];
    private from: number;
    private to: number;
    private inputDisable: boolean = true;
    constructor(private rsExportService: RsExportService) {
        this.from = 1;
        this.to = this.rsExportService.totalPages;
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
        this.rsExportService.initiaziedExport(this.from, this.to);
        this.rsExportService.exportfile = false;
    }

    setAllPages(){
        this.from = 1;
        this.to = this.rsExportService.totalPages;
        this.inputDisable = true;
    }

    setRangePages(){
        this.inputDisable = false;
    }

}

