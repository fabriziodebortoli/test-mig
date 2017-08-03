import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { routing } from './app.routing';

import { SharedModule } from './shared/shared.module';

import { AppComponent } from './app.component';

import { TbCoreModule, ComponentService } from '@taskbuilder/core';
import { TbIconsModule } from '@taskbuilder/icons';
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
        TbIconsModule,
        TbCoreModule.forRoot()
    ],
    providers: [CookieService],
    bootstrap: [AppComponent]
})
export class AppModule { }