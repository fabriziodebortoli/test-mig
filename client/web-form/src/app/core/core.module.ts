import { DataService } from './data.service';
import { ModuleWithProviders, NgModule, Optional, SkipSelf } from '@angular/core';
import { HttpModule } from '@angular/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';

import { ComponentService } from '@taskbuilder/core';
import { WebSocketService } from '@taskbuilder/core';

import { LoginSessionService } from '@taskbuilder/core';
import { ExplorerService } from './explorer.service';
import { EnumsService } from './enums.service';

import { CoreGuard } from './core.guard';

import 'hammerjs';

import { CookieService } from 'angular2-cookie/services/cookies.service';

const TB_SERVICES = [
  CookieService,
  LoginSessionService,
  WebSocketService,
  ComponentService,
  CoreGuard,
  ExplorerService,
  EnumsService,
  DataService
];

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    FormsModule,
    HttpModule,
    MaterialModule
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
