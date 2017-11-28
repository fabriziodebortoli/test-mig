import { AnyFn } from './../../../shared/commons/selector';
import { OldLocalizationService } from './../../../core/services/oldlocalization.service';
import { LoadingService } from './../../../core/services/loading.service';
import { MenuService } from './../../services/menu.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription, Observable } from '../../../rxjs.imports';
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";

import { LoginSession } from './../../../shared/models/login-session.model';

import { UtilsService } from './../../../core/services/utils.service';
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

  public placeHolder: { name: string } = { name: "Select company..." };
  // public companies: Array<{ name: string }> = [];
  public companies: any = [];

  connectionData: LoginSession = new LoginSession();
  errorMessages: string[] = [];
  userAlreadyConnectedOpened: boolean = false;
  changePasswordOpened: boolean = false;
  clearCachedData: boolean = false;
  confirmPassword: string = "";
  newPassword: string = "";

  constructor(
    public authService: AuthService,
    public router: Router,
    public logger: Logger,
    public httpService: HttpService,
    public utilsService: UtilsService,
    public menuService: MenuService,
    public localizationService: OldLocalizationService,
    public loadingService: LoadingService
  ) {
    this.loadingService.setLoading(true);
    this.localizationService.loadLocalizedElements()
  }

  //-------------------------------------------------------------------------------------
  ngOnInit() {

    this.httpService.isServerUp().subscribe(isServerUp => {
      this.loadingService.setLoading(false);

      let subIsLogged = this.authService.isLogged().subscribe(isLogged => {
        if (isLogged) {
          this.router.navigate([this.authService.getDefaultUrl()]);
        }
        subIsLogged.unsubscribe();
      });

      this.loadState();
      setTimeout(() => {
        if (this.connectionData.user) {
          this.getCompaniesForUser(this.connectionData.user);
        }
      }, 200);

    },
      ((error: any) => {
        let errMsg = (error.message) ? error.message :
          error.status ? `${error.status} - ${error.statusText}` : 'Server error';
        if (this.logger)
          this.logger.error(errMsg);


        this.router.navigate([this.authService.getServerDownPageUrl()]);
        return Observable.throw(errMsg);
      }));

  }

  //-------------------------------------------------------------------------------------
  ngOnDestroy() {
    this.saveState();
  }

  //-------------------------------------------------------------------------------------
  getCompaniesForUser(user: string) {
    let subs = this.httpService.getCompaniesForUser(user).subscribe((result) => {
      this.companies = result.Companies.Company.sort(this.compareCompanies).map(c => c.name);

      if (this.companies.length > 0 && !this.connectionData.company) {
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
    this.connectionData.user = localStorage.getItem('_user');
    this.connectionData.company = localStorage.getItem('_company');
  }

  //-------------------------------------------------------------------------------------
  saveState() {
    localStorage.setItem('_user', this.connectionData.user);
    localStorage.setItem('_company', this.connectionData.company);
  }

  //-------------------------------------------------------------------------------------
  changePasswordlogin() {
  }

  //-------------------------------------------------------------------------------------
  login(overwrite: boolean = false) {
    this.saveState();
    this.loadingService.setLoading(true, this.localizationService.localizedElements.Loading);
    this.connectionData.overwrite = overwrite;
    let subs = this.authService.login(this.connectionData).subscribe(result => {
      if (result.success) {
        this.menuService.clearCachedData = this.clearCachedData;
        this.connectionData.overwrite = false;
        let url = this.authService.getRedirectUrl();
        this.logger.debug('Redirect Url', url);
        this.loadingService.setLoading(false);
        this.router.navigate([url]);
      } else {
        this.logger.debug('Login Error', this.authService.errorMessage);
        this.loadingService.setLoading(false);
        if (result.errorCode == 9) //UserAlreadyLoggedError
        {
          this.userAlreadyConnectedOpened = true;
        }
        else if (result.errorCode == 19) //UserMustChangePasswordError
        {
          this.changePasswordOpened = true;
        }
      }
      subs.unsubscribe();
    });
  }

  keyUpFunction(event) {
    if (event.keyCode === 13) {
      this.login();
    }
  }

  userConnectedYes() {
    this.login(true);
    this.userAlreadyConnectedOpened = false;

  }
  userConnectedCancel() {
    this.userAlreadyConnectedOpened = false;
  }

  changePasswordOk() {
    this.authService.changePassword(this.connectionData, this.newPassword).
      subscribe((res) => {

        if (res.success) {
          this.changePasswordOpened = false;
          this.connectionData.password = this.newPassword;
          this.login();
        }
      });
  }
}
