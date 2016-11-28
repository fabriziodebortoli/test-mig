import { OperationResult } from './operation.result';
import { DocumentInfo, LoginSession } from 'tb-shared';
import { UtilsService } from './utils.service';
import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs';
import { Logger } from 'libclient';
import { CookieService } from 'angular2-cookie/services/cookies.service';

@Injectable()
export class HttpService {
    private useGate = false;
    private gatePort = 5000;
    private baseUrl = this.useGate
        ? 'http://localhost:' + this.gatePort + '/tbloader/api/'
        : 'http://localhost:10000/';

    constructor(
        protected http: Http,
        protected utils: UtilsService,
        protected logger: Logger,
        protected cookieService: CookieService) {
        console.log('HttpService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }
    createOperationResult(res: Response): OperationResult {
        let jObject = res.ok ? res.json() : null;
        let ok = jObject && jObject.success === true;
        let messages = jObject ? jObject.messages : [];
        return new OperationResult(!ok, messages);
    }
    isLogged(): Observable<string> {
        return this.postData(this.getMenuBaseUrl() + 'isLogged/', {})
            .map((res: Response) => {
                return res.ok && res.json().success === true;
            })
            .catch(this.handleError);
    }

    login(connectionData: LoginSession): Observable<OperationResult> {
        return this.postData(this.getMenuBaseUrl() + 'doLogin/', connectionData)
            .map((res: Response) => {
                return this.createOperationResult(res);
            })
            .catch(this.handleError);
    }

    logout(): Observable<OperationResult> {
        let token = this.cookieService.get('authtoken');
        this.logger.debug('httpService.logout (' + token + ')');
        return this.postData(this.getMenuBaseUrl() + 'doLogoff/', token)
            .map((res: Response) => {
                return this.createOperationResult(res);
            })
            .catch(this.handleError);
    }


    getWebSocketPort(): Observable<string> {
        if (this.useGate){
            return new Observable<string>(subs => {
                subs(this.gatePort);
            });
        }
        return this.http.get(this.getMenuBaseUrl() + 'getWebSocketsPort/')
            .map((res: Response) => res.json())
            .catch(this.handleError);
    }

    doCommand(cmpId: String, id: String): void {
        let subs = this.postData(this.getDocumentBaseUrl() + 'command/', { cmpId: cmpId, id: id })
            .map((res: Response) => {
                return res.ok && res.json().success === true;
            })
            .catch(this.handleError)
            .subscribe(result => {
                console.log(result);
                subs.unsubscribe();
            });
    }

    getLoginActiveThreads() {
        /*return new Promise(function (resolve, reject) {
         me.http.get(me.getDocumentBaseUrl() + "getLoginActiveThreads/")
         .subscribe(response => {
         if (response.ok) {
         resolve(response.text());
         }
         else
         reject(response.toString());
         });
         });*/
    }

    postData(url: string, data: Object): Observable<Response> {
        return this.http.post(url, this.utils.serializeData(data), { withCredentials: true });
    }

    getComponentUrl(url: string) {
        if (url[0] === '\\') {
            url = url.substring(1);
        }
        return 'app/htmlforms/' + url + '.js';
    }

    getDocumentBaseUrl() {
        return this.baseUrl + 'tb/document/';
    }

    getMenuBaseUrl() {
        return this.baseUrl + 'tb/menu/';
    }

    getNeedLoginThread() {
        return 'needLoginThread/';
    }

    public runObject(documentData: DocumentInfo): void {
        let subs = this.postData(this.getMenuBaseUrl() + 'runObject/', documentData)
            .map((res: Response) => {
                return res.ok && res.json().success === true;
            })
            .catch(this.handleError)
            .subscribe(result => {
                console.log(result);
                subs.unsubscribe();
            });
    }



    protected handleError(error: any) {
        // In a real world app, we might use a remote logging infrastructure
        // We'd also dig deeper into the error to get a better message
        let errMsg = (error.message) ? error.message :
            error.status ? `${error.status} - ${error.statusText}` : 'Server error';
        console.error(errMsg);
        return Observable.throw(errMsg);
    }
}
