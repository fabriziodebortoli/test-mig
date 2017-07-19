import { AuthorizationProperties } from './../authentication/auth-info';
import { SubscriptionDatabase } from './../model/subscriptionDatabase';
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

  // returns the complete AuthorizationHeader with token read from localStorage
  //--------------------------------------------------------------------------------------------------------
  createAuthorizationHeader(authorizationType: string): string {
    let authorizationStored = localStorage.getItem('auth-info');

    if (authorizationStored !== null) {
      let authorizationProperties: AuthorizationProperties = JSON.parse(authorizationStored);

      let authorizationHeader: string;

      if (authorizationType.toLowerCase() === 'jwt') {
        authorizationHeader = '{ "Type": "Jwt", "SecurityValue": "' + authorizationProperties.jwtEncoded + '"}';
      }

      if (authorizationType.toLowerCase() === 'app') {
        authorizationHeader = '{ "Type": "App", ' + 
                              '"AppId": "' + authorizationProperties.appId + 
                              '", "SecurityValue": "' + authorizationProperties.appSecurityValue + '"}';
      }
      
      return authorizationHeader;
    }

    return '';
  }

  //--------------------------------------------------------------------------------------------------------
  addAccount(body: Object): Observable<OperationResult> {
    let authorizationHeader = this.createAuthorizationHeader('app');

    if (authorizationHeader !== '') {
      let bodyString = JSON.stringify(body);
      let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
      let options = new RequestOptions({ headers: headers });

      return this.http.put(environment.gwamAPIUrl + 'accounts', bodyString, options)
        .map((res: Response) => {
          console.log(res.json());
          return res.json();
        })
        .catch((error: any) => Observable.throw(error.json().error || 'server error'));
    }

    return Observable.throw('AuthorizationHeader is missing!');
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

  //--------------------------------------------------------------------------------------------------------
  addSubscription(body: Object): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('app');

    if (authorizationHeader !== '') {
      let bodyString = JSON.stringify(body);
      let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
      let options = new RequestOptions({ headers: headers });

      return this.http.post(environment.gwamAPIUrl + 'subscriptions', body, options)
        .map((res: Response) => {
          console.log(res.json());
          return res.json();
        })
      .catch((error: any) => Observable.throw(error.json().error || 'server error'));
    }

    return Observable.throw('AuthorizationHeader is missing!');
  }
}
