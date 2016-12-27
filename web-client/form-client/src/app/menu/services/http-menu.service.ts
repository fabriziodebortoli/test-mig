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
        return this.http.get(this.getMenuBaseUrl() + 'getMenuElements/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    getProductInfo(): Observable<any> {
        return this.http.get(this.getMenuBaseUrl() + 'getProductInfo/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    getPreferences(): Observable<any> {
        return this.http.get(this.getMenuBaseUrl() + 'getPreferences/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    //---------------------------------------------------------------------------------------------
    setPreference(referenceName, referenceValue): Observable<any> {
        var urlToRun = this.getMenuBaseUrl() + 'setPreference/?name=' + referenceName + '&value=' + referenceValue;
        return this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError);
    }

     addToHiddenTiles(tile, applicationName, groupName, menuName) : Observable<any> {
        var urlToRun = this.getMenuBaseUrl() + 'addToHiddenTiles/?application=' + applicationName + '&group=' + groupName + '&menu=' + menuName + '&tile=' + tile.name;
        return this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError);
    }
    
    removeFromHiddenTiles(tile, applicationName, groupName, menuName) : Observable<any> {
        var urlToRun = this.getMenuBaseUrl() + 'removeFromHiddenTiles/?application=' + applicationName + '&group=' + groupName + '&menu=' + menuName + '&tile=' + tile.name;
        return this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError);
    }
    //---------------------------------------------------------------------------------------------
    getThemedSettings(): Observable<any> {
        return this.http.get(this.getMenuBaseUrl() + 'getThemedSettings/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }


    getConnectionInfo(): Observable<any> {
        return this.http.get(this.getMenuBaseUrl() + 'getConnectionInfo/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    //---------------------------------------------------------------------------------------------
    activateViaSMS() {
        var urlToRun = this.getMenuBaseUrl() + 'activateViaSMS/';
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
        var urlToRun = this.getMenuBaseUrl() + 'producerSite/';
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
        var urlToRun = this.getMenuBaseUrl() + 'clearCachedData/';
        return this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError);
    }



    //---------------------------------------------------------------------------------------------
    activateViaInternet() {
        var urlToRun = this.getMenuBaseUrl() + 'activateViaInternet/';
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
        var urlToRun = this.getMenuBaseUrl() + 'favoriteObject/?target=' + object.target + '&objectType=' + object.objectType;
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
        var urlToRun = this.getMenuBaseUrl() + 'unFavoriteObject/?target=' + object.target + '&objectType=' + object.objectType;
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
        return this.http.get(this.getMenuBaseUrl() + 'clearAllMostUsed/', { withCredentials: true })
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError);

    };

    //---------------------------------------------------------------------------------------------
    getMostUsedShowNr(callback) {

        var urlToRun = this.getMenuBaseUrl() + 'getMostUsedShowNr/';
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


        return this.http.get(this.getMenuBaseUrl() + 'addToMostUsed/?target=' + object.target + '&objectType=' + object.objectType, { withCredentials: true })
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError);
    };

    //---------------------------------------------------------------------------------------------
    removeFromMostUsed = function (object) {

        return this.http.get(this.getMenuBaseUrl() + 'removeFromMostUsed/?target=' + object.target + '&objectType=' + object.objectType, { withCredentials: true })
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError);
    };

    //---------------------------------------------------------------------------------------------
    loadLocalizedElements(needLoginThread): Observable<any> {
        return this.http.get(this.getMenuBaseUrl() + 'getLocalizedElements/?needLoginThread=' + needLoginThread, { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    };
}