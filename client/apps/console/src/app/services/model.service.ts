import { DatabaseCredentials, ExtendedSubscriptionDatabase } from './../authentication/credentials';
import { AuthorizationProperties } from './../authentication/auth-info';
import { SubscriptionDatabase } from './../model/subscriptionDatabase';
import { Account } from '../model/account';
import { environment } from './../../environments/environment';
import { Injectable } from '@angular/core';
import { Http, RequestOptions, Headers, Response } from '@angular/http';
import 'rxjs/Rx';
import { Observable } from "rxjs/Observable";
import { OperationResult } from './operationResult';
import { AccountInfo } from '../authentication/account-info';
import { MessageData } from './messageData';

@Injectable()
export class ModelService {

  // this is the current accountName read from authorizationProperties in local storage
  currentAccountName: string = '';

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

      this.currentAccountName = authorizationProperties.accountName;

      let authorizationHeader: string;

      if (authorizationType.toLowerCase() === 'jwt') {
        authorizationHeader = '{ "Type": "Jwt", "SecurityValue": "' + authorizationProperties.jwtEncoded + '"}';
      }

      if (authorizationType.toLowerCase() === 'app') {
        authorizationHeader = '{ "Type": "App", ' +
          '"AppId": "' + authorizationProperties.AppSecurityInfo.AppId +
          '", "SecurityValue": "' + authorizationProperties.AppSecurityInfo.SecurityValue + '"}';
      }

