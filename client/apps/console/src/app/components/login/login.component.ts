import { Component, OnInit } from '@angular/core';
import { LoginInfo } from './login-info';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  loginInfo: LoginInfo;

  constructor() { 
    this.loginInfo = new LoginInfo();
  }

  ngOnInit() {
  }

}
