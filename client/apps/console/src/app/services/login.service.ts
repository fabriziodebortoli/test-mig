import { Injectable } from '@angular/core';
import {Response, Http,  RequestOptions,  Headers} from '@angular/http';
import { Router } from "@angular/router";
import { Observable, Subject } from "rxjs";
import { environment } from './../../environments/environment';
import { AuthorizationInfo, AuthorizationProperties } from "app/authentication/auth-info";
import { Credentials } from './../authentication/credentials';
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
  private loginOperationCompleted = new Subject<string>();

  //--------------------------------------------------------------------------------
  constructor(private http: Http,  private router: Router) { 

    this.tokenAPIUrl = environment.adminAPIUrl + "tokens" + "/"; // InstanceKey must be set from outside
    this.instancesListAPIUrl = environment.adminAPIUrl + "listInstances" + "/";
  }

  //--------------------------------------------------------------------------------
  sendMessage(message:string) {
    this.loginOperationCompleted.next(message);
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
            alert('Cannot do the login ' + data.Message);
            return;
          }

          if (data.JwtToken == '')
          {
            alert('Empty token');
            return;
          }

          let authInfo: AuthorizationInfo;

          try
          {
            authInfo = this.GetAuthorizationsFromJWT(data.JwtToken);

            if (authInfo == null) {
              alert('Could not get valid AuthorizationInfo from jwt token, cannot proceed.');
              this.router.navigateByUrl('/');
              return;
            }

            // save in localstorage all the information about authorization
            localStorage.setItem('auth-info', JSON.stringify(authInfo.authorizationProperties));
            
            // save in localstorage all the information about account choices
            let accountInfo: AccountInfo = new AccountInfo(instance);
            localStorage.setItem(authInfo.authorizationProperties.accountName, JSON.stringify(accountInfo));

            // sending message to subscribers to notify login
            this.sendMessage(authInfo.authorizationProperties.accountName);

            // checking cloud-admin only urls
            let opRes: OperationResult = UrlGuard.CanNavigate(returnUrl, authInfo);

            if (!opRes.Result && returnUrl == '/') {
              // this is the case where user clicked the "sign in" link
              this.router.navigateByUrl('/');
              return;              
            }

            if (!opRes.Result) {
              alert(opRes.Message);
              this.router.navigateByUrl('/');
              return;
            }

            this.router.navigateByUrl(returnUrl);
          }
          catch (exc)
          {
            localStorage.removeItem('auth-info');
            if (authInfo != null)
              localStorage.removeItem(authInfo.authorizationProperties.accountName);
            alert('Error while processing authorization info from jwt token ' + exc + '. Login failed.');
            return;
          }   
        },
        error => alert('An error occurred while executing login service: ' + error)
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
    this.sendMessage("");

    // routing to the app home
    this.router.navigateByUrl('/appHome');
  }
}
