import { Injectable, EventEmitter, Output } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { InfoService } from './info.service';
import { ComponentService } from './component.service';

@Injectable()
export class RsSnapshotService {
    @Output() eventSnapshot = new EventEmitter<void>();
    @Output() deleteSnapshot = new EventEmitter<void>();

   
    constructor(
        public httpClient: HttpClient,
        public infoService: InfoService,
        public componentService: ComponentService) { }

    runSnapshot(nameSnap: string, dateSnap: string, allusers: boolean, nameSpace: string) {
        let outerSnapshot: any;
        outerSnapshot = {};
        outerSnapshot.snapshot = {
          name: nameSnap,
          date: dateSnap,
          allUsers: allusers
        };
        this.componentService.createReportComponent(nameSpace, true, outerSnapshot);
    }

    getSnapshotData(namespace: string): Observable<any> {
        return this.httpClient.get(this.infoService.getReportServiceUrl() + 'snapshot/list/' + namespace);
    }

    deleteSnapshotData(namespace: string, name: string): Observable<any> {
        return this.httpClient.get(this.infoService.getReportServiceUrl() + 'snapshot/delete/' + namespace +'/' + name);
    }

}