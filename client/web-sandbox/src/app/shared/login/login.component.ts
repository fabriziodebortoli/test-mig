import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';

import { Subscription } from 'rxjs/Subscription';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { LoginService } from './../../core/login.service';
import { LoginModel } from './../models/login.model';
import { LoginResponse } from './../models/login-response.model';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {

  private companies: any[] = [];
  private loginModel: LoginModel = new LoginModel();
  private loggingIn: boolean = false;

  private companySubs: Subscription;
  private loginSubs: Subscription;

  private errorCode: number;
  private errorMessage: string;

  constructor(
    private cookieService: CookieService,
    private loginService: LoginService,
    private router: Router
  ) { }

  ngOnInit() {
    this.loadCookies();

    if (this.loginModel.user) {
      this.getCompaniesForUser(this.loginModel.user);
    }
  }

  ngOnDestroy() {
    this.companySubs.unsubscribe();
    this.loginSubs.unsubscribe();
  }

  login() {
    console.log('loginModel', this.loginModel);

    this.loggingIn = true;
    this.loginSubs = this.loginService.login(this.loginModel)
      .subscribe((loginResponse: LoginResponse) => {
        console.log("l", loginResponse)
        if (+loginResponse.result === 0) {
          this.saveLoginData();
        } else {
          this.errorCode = +loginResponse.errorCode;
          this.errorMessage = loginResponse.errorMessage;

          this.loggingIn = false;
        }
      },
      error => {
        console.error(error);
        this.loggingIn = false;
      });
  }

  getCompaniesForUser(user: string) {
    this.companySubs = this.loginService.getCompaniesForUser(user).subscribe((result) => {

      this.companies = result.Companies.Company;

      if (this.companies.length === 1) {
        this.loginModel.company = this.companies[0].name;
      }

    });
  }

  loadCookies() {
    this.loginModel.user = this.cookieService.get('_user');
    this.loginModel.company = this.cookieService.get('_company');
  }

  saveLoginData() {
    this.cookieService.put('_user', this.loginModel.user);
    this.cookieService.put('_company', this.loginModel.company);
  }

  keyDownFunction(event) {
    if (event.keyCode === 13) {
      this.login();
    }
  }
}
