import { UtilsService, HttpService } from 'tb-core';
import { Observable } from 'rxjs';
import { Logger } from 'libclient';
import { Http, Response } from '@angular/http';
import { Injectable } from '@angular/core';
import { CookieService } from 'angular2-cookie/services/cookies.service';

@Injectable()
export class HttpMenuService extends HttpService {

    constructor(
        protected http: Http,
        protected utilsService: UtilsService,
        protected logger: Logger,
        protected cookieService: CookieService) {
        super(http, utilsService, logger, cookieService)
    }

    getMenuElements(): Observable<any> {
        return this.http.get(this.getMenuBaseUrl(true) + 'getMenuElements/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    getProductInfo(): Observable<any> {
        return this.http.get(this.getMenuBaseUrl(true) + 'getProductInfo/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    getPreferences(): Observable<any> {
        return this.http.get(this.getMenuBaseUrl(true) + 'getPreferences/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    //---------------------------------------------------------------------------------------------
    setPreference(referenceName, referenceValue): Observable<any> {
        var urlToRun = this.getMenuBaseUrl(true) + 'setPreference/?name=' + referenceName + '&value=' + referenceValue;
        return this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError);
    }

     addToHiddenTiles(tile, applicationName, groupName, menuName) : Observable<any> {
        var urlToRun = this.getMenuBaseUrl(true) + 'addToHiddenTiles/?application=' + applicationName + '&group=' + groupName + '&menu=' + menuName + '&tile=' + tile.name;
        return this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError);
    }
    
    removeFromHiddenTiles(tile, applicationName, groupName, menuName) : Observable<any> {
        var urlToRun = this.getMenuBaseUrl(true) + 'removeFromHiddenTiles/?application=' + applicationName + '&group=' + groupName + '&menu=' + menuName + '&tile=' + tile.name;
        return this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError);
    }
    //---------------------------------------------------------------------------------------------
    getThemedSettings(): Observable<any> {
        return this.http.get(this.getMenuBaseUrl(true) + 'getThemedSettings/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }


    getConnectionInfo(): Observable<any> {
        return this.http.get(this.getMenuBaseUrl(true) + 'getConnectionInfo/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    //---------------------------------------------------------------------------------------------
    activateViaSMS() {
        var urlToRun = this.getMenuBaseUrl(true) + 'activateViaSMS/';
        let subs = this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError)
            .subscribe(result => {
                subs.unsubscribe();
            });
    }


    //---------------------------------------------------------------------------------------------
    goToSite() {
        var urlToRun = this.getMenuBaseUrl(true) + 'producerSite/';
        let subs = this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError)
            .subscribe(result => {
                subs.unsubscribe();
            });
    }

    //---------------------------------------------------------------------------------------------
    clearCachedData(): Observable<any> {
        var urlToRun = this.getMenuBaseUrl(true) + 'clearCachedData/';
        return this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError);
    }



    //---------------------------------------------------------------------------------------------
    activateViaInternet() {
        var urlToRun = this.getMenuBaseUrl(true) + 'activateViaInternet/';
        let subs = this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError)
            .subscribe(result => {
                subs.unsubscribe();
            });
    }



    //---------------------------------------------------------------------------------------------
    favoriteObject(object) {
        var urlToRun = this.getMenuBaseUrl(true) + 'favoriteObject/?target=' + object.target + '&objectType=' + object.objectType;
        let subs = this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError)
            .subscribe(result => {
                subs.unsubscribe();
            });
    }

    //---------------------------------------------------------------------------------------------
    unFavoriteObject(object) {
        var urlToRun = this.getMenuBaseUrl(true) + 'unFavoriteObject/?target=' + object.target + '&objectType=' + object.objectType;
        let subs = this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError)
            .subscribe(result => {
                subs.unsubscribe();
            });
    }

    //---------------------------------------------------------------------------------------------
    mostUsedClearAll(): Observable<any> {
        return this.http.get(this.getMenuBaseUrl(true) + 'clearAllMostUsed/', { withCredentials: true })
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError);

    };

    //---------------------------------------------------------------------------------------------
    getMostUsedShowNr(callback) {

        var urlToRun = this.getMenuBaseUrl(true) + 'getMostUsedShowNr/';
        let subs = this.postData(urlToRun, undefined)
            .map((res: Response) => {
                callback(res);
                return res.ok;
            })
            .catch(this.handleError)
            .subscribe(result => {
                subs.unsubscribe();
            });
    }

    //---------------------------------------------------------------------------------------------
    addToMostUsed(object): Observable<any> {


        return this.http.get(this.getMenuBaseUrl(true) + 'addToMostUsed/?target=' + object.target + '&objectType=' + object.objectType, { withCredentials: true })
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError);
    };

    //---------------------------------------------------------------------------------------------
    removeFromMostUsed = function (object) {

        return this.http.get(this.getMenuBaseUrl(true) + 'removeFromMostUsed/?target=' + object.target + '&objectType=' + object.objectType, { withCredentials: true })
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError);
    };

    //---------------------------------------------------------------------------------------------
    loadLocalizedElements(needLoginThread): Observable<any> {
        return this.http.get(this.getMenuBaseUrl(needLoginThread) + 'getLocalizedElements/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    };
}