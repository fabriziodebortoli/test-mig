import { Injectable } from '@angular/core';
import { Http, Headers } from '@angular/http';
import { Observable, ErrorObservable } from '../../rxjs.imports';

import { CookieService } from 'ngx-cookie';

import { HttpMenuService } from './../../menu/services/http-menu.service';
import { Logger } from './logger.service';

export function loadConfig(config) {
    return () => config.load()
}

@Injectable()
export class InfoService {

    baseUrl: string;
    wsBaseUrl: string;
    isDesktop: boolean = false;

    productInfo: any;
    dictionaries: any;
    culture = { enabled: true, value: '' };
    cultureId = 'ui_culture';

    constructor(
        public http: Http,
        public cookieService: CookieService,
        public logger: Logger
    ) {
        this.culture.value = cookieService.get(this.cultureId);
    }

    resetCulture() {
        this.cookieService.remove(this.cultureId);
        this.culture.value = null;
    }

    saveCulture() {
        this.cookieService.put(this.cultureId, this.culture.value);
    }
    setCulture(culture: string) {
        this.culture.value = culture;
    }

    getAuthorization():string{
        return JSON.stringify({ ui_culture: this.culture.value });
    }
    load() {
        return new Promise((resolve, reject) => {
            this.http.get('assets/config.json')
                .map(res => res.json())
                .subscribe(config => {
                    this.logger.debug("App Configuration", config)

                    this.baseUrl = config.baseUrl;
                    this.wsBaseUrl = config.wsBaseUrl;
                    this.isDesktop = config.isDesktop;

                    resolve(true);
                });
        });
    }

    public getProductInfo(): Observable<any> {
        return Observable.create(observer => {
            if (this.productInfo) {
                observer.next(this.productInfo);
                observer.complete();
            }
            else {

                let params = { authtoken: this.cookieService.get('authtoken') };
                let url = this.getDocumentBaseUrl() + 'getProductInfo/';

                let sub = this.request(url, params)
                    .subscribe(result => {
                        this.productInfo = result.ProductInfos;
                        observer.next(this.productInfo);
                        observer.complete();
                        sub.unsubscribe();
                    });
            }
        });
    }

    public getDictionaries(): Observable<any> {
        return Observable.create(observer => {
            if (this.dictionaries) {
                observer.next(this.dictionaries);
                observer.complete();
            }
            else {
                let url = this.getDataServiceUrl() + 'getinstalleddictionaries';
                let sub = this.request(url, {})
                    .subscribe(result => {
                        this.logger.debug("dictionaries", result);
                        this.dictionaries = result.dictionaries;
                        sub.unsubscribe();
                        observer.next(this.dictionaries);
                        observer.complete();
                    });
            }
        });
    }

    getBaseUrl() {
        return this.baseUrl;
    }

    getApiUrl() {
        return this.getBaseUrl() + '/tbloader/api/';
    }

    getWsBaseUrl() {
        return this.wsBaseUrl;
    }

    getWsUrl() {
        return this.getWsBaseUrl() + '/tbloader';
    }

    getDocumentBaseUrl() {
        let url = this.isDesktop ? 'http://localhost/' : this.getApiUrl();
        return url + 'tb/document/';
    }

    getMenuBaseUrl() {
        let url = this.isDesktop ? 'http://localhost/' : this.getApiUrl();
        return url + 'tb/menu/';
    }

    //TODO da spostare nel httpservice della libreria di controlli
    getErpCoreBaseUrl() {
        let url = this.isDesktop ? 'http://localhost/' : this.getApiUrl();
        return url + 'erp/core/';
    }

    //TODO da spostare nel httpservice della libreria di controlli
    getNetCoreErpCoreBaseUrl() {
        return this.getBaseUrl() + '/erp-core/';
    }

    getAccountManagerBaseUrl() {
        return this.getBaseUrl() + '/account-manager/';
    }

    getMenuServiceUrl() {
        return this.getBaseUrl() + '/menu-service/';
    }

    getEnumsServiceUrl() {
        let url = this.getBaseUrl() + '/enums-service/';
        return url;
    }
    getDataServiceUrl() {
        let url = this.getBaseUrl() + '/data-service/';
        return url;
    }

    getReportServiceUrl() {
        let url = this.getBaseUrl() + '/rs/';
        return url;
    }

    request(url: string, data: Object): Observable<any> {
        let headers = new Headers();
        headers.append('Content-Type', 'application/x-www-form-urlencoded');

        return this.http.post(url, data, { withCredentials: true, headers: headers })
            .map(res => res.json())
            .catch((error: any): ErrorObservable => {
                let errMsg = (error.message) ? error.message :
                    error.status ? `${error.status} - ${error.statusText}` : 'Server error';
                this.logger.error(errMsg);

                return Observable.throw(errMsg);
            });
    }
}