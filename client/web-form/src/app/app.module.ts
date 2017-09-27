import { NgModule, APP_INITIALIZER } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { routing } from './app.routing';

import { SharedModule } from './shared/shared.module';

import { AppComponent } from './app.component';

import { TbCoreModule, ComponentService, AppConfigService } from '@taskbuilder/core';

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
        AppConfigService,
        { provide: APP_INITIALIZER, useFactory: (config: AppConfigService) => () => config.load(), deps: [AppConfigService], multi: true }
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }