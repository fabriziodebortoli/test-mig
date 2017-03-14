import { Injectable } from '@angular/core';
import { Http, URLSearchParams, Headers, RequestOptionsArgs } from '@angular/http';
import { Router } from '@angular/router';

import { Observable } from 'rxjs/Rx';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { UtilsService } from './utils.service';

@Injectable()
export class HttpService {

  constructor(
    private http: Http,
    private utils: UtilsService
  ) { }

  postData(url: string, data: Object) {
    const headers = new Headers();
    headers.append('Content-Type', 'application/x-www-form-urlencoded');

    return this.http.post(url, this.utils.serializeData(data), { withCredentials: true, headers: headers });
  }

  protected handleError(error: any) {
    let errMsg = (error.message) ? error.message :
      error.status ? `${error.status} - ${error.statusText}` : 'Server error';
    console.error(errMsg);
    return Observable.throw(errMsg);
  }
}
