import { ReportingStudioModule } from './reporting-studio/reporting-studio.module';
import { HomeComponent } from './home.component';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';

import { MaterialModule } from '@angular/material';

import { AppComponent } from './app.component';
import { routing } from './app.routing';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent
  ],
  imports: [
    BrowserModule,
    HttpModule,
    MaterialModule.forRoot(),
    routing,

    ReportingStudioModule
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
