
import { routing } from './app.routing';

import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { MaterialModule } from '@angular/material';

import { SharedModule } from './shared/shared.module';

import { AppComponent } from './app.component';

import { MomentModule } from 'angular2-moment';

import { LayoutModule } from '@progress/kendo-angular-layout';

import { TbCoreModule } from '@taskbuilder/core';

@NgModule({
    declarations: [AppComponent],
    imports: [
        FormsModule,
        BrowserModule,
        BrowserAnimationsModule,
        HttpModule,
        MaterialModule,
        SharedModule,
        routing,
        MomentModule,
        LayoutModule,
        TbCoreModule.forRoot()
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
