import { Injectable } from '@angular/core';
import { Http, Response, Headers } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { UtilsService } from '../../core/services/utils.service';
import { HttpService } from '../../core/services/http.service';
import { Logger } from '../../core/services/logger.service';

import { CookieService } from 'angular2-cookie/services/cookies.service';

@Injectable()
export class HttpMenuService {


    constructor(
        private http: Http,
        private utilsService: UtilsService,
        private logger: Logger,
        private cookieService: CookieService,
        private httpService: HttpService) {
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
     * API /favoriteObject
     * 
     * @returns {Observable<any>} favoriteObject
     */
    favoriteObject(object) {
        let obj = { target: object.target, objectType: object.objectType, objectName: object.objectName, user: this.cookieService.get('_user'), company: this.cookieService.get('_company') };

        var urlToRun = this.httpService.getMenuServiceUrl() + 'favoriteObject/';
        let subs = this.postData(urlToRun, obj)
            .map((res: Response) => {
                return res.ok;
            })
            .subscribe(result => {
                subs.unsubscribe();
            });
    }

    /**
     * API /unFavoriteObject
     * 
     * @returns {Observable<any>} unFavoriteObject
     */
    unFavoriteObject(object) {
        let obj = { target: object.target, objectType: object.objectType, objectName: object.objectName, user: this.cookieService.get('_user'), company: this.cookieService.get('_company') };
        var urlToRun = this.httpService.getMenuServiceUrl() + 'unFavoriteObject/';
        let subs = this.postData(urlToRun, obj)
            .map((res: Response) => {
                return res.ok;
            })
            .subscribe(result => {
                subs.unsubscribe();
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
     * API /addToMostUsed
     * 
     * @returns {Observable<any>} addToMostUsed
     */
    addToMostUsed(object): Observable<any> {
        let obj = { target: object.target, objectType: object.objectType, objectName: object.objectName, user: this.cookieService.get('_user'), company: this.cookieService.get('_company') };
        return this.postData(this.httpService.getMenuServiceUrl() + 'addToMostUsed/', obj)
            .map((res: Response) => {
                return res.ok;
            });
    };

    /**
     * API /removeFromMostUsed
     * 
     * @returns {Observable<any>} removeFromMostUsed
     */
    removeFromMostUsed = function (object) {
        let obj = { target: object.target, objectType: object.objectType, objectName: object.objectName, user: this.cookieService.get('_user'), company: this.cookieService.get('_company') };

        return this.postData(this.httpService.getMenuServiceUrl() + 'removeFromMostUsed/', obj)
            .map((res: Response) => {
                return res.ok;
            });
    };

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