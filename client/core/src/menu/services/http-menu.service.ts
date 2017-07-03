import { Injectable } from '@angular/core';
import { Http, Response, Headers } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { HttpService } from './../../core/services/http.service';
import { Logger } from './../../core/services/logger.service';
import { UtilsService } from './../../core/services/utils.service';

@Injectable()
export class HttpMenuService {


    constructor(
        private http: Http,
        private utilsService: UtilsService,
        private logger: Logger,
        private cookieService: CookieService,
        private httpService: HttpService) {

        this.logger.debug('HttpMenuService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }

    postData(url: string, data: Object): Observable<Response> {
        let headers = new Headers();
        headers.append('Content-Type', 'application/x-www-form-urlencoded');
        return this.http.post(url, this.utilsService.serializeData(data), { withCredentials: true, headers: headers });
    }

    /**
     * API /getMenuElements
     * 
     * @returns {Observable<any>} getMenuElements
     */
    getMenuElements(): Observable<any> {
        let obj = { user: this.cookieService.get('_user'), company: this.cookieService.get('_company'), token: this.cookieService.get('authtoken') }
        let urlToRun = this.httpService.getMenuServiceUrl() + 'getMenuElements/';
        return this.postData(urlToRun, obj)
            .map((res: any) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    /**
     * API /getPreferences
     * 
     * @returns {Observable<any>} getPreferences
     */
    getPreferences(): Observable<any> {
        let urlToRun = this.httpService.getMenuServiceUrl() + 'getPreferences/';
        let obj = { user: this.cookieService.get('_user'), company: this.cookieService.get('_company') }

        return this.postData(urlToRun, obj)
            .map((res: any) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    /**
     * API /setPreference
     * 
     * @param {string} referenceName
     * @param {string} referenceValue
     * 
     * @returns {Observable<any>} setPreference
     */
    setPreference(referenceName: string, referenceValue: string): Observable<any> {
        let obj = { name: referenceName, value: referenceValue, user: this.cookieService.get('_user'), company: this.cookieService.get('_company') };
        var urlToRun = this.httpService.getMenuServiceUrl() + 'setPreference/';
        return this.postData(urlToRun, obj)
            .map((res: Response) => {
                return res.ok;
            });
    }

    /**
  * API /getThemedSettings
  * 
  * @returns {Observable<any>} getThemedSettings
  */
    getThemedSettings(): Observable<any> {
        let obj = { token: this.cookieService.get('authtoken') };
        var urlToRun = this.httpService.getMenuServiceUrl() + 'getThemedSettings/';
        return this.postData(urlToRun, obj)
            .map((res: Response) => {
                return res.json();
            });
    }

    /**
     * API /getConnectionInfo
     * 
     * @returns {Observable<any>} getConnectionInfo
     */
    getConnectionInfo(): Observable<any> {

        let obj = { token: this.cookieService.get('authtoken') };
        var urlToRun = this.httpService.getMenuServiceUrl() + 'getConnectionInfo/';
        return this.postData(urlToRun, obj)
            .map((res: Response) => {
                return res.json();
            });
    }

    /**
     * API /mostUsedClearAll
     *
     * @returns {Observable<any>} mostUsedClearAll
     */
    mostUsedClearAll(): Observable<any> {
        let obj = { user: this.cookieService.get('_user'), company: this.cookieService.get('_company') }
        return this.postData(this.httpService.getMenuServiceUrl() + 'clearAllMostUsed/', obj)
            .map((res: Response) => {
                return res.ok;
            });

    };

    /**
     * API /getMostUsedShowNr
     * 
     * @returns {Observable<any>} getMostUsedShowNr
     */
    getMostUsedShowNr(callback) {
        let obj = { user: this.cookieService.get('_user'), company: this.cookieService.get('_company') }
        return this.postData(this.httpService.getMenuServiceUrl() + 'getMostUsedShowNr/', obj)
            .map((res: Response) => {
                return res.json();
            });
    }

    /**
    * API /favoriteObject
    * 
    * @returns {Observable<boolean>}
    */
    updateAllFavoritesAndMostUsed(favorites: any, mostUsed: any): Observable<Response> {
        let obj = {
            user: this.cookieService.get('_user'), company: this.cookieService.get('_company'),
            favorites: JSON.stringify(favorites), mostUsed: JSON.stringify(mostUsed)
        };
        var urlToRun = this.httpService.getMenuServiceUrl() + 'updateAllFavoritesAndMostUsed/';
        return this.postData(urlToRun, obj)
            .map((res: Response) => {
                return res;
            });
    }

    /**
     * API /clearCachedData
     * 
     * @returns {Observable<any>} clearCachedData
     */
    clearCachedData(): Observable<any> {
        var urlToRun = this.httpService.getMenuServiceUrl() + 'clearCachedData/';
        return this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            });
    }

    /**
     * API /loadLocalizedElements
     * 
     * @returns {Observable<any>} loadLocalizedElements
     */
    loadLocalizedElements(needLoginThread): Observable<any> {
        let obj = { token: this.cookieService.get('authtoken') }
        return this.postData(this.httpService.getMenuServiceUrl() + 'getLocalizedElements/', obj)
            .map((res: Response) => {
                return res.json();
            });
    };



    /**
 * API /getProductInfo
 * 
 * @returns {Observable<any>} getProductInfo
 */
    getProductInfo(): Observable<any> {
        let obj = { token: this.cookieService.get('authtoken') }
        return this.postData(this.httpService.getMenuServiceUrl() + 'getProductInfo/', obj)
            .map((res: Response) => {
                return res.json();
            });
    }

    /**
  * API /activateViaSMS
  * 
  * @returns {Observable<any>} activateViaSMS
  */
    activateViaSMS(): Observable<any> {
        return this.http.get(this.httpService.getMenuServiceUrl() + 'getPingViaSMSUrl/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            });
    }

    /**
     * API /goToSite
     * 
     * @returns {Observable<any>} goToSite
     */
    goToSite(): Observable<any> {

        return this.http.get(this.httpService.getMenuServiceUrl() + 'getProducerSite/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            });
    }

    /**
     * TODO refactor with custom logger
     */
    private handleError(error: any) {
        // In a real world app, we might use a remote logging infrastructure
        // We'd also dig deeper into the error to get a better message
        let errMsg = (error.message) ? error.message :
            error.status ? `${error.status} - ${error.statusText}` : 'Server error';
        console.error(errMsg);
        return Observable.throw(errMsg);
    }


}