import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { UtilsService } from '../../core/services/utils.service';
import { HttpService } from '../../core/services/http.service';
import { Logger } from '../../core/services/logger.service';
import { CookieService } from 'angular2-cookie/services/cookies.service';
export declare class HttpMenuService {
    private http;
    private utilsService;
    private logger;
    private cookieService;
    private httpService;
    constructor(http: Http, utilsService: UtilsService, logger: Logger, cookieService: CookieService, httpService: HttpService);
    postData(url: string, data: Object): Observable<Response>;
    /**
     * API /getMenuElements
     *
     * @returns {Observable<any>} getMenuElements
     */
    getMenuElements(): Observable<any>;
    /**
     * API /getPreferences
     *
     * @returns {Observable<any>} getPreferences
     */
    getPreferences(): Observable<any>;
    /**
     * API /setPreference
     *
     * @param {string} referenceName
     * @param {string} referenceValue
     *
     * @returns {Observable<any>} setPreference
     */
    setPreference(referenceName: string, referenceValue: string): Observable<any>;
    /**
  * API /getThemedSettings
  *
  * @returns {Observable<any>} getThemedSettings
  */
    getThemedSettings(): Observable<any>;
    /**
     * API /getConnectionInfo
     *
     * @returns {Observable<any>} getConnectionInfo
     */
    getConnectionInfo(): Observable<any>;
    /**
     * API /favoriteObject
     *
     * @returns {Observable<any>} favoriteObject
     */
    favoriteObject(object: any): void;
    /**
     * API /unFavoriteObject
     *
     * @returns {Observable<any>} unFavoriteObject
     */
    unFavoriteObject(object: any): void;
    /**
     * API /mostUsedClearAll
     *
     * @returns {Observable<any>} mostUsedClearAll
     */
    mostUsedClearAll(): Observable<any>;
    /**
     * API /getMostUsedShowNr
     *
     * @returns {Observable<any>} getMostUsedShowNr
     */
    getMostUsedShowNr(callback: any): Observable<any>;
    /**
     * API /addToMostUsed
     *
     * @returns {Observable<any>} addToMostUsed
     */
    addToMostUsed(object: any): Observable<any>;
    /**
     * API /removeFromMostUsed
     *
     * @returns {Observable<any>} removeFromMostUsed
     */
    removeFromMostUsed: (object: any) => any;
    /**
     * API /clearCachedData
     *
     * @returns {Observable<any>} clearCachedData
     */
    clearCachedData(): Observable<any>;
    /**
     * API /loadLocalizedElements
     *
     * @returns {Observable<any>} loadLocalizedElements
     */
    loadLocalizedElements(needLoginThread: any): Observable<any>;
    /**
 * API /getProductInfo
 *
 * @returns {Observable<any>} getProductInfo
 */
    getProductInfo(): Observable<any>;
    /**
  * API /activateViaSMS
  *
  * @returns {Observable<any>} activateViaSMS
  */
    activateViaSMS(): Observable<any>;
    /**
     * API /goToSite
     *
     * @returns {Observable<any>} goToSite
     */
    goToSite(): Observable<any>;
    /**
     * TODO refactor with custom logger
     */
    private handleError(error);
}
