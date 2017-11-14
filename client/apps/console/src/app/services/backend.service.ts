import { Injectable } from '@angular/core';
import { Http, RequestOptions, Headers, Response } from '@angular/http';
import { Observable } from "rxjs/Observable";
import { OperationResult } from './operationResult';
import { environment } from './../../environments/environment';

@Injectable()
export class BackendService {

  constructor(private http: Http) {}

  checkStartup(): Observable<OperationResult> {

    return this.http.get(environment.adminAPIUrl + 'startup')
      .map((res: Response) => res.json())
      .catch((error: any) => Observable.throw(error.json().error || 'server error on checkStartup')).retry(3);
  }

}
