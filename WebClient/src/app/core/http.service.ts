import { DocumentInfo } from './../shared';
import { UtilsService } from './utils.service';
import { LoginSession } from './../shared/';
import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs';
import { Logger } from './logger.service';
import { CookieService } from 'angular2-cookie/services/cookies.service';

@Injectable()
export class HttpService {

    private baseUrl = 'http://localhost:10000/';

    constructor(private http: Http,
        private utils: UtilsService,
        private logger: Logger,
        private cookieService: CookieService) {
        console.log('HttpService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }

    isLogged(): Observable<string> {
        return this.postData(this.getMenuBaseUrl() + 'isLogged/', {})
            .map((res: Response) => {
                return res.ok && res.json().success === true;
            })
            .catch(this.handleError);
    }

    login(connectionData: LoginSession): Observable<boolean> {
        return this.postData(this.getMenuBaseUrl() + 'doLogin/', connectionData)
            .map((res: Response) => {
                 return res.ok && res.json().success === true;
            })
            .catch(this.handleError);
    }

    logout(): Observable<boolean> {
        let token = this.cookieService.get('authtoken');
        this.logger.debug('httpService.logout (' + token + ')');
        return this.postData(this.getMenuBaseUrl() + 'doLogoff/', token)
            .map((res: Response) => {
                return res.ok && res.json().success === true;
            })
            .catch(this.handleError);
    }


    getWebSocketPort(): Observable<string> {
        return this.http.get(this.getMenuBaseUrl() + 'getWebSocketsPort/')
            .map((res: Response) => res.json())
            .catch(this.handleError);
    }

    runObject(documentData: DocumentInfo): Observable<boolean> {
        return this.postData(this.getMenuBaseUrl() + 'runObject/', documentData)
            .map((res: Response) => {
                return res.ok && res.json().success === true;
            })
            .catch(this.handleError);
    }

    doCommand(cmpId: String, id: String) {
        let resOuter: Observable<Response> = this.postData(this.getDocumentBaseUrl() + 'command/', { cmpId: cmpId, id: id })
            .map((res: Response) => {
                return res.ok && res.json().success === true;
            })
            .catch(this.handleError);

        resOuter.subscribe(result => {
            console.log(result);
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


    private handleError(error: any) {
        // In a real world app, we might use a remote logging infrastructure
        // We'd also dig deeper into the error to get a better message
        let errMsg = (error.message) ? error.message :
            error.status ? `${error.status} - ${error.statusText}` : 'Server error';
        console.error(errMsg);
        return Observable.throw(errMsg);
    }

    getMenuElements(): Observable<any> {
              return this.http.get(this.getMenuBaseUrl() + this.getNeedLoginThread() + 'getMenuElements/', { withCredentials: true })
            .map((res: Response) => {
                return  res.json();
            })
            .catch(this.handleError);
    }
}
