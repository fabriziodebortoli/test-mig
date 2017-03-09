import { LoginService } from './../../core/login.service';
import { LoginModel } from './../models/login.model';
import { Router } from '@angular/router';
import { Component, OnInit, OnDestroy } from '@angular/core';

import { CookieService } from 'angular2-cookie/services/cookies.service';
import { Subscription } from "rxjs/Subscription";

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
    this.saveLoginData();
    console.log('loginModel', this.loginModel);

    this.loggingIn = true;
    this.loginSubs = this.loginService.login(this.loginModel)
      .subscribe(response => {
        console.log(response);

        this.cookieService.put('authtoken', response.authenticationToken);

        this.loggingIn = false;
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

  //-------------------------------------------------------------------------------------
  saveLoginData() {
    this.cookieService.put('_user', this.loginModel.user);
    this.cookieService.put('_company', this.loginModel.company);
  }

}
