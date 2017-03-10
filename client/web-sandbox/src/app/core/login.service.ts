import { LogoutResponse } from './../shared/models/logout-response.model';
import { LoginResponse } from './../shared/models/login-response.model';
import { Injectable } from '@angular/core';
import { Http, URLSearchParams, Response, Headers } from '@angular/http';
import { Router } from '@angular/router';

import { Observable } from 'rxjs/Rx';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { environment } from './../../environments/environment';
import { LoginModel } from './../shared/models/login.model';

import { UtilsService } from './utils.service';
import { HttpService } from './http.service';

@Injectable()
export class LoginService {

  private baseUrl = environment.baseUrl;
  private logged: boolean;

  private authtoken: string;

  constructor(
    private httpService: HttpService,
    private utils: UtilsService,
    private cookieService: CookieService,
    private router: Router) {
    this.authtoken = this.cookieService.get('authtoken');
    this.logged = (this.logged || (this.authtoken ? true : false));
  }

  getAccountManagerBaseUrl() {
    return this.baseUrl + 'account-manager/';
  }

  getCompaniesForUser(user: string) {
    const params: Object = {
      user: user
    };

    return this.httpService.postData(this.getAccountManagerBaseUrl() + 'getCompaniesForUser/', params)
      .map((res: Response) => res.json());
  }

  serializeData(data) {
    let buffer = [];

    // Serialize each key in the object.
    for (let name in data) {
      if (!data.hasOwnProperty(name)) {
        continue;
      }

      let value = data[name];

      buffer.push(
        encodeURIComponent(name) + '=' + encodeURIComponent((value == null) ? '' : value)
      );
    }

    // Serialize the buffer and clean it up for transportation.
    let source = buffer.join('&').replace(/%20/g, '+');
    return (source);
  };

  isLogged(): boolean {
    return this.logged;
  }

  login(loginModel: LoginModel): Observable<LoginResponse> {

    return this.loginRequest(loginModel)
      .map((response: LoginResponse) => {

        if (+response.result === 0) {
          this.logged = true;
          this.cookieService.put('authtoken', response.authenticationToken);
          this.router.navigate([''], { skipLocationChange: false, replaceUrl: false });
        }

        return response;
      },
      error => {
        return error;
      });

  }

  private loginRequest(loginModel: LoginModel) {
    const params: Object = {
      user: loginModel.user,
      password: loginModel.password,
      company: loginModel.company,
      askingProcess: 'puppa'
    };
    return this.httpService.postData(this.getAccountManagerBaseUrl() + 'login-compact/', params)
      .map((response: Response) => {
        const loginResponse = <LoginResponse>response.json();
        console.log('loginResponse', loginResponse);
        return loginResponse;
      },
      error => {
        const loginResponse = <LoginResponse>error.json();
        console.error('loginResponse', loginResponse);
        return loginResponse;
      }).catch(this.handleError);
  }

  logout(): Observable<LogoutResponse> {
    const token = this.cookieService.get('authtoken');
    return this.logoutRequest(token)
      .map((logoutResponse: LogoutResponse) => {

        if (logoutResponse.success) {
          this.logged = false;
          this.cookieService.remove('authtoken');
          this.router.navigate(['login'], { skipLocationChange: false, replaceUrl: false });
        }

        return logoutResponse;
      },
      error => {
        return error;
      });
  }

  private logoutRequest(token: string) {

    return this.httpService.postData(this.getAccountManagerBaseUrl() + 'logout/', token)
      .map((response: Response) => {
        const logoutResponse = <LogoutResponse>response.json();
        console.log('logoutResponse', logoutResponse);
        return logoutResponse;
      },
      error => {
        const logoutResponse = <LogoutResponse>error.json();
        console.error('logoutResponse', logoutResponse);
        return logoutResponse;
      }).catch(this.handleError);
  }

  protected handleError(error: any) {
    // In a real world app, we might use a remote logging infrastructure
    // We'd also dig deeper into the error to get a better message
    let errMsg = (error.message) ? error.message :
      error.status ? `${error.status} - ${error.statusText}` : 'Server error';
    console.error('handleError', errMsg);
    return Observable.throw(errMsg);
  }

}
