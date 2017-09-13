import { Subscription } from 'rxjs/Subscription';
import { Component } from '@angular/core';
import { ReportingStudioService } from './../../reporting-studio.service';

@Component({
    selector: 'rs-snapshotdialog',
    templateUrl: './snapshotdialog.component.html',
    styleUrls: ['./snapshotdialog.component.scss'],
})

export class SnapshotdialogComponent {
    subscriptions: Subscription[] = [];


    constructor(private rsService: ReportingStudioService) {
        
    };

    ngOnDestroy() {
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }

    close() {
        this.rsService.snapshot = false;
        this.ngOnDestroy();
    }

    createFileJson(){
        //this.rsService.snapshotEv();
        //this.rsService.snapshot = false;
    }
}
