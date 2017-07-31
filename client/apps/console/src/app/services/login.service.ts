import { Injectable } from '@angular/core';
import { Http, RequestOptions, Headers } from '@angular/http';
import 'rxjs/add/operator/map';
import { Credentials } from './../authentication/credentials';
import { Observable } from "rxjs/Observable";
import { OperationResult } from './operationResult';
import { AuthorizationInfo, AuthorizationProperties } from "app/authentication/auth-info";
import { environment } from './../../environments/environment';
import { RoleNames, RoleLevels } from './../authentication/auth-helpers';
import { Router } from "@angular/router";
import { Subject } from "rxjs/Subject";
import { UrlGuard } from "app/authentication/url-guard";

@Injectable()
export class LoginService {

  modelBackEndUrl: string;

  // Observable string sources
  private loginCompleted = new Subject<string>();
  private logOffCompleted = new Subject<string>();
  
  constructor(private http: Http,  private router: Router) { 

    this.modelBackEndUrl = environment.adminAPIUrl + "api/tokens";
  }

  login(body:Object, returnUrl: string) {

    let bodyString  = JSON.stringify(body);
    let headers = new Headers({ 'Content-Type': 'application/json' });
    let options = new RequestOptions({ headers: headers });
    
    this.http.post(this.modelBackEndUrl, bodyString, options)
      .map(res => res.json())
      .subscribe(
        // We're assuming the response will be an object
        // with the JWT on an jwttoken key
      data =>
      {
        if (data.Result)
        {
          if (data.JwtToken == '')
          {
            alert('Empty token');
            return;
          }

          try
          {
            let tokenParts: Array<string> = data.JwtToken.split('.');

            if (tokenParts.length != 3)
            {
              alert('Invalid token');
              return;
            }  

            let decodedToken: string = atob(tokenParts[1]);
            let parsedToken = JSON.parse(decodedToken);

            let authInfo: AuthorizationInfo = new AuthorizationInfo(data.JwtToken,
              parsedToken.AccountName);
            
            authInfo.SetInstances(parsedToken.Instances);
            authInfo.SetSubscriptions(parsedToken.Subscriptions);
            authInfo.SetServerUrls(parsedToken.Urls);
            authInfo.SetTokens(parsedToken.UserTokens);
            authInfo.SetSecurityValues(parsedToken.AppSecurity);
            authInfo.SetRoles(parsedToken.Roles);
            
            // save in localstorage all the information about authorization

            localStorage.setItem('auth-info', JSON.stringify(authInfo.authorizationProperties));

            // checking cloud-admin only urls

            let opRes: OperationResult;

            if (returnUrl == '/instancesHome') {
              opRes = UrlGuard.CanNavigateURL(returnUrl, authInfo);
              if (opRes.Result){
                this.router.navigateByUrl(returnUrl);
                return true;
              } else {
                alert(opRes.Message);
                this.router.navigateByUrl('/');
                return false;
              }
            }
            
            // checking provisioning-admin only urls

            opRes = UrlGuard.CanNavigateLevel(RoleLevels.Subscription, authInfo);

            if (!opRes.Result) {
              alert(opRes.Message);
              this.router.navigateByUrl('/');
              return false;
            }
            
            // user is permitted to navigate requestes urls
            this.router.navigateByUrl(returnUrl);
          }
          catch (exc)
          {
              alert('Error while decoding jwt token ' + exc + '. Login failed.');
              return;
          }
        }
        else
          alert('Cannot do the login ' + data.Message);  
      },
        error => alert(error)
      );
  }
}
