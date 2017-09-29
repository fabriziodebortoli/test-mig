import { RsExportService } from './../../rs-export.service';
import { Subscription } from 'rxjs/Subscription';
import { Component } from '@angular/core';
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
    private openSnapshot: string;


    constructor(private rsExportService: RsExportService) {
      this.nameSnapshot = "";
      this.openSnapshot= "";
    };

    ngOnDestroy() {
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }

    close() {
        this.rsExportService.snapshot = false;
        this.ngOnDestroy();
    }

    createFileJson(){
        this.rsExportService.initiaziedSnapshot(this.nameSnapshot, this.allUsers);
        this.rsExportService.snapshot = false;
    }

    setSingleUser(){
        this.allUsers = false;
    }

    setAllusers(){
        this.allUsers = true;
    }

    runSnapshot(name: string, date: string, allusers: boolean){
        this.rsExportService.startRunSnapshot(name, date, allusers);
    }


}
