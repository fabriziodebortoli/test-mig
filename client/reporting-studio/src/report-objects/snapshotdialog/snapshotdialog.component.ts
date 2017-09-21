import { Subscription } from 'rxjs/Subscription';
import { Component } from '@angular/core';
import { ReportingStudioService } from './../../reporting-studio.service';
import { Snapshot } from './snapshot';

@Component({
    selector: 'rs-snapshotdialog',
    templateUrl: './snapshotdialog.component.html',
    styleUrls: ['./snapshotdialog.component.scss'],
})

export class SnapshotdialogComponent {
    subscriptions: Subscription[] = [];
    private allUsers : boolean = false;
    private nameSnapshot: string;


    constructor(private rsService: ReportingStudioService) {
      this.nameSnapshot = "";  
    };

    ngOnDestroy() {
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }

    close() {
        this.rsService.snapshot = false;
        this.ngOnDestroy();
    }

    createFileJson(){
        console.log("createFileJson");
        this.rsService.initiaziedSnapshot(this.nameSnapshot, this.allUsers);
        this.rsService.snapshot = false;
    }

    setSingleUser(){
        this.allUsers = false;
    }

    setAllusers(){
        this.allUsers = true;
    }


}
