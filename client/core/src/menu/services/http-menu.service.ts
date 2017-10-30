import { Injectable } from '@angular/core';
import { Http, Response, Headers } from '@angular/http';
import { Observable } from '../../rxjs.imports';

import { CookieService } from 'ngx-cookie';

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

    /**
     * API /getMenuElements
     * 
     * @returns {Observable<any>} getMenuElements
     */
    getMenuElements(clearCachedData: boolean): Observable<any> {
        let obj = { user: this.cookieService.get('_user'), company: this.cookieService.get('_company'), authtoken: this.cookieService.get('authtoken'), clearCachedData: clearCachedData }
        let url = this.infoService.getMenuServiceUrl() + 'getMenuElements/';
        return this.httpService.postData(url, obj)
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
        let url = this.infoService.getDocumentBaseUrl() + 'getAllAppsAndModules/';
        return this.httpService.postData(url, obj)
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
        let obj = { user: this.cookieService.get('_user'), app: app, mod: mod, def: isThisPairDefault };
        let url = this.infoService.getDocumentBaseUrl() + 'setAppAndModule/';
        return this.httpService.postData(url, obj)
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
        let obj = { user: this.cookieService.get('_user'), app: app, mod: mod, type: type };
        let url = this.infoService.getDocumentBaseUrl() + 'createNewContext/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    /**
  * API /runEasyStudio
  * 
  * @returns {Observable<any>} runEasyStudio
  */
    runEasyStudio(ns: string, customizationName: string): Observable<any> {
        let obj = { user: this.cookieService.get('_user'), ns: encodeURIComponent(ns), customization: encodeURIComponent(customizationName) };
        let url = this.infoService.getDocumentBaseUrl() + 'runEasyStudio/';
        // if (customizationName != undefined)
        //     url += "&customization=" + encodeURIComponent(customizationName);
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    /**
* API /closeCustomizationContext
* 
* @returns {Observable<any>} closeCustomizationContext
*/
    closeCustomizationContext(): Observable<any> {
        let obj = { user: this.cookieService.get('_user') };
        let url = this.infoService.getDocumentBaseUrl() + 'closeCustomizationContext/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    /**
* API /isEasyStudioDocument
* 
* @returns {Observable<any>} isEasyStudioDocument
*/
    isEasyStudioDocument(object): Observable<any> {
        if (object.isEasyStudioDocument != undefined)
            return object.isEasyStudioDocument;

        let obj = { user: this.cookieService.get('_user'), ns: encodeURIComponent(object.target) };
        let url = this.infoService.getDocumentBaseUrl() + 'isEasyStudioDocument/';
        return this.httpService.postData(url, obj)
            .map((data: any) => {
                if (data && data.message && data.message.text) {
                    object.isEasyStudioDocument = data.message.text == "true";
                    return object.isEasyStudioDocument;
                }
            })
            .catch(this.handleError);
    }

    /**
  * API /getDefaultContext
  * 
  * @returns {Observable<any>} getDefaultContext
  */
    getDefaultContext(app: string, mod: string, type: string): Observable<any> {
        let obj = { user: this.cookieService.get('_user') };
        let url = this.infoService.getDocumentBaseUrl() + 'getDefaultContext/';
        return this.httpService.postData(url, obj)
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
        let url = this.infoService.getDocumentBaseUrl() + 'refreshEasyBuilderApps/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    /**
* API /getDefaultContext
* 
* @returns {Observable<any>} getDefaultContext
*/
    getCurrentContext(): Observable<any> {
        let obj = { user: this.cookieService.get('_user') };
        let url = this.infoService.getDocumentBaseUrl() + 'getCurrentContext/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    /**
* API /getCustomizationsForDocument
* 
* @returns {Observable<any>} getCustomizationsForDocument
*/
    initEasyStudioData(object): Observable<any> {

        var ns = object.target;
        ns = 'document' + "." + ns;
        let obj = { user: this.cookieService.get('_user'), ns: encodeURIComponent(ns) };
        var url = this.infoService.getDocumentBaseUrl() + 'getCustomizationsForDocument/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            }).catch(this.handleError);
    }

    /**
* API /getDefaultContext
* 
* @returns {Observable<any>} getDefaultContext
*/
    cloneAsEasyStudioDocument(object): Observable<any> {
        return null;
        // guarda EasyStudioService.js
    }



    /***************************************************** */







    /**
     * API /getConnectionInfo
     * 
     * @returns {Observable<any>} getConnectionInfo
     */
    getConnectionInfo(): Observable<any> {

        let obj = { authtoken: this.cookieService.get('authtoken') };
        var url = this.infoService.getMenuServiceUrl() + 'getConnectionInfo/';
        return this.httpService.postData(url, obj)
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
        var url = this.infoService.getDocumentBaseUrl() + 'getApplicationDate/';
        return this.httpService.postData(url, obj)
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
        var url = this.infoService.getDocumentBaseUrl() + 'changeApplicationDate/?day=' + day + '&month=' + month + '&year=' + year;
        return this.httpService.postData(url, obj)
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
        let url = this.infoService.getMenuServiceUrl() + 'clearAllMostUsed/';
        return this.httpService.postData(url, obj)
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
        let url = this.infoService.getMenuServiceUrl() + 'getMostUsedShowNr/';
        return this.httpService.postData(url, obj)
        
            .map((res: Response) => {
                return res.json();
            });
    }

    /**
    * API /favoriteObject
    * 
    * @returns {Observable<boolean>}
    */
    updateAllFavoritesAndMostUsed(favorites: any, mostUsed: any): Observable<boolean> {
        let obj = {
            user: this.cookieService.get('_user'), company: this.cookieService.get('_company'),
            favorites: JSON.stringify(favorites), mostUsed: JSON.stringify(mostUsed)
        };
        var url = this.infoService.getMenuServiceUrl() + 'updateAllFavoritesAndMostUsed/';
        return this.httpService.postData(url, obj)
            .map((res: Response) => {
                return res.ok;
            });
    }

    /**
     * API /clearCachedData
     * 
     * @returns {Observable<any>} clearCachedData
     */
    clearCachedData(): Observable<any> {
        var url = this.infoService.getMenuServiceUrl() + 'clearCachedData/';
        return this.httpService.postData(url, undefined)
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
        let url = this.infoService.isDesktop ? this.infoService.getDocumentBaseUrl() : this.infoService.getMenuServiceUrl() ;
        url += + 'getOnlineHelpUrl/';
        return this.httpService.postData(url, obj)
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
        var url = this.infoService.getDocumentBaseUrl() + 'getThemes/';
        return this.httpService.postData(url, obj)
            .map((res: Response) => {
                return res.json();
            });
    }


    /**
     * API /getThemes
     * 
     * @returns {Observable<any>} changeThemes
     */
    changeThemes(theme: string): Observable<OperationResult> {

        let obj = { authtoken: this.cookieService.get('authtoken') };
        var url = this.infoService.getDocumentBaseUrl() + 'changeThemes/?theme=' + theme;
        return this.httpService.postData(url, obj)
            .map((res: Response) => {
                return this.httpService.createOperationResult(res);
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