import { ReportingStudioService } from '../../reporting-studio.service';
import { Subscription } from '../../rxjs.imports';
import { Component, Input } from '@angular/core';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';

@Component({
    selector: 'rs-snapshotdialog',
    templateUrl: './snapshotdialog.component.html',
    styleUrls: ['./snapshotdialog.component.scss'],
})

export class SnapshotdialogComponent {
    subscriptions: Subscription[] = [];
    allUsers: boolean = false;
    nameSnapshot: string = "";
    openSnapshot: string = "";
    opened: boolean = false;

    constructor(public rsService: ReportingStudioService) {
        this.nameSnapshot = "";
        this.openSnapshot = "";
    };

    ngOnDestroy() {
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }

    close() {
        this.rsService.showSnapshotDialog = false;
        this.ngOnDestroy();
    }

    saveSnapshot() {
        this.rsService.initiaziedSnapshot(this.nameSnapshot, this.allUsers);
        this.rsService.showSnapshotDialog = false;
    }

    setSingleUser() {
        this.allUsers = false;
    }

    setAllusers() {
        this.allUsers = true;
    }
    
    openCollapse() {
      this.opened = !this.opened;
    }
}
