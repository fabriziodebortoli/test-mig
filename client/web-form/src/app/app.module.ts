import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { routing } from './app.routing';

import { SharedModule } from './shared/shared.module';

import { AppComponent } from './app.component';


import { TbCoreModule } from '@taskbuilder/core';
import { ReportingStudioModule } from '@taskbuilder/reporting-studio';

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
    bootstrap: [AppComponent]
})
export class AppModule { }