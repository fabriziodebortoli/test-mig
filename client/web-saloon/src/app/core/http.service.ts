import { Observable } from 'rxjs/Rx';
import { Injectable } from '@angular/core';
import { Http, URLSearchParams, Headers, RequestOptionsArgs } from '@angular/http';

@Injectable()
export class HttpService {

  constructor(private http: Http) { }

  post(url: string, params: URLSearchParams) {
    const headers = new Headers();
    headers.append('Content-Type', 'application/x-www-form-urlencoded');

    return this.http.post(url, {}, {
      search: params,
      withCredentials: true,
      headers: headers
    })
      .catch(this.handleError);
  }

  protected handleError(error: any) {
    let errMsg = (error.message) ? error.message :
      error.status ? `${error.status} - ${error.statusText}` : 'Server error';
    console.error(errMsg);
    return Observable.throw(errMsg);
  }
}
