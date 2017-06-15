import { name } from './../model/instance';
import { Company } from './../model/company';
import { OperationResult } from './operationResult';
import { Http, Response, RequestOptions, Headers } from '@angular/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Rx';

@Injectable()
export class CompanyService {
  
  companyBackEndUrl:string;

  constructor(private http: Http) { 
    this.companyBackEndUrl = "http://localhost:5052/api/companies";
  }

   addCompany(body:Object): Observable<OperationResult> {
    let bodyString   = JSON.stringify(body);
    let headers      = new Headers({ 'Content-Type': 'application/json' });
    let options      = new RequestOptions({ headers: headers });
    let addedCompany = body as Company;

    return this.http.post(this.companyBackEndUrl + '/' + addedCompany.name, body, options)
      .map((res:Response)=> 
      { 
        console.log(res.json()); 
        return res.json(); 
      })
      .catch((error:any)=>Observable.throw(error.json().error || 'server error'));
      //.catch((error:OperationResult)=>Observable.throw(error.Message)); // prova
  }

}
