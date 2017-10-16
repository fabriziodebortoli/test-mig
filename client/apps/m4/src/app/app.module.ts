import { SharedModule } from './../shared/shared.module';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule, APP_INITIALIZER } from '@angular/core';
import { HttpModule } from '@angular/http';


import { TbCoreModule, InfoService, loadConfig } from '@taskbuilder/core';
import { AppComponent } from './app.component';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    SharedModule,
    HttpModule,
    TbCoreModule.forRoot()
  ],
  providers: [
    InfoService,
    { provide: APP_INITIALIZER, useFactory: loadConfig, deps: [InfoService], multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
