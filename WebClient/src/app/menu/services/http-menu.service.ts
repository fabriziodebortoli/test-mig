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
        protected utilService: UtilsService,
        protected logger: Logger,
        protected cookieService: CookieService) {
        super(http, utilService, logger, cookieService)
    }

    getMenuElements(): Observable<any> {
        return this.http.get(this.getMenuBaseUrl(true) + 'getMenuElements/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            })
            .catch(this.handleError);
    }

     //---------------------------------------------------------------------------------------------
    favoriteObject  (object) {
        var urlToRun = this.getMenuBaseUrl(true) + 'favoriteObject/?target=' + object.target + '&objectType=' + object.objectType;
         let subs = this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError)
            .subscribe(result => {
                console.log(result);
                subs.unsubscribe();
            });
    }

    //---------------------------------------------------------------------------------------------
    unFavoriteObject (object) {
        var urlToRun = this.getMenuBaseUrl(true) + 'unFavoriteObject/?target=' + object.target + '&objectType=' + object.objectType;
          let subs = this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            })
            .catch(this.handleError)
            .subscribe(result => {
                console.log(result);
                subs.unsubscribe();
            });
    }
}