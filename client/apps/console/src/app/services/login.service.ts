import { Injectable } from '@angular/core';
import { Http, RequestOptions, Headers } from '@angular/http';
import 'rxjs/add/operator/map';
import { Credentials } from './../components/login/credentials';
import { Observable } from "rxjs/Observable";
import { OperationResult } from './operationResult';

@Injectable()
export class LoginService {

  modelBackEndUrl: string;
  
  constructor(private http: Http) { 
    this.modelBackEndUrl = "http://localhost:10344/api/tokens";
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
          localStorage.setItem('jwt-token', data.jwttoken)
        else
          alert('Cannot do the login ' + data.Message);  
      },
        error => alert(error)
      );
  }
  

}
