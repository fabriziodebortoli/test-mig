import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { routing } from './app.routing';

import { SharedModule } from './shared/shared.module';

import { AppComponent } from './app.component';


import { TbCoreModule } from '@taskbuilder/core';

@NgModule({
    declarations: [AppComponent],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        SharedModule,
        routing,
        TbCoreModule.forRoot()
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }