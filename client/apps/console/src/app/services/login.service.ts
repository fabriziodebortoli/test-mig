import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import 'rxjs/add/operator/map';

@Injectable()
export class LoginService {

  modelBackEndUrl: string;
  
  constructor(private http: Http) { 
    this.modelBackEndUrl = "http://localhost:5052/api/tokens";
  }

  login(credentials) {
      this.http.post('', credentials)
        .map(res => res.json())
        .subscribe(
          // We're assuming the response will be an object
          // with the JWT on an jwttoken key
          data => localStorage.setItem('jwt-token', data.jwttoken),
          error => console.log(error)
        );
    }  
}
