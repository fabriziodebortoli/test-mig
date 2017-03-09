import { HttpService } from './http.service';
import { LoginModel } from './../shared/models/login.model';
import { Injectable } from '@angular/core';
import { Http, URLSearchParams, Response, Headers } from '@angular/http';

import { environment } from './../../environments/environment';
import { Observable } from 'rxjs/Rx';

@Injectable()
export class LoginService {

  private baseUrl = environment.baseUrl;
  private logged: boolean;

  constructor(private httpService: HttpService) { }

  getAccountManagerBaseUrl() {
    return this.baseUrl + 'account-manager/';
  }

  getCompaniesForUser(user: string) {

    return Observable.create(observer => {
      observer.next({
        "Companies": {
          "Company": [
            {
              "name": "CompanyMago4"
            }
          ]
        }
      });
      observer.complete();
    })


    // const params: URLSearchParams = new URLSearchParams();
    // params.set('user', user);

    // return this.httpService.post(this.getAccountManagerBaseUrl() + 'getCompaniesForUser/', params)
    //   .map((res: Response) => {
    //     return res.json();
    //   });
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

  login(loginModel: LoginModel) {
    this.logged = true;
    return Observable.create(observer => {
      observer.next({
        "result": "35",
        "authenticationToken": "35"
      });
      observer.complete();
    });

    // const params: URLSearchParams = new URLSearchParams();
    // params.set('user', loginModel.user);
    // params.set('password', loginModel.password);
    // params.set('company', loginModel.company);
    // params.set('askingProcess', '');

    // return this.httpService.post(this.getAccountManagerBaseUrl() + 'login-compact/', params)
    //   .map((res: Response) => {
    //     return res.json();
    //   });
  }

  protected handleError(error: any) {
    // In a real world app, we might use a remote logging infrastructure
    // We'd also dig deeper into the error to get a better message
    let errMsg = (error.message) ? error.message :
      error.status ? `${error.status} - ${error.statusText}` : 'Server error';
    console.error(errMsg);
    return Observable.throw(errMsg);
  }

}
