import { Injectable } from '@angular/core';
import { Http, Response, Headers } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { CookieService } from 'ngx-cookie';
import { HttpService, InfoService, UtilsService, Logger } from '@taskbuilder/core';

@Injectable()
export class ErpHttpService {

    constructor(
        public http: Http,
        public httpService: HttpService,
        public utils: UtilsService,
        public logger: Logger,
        public cookieService: CookieService,
        public infoService: InfoService) {
    }

    postToDocumentBaseUrl(api: string, obj: any): Observable<Response> {
        let params = { authtoken: this.cookieService.get('authtoken') };
        // let url = this.infoService.getDocumentBaseUrl() + api;
        let url = 'http://localhost:5000/' + api;
        let headers = new Headers({ 'Content-Type': 'application/json' });
        return this.http.post(url, JSON.stringify(obj), { withCredentials: true, headers: headers});
    }

    isVatDuplicate(vat: string): Observable<Response> {
        return this.postToDocumentBaseUrl('erp-core/CheckVatDuplicate', vat);
    }
}
