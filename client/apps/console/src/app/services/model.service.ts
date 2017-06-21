import { Company } from './../model/company';
import { Account } from '../model/account';
import { environment } from './../../environments/environment';
import { Injectable } from '@angular/core';
import { Http, RequestOptions, Headers, Response } from '@angular/http';
import 'rxjs/add/operator/map';
import { Observable } from "rxjs/Observable";
import { OperationResult } from './operationResult';

@Injectable()
export class ModelService {

  //--------------------------------------------------------------------------------------------------------
  constructor(private http: Http) {
    this.http = http;
  }

  //--------------------------------------------------------------------------------------------------------
  addAccount(body: Object): Observable<OperationResult> {
    let headers = new Headers({ 'Content-Type': 'application/json', 'Access-Control-Allow-Origin': true });
    let options = new RequestOptions({ headers: headers });

    /*return this.http.put(environment.gwamAPIUrl + 'accounts', body, options)
        .map((res: Response) => {
        console.log(res.json());
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error'));*/
    
    return this.http.post(environment.adminAPIUrl + 'accounts', body, options)
      .map((res: Response) => {
        console.log(res.json());
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error'));
    //.catch((error:OperationResult)=>Observable.throw(error.Message)); // prova
  }

  //--------------------------------------------------------------------------------------------------------
  getAccounts(): Observable<Account[]> {
    let headers = new Headers({ 'Content-Type': 'application/json' });
    let options = new RequestOptions({ headers: headers });

    return this.http.get(environment.adminAPIUrl + 'accounts', options)
      .map((res: Response) => {
        console.log(res.json());
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error'));
  }

  //--------------------------------------------------------------------------------------------------------
  addCompany(body: Object): Observable<OperationResult> {
    let headers = new Headers({ 'Content-Type': 'application/json' });
    let options = new RequestOptions({ headers: headers });

    return this.http.post(environment.adminAPIUrl + 'companies', body, options)
      .map((res: Response) => {
        console.log(res.json());
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error'));
  }
}
