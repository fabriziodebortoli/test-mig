import { NgModule, APP_INITIALIZER } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { routing } from './app.routing';

import { SharedModule } from './shared/shared.module';

import { AppComponent } from './app.component';

import { TbCoreModule, ComponentService, InfoService, loadConfig } from '@taskbuilder/core';

import { ReportingStudioModule } from '@taskbuilder/reporting-studio';

import { CookieService } from 'angular2-cookie/services/cookies.service';

@NgModule({
    declarations: [AppComponent],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        SharedModule,
        routing,
        ReportingStudioModule,
        TbCoreModule.forRoot()
    ],
    providers: [
        CookieService,
        InfoService,
        { provide: APP_INITIALIZER, useFactory: loadConfig, deps: [InfoService], multi: true }
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }