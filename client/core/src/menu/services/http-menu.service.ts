import { Injectable } from '@angular/core';
import { Http, Response, Headers } from '@angular/http';
import { Observable } from 'rxjs/Observable';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { OperationResult } from './../../shared/models/operation-result.model';

import { InfoService } from './../../core/services/info.service';
import { HttpService } from './../../core/services/http.service';
import { Logger } from './../../core/services/logger.service';
import { UtilsService } from './../../core/services/utils.service';

@Injectable()
export class HttpMenuService {

    constructor(
        public http: Http,
        public utilsService: UtilsService,
        public logger: Logger,
        public cookieService: CookieService,
        public httpService: HttpService,
        public infoService: InfoService) {

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
    getMenuElements(clearCachedData: boolean): Observable<any> {
        let obj = { user: this.cookieService.get('_user'), company: this.cookieService.get('_company'), authtoken: this.cookieService.get('authtoken'), clearCachedData: clearCachedData }
        let urlToRun = this.infoService.getMenuServiceUrl() + 'getMenuElements/';
        return this.postData(urlToRun, obj)
            .map((res: any) => {
                return res.json();
            })
            .catch(this.handleError);
    }


    /************************************************************** */

    /**
     * API /getEsAppsAndModules
     * 
     * @returns {Observable<any>} getEsAppsAndModules
     */
    getEsAppsAndModules(): Observable<any> {
        let obj = { user: this.cookieService.get('_user') };
        let urlToRun = this.infoService.getDocumentBaseUrl() + 'getAllAppsAndModules/';
        return this.postData(urlToRun, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    /**
  * API /setAppAndModule
  * 
  * @returns {Observable<any>} setAppAndModule
  */
    setAppAndModule(app: string, mod: string, isThisPairDefault: boolean): Observable<any> {
        let obj = { user: this.cookieService.get('_user') };
        let urlToRun = this.infoService.getDocumentBaseUrl() +
            '/setAppAndModule/?app=' + app + '&mod=' + mod + '&def=' + isThisPairDefault;
        return this.postData(urlToRun, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    /**
  * API /createNewContext
  * 
  * @returns {Observable<any>} createNewContext
  */
    createNewContext(app: string, mod: string, type: string): Observable<any> {
        let obj = { user: this.cookieService.get('_user') };
        let urlToRun = this.infoService.getDocumentBaseUrl() +
            '/createNewContext/?app=' + app + '&mod=' + mod + '&type=' + type;
        return this.postData(urlToRun, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }


    /*   runEasyStudio(app: string, mod:string, type:string): Observable<any> {
           let obj = { user: this.cookieService.get('_user')};
           let urlToRun = this.infoService.getDocumentBaseUrl() +   
           var urlToRun = 'runEasyStudio/?ns=' + encodeURIComponent(ns);
           
                   if (customizationName != undefined)
                       urlToRun += "&customization=" + encodeURIComponent(customizationName);
           return this.postData(urlToRun, obj)
           .map((res: any) => {
               return res;
           })
           .catch(this.handleError);
       }*/

    /**
  * API /getDefaultContext
  * 
  * @returns {Observable<any>} getDefaultContext
  */
    getDefaultContext(app: string, mod: string, type: string): Observable<any> {
        let obj = { user: this.cookieService.get('_user') };
        let urlToRun = this.infoService.getDocumentBaseUrl() + '/getDefaultContext/';
        return this.postData(urlToRun, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    /**
    * API /refreshEasyBuilderApps
    * 
    * @returns {Observable<any>} refreshEasyBuilderApps
    */
    refreshEasyBuilderApps(): Observable<any> {
        let obj = { user: this.cookieService.get('_user') };
        let urlToRun = this.infoService.getDocumentBaseUrl() + 'refreshEasyBuilderApps/';
        return this.postData(urlToRun, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    /***************************************************** */






    /**
     * API /getPreferences
     * 
     * @returns {Observable<any>} getPreferences
     */
    getPreferences(): Observable<any> {
        let urlToRun = this.infoService.getMenuServiceUrl() + 'getPreferences/';
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
        var urlToRun = this.infoService.getMenuServiceUrl() + 'setPreference/';
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
        let obj = { authtoken: this.cookieService.get('authtoken') };
        var urlToRun = this.infoService.getMenuServiceUrl() + 'getThemedSettings/';
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

        let obj = { authtoken: this.cookieService.get('authtoken') };
        var urlToRun = this.infoService.getMenuServiceUrl() + 'getConnectionInfo/';
        return this.postData(urlToRun, obj)
            .map((res: Response) => {
                return res.json();
            });
    }

    /**
   * API /getApplicationDate
   * 
   * @returns {Observable<any>} getApplicationDate
   */
    getApplicationDate(): Observable<any> {

        let obj = { authtoken: this.cookieService.get('authtoken') };
        var urlToRun = this.infoService.getDocumentBaseUrl() + 'getApplicationDate/';
        return this.postData(urlToRun, obj)
            .map((res: Response) => {
                return res.json();
            });
    }

    /**
   * API /changeApplicationDate
   * 
   * @returns {Observable<OperationResult>} changeApplicationDate
   */
    changeApplicationDate(date: Date): Observable<OperationResult> {

        let day = date.getDate();
        let month = date.getMonth() + 1;
        let year = date.getFullYear();
        let obj = { authtoken: this.cookieService.get('authtoken') };
        var urlToRun = this.infoService.getDocumentBaseUrl() + 'changeApplicationDate/?day=' + day + '&month=' + month + '&year=' + year;
        return this.postData(urlToRun, obj)
            .map((res: Response) => {
                return this.httpService.createOperationResult(res);
            });
    }

    /**
     * API /mostUsedClearAll
     *
     * @returns {Observable<any>} mostUsedClearAll
     */
    mostUsedClearAll(): Observable<any> {
        let obj = { user: this.cookieService.get('_user'), company: this.cookieService.get('_company') }
        return this.postData(this.infoService.getMenuServiceUrl() + 'clearAllMostUsed/', obj)
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
        return this.postData(this.infoService.getMenuServiceUrl() + 'getMostUsedShowNr/', obj)
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
        var urlToRun = this.infoService.getMenuServiceUrl() + 'updateAllFavoritesAndMostUsed/';
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
        var urlToRun = this.infoService.getMenuServiceUrl() + 'clearCachedData/';
        return this.postData(urlToRun, undefined)
            .map((res: Response) => {
                return res.ok;
            });
    }


    /**
    * API /activateViaSMS
    * 
    * @returns {Observable<any>} activateViaSMS
    */
    activateViaSMS(): Observable<any> {
        return this.http.get(this.infoService.getMenuServiceUrl() + 'getPingViaSMSUrl/', { withCredentials: true })
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

        return this.http.get(this.infoService.getMenuServiceUrl() + 'getProducerSite/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            });
    }

    /**
    * API /goToSite
    * 
    * @returns {Observable<any>} goToSite
    */
    callonlineHelpUrl(ns: string, culture: string): Observable<any> {
        let obj = { nameSpace: ns, culture: culture }
        let url = this.infoService.isDesktop ? this.infoService.getDocumentBaseUrl() : this.infoService.getMenuServiceUrl();
        return this.postData(url + 'getOnlineHelpUrl/', obj)
            .map((res: Response) => {
                return res.json();
            });
    }

    /**
     * API /getThemes
     * 
     * @returns {Observable<any>} getThemes
     */
    getThemes(): Observable<any> {

        let obj = { authtoken: this.cookieService.get('authtoken') };
        var urlToRun = this.infoService.getDocumentBaseUrl() + 'getThemes/';
        return this.postData(urlToRun, obj)
            .map((res: Response) => {
                return res.json();
            });
    }


    /**
     * API /getThemes
     * 
     * @returns {Observable<any>} changeThemes
     */
    changeThemes(theme: string): Observable<any> {

        let obj = { authtoken: this.cookieService.get('authtoken') };
        var urlToRun = this.infoService.getDocumentBaseUrl() + 'changeThemes/?theme=' + theme;
        return this.postData(urlToRun, obj)
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