import { Injectable } from '@angular/core';
import { Http, RequestOptions, Headers } from '@angular/http';
import 'rxjs/add/operator/map';
import { Credentials } from './../authentication/credentials';
import { Observable } from "rxjs/Observable";
import { OperationResult } from './operationResult';
import { AuthorizationInfo, AuthorizationProperties } from "app/authentication/auth-info";
import { environment } from './../../environments/environment';
import { RoleNames } from './../authentication/auth-helpers';

@Injectable()
export class LoginService {

  modelBackEndUrl: string;
  
  constructor(private http: Http) { 

    this.modelBackEndUrl = environment.adminAPIUrl + "api/tokens";
  }

  login(body:Object) {

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
            
            authInfo.SetSubscriptions(parsedToken.Subscriptions);
            authInfo.SetServerUrls(parsedToken.Urls);
            authInfo.SetTokens(parsedToken.UserTokens);

            // creating roles array;; waiting for roles object migration
            let roles: Array<string> = new Array<string>();

            if (parsedToken.ProvisioningAdmin) {
              roles.push(RoleNames.ProvisioningAdmin.toString());
            }

            if (parsedToken.CloudAdmin) {
              roles.push(RoleNames.CloudAdmin.toString());
            }

            authInfo.SetRoles(roles);
            localStorage.setItem('auth-info', JSON.stringify(authInfo.authorizationProperties));
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
