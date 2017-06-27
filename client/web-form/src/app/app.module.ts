
import { routing } from './app.routing';

import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { MaterialModule } from '@angular/material';

import { CoreModule } from './core/core.module';
import { SharedModule } from './shared/shared.module';
import { MenuModule } from './menu/menu.module';
import { DashboardModule } from './dashboard/dashboard.module';

import { HomeComponent, HomeSidenavComponent } from './home';
import { UnsupportedFactoryComponent, UnsupportedComponent } from './unsupported.component';
import { AppComponent } from './app.component';

import { MomentModule } from 'angular2-moment';
import { ProxyRouteComponent } from './proxy-route/proxy-route.component';

import { LayoutModule } from '@progress/kendo-angular-layout';
import { StandaloneDocumentComponent } from './home/standalone.document/standalone.document.component';
import { StandaloneReportComponent } from './home/standalone.report/standalone.report.component';

import { TbCoreModule } from '@taskbuilder/core';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent, HomeSidenavComponent,
    UnsupportedFactoryComponent,
    UnsupportedComponent,
    ProxyRouteComponent,
    StandaloneDocumentComponent,
    StandaloneReportComponent

  ],
  imports: [
    FormsModule,
    BrowserModule,
    BrowserAnimationsModule,
    HttpModule,
    MaterialModule,
    CoreModule.forRoot(),
    SharedModule,
    MenuModule.forRoot(),
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
