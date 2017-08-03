import { Subscription } from 'rxjs/Subscription';
import { ReportingStudioService } from './../../reporting-studio.service';
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
    constructor(private rsService: ReportingStudioService) {
        this.from = 1;
        this.to = this.rsService.totalPages;
    };

    ngOnDestroy() {
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }

    close() {
        this.rsService.exportfile = false;
        this.rsService.exportexcel = false;
        this.rsService.exportpdf = false;
        this.ngOnDestroy();
    }

    startExport() {
        this.rsService.initiaziedExport(this.from, this.to);
        this.rsService.exportfile = false;
    }

    setAllPages(){
        this.from = 1;
        this.to = this.rsService.totalPages;
        this.inputDisable = true;
    }

    setRangePages(){
        this.inputDisable = false;
    }

}

