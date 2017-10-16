import { NgModule, APP_INITIALIZER } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpModule } from '@angular/http';

import { TbCoreModule, InfoService, loadConfig } from '@taskbuilder/core';

import { routing } from './app.routing';
import { AppComponent } from './app.component';
import { SharedModule } from './../shared/shared.module';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    SharedModule,
    HttpModule,
    TbCoreModule.forRoot(),
    routing
  ],
  providers: [
    InfoService,
    { provide: APP_INITIALIZER, useFactory: loadConfig, deps: [InfoService], multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
