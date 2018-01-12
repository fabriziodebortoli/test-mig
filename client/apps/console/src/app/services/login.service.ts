import { Injectable } from '@angular/core';
import {Response, Http,  RequestOptions,  Headers} from '@angular/http';
import { Router } from "@angular/router";
import { Observable, Subject } from "rxjs";
import { environment } from './../../environments/environment';
import { AuthorizationInfo, AuthorizationProperties } from "app/authentication/auth-info";
import { Credentials , ChangePasswordInfo} from './../authentication/credentials';
import { OperationResult } from './operationResult';
import { RoleNames, RoleLevels } from './../authentication/auth-helpers';
import { UrlGuard } from "app/authentication/url-guard";
import { AccountInfo } from '../authentication/account-info';

@Injectable()
//================================================================================
export class LoginService {

  private tokenAPIUrl: string;
  private instancesListAPIUrl: string;
  private accountName: string;
  private loginOperationCompleted = new Subject<OperationResult>();
  private opRes:OperationResult = new OperationResult();

  //--------------------------------------------------------------------------------
  constructor(private http: Http,  private router: Router) { 

    this.tokenAPIUrl = environment.adminAPIUrl + "tokens" + "/"; // InstanceKey must be set from outside
    this.instancesListAPIUrl = environment.adminAPIUrl + "listInstances" + "/";
  }

  //--------------------------------------------------------------------------------
  sendMessage(operationResult: OperationResult) {
    this.loginOperationCompleted.next(operationResult);
  }

  //--------------------------------------------------------------------------------
  getMessage(): Observable<any> {
    return this.loginOperationCompleted.asObservable();
  }

  //--------------------------------------------------------------------------------
  getInstances(accountName: string): Observable<OperationResult> {

    // called by pre-login process to load instances for account

    let bodyString  = JSON.stringify(accountName);
    let headers = new Headers({ 'Content-Type': 'application/json' });
    let options = new RequestOptions({ headers: headers });

    return this.http.post(this.instancesListAPIUrl, bodyString, options)
    .map((res : Response) => {
      return res.json();
      }, 
      err => { 
        return err; } 
    )
    .catch((error: any) => Observable.throw(error.json().error || 'server error'));    
  }

  //--------------------------------------------------------------------------------
  login(body:Object, returnUrl: string, instance: string) {

    let bodyString  = JSON.stringify(body);
    let headers = new Headers({ 'Content-Type': 'application/json' });
    let options = new RequestOptions({ headers: headers });
    
    this.http.post(this.tokenAPIUrl + instance, bodyString, options)
      .map(res => res.json())
      .catch((error:any) => Observable.throw(error.json().error || 'Error while posting login to the server'))
      .subscribe(
        data =>
        {
          if (!data.Result)
          {
            this.opRes.Result = false;
            this.opRes.Message = 'Cannot do the login. ' + data.Message;
            this.opRes.Code = data.ResultCode;
            this.sendMessage(this.opRes);
            return;
          }

          if (data.JwtToken == '')
          {
            this.opRes.Result = false;
            this.opRes.Message = 'Empty token';
            this.opRes.Code = data.ResultCode;
            this.sendMessage(this.opRes);
            return;
          }

          let authInfo: AuthorizationInfo;

          try
          {
            authInfo = this.GetAuthorizationsFromJWT(data.JwtToken);

            if (authInfo == null) {
              this.router.navigateByUrl('/');
              this.opRes.Result = false;
              this.opRes.Message = 'Could not get valid AuthorizationInfo from jwt token, cannot proceed.';
              this.sendMessage(this.opRes);
              return;
            }

            // save in localstorage all the information about authorization
            localStorage.setItem('auth-info', JSON.stringify(authInfo.authorizationProperties));
            
            // save in localstorage all the information about account choices
            let accountInfo: AccountInfo = new AccountInfo(instance);
            localStorage.setItem(authInfo.authorizationProperties.accountName, JSON.stringify(accountInfo));

            // sending message to subscribers to notify login
            this.opRes.Result = true;
            this.opRes.Message = authInfo.authorizationProperties.accountName;

            // checking cloud-admin only urls
            let opRes: OperationResult = UrlGuard.CanNavigate(returnUrl, authInfo);

            if (!opRes.Result && returnUrl == '/') {
              // this is the case where user clicked the "sign in" link
              this.router.navigateByUrl('/');
              this.opRes.Result = true;
              this.opRes.Message = authInfo.authorizationProperties.accountName;
              this.sendMessage(opRes);
              return;              
            }

            if (!opRes.Result) {
              this.opRes.Result = false;
              this.opRes.Message = '';
              this.sendMessage(opRes);
              this.router.navigateByUrl('/');
              return;
            }

            this.router.navigateByUrl(returnUrl);
          }
          catch (exc)
          {
            localStorage.removeItem('auth-info');
            this.opRes.Result = false;
            this.opRes.Message = 'Error while processing authorization info from jwt token ' + exc + '. Login failed.';
            this.sendMessage(this.opRes);
            if (authInfo != null)
              localStorage.removeItem(authInfo.authorizationProperties.accountName);
            return;
          }   
        },
        error => { 
          this.opRes.Result = false;
          this.opRes.Message = 'An error occurred while executing login service: ' + error;
          this.sendMessage(this.opRes);
        }
      );
  }

  //--------------------------------------------------------------------------------
  GetAuthorizationsFromJWT(unparsedToken: string): AuthorizationInfo {
    // decoding jwt token
    let tokenParts: Array<string> = unparsedToken.split('.');

    if (tokenParts.length != 3)
    {
      return null;
    }  

    let decodedToken: string = atob(tokenParts[1]);
    let parsedToken = JSON.parse(decodedToken);

    let authInfo: AuthorizationInfo = new AuthorizationInfo(
      unparsedToken,
      parsedToken.AccountName);

    authInfo.SetInstances(parsedToken.Instances);
    authInfo.SetSubscriptions(parsedToken.Subscriptions);
    authInfo.SetServerUrls(parsedToken.Urls);
    authInfo.SetTokens(parsedToken.UserTokens);
    authInfo.SetSecurityValues(parsedToken.AppSecurity);
    authInfo.SetRoles(parsedToken.Roles);      

    return authInfo;
  }

  //--------------------------------------------------------------------------------
  logout() {
    
    if (localStorage.length == 0) {
      return;
    }

    // removing token informations
    localStorage.removeItem('auth-info');

    // sending message to subscribers to notify login
    this.opRes.Result = true;
    this.opRes.Message = '';
    this.sendMessage(this.opRes);

    // routing to the app home
    this.router.navigateByUrl('/appHome', { skipLocationChange:true });
  }
  
  
  // change account password
  //--------------------------------------------------------------------------------------------------------
  changePassword(body: ChangePasswordInfo): Observable<OperationResult> {

    let headers = new Headers({ 'Content-Type': 'application/json' });
    let options = new RequestOptions({ headers: headers });
    let bodyString = JSON.stringify(body);

    return this.http.post(environment.gwamAPIUrl + 'password', bodyString, options)
      .map((res: Response) => {
        return res.json();
      })
      .catch((error: any) => Observable.throw(error.json().error || 'server error (changePassword)'));
  }
}