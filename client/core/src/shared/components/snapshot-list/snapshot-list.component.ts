import { Component, OnInit, Input } from '@angular/core';
import { RsSnapshotService } from '../../../core/services/rs-snapshot.service';
import { Snapshot } from '../../models/snapshot';


@Component({
    selector: 'rs-snapshot-list',
    styleUrls: ['./snapshot-list.component.scss'],
    templateUrl: './snapshot-list.component.html',
    providers: [RsSnapshotService]
})
export class SnapshotListComponent implements OnInit {
    @Input() namespace;
    snapshots: Snapshot[];

    constructor(public rsSnapshotService: RsSnapshotService) {
        
    }

    ngOnInit() {
        this.rsSnapshotService.getSnapshotData(this.namespace).subscribe(resp => this.createTableSnapshots(resp));
    }

    createTableSnapshots(k: Snapshot[]) {
        this.snapshots = k;
    }

    runSnapshot(name: string, date: string, allusers: boolean) {
        this.rsSnapshotService.runSnapshot(name, date, allusers, this.namespace);
    }

    deleteSnapshot(name: string, date: string, allusers: boolean) {
        this.rsSnapshotService.deleteSnapshotData(this.namespace, date + "_" + name)
          .subscribe(resp => this.createTableSnapshots(resp));
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


}