import { Subscription } from 'rxjs/Subscription';
import { ReportingStudioService } from './../../reporting-studio.service';
import { Component, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'rs-exportdialog',
    templateUrl: './exportdialog.component.html',
    styleUrls: ['./exportdialog.component.scss'],
})

export class ExportdialogComponent {
    subscriptions: Subscription[] = [];

    constructor(private rsService: ReportingStudioService) { };

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
        var from = (<HTMLInputElement>document.getElementById('fromPage')).value;
        var to = (<HTMLInputElement>document.getElementById('toPage')).value;
        this.rsService.initiaziedExport(from, to);
        
        this.rsService.exportfile = false;
        // this.rsService.exportpdf = false;
        // this.rsService.exportexcel = false;
    }

}

