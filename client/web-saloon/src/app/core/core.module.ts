import { NgModule, ModuleWithProviders, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CoreGuard } from './core.guard';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { HttpService } from './http.service';
import { LoginService } from './login.service';
import { UtilsService } from './utils.service';

const TB_SERVICES = [
  HttpService,
  LoginService,
  UtilsService,
  CoreGuard
];

@NgModule({
  imports: [
    CommonModule
  ],
  providers: [
    CookieService,
    TB_SERVICES
  ]
})
export class CoreModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: CoreModule,
      providers: [TB_SERVICES]
    };
  }
  constructor( @Optional() @SkipSelf() parentModule: CoreModule) {
    if (parentModule) {
      throw new Error(
        'CoreModule is already loaded. Import it in the AppModule only');
    }
  }
}
