import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';

import { LoginSession } from './../../../shared';

import { HttpService, } from '../../../core/services/http.service';
import { LoginSessionService } from '../../../core/services/login-session.service';

import { CookieService } from 'angular2-cookie/services/cookies.service';

@Component({
  selector: 'tb-login',
  template: "<header> <md-toolbar color=\"primary\"> <img src=\"assets/images/logoM4_h40.png\" /> <span class=\"fill-remaining-space\"></span> <span>Microarea Spa</span> </md-toolbar> </header> <main class=\"login-content\"> <md-card class=\"login-form\" *ngIf=\"!working\"> <md-card-title>Log In</md-card-title> <md-card-content> <div class=\"form-group\"> <md-input-container class=\"form-control\"> <input mdInput type=\"text\" [placeholder]=\"'User'\" autofocus [(ngModel)]=\"connectionData.user\" (blur)=getCompaniesForUser(connectionData.user) (keydown)=\"keyDownFunction($event)\" /> </md-input-container> <md-input-container class=\"form-control\"> <input mdInput type=\"password\" [placeholder]=\"'Password'\" [(ngModel)]=\"connectionData.password\" (keydown)=\"keyDownFunction($event)\" /> </md-input-container> <md-select class=\"form-control\" placeholder=\"Company\" [(ngModel)]=\"connectionData.company\"> <md-option *ngIf=\"!companies\">Loading...</md-option> <md-option *ngFor=\"let company of companies\" [value]=\"company?.name\"> {{ company?.name }} </md-option> </md-select> <div class=\"error\" *ngFor=\"let errorMessage of loginSessionService.errorMessages\">{{errorMessage.text}}</div> <button class=\"form-button\" md-raised-button color=\"primary\" (click)=\"login();\" [disabled]=\"companies.length == 0\">Login</button> </div> </md-card-content> </md-card> <div class=\"spinner\" *ngIf=\"working\"> <md-spinner mode=\"indeterminate\" color=\"primary\"></md-spinner> </div> </main> <footer> <span>Copyright © 1984-2017 Microarea SpA - Tutti i diritti riservati | <a href=\"http://www.microarea.it\">Microarea.It</a></span> </footer>",
  styles: [":host { display: flex; min-height: 100vh; flex-direction: column; } .fill-remaining-space { flex: 1 1 auto; } .login-content { flex: 1; background: #efefef; } .login-form { flex: 1; max-width: 400px; margin: 100px auto; } footer { background: #0277bd; padding: 10px 15px; color: #fff; font-weight: 300; font-size: 12px; } footer a { color: #fff; } .form-group .form-control { width: 100%; margin: 10px 0; } .form-group .form-button { margin-top: 20px; width: 100%; } div.error { color: red; margin: 20px 0 0; } .spinner { margin: 200px auto; width: 100px; } .md-spinner path { stroke: #0277BD; } "]
})
export class LoginComponent implements OnInit, OnDestroy {

  companies: any[] = [];
  connectionData: LoginSession = new LoginSession();
  working: boolean = false;
  constructor(
    private loginSessionService: LoginSessionService,
    private cookieService: CookieService,
    private router: Router,
    private httpService: HttpService
  ) {

  }

  //-------------------------------------------------------------------------------------
  ngOnInit() {
    this.loadState();

    if (this.connectionData.user != undefined)
      this.getCompaniesForUser(this.connectionData.user);
  }

  //-------------------------------------------------------------------------------------
  ngOnDestroy() {
    this.saveState();
  }

  //-------------------------------------------------------------------------------------
  getCompaniesForUser(user: string) {

    let subs = this.httpService.getCompaniesForUser(user).subscribe((result) => {

      this.companies = result.Companies.Company;
      if (this.companies.length > 0 && this.connectionData.company == undefined)
        this.connectionData.company = this.companies[0].name;

      subs.unsubscribe();
    });
  }

  //-------------------------------------------------------------------------------------
  loadState() {
    this.connectionData.user = this.cookieService.get('_user');
    this.connectionData.company = this.cookieService.get('_company');
  }

  //-------------------------------------------------------------------------------------
  saveState() {
    this.cookieService.put('_user', this.connectionData.user);
    this.cookieService.put('_company', this.connectionData.company);
  }

  //-------------------------------------------------------------------------------------
  login() {
    this.saveState();
    this.working = true;
    let subs = this.loginSessionService.login(this.connectionData)
      .subscribe(result => {
        this.working = false;
        subs.unsubscribe();
      },
      error => {
        this.working = false;
        subs.unsubscribe();
      });
  }

  keyDownFunction(event) {
    if (event.keyCode === 13) {
      this.login();
    }
  }
}
