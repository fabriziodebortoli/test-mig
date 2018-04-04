import { UtilsService } from './utils.service';
import { Injectable } from '@angular/core';
import { Http, Headers } from '@angular/http';
import { Observable, ErrorObservable } from '../../rxjs.imports';
import { Logger } from './logger.service';
import { TBLoaderInfo } from './../../shared/models/tbloader-info.model';
import { addModelBehaviour, createEmptyModel } from './../../shared/models/control.model';

@Injectable()
export class InfoService {

    baseUrl: string;
    wsBaseUrl: string;
    isDesktop = false;

    productInfo: any = null;
    dictionaries: any = null;
    culture = createEmptyModel();
    cultureId = 'ui_culture';
    tbLoaderInfoId = 'tbLoaderInfo';
    applicationDateId = 'application_date';
    tbLoaderInfo: TBLoaderInfo;
    applicationDate = new Date(); //TODO Luca/Silvano il client dovra pilotare data applicazione anche verso il tbloader

    getProductInfoPromise: Promise<void>;

    constructor(
        public http: Http,
        public logger: Logger,
        public utilsService: UtilsService
    ) {
        addModelBehaviour(this.culture, "culture");
        this.culture.value = localStorage.getItem(this.cultureId);
    }

    saveCulture() {
        localStorage.setItem(this.cultureId, this.culture.value);
    }

    setCulture(culture: string) {
        this.culture.value = culture;
    }

    getCulture(): string {
        return this.culture.value;
    }

    setApplicationDate(applicationDate: Date) {
        this.applicationDate = applicationDate;
        localStorage.setItem(this.applicationDateId, JSON.stringify(this.applicationDate));
    }

    getApplicationDate(): Date {
        if (!this.applicationDate) {
            let s = localStorage.getItem(this.applicationDateId);
            if (s) {
                this.applicationDate = JSON.parse(s);
            }
            else {
                this.applicationDate = new Date();
            }
        }

        return this.applicationDate;
    }
    
    getTbLoaderInfo(): TBLoaderInfo {
        if (!this.tbLoaderInfo) {
            let s = localStorage.getItem(this.tbLoaderInfoId);
            if (s) {
                this.tbLoaderInfo = JSON.parse(s);
            }
            else {
                this.tbLoaderInfo = new TBLoaderInfo("", 0);
            }
        }

        return this.tbLoaderInfo;
    }
    
    setTbLoaderInfo(info: TBLoaderInfo) {
        this.tbLoaderInfo = info;
        localStorage.setItem(this.tbLoaderInfoId, JSON.stringify(this.tbLoaderInfo));
    }
    getAuthorization(): string {
        return JSON.stringify(
            {
                ui_culture: this.culture.value,
                authtoken: sessionStorage.getItem('authtoken'),
                tbLoaderName: this.getTbLoaderInfo().name,
                applicationDate: this.applicationDate,
                isDesktop: this.isDesktop
            });
    }

    load(env) {

        this.isDesktop = env.desktop;

        return new Promise((resolve, reject) => {
            this.http.get('assets/config.json')
                .map(res => res.json())
                .subscribe(config => {
                    this.logger.debug('App Configuration', config)

                    this.baseUrl = config.baseUrl;
                    this.wsBaseUrl = config.wsBaseUrl;

                    resolve(true);
                });
        });
    }

    public getProductInfo(ensureIsLogged: boolean): Observable<any> {
        // posso chiamarla prima della login, allora avrò meno informazioni
        // se la richiamo a login effettuata mi popola le informazioni mancanti
        return Observable.create(observer => {
            if (this.productInfo && (!ensureIsLogged || this.productInfo.userLogged)) {
                observer.next(this.productInfo);
                observer.complete();
            } else {

                if (!this.getProductInfoPromise) {
                    this.getProductInfoPromise = new Promise((resolve, reject) => {
                        const params = { authtoken: sessionStorage.getItem('authtoken') };
                        const url = this.getMenuServiceUrl() + 'getProductInfo/';
                        const sub = this.request(url, params)
                            .subscribe(result => {
                                this.productInfo = result.ProductInfos;
                                delete this.getProductInfoPromise;
                                resolve();
                                sub.unsubscribe();
                            });

                    });
                }
                this.getProductInfoPromise.then(() => {
                    observer.next(this.productInfo);
                    observer.complete();
                });
            }
        });
    }

    public getDictionaries(): Observable<any> {
        return Observable.create(observer => {
            if (this.dictionaries) {
                observer.next(this.dictionaries);
                observer.complete();
            } else {
                const url = this.getDataServiceUrl() + 'getinstalleddictionaries';
                const sub = this.request(url, {})
                    .subscribe(result => {
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

    getEasyStudioServiceUrl() {
        return this.getBaseUrl() + '/easystudio/';
    }
    
    getLocalizationServiceUrl() {
        return this.getBaseUrl() + '/localization-service/';
    }

    getEnumsServiceUrl() {
        const url = this.getBaseUrl() + '/enums-service/';
        return url;
    }

    getFormattersServiceUrl() {
        const url = this.getBaseUrl() + '/formatters-service/';
        return url;
    }

    getDataServiceUrl() {
        const url = this.getBaseUrl() + '/data-service/';
        return url;
    }

    getReportServiceUrl() {
        const url = this.getBaseUrl() + '/rs/';
        return url;
    }

    request(url: string, data: Object): Observable<any> {
        const headers = new Headers();
        headers.append('Content-Type', 'application/x-www-form-urlencoded');
        return this.http.post(url, this.utilsService.serializeData(data), { withCredentials: true, headers: headers })
            .map(res => res.json())
            .catch((error: any): ErrorObservable => {
                const errMsg = (error.message) ? error.message :
                    error.status ? `${error.status} - ${error.statusText}` : 'Server error';
                this.logger.error(errMsg);

                return Observable.throw(errMsg);
            });
    }
}