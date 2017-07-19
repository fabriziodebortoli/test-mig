import { Injectable } from '@angular/core';
import { Http, RequestOptions, Headers } from '@angular/http';
import 'rxjs/add/operator/map';
import { Credentials } from './../authentication/credentials';
import { Observable } from "rxjs/Observable";
import { OperationResult } from './operationResult';
import { AuthorizationInfo, AuthorizationProperties } from "app/authentication/auth-info";
import { environment } from './../../environments/environment';
import { RoleNames } from './../authentication/auth-helpers';
import { Router } from "@angular/router";

@Injectable()
export class LoginService {

  modelBackEndUrl: string;
  
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

            //@@TODO: attrezzare il back-end per passare anche l'AppId e il SecurityValue dell'istanza corrente
            // da utilizzare per generare un AuthenticationHeader valido per il GWAM
            authInfo.SetSecurityValues('I-M4', 'ju23ff-KOPP-0911-ila');

            //@@TODO gestione ruoli: nel token deve essere passato anche un array di ruoli
            // che andremo a copiare dentro il ns oggetto locale authInfo nel localstorage
            let roles: Array<string> = new Array<string>();
            if (parsedToken.ProvisioningAdmin) {
              roles.push(RoleNames.ProvisioningAdmin.toString());
            }
            if (parsedToken.CloudAdmin) {
              roles.push(RoleNames.CloudAdmin.toString());
            }
            authInfo.SetRoles(roles);

            // save in localstorage all the information about authorization

            localStorage.setItem('auth-info', JSON.stringify(authInfo.authorizationProperties));

            this.router.navigateByUrl(returnUrl);

            if (authInfo.HasRole(RoleNames.ProvisioningAdmin))
            {
               this.router.navigateByUrl(returnUrl);
              return;
            }

            // user has no roles to navigate the requested url sending him back to home component
            alert(RoleNames[RoleNames.ProvisioningAdmin] + ' role missing');
            this.router.navigateByUrl('/');
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
