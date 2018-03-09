import { Snapshot } from './../report-objects/snapshotdialog/snapshot';
import { Observable } from 'rxjs/Observable';
import { Injectable } from '@angular/core';
import { InfoService } from '@taskbuilder/core';
import { HttpClient, HttpResponse } from '@angular/common/http';

@Injectable()
export class HttpServiceRs {
    constructor(
        public httpClient: HttpClient,
        public infoService: InfoService) {
    }

    getSnapshotData(namespace: string): Observable<any> {
        return this.httpClient.get(this.infoService.getReportServiceUrl() + 'snapshot/list/' + namespace);
    }
}