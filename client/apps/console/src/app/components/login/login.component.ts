import { Component, OnInit } from '@angular/core';
import { Credentials } from './../../authentication/credentials';
import { LoginService } from './../../services/login.service';
import { NgForm } from '@angular/forms';
import { ActivatedRoute, Router } from "@angular/router";

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  credentials: Credentials;
  returnUrl: string;

  constructor(
      private route: ActivatedRoute,
      private router: Router,
      private loginService: LoginService) { 
    this.credentials = new Credentials();
  }

  submitLogin() {
    if (this.credentials.accountName == '' || this.credentials.password == '') {
      return;
    }

    this.loginService.login(this.credentials, this.returnUrl);
  }

  ngOnInit() {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }
}
