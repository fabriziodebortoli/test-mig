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
    private allUsers = false;


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
        this.rsService.initiaziedSnapshot();
        this.rsService.snapshot = false;
    }

    setSingleUser(){
        this.allUsers = false;
    }

    setAllusers(){
        this.allUsers = true;
    }
}
