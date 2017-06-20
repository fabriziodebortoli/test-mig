import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';

import { LoginSession } from './../../../shared/models/login-session';

import { HttpService } from '@taskbuilder/core';
import { LoginSessionService } from './../../../core/login-session.service';

import { CookieService } from 'angular2-cookie/services/cookies.service';

@Component({
  selector: 'tb-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
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
