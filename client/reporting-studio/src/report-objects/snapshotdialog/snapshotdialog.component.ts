import { RsExportService } from './../../rs-export.service';
import { Subscription } from '../../rxjs.imports';
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
    allUsers: boolean = false;
    nameSnapshot: string;
    openSnapshot: string;
    opened: boolean = false;

    constructor(public rsExportService: RsExportService) {
        this.nameSnapshot = "";
        this.openSnapshot = "";
    };

    ngOnDestroy() {
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }

    close() {
        this.rsExportService.snapshot = false;
        this.ngOnDestroy();
    }

    createFileJson() {
        this.rsExportService.initiaziedSnapshot(this.nameSnapshot, this.allUsers);
        this.rsExportService.snapshot = false;
    }

    setSingleUser() {
        this.allUsers = false;
    }

    setAllusers() {
        this.allUsers = true;
    }

    runSnapshot(name: string, date: string, allusers: boolean) {
        this.rsExportService.startSnapshot(name, date, allusers, true, false);
    }

    deleteSnapshot(name: string, date: string, allusers: boolean) {
        this.rsExportService.startSnapshot(name, date, allusers, false, true);
    }

    sortTable(column: number) {
        var table, rows, switching, i, x, y, shouldSwitch;

        table = document.getElementById("myTable");
        switching = true;
        while (switching) {
            switching = false;
            rows = table.getElementsByTagName("TR");
            for (i = 1; i < (rows.length - 1); i++) {
                shouldSwitch = false;
                x = rows[i].getElementsByTagName("TD")[column];
                y = rows[i + 1].getElementsByTagName("TD")[column];
                if (x.innerHTML.toLowerCase() > y.innerHTML.toLowerCase()) {
                    shouldSwitch = true;
                    break;
                }
            }
            if (shouldSwitch) {
                rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
                switching = true;
            }
        }
    }
    
    openCollapse() {
      this.opened = !this.opened;
    }
}
