import { InfoService } from './info.service';
import { ModuleWithProviders, NgModule, Optional, SkipSelf } from '@angular/core';
import { HttpModule } from '@angular/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';

import { ComponentService } from './component.service';
import { SidenavService } from './sidenav.service';
import { WebSocketService } from './websocket.service';
import { Logger } from 'libclient';
import { UtilsService } from './utils.service';
import { HttpService } from './http.service';
import { CommandService } from './command.service';
import { LoginSessionService } from './login-session.service';

import { CoreGuard } from './core.guard';

import 'hammerjs';

import { CookieService } from 'angular2-cookie/services/cookies.service';

const TB_SERVICES = [
  CookieService,
  HttpService,
  UtilsService,
  Logger,
  LoginSessionService,
  WebSocketService,
  SidenavService,
  ComponentService,
  CommandService,
  CoreGuard,
  InfoService
];

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    FormsModule,
    HttpModule,
    MaterialModule.forRoot()
  ],
  exports: [],
  providers: [TB_SERVICES]
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
