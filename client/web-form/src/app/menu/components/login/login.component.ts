import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { LoginSession } from './../../../shared/models/login-session';
import { LoginSessionService } from './../../../core/login-session.service';

@Component({
  selector: 'tb-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {
  connectionData: LoginSession = new LoginSession();
  working: boolean = false;
  constructor(
    private loginSessionService: LoginSessionService,
    private cookieService: CookieService,
    private router: Router) {

  }

  ngOnInit() {
    this.loadState();
  }
  ngOnDestroy() {
    this.saveState();
  }

  loadState() {
    this.connectionData.user = this.cookieService.get('_user');
    this.connectionData.company = this.cookieService.get('_company');
  }
  saveState() {
    this.cookieService.put('_user', this.connectionData.user);
    this.cookieService.put('_company', this.connectionData.company);
  }
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
}
