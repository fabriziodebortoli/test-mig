import { LoginComponent } from './components/login/login.component';
import { TabberComponent } from './components/tabber/tabber.component';
import { TabComponent } from './components/tabber/tab.component';
import { PageNotFoundComponent } from './components/page-not-found.component';
import { ToolbarComponent } from './components/toolbar/toolbar.component';
import { ModuleWithProviders, NgModule, Optional, SkipSelf } from '@angular/core';

import { MaterialModule } from '@angular/material';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';

import 'hammerjs';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { LoginSessionService, WebSocketService, HttpService, UtilsService, Logger } from './services';
import { SidenavService } from './services/sidenav.service';


@NgModule({
  declarations: [PageNotFoundComponent, ToolbarComponent, TabComponent, TabberComponent, LoginComponent],
  imports: [
    CommonModule,
    FormsModule,
    HttpModule,
    MaterialModule.forRoot()
  ],
  exports: [PageNotFoundComponent, ToolbarComponent, TabComponent, TabberComponent, LoginComponent],
  providers: [
    CookieService, HttpService, UtilsService, Logger, LoginSessionService, WebSocketService, SidenavService]
})
export class CoreModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: CoreModule,
      providers: [
        CookieService, HttpService, UtilsService, Logger, LoginSessionService, WebSocketService, SidenavService
      ]
    };
  }
  constructor( @Optional() @SkipSelf() parentModule: CoreModule) {
    if (parentModule) {
      throw new Error(
        'CoreModule is already loaded. Import it in the AppModule only');
    }
  }
}
