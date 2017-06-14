import { Injectable } from '@angular/core';
import { Http, Response, Headers, RequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import { environment } from './../../environments/environment';
import { Account } from '../model/account';

@Injectable()
export class ModelService {

  modelBackEndUrl:string;

  constructor(private http: Http) { 
    this.modelBackEndUrl = "http://localhost:5052/api/accounts";
  }

  addAccount(body:Object): Observable<Account[]> {
    let bodyString   = JSON.stringify(body);
    let headers      = new Headers({ 'Content-Type': 'application/json' });
    let options      = new RequestOptions({ headers: headers });
    let addedAccount = body as Account;

    return this.http.post(this.modelBackEndUrl + '/' + addedAccount.email, body, options)
      .map((res:Response)=>res.json())
      .catch((error:any)=>Observable.throw(error.json().error || 'server error'));
  }
}
