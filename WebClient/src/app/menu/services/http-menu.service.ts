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
    mostUsedClearAll = function () {

        var urlToRun = this.getMenuBaseUrl(true) + 'clearAllMostUsed/';
        let subs = this.postData(urlToRun, undefined)
            .map((res: Response) => {

                //                  $scope.mostUsed.splice(0, $scope.mostUsed.length);
                //			    menuService.MostUsedCount = 0;
                return res.ok;
            })
            .catch(this.handleError)
            .subscribe(result => {
                subs.unsubscribe();
            });

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
}