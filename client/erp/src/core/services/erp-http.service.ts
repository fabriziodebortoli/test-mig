﻿import { Injectable } from '@angular/core';
import { Http, Response, Headers } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { HttpService, InfoService, UtilsService, Logger } from '@taskbuilder/core';

@Injectable()
export class ErpHttpService {

    constructor(
        public http: Http,
        public httpService: HttpService,
        public utils: UtilsService,
        public logger: Logger,
        public infoService: InfoService) {
    }

    postToDocumentBaseUrl(api: string, obj: any): Observable<Response> {
        const params = { authtoken: localStorage.getItem('authtoken') };
        const url = this.infoService.getBaseUrl() + api;
        const headers = new Headers({ 'Content-Type': 'application/json' });
        return this.http.post(url, JSON.stringify(obj), { withCredentials: true, headers: headers });
    }

    isVatDuplicate(vat: string): Observable<Response> {
        return this.postToDocumentBaseUrl('/erp-core/CheckVatDuplicate', vat);
    }

    checkBinUsesStructure(zone: string, storage: string): Observable<Response> {
        return this.postToDocumentBaseUrl('/erp-core/CheckBinUsesStructure', { "zone": zone, "storage": storage });
    }
}
