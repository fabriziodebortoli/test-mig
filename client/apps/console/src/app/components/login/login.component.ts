import { Component, OnInit } from '@angular/core';
import { Credentials } from './credentials';
import { LoginService } from './../../services/login.service';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  credentials: Credentials;

  constructor(private loginService: LoginService) { 
    this.credentials = new Credentials();
  }

  submitLogin() {
    if (this.credentials.accountName == '' || this.credentials.password == '') {
      return;
    }

    this.loginService.login(this.credentials);
  }

  ngOnInit() {
  }
}
