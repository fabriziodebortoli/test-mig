import { RsExportService } from './../../rs-export.service';
import { Subscription } from 'rxjs/Subscription';
import { Component, Input } from '@angular/core';
import { Snapshot } from './snapshot';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';

@Component({
    selector: 'rs-snapshotdialog',
    templateUrl: './snapshotdialog.component.html',
    styleUrls: ['./snapshotdialog.component.scss'],
})

export class SnapshotdialogComponent {
    subscriptions: Subscription[] = [];
    public allUsers : boolean = false;
    public nameSnapshot: string;
    public openSnapshot: string;

    public multiple: boolean = false;
    public allowUnsort: boolean = true;
    
    public sort: SortDescriptor[] = [];
    public gridView: GridDataResult;
    
    

    constructor(public rsExportService: RsExportService) {
      this.nameSnapshot = "";
      this.openSnapshot= "";
      this.loadSnapshots();
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

    protected sortChange(sort: SortDescriptor[]): void {
        this.sort = sort;
        this.loadSnapshots();
    }
    
    private loadSnapshots(): void {
        this.gridView = {
            data: orderBy(this.rsExportService.snapshots, this.sort),
            total: this.rsExportService.snapshots
        };
    }
}
