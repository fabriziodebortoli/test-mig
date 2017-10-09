import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { LoginSession } from './../../../shared/models/login-session.model';

import { Logger } from './../../../core/services/logger.service';
import { HttpService } from './../../../core/services/http.service';
import { AuthService } from './../../../core/services/auth.service';

@Component({
  selector: 'tb-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  animations: [
    trigger(
      'fadeInOut', [
        transition(':enter', [style({ 'opacity': 0 }), animate('100ms', style({ 'opacity': 1 }))]),
        transition(':leave', [style({ 'opacity': 1 }), animate('500ms', style({ 'opacity': 0 }))])
      ]
    )
  ]
})
export class LoginComponent implements OnInit, OnDestroy {

  companies: any[] = [];
  connectionData: LoginSession = new LoginSession();
  loading: boolean = false;
  errorMessages: string[] = [];
  userAlreadyConnectedOpened: boolean = false;

  constructor(
    public authService: AuthService,
    public cookieService: CookieService,
    public router: Router,
    public logger: Logger,
    public httpService: HttpService
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


      this.companies = result.Companies.Company.sort(this.compareCompanies);
      if (this.companies.length > 0 && this.connectionData.company == undefined)
        this.connectionData.company = this.companies[0].name;

      subs.unsubscribe();
    });
  }

  //---------------------------------------------------------------------------------------------
  compareCompanies(c1, c2) {
    if (c1.name > c2.name) {
      return 1;
    }
    else
      if (c1.name < c2.name) {
        return -1;
      }
    return 0;
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
    this.authService.login(this.connectionData).subscribe(result => {
      if (result.success) {
        let url = this.authService.getRedirectUrl();
        this.logger.debug('Redirect Url', url);
        this.loading = false;
        this.router.navigate([url]);
      } else {
        this.logger.debug('Login Error', this.authService.errorMessage);
        this.loading = false;
        if (result.errorCode == 9)
          this.userAlreadyConnectedOpened = true;
      }
    });
  }

  keyDownFunction(event) {
    if (event.keyCode === 13) {
      this.login();
    }
  }

  userConnectedYes() {
    this.connectionData.overwrite = true;
    this.login();
    this.connectionData.overwrite = false;

    this.userAlreadyConnectedOpened = false;

  }
  userConnectedCancel() {
    this.userAlreadyConnectedOpened = false;
  }
}
