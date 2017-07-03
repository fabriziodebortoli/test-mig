
import { routing } from './app.routing';

import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { MaterialModule } from '@angular/material';

import { SharedModule } from './shared/shared.module';
import { DashboardModule } from './dashboard/dashboard.module';

import { UnsupportedFactoryComponent, UnsupportedComponent } from './unsupported.component';
import { AppComponent } from './app.component';

import { MomentModule } from 'angular2-moment';

import { LayoutModule } from '@progress/kendo-angular-layout';

import { TbCoreModule } from '@taskbuilder/core';

@NgModule({
    declarations: [
        AppComponent,
        UnsupportedFactoryComponent,
        UnsupportedComponent,

    ],
    imports: [
        FormsModule,
        BrowserModule,
        BrowserAnimationsModule,
        HttpModule,
        MaterialModule,
        SharedModule,
        DashboardModule.forRoot(),
        routing,
        MomentModule,
        LayoutModule,
        TbCoreModule.forRoot()
    ],
    bootstrap: [AppComponent],
    entryComponents: [
        UnsupportedComponent
    ]
})
export class AppModule { }