      return authorizationHeader;
    }

    return '';
  }

  //--------------------------------------------------------------------------------------------------------
  saveAccount(body: Object): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('app');

    if (authorizationHeader !== '') {
      let bodyString = JSON.stringify(body);
      let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
      let options = new RequestOptions({ headers: headers });

      return this.http.put(environment.gwamAPIUrl + 'accounts', bodyString, options)
        .map((res: Response) => {
          return res.json();
        })
        .catch((error: any) => Observable.throw(error.json().error || 'server error (saveAccount)'));
    }

    return Observable.throw('AuthorizationHeader is missing!');
  }

  //--------------------------------------------------------------------------------------------------------
  getAccounts(body): Observable<Account[]> {

    let authorizationHeader = this.createAuthorizationHeader('app');

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    let bodyString = JSON.stringify(body);
    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });

    return this.http.post(environment.gwamAPIUrl + 'query/accounts', bodyString, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (getAccounts)'));
  }

  //--------------------------------------------------------------------------------------------------------
  saveSubscription(body: Object): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('app');

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    let bodyString = JSON.stringify(body);
    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });

    return this.http.post(environment.gwamAPIUrl + 'subscriptions', body, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (saveSubscription)'));
  }

  //--------------------------------------------------------------------------------------------------------
  getSubscriptions(accountName: string, instanceKey: string, subscriptionKey?: string): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('app');

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    let urlSubscriptionSegment: string = 'subscriptions';

    urlSubscriptionSegment += "/" + accountName + "/" + instanceKey;

    if (subscriptionKey !== undefined) {
      urlSubscriptionSegment += "/" + subscriptionKey;
    }

    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });

    return this.http.get(environment.gwamAPIUrl + urlSubscriptionSegment, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (getSubscriptions)'));
  }

  //--------------------------------------------------------------------------------------------------------
  registerInstance(body: Object, activationKey): Observable<OperationResult> {
    
    if (activationKey === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    let bodyString = JSON.stringify(body);
    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': activationKey });
    let options = new RequestOptions({ headers: headers });

    return this.http.put(environment.gwamAPIUrl + 'instances', body, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (saveInstance)'));
  }

  //--------------------------------------------------------------------------------------------------------
  setData(body: Object, goGWAM: boolean, activationCode: string, rowId: string): Observable<OperationResult> {
    
    let authorizationHeader = this.createAuthorizationHeader('app');

    if (authorizationHeader === '' && activationCode === undefined) {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    if (authorizationHeader === '') {
      authorizationHeader = activationCode;
    }    

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    let bodyString = JSON.stringify(body);
    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });

    let baseUrl = goGWAM ? environment.gwamAPIUrl : environment.adminAPIUrl; 

    return this.http.post(baseUrl + 'setdata/instances/' + rowId + '/activated' + '/1' , {}, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (saveInstance)'));
  }  

  //--------------------------------------------------------------------------------------------------------
  saveInstance(body: Object, goGWAM: boolean, activationCode: string): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('app');

    if (authorizationHeader === '' && activationCode === undefined) {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    if (authorizationHeader === '') {
      authorizationHeader = activationCode;
    }    

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    let bodyString = JSON.stringify(body);
    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });

    let baseUrl = goGWAM ? environment.gwamAPIUrl : environment.adminAPIUrl; 

    return this.http.post(baseUrl + 'instances', body, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (saveInstance)'));
  }

  //--------------------------------------------------------------------------------------------------------
  getInstances(body: string = '', activationCode?: string): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('app');
    
    if (authorizationHeader === '' && activationCode === undefined) {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    if (authorizationHeader === '') {
      authorizationHeader = activationCode;
    }    

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    // if body is not empty I add the instancekey

    let urlInstanceSegment: string = 'instances';
    
    if (body !== '') {
      urlInstanceSegment += "/" + body;
    }

    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });

    return this.http.get(environment.gwamAPIUrl + urlInstanceSegment, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (getInstances)'));
  }

  //--------------------------------------------------------------------------------------------------------
  query(modelName: string, body: Object, activationCode?: string): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('app');

    if (authorizationHeader === '' && activationCode === undefined) {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    if (authorizationHeader === '') {
      authorizationHeader = activationCode;
    }

    if (modelName === '') {
      return Observable.throw('The model name to query is missing!');
    }

    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });

    return this.http.post(environment.gwamAPIUrl + 'query/' + modelName, body, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (query)'));
  }

  //--------------------------------------------------------------------------------------------------------
  queryDelete(modelName: string, body: Object): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('app');

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    if (modelName === '') {
      return Observable.throw('The model name to query is missing!');
    }

    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers, body: body });

    return this.http.delete(environment.gwamAPIUrl + 'query/' + modelName, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (queryDelete)'));
  }

  //--------------------------------------------------------------------------------------------------------
  addAccountSubscriptionAssociation(accountName: string, subscriptionList: string[]): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('app');

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });
    return this.http.post(environment.gwamAPIUrl + 'accountSubscriptions/' + accountName, subscriptionList, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (addAccountSubscriptionAssociation)'));
  }

  //--------------------------------------------------------------------------------------------------------
  addInstanceSubscriptionAssociation(instanceKey: string, subscriptionKey: string): Observable<OperationResult> {
    
    let authorizationHeader = "code";

    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });
    return this.http.post(environment.gwamAPIUrl + 'instanceSubscriptions/' + subscriptionKey + '/' + instanceKey, {}, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (addAccountSubscriptionAssociation)'));
  }  

  // returns all databases for the couple instanceKey + subscriptionKey
  //--------------------------------------------------------------------------------------------------------
  getDatabases(subscriptionKey: string): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('jwt');

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    if (this.currentAccountName === '') {
      return Observable.throw('AccountName is missing!');
    }

    // I need the instanceKey where the currentAccount is logged
    let localAccountInfo = localStorage.getItem(this.currentAccountName);
    let instancekey: string = '';

    if (localAccountInfo != null && localAccountInfo != '') {
      let accountInfo: AccountInfo = JSON.parse(localAccountInfo);
      instancekey = accountInfo.instanceKey;
    }

    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });

    return this.http.get(environment.adminAPIUrl + 'databases/' + instancekey + '/' + subscriptionKey, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (getDatabases)'));
  }

  // returns a specific database by instanceKey + subscriptionKey + name
  //--------------------------------------------------------------------------------------------------------
  getDatabase(subscriptionKey: string, name: string): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('jwt');

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    if (this.currentAccountName === '') {
      return Observable.throw('AccountName is missing!');
    }

    // I need the instanceKey where the currentAccount is logged
    let localAccountInfo = localStorage.getItem(this.currentAccountName);
    let instancekey: string = '';

    if (localAccountInfo != null && localAccountInfo != '') {
      let accountInfo: AccountInfo = JSON.parse(localAccountInfo);
      instancekey = accountInfo.instanceKey;
    }

    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });

    return this.http.get(environment.adminAPIUrl + 'databases/' + instancekey + '/' + subscriptionKey + '/' + name, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (getDatabase)'));
  }

  //--------------------------------------------------------------------------------------------------------
  saveDatabase(body: Object): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('jwt');

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });

    return this.http.post(environment.adminAPIUrl + 'databases', body, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (saveDatabase)'));
  }

  // 
  //--------------------------------------------------------------------------------------------------------
  quickConfigureDatabase(subscriptionKey: string): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('jwt');

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    // I need the instanceKey where the currentAccount is logged
    let localAccountInfo = localStorage.getItem(this.currentAccountName);
    let instancekey: string = '';

    if (localAccountInfo != null && localAccountInfo != '') {
      let accountInfo: AccountInfo = JSON.parse(localAccountInfo);
      instancekey = accountInfo.instanceKey;
    }

    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });

    return this.http.post(environment.adminAPIUrl + 'database/quickcreate/' + instancekey + '/' + subscriptionKey, '', options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (quickConfigureDatabase)'));
  }

  // test connection with given credentials
  //--------------------------------------------------------------------------------------------------------
  testConnection(subscriptionKey: string, body: DatabaseCredentials): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('jwt');

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    let bodyString = JSON.stringify(body);
    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });

    return this.http.post(environment.adminAPIUrl + 'database/testconnection/' + subscriptionKey, bodyString, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (testConnection)'));
  }

  // Update subscription database
  //--------------------------------------------------------------------------------------------------------
  updateDatabase(subscriptionKey: string, body: ExtendedSubscriptionDatabase): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('jwt');

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    let bodyString = JSON.stringify(body);
    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });

    return this.http.post(environment.adminAPIUrl + 'database/update/' + subscriptionKey, bodyString, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (updateDatabase)'));
  }

  // Subscription database preliminary check
  //--------------------------------------------------------------------------------------------------------
  checkDatabase(subscriptionKey: string, body: ExtendedSubscriptionDatabase): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('jwt');

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    let bodyString = JSON.stringify(body);
    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });

    return this.http.post(environment.adminAPIUrl + 'database/check/' + subscriptionKey, bodyString, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (checkDatabase)'));
  }

  // send a message via email
  //--------------------------------------------------------------------------------------------------------
  sendMessage(body: MessageData): Observable<OperationResult> {

    let authorizationHeader = this.createAuthorizationHeader('jwt');

    if (authorizationHeader === '') {
      return Observable.throw('AuthorizationHeader is missing!');
    }

    // I need the instanceKey where the currentAccount is logged
    let localAccountInfo = localStorage.getItem(this.currentAccountName);
    let instancekey: string = '';

    if (localAccountInfo != null && localAccountInfo != '') {
      let accountInfo: AccountInfo = JSON.parse(localAccountInfo);
      instancekey = accountInfo.instanceKey;
    }

    let bodyString = JSON.stringify(body);
    let headers = new Headers({ 'Content-Type': 'application/json', 'Authorization': authorizationHeader });
    let options = new RequestOptions({ headers: headers });

    return this.http.post(environment.adminAPIUrl + 'messages/' + instancekey, bodyString, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (sendMessage)'));
  }
}
