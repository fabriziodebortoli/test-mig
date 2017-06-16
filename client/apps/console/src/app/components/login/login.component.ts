import { Component, OnInit } from '@angular/core';
import { Credentials } from './credentials';
import { LoginService } from './../../services/login.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  loginInfo: Credentials;

  constructor(private loginService: LoginService) { 
    this.loginInfo = new Credentials();
  }

  submitLogin() {
    if (this.loginInfo.accountName == '' || this.loginInfo.password == '') {
      return;
    }

    this.loginService.login(this.loginInfo);
  }

  ngOnInit() {
  }
}
