import { Injectable } from '@angular/core';
import { Http, RequestOptions, Headers } from '@angular/http';
import 'rxjs/add/operator/map';
import { Credentials } from './../components/login/credentials';

@Injectable()
export class LoginService {

  modelBackEndUrl: string;
  
  constructor(private http: Http) { 
    this.modelBackEndUrl = "http://localhost:5052/api/tokens";
  }

  login(credentials:Credentials) {

    let bodyString = JSON.stringify(credentials);
    
    this.http.post(this.modelBackEndUrl, credentials)
      .map(res => res.json())
      .subscribe(
        // We're assuming the response will be an object
        // with the JWT on an jwttoken key
        data => localStorage.setItem('jwt-token', data.jwttoken),
        error => alert(error)
      );
    }  
}
