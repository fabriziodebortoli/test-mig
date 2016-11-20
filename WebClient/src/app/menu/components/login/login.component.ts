import { CookieService } from 'angular2-cookie/services/cookies.service';
import { LoginSessionService } from './../../../core/';
import { LoginSession } from './../../../shared/';
import { Component, OnInit, OnDestroy } from '@angular/core';

@Component({
  selector: 'tb-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit, OnDestroy {
  connectionData: LoginSession = new LoginSession();
  working: boolean = false;
  constructor(
    private loginSessionService: LoginSessionService,
    private cookieService: CookieService) {

    this.connectionData.user = this.cookieService.get('user');
    this.connectionData.company = this.cookieService.get('company');
  }

  ngOnInit() {
  }
  ngOnDestroy() {
    this.cookieService.put('user', this.connectionData.user);
    this.cookieService.put('company', this.connectionData.company);
  }
  login() {
    this.working = true;
    return this.loginSessionService.login(this.connectionData).subscribe(connected => {
      this.working = false;
    });
  }
}
