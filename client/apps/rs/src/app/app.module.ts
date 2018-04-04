import { NgModule, APP_INITIALIZER } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { routing } from './app.routing';

import { SharedModule } from './shared/shared.module';

import { AppComponent } from './app.component';

import { TbCoreModule, ComponentService, InfoService } from '@taskbuilder/core';

import { TbMenuModule } from '@taskbuilder/menu';

import { ReportingStudioModule } from '@taskbuilder/reporting-studio';

import { environment } from './../environments/environment';
export function loadConfig(config) {
  return () => config.load(environment);
}

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    SharedModule,
    routing,
    TbCoreModule.forRoot(),
    TbMenuModule.forRoot(),
    ReportingStudioModule
  ],
  providers: [
    InfoService,
    { provide: APP_INITIALIZER, useFactory: loadConfig, deps: [InfoService], multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
