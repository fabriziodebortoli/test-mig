import { Injectable } from '@angular/core';
import { Http, Response, Headers } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { InfoService, UtilsService, Logger } from '@taskbuilder/core';

@Injectable()
export class ErpHttpService {

    constructor(
        public http: Http,
        public utils: UtilsService,
        public logger: Logger,
        public infoService: InfoService) {
    }

    postData(api: string, obj: any = null): Observable<Response> {
        const url = this.infoService.getBaseUrl() + api;
        const headers = new Headers({ 'Content-Type': 'application/json' });
        //headers.append('Authorization', this.infoService.getAuthorization());
        return this.http.post(url, JSON.stringify(obj), { withCredentials: true, headers });
    }

    // getData(api: string, parameters: any): Observable<Response> {
    //     const url = this.infoService.getBaseUrl() + api;
    //     const headers = new Headers({
    //         'Content-Type': 'application/json',
    //         'Authorization': this.infoService.getAuthorization()
    //     });
    //     return this.http.get(url + '/' + parameters, { withCredentials: true, headers: headers });
    // }

    isVatDuplicate(vat: string): Observable<Response> {
        return this.postData('/erp-core/CheckVatDuplicate', vat);
    }

    checkBinUsesStructure(zone: string, storage: string): Observable<Response> {
        return this.postData('/erp-core/CheckBinUsesStructure', { 'zone': zone, 'storage': storage });
    }

    checkItemsAutoNumbering(): Observable<Response> {
        return this.postData('/erp-core/CheckItemsAutoNumbering');
    }

    getItemsSearchList(queryType: string): Observable<Response> {
        return this.postData('/erp-core/GetItemsSearchList', queryType);
    }

}
