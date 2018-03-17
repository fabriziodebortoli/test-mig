import { RsSnapshotService } from '@taskbuilder/core';
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
    listSnap: boolean = true;

    constructor(public rsService: ReportingStudioService, public rsSnapshotService: RsSnapshotService) {
        this.nameSnapshot = "";
        this.openSnapshot = "";
    };

    ngOnDestroy() {
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }

    close() {
        this.rsSnapshotService.showSnapshotDialog = false;
        this.ngOnDestroy();
    }

    saveSnapshot() {
        this.rsService.initiaziedSnapshot(this.nameSnapshot, this.allUsers);
        this.rsSnapshotService.showSnapshotDialog = false;
    }

    setSingleUser() {
        this.allUsers = false;
    }

    setAllusers() {
        this.allUsers = true;
    }

    onShowList(isNotEmpty: boolean){
        this.listSnap = isNotEmpty;
    }
    
    openCollapse() {
      this.opened = !this.opened;
    }
}
