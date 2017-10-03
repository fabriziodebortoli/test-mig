import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { ErrorObservable } from 'rxjs/observable/ErrorObservable';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { HttpMenuService } from './../../menu/services/http-menu.service';
import { HttpService } from './http.service';
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
    cultureId = '_culture';

    constructor(
        public http: Http,
        public cookieService: CookieService,
        public logger: Logger
    ) {
        this.culture.value = cookieService.get(this.cultureId);
    }

    saveCulture() {
        this.cookieService.put(this.cultureId, this.culture.value);
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
                let params = { authtoken: this.cookieService.get('authtoken') }

                let sub = this.http.get(this.getDocumentBaseUrl() + 'getProductInfo/', params)
                    .map(res => res.json())
                    .subscribe(result => {
                        this.logger.debug("productInfo", result);
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
                let sub = this.http.get(this.getDataServiceUrl() + 'getinstalleddictionaries', {})
                    .map(res => res.json())
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
       return this.getBaseUrl() +  '/erp-core/';
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
}