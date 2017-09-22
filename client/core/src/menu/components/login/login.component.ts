import { Subscription } from 'rxjs';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { LoginSession } from './../../../shared/models/login-session.model';

import { Logger } from './../../../core/services/logger.service';
import { HttpService } from './../../../core/services/http.service';
import { AuthService } from './../../../core/services/auth.service';
import { LoginSessionService } from './../../../core/services/login-session.service';

@Component({
  selector: 'tb-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {

  companies: any[] = [];
  connectionData: LoginSession = new LoginSession();
  loading: boolean = false;
  errorMessages: string[] = [];

  constructor(
    private loginSessionService: LoginSessionService,
    private authService: AuthService,
    private cookieService: CookieService,
    private router: Router,
    private logger: Logger,
    private httpService: HttpService
  ) {

  }

  //-------------------------------------------------------------------------------------
  ngOnInit() {
    this.loadState();

    if (this.connectionData.user != undefined) {
      this.getCompaniesForUser(this.connectionData.user);
    }

    this.authService.isLogged().subscribe(isLogged => {
      if (isLogged) {
        this.router.navigate([this.authService.getDefaultUrl()]);
      } else {
        console.log("DIOCANE")
      }
    });
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
    this.loading = true;
    this.authService.login(this.connectionData).subscribe(authenticated => {
      if (authenticated) {
        let url = this.authService.getRedirectUrl();
        this.logger.debug('Redirect Url', url);
        this.loading = false;
        this.router.navigate([url]);
      } else {
        this.logger.debug('Login Error', this.authService.errorMessage);
        this.loading = false;
      }
    });
  }

  keyDownFunction(event) {
    if (event.keyCode === 13) {
      this.login();
    }
  }
}
