import { MenuService } from './../../services/menu.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { LoadingService, LocalizationService, AuthService, HttpService, Logger, LoginSession, UtilsService } from '@taskbuilder/core';

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

  public placeHolder: { name: string } = { name: "Select company..." };
  // public companies: Array<{ name: string }> = [];
  public companies: any = [];

  connectionData: LoginSession = new LoginSession();
  errorMessages: string[] = [];
  userAlreadyConnectedOpened: boolean = false;
  clearCachedData: boolean = false;

  constructor(
    public authService: AuthService,
    public cookieService: CookieService,
    public router: Router,
    public logger: Logger,
    public httpService: HttpService,
    public utilsService: UtilsService,
    public menuService: MenuService,
    public localizationService: LocalizationService,
    public loadingService: LoadingService
  ) {
    this.loadingService.setLoading(false);
    this.localizationService.loadLocalizedElements()
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
      this.companies = result.Companies.Company.sort(this.compareCompanies).map(c => c.name);

      if (this.companies.length > 0 && this.connectionData.company == undefined) {
        this.logger.log("this.companies.length", this.companies.length)
        this.connectionData.company = this.companies[0];
      }

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
    this.loadingService.setLoading(true, this.localizationService.localizedElements.Loading);
    this.authService.login(this.connectionData).subscribe(result => {
      if (result.success) {
        console.log(this.clearCachedData);
        this.menuService.clearCachedData = this.clearCachedData;
        let url = this.authService.getRedirectUrl();
        this.logger.debug('Redirect Url', url);
        this.loadingService.setLoading(false);
        this.router.navigate([url]);
      } else {
        this.logger.debug('Login Error', this.authService.errorMessage);
        this.loadingService.setLoading(false);
        if (result.errorCode == 9)
          this.userAlreadyConnectedOpened = true;
      }
    });
  }

  keyUpFunction(event) {
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