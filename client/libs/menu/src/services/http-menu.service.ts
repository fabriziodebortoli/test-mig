import { EasystudioService } from './easystudio.service';
import { UtilsService, Logger, HttpService, InfoService, OperationResult } from '@taskbuilder/core';
import { Injectable } from '@angular/core';
import { Http, Response, Headers } from '@angular/http';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class HttpMenuService {
    callInfoService: string;

    constructor(
        public http: Http,
        public utilsService: UtilsService,
        public logger: Logger,
        public httpService: HttpService,
        public infoService: InfoService
    ) {
        this.callInfoService = this.infoService.getEasyStudioServiceUrl();//per usare es-service .net core
    }

    isCachedMenuTooOld(): Observable<any> {

        let storageMenuDate = !localStorage.getItem('_lastAllMenuTimeStamp') || !localStorage.getItem('_lastAllMenu')
            ? new Date().getTime()
            : localStorage.getItem('_lastAllMenuTimeStamp');

        let obj = {
            user: localStorage.getItem('_user'),
            company: localStorage.getItem('_company'),
            authtoken: sessionStorage.getItem('authtoken'),
            storageMenuDate: storageMenuDate,
        }

        let url = this.infoService.getMenuServiceUrl() + 'isCachedMenuTooOld/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    getMenuElements(clearCachedData: boolean): Observable<any> {
        let storageMenuDate = (clearCachedData || !localStorage.getItem('_lastAllMenuTimeStamp'))
            ? new Date().getTime()
            : localStorage.getItem('_lastAllMenuTimeStamp');

        let obj = {
            user: localStorage.getItem('_user'),
            company: localStorage.getItem('_company'),
            storageMenuDate: storageMenuDate,
            authtoken: sessionStorage.getItem('authtoken'),
            clearCachedData: clearCachedData
        }

        let timeStamp = new Date().getTime().toString();
        localStorage.setItem('_lastAllMenuTimeStamp', timeStamp);

        let url = this.infoService.getMenuServiceUrl() + 'getMenuElements/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res.json();
            })
            .catch(this.handleError);
    }

    //#region easystudio su webserver.sln
    getEsAppsAndModules(type: string): Observable<any> {
        let obj = { user: localStorage.getItem('_user'), applicationType: type };
        let url = this.callInfoService + 'application/getAllAppsAndModules/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    createNewContext(app: string, mod: string, type: string): Observable<any> {
        let obj = { user: localStorage.getItem('_user'), applicationName: app, moduleName: mod, applicationType: type };
        let url = this.callInfoService + 'application/create/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    checkAfterRefresh(type: string): Observable<any> { //aggiornamento file preferences se necessario
        let obj = { user: localStorage.getItem('_user'), applicationType: type };
        let url = this.callInfoService + 'checkAfterRefresh/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    cleanApplicationInfosPathFinder(): Observable<any> { //aggiornamento file preferences se necessario
        let obj = { user: localStorage.getItem('_user') };
        let url = this.callInfoService + 'application/refreshAll/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    initEasyStudioData(object): Observable<any> {
        var ns = object.target;
        ns = 'document' + "." + ns;
        let obj = { user: localStorage.getItem('_user'), ns: encodeURIComponent(ns) };
        var url = this.callInfoService + 'application/getEasyStudioCustomizationsListFor/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            }).catch(this.handleError);
    }


    setAppAndModule(app: string, mod: string, isThisPairDefault: boolean): Observable<any> {
        let obj = { user: localStorage.getItem('_user'), applicationName: app, moduleName: mod, defaultPair: isThisPairDefault };
        let url = this.callInfoService + 'setCurrentContextFor/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    isEasyStudioDocument(object): Observable<any> {
        if (object.isEasyStudioDocument != undefined)
            return object.isEasyStudioDocument;

        let obj = { user: localStorage.getItem('_user'), ns: encodeURIComponent(object.target) };
        let url = this.callInfoService + 'isEasyStudioDocument/';
        return this.httpService.postData(url, obj)
            .map((data: any) => {
                if (data && data.message && data.message.text) {
                    object.isEasyStudioDocument = data.message.text == "true";
                    return object.isEasyStudioDocument;
                }
            })
            .catch(this.handleError);
    }

    getDefaultContext(): Observable<any> {
        let obj = { user: localStorage.getItem('_user'), defaultReq: true };
        let url = this.callInfoService + 'getCurrentContextFor/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }
    getCurrentContext(): Observable<any> {
        let obj = { user: localStorage.getItem('_user'), defaultReq: false };
        let url = this.callInfoService + 'getCurrentContextFor/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }
    //#endregion 

    //#region easystudio su c++

    cloneAsEasyStudioDocument(object: any, docName: string, docTitle: string, esServices: EasystudioService): Observable<any> {
        var ns = object.target;
        let obj = { user: localStorage.getItem('_user') };
        var newNs = esServices.currentApplication + "." + esServices.currentModule + ".DynamicDocuments." + docName;
        var objType = object.objectType.toLowerCase();
        var urlToRun = this.infoService.getDocumentBaseUrl() + 'cloneEasyStudioDocument/?ns=' + encodeURIComponent(ns);
        urlToRun += "&newNamespace=" + encodeURIComponent(newNs);
        urlToRun += "&newTitle=" + encodeURIComponent(docTitle);
        return this.httpService.postData(urlToRun, obj)
            .map((res: any) => {
                return res;
            }).catch(this.handleError);
    }

    canModifyContext(): Observable<any> {
        let obj = { user: localStorage.getItem('_user') };
        let url = this.infoService.getDocumentBaseUrl() + 'canModifyContext/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    updateBaseCustomizationContext(app: string, mod: string): Observable<any> {
        let obj = { user: localStorage.getItem('_user'), applicationName: app, moduleName: mod };
        let url = this.infoService.getDocumentBaseUrl() + 'updateBaseCustomizationContext/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    runEasyStudio(ns: string, customizationName: string): Observable<any> {
        let obj = { user: localStorage.getItem('_user'), ns: encodeURIComponent(ns), customization: encodeURIComponent(customizationName) };
        let url = this.infoService.getDocumentBaseUrl() + 'runEasyStudio/?ns=' + encodeURIComponent(ns);
        if (customizationName != undefined)
            url += "&customization=" + encodeURIComponent(customizationName);
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    closeCustomizationContext(): Observable<any> {
        let obj = { user: localStorage.getItem('_user') };
        let url = this.infoService.getDocumentBaseUrl() + 'closeCustomizationContext/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    //#endregion 

    updateCachedDateAndSave(): Observable<any> {
        let obj = { user: sessionStorage.getItem('authtoken') };
        let url = this.infoService.getMenuServiceUrl() + 'updateCachedDateAndSave/';
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res;
            })
            .catch(this.handleError);
    }

    getConnectionInfo(): Observable<any> {

        let obj = { authtoken: sessionStorage.getItem('authtoken') };
        var url = this.infoService.getMenuServiceUrl() + 'getConnectionInfo/';
        return this.httpService.postData(url, obj)
            .map((res: Response) => {
                return res.json();
            });
    }

    getApplicationDate(): Observable<any> {

        let obj = { authtoken: sessionStorage.getItem('authtoken') };
        var url = this.infoService.getDocumentBaseUrl() + 'getApplicationDate/';
        return this.httpService.postData(url, obj)
            .map((res: Response) => {
                return res.json();
            });
    }

    changeApplicationDate(date: Date): Observable<OperationResult> {

        let day = date.getDate();
        let month = date.getMonth() + 1;
        let year = date.getFullYear();
        let obj = { authtoken: sessionStorage.getItem('authtoken') };
        var url = this.infoService.getDocumentBaseUrl() + 'changeApplicationDate/?day=' + day + '&month=' + month + '&year=' + year;
        return this.httpService.postData(url, obj)
            .map((res: Response) => {
                return this.httpService.createOperationResult(res);
            });
    }

    mostUsedClearAll(): Observable<any> {
        let obj = { user: localStorage.getItem('_user'), company: localStorage.getItem('_company') }
        let url = this.infoService.getMenuServiceUrl() + 'clearAllMostUsed/';
        return this.httpService.postData(url, obj)
            .map((res: Response) => {
                return res.ok;
            });

    };

    getMostUsedShowNr(callback) {
        let obj = { user: localStorage.getItem('_user'), company: localStorage.getItem('_company') }
        let url = this.infoService.getMenuServiceUrl() + 'getMostUsedShowNr/';
        return this.httpService.postData(url, obj)

            .map((res: Response) => {
                return res.json();
            });
    }

    updateMostUsed(mostUsed: any): Observable<boolean> {
        let obj = {
            user: localStorage.getItem('_user'), company: localStorage.getItem('_company'),
            mostUsed: JSON.stringify(mostUsed)
        };
        var url = this.infoService.getMenuServiceUrl() + 'updateMostUsed/';
        return this.httpService.postData(url, obj)
            .map((res: Response) => {
                return res.ok;
            });
    }

    updateFavorites(favorites: any): Observable<boolean> {
        let obj = {
            user: localStorage.getItem('_user'), company: localStorage.getItem('_company'),
            favorites: JSON.stringify(favorites)
        };
        var url = this.infoService.getMenuServiceUrl() + 'updateFavorites/';
        return this.httpService.postData(url, obj)
            .map((res: Response) => {
                return res.ok;
            });
    }

    activateViaSMS(): Observable<any> {
        return this.http.get(this.infoService.getMenuServiceUrl() + 'getPingViaSMSUrl/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            });
    }

    goToSite(): Observable<any> {

        return this.http.get(this.infoService.getMenuServiceUrl() + 'getProducerSite/', { withCredentials: true })
            .map((res: Response) => {
                return res.json();
            });
    }

    callonlineHelpUrl(ns: string, culture: string): Observable<any> {
        let obj = { nameSpace: ns, culture: culture }
        let url = this.infoService.isDesktop ? this.infoService.getDocumentBaseUrl() : this.infoService.getMenuServiceUrl();
        url = url + 'getOnlineHelpUrl/';
        return this.httpService.postData(url, obj)
            .map((res: Response) => {
                return res.json();
            });
    }

    addToHiddenTiles(application: string, group: string, menu: string, tile: string): Observable<OperationResult> {

        let obj = {
            user: localStorage.getItem('_user'),
            company: localStorage.getItem('_company'),
            authtoken: sessionStorage.getItem('authtoken'),
            application: application,
            group: group,
            menu: menu,
            tile: tile
        };
        var url = this.infoService.getMenuServiceUrl() + 'addToHiddenTiles/'
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res.ok;
            })
            .catch(this.handleError);
    }

    removeFromHiddenTiles(application: string, group: string, menu: string, tile: string): Observable<OperationResult> {

        let obj = {
            user: localStorage.getItem('_user'),
            company: localStorage.getItem('_company'),
            authtoken: sessionStorage.getItem('authtoken'),
            application: application,
            group: group,
            menu: menu,
            tile: tile
        };
        var url = this.infoService.getMenuServiceUrl() + 'removeFromHiddenTiles/'
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res.ok;
            })
            .catch(this.handleError);
    }

    removeAllHiddenTiles(): Observable<OperationResult> {

        let obj = {
            user: localStorage.getItem('_user'),
            company: localStorage.getItem('_company'),
            authtoken: sessionStorage.getItem('authtoken'),
        };
        var url = this.infoService.getMenuServiceUrl() + 'removeAllHiddenTiles/'
        return this.httpService.postData(url, obj)
            .map((res: any) => {
                return res.ok;
            })
            .catch(this.handleError);
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