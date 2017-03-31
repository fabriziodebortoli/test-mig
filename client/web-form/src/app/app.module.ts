import { ReportingStudioModule } from './reporting-studio/reporting-studio.module';
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

import { HomeComponent, HomeSidenavComponent } from './home';
import { UnsupportedFactoryComponent, UnsupportedComponent } from './unsupported.component';
import { AppComponent } from './app.component';
import { appRouting } from './applications/app.routing';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent, HomeSidenavComponent,
    UnsupportedFactoryComponent,
    UnsupportedComponent
  ],
  imports: [
    FormsModule,
    BrowserModule,
    BrowserAnimationsModule,
    HttpModule,
    MaterialModule.forRoot(),
    CoreModule.forRoot(),
    SharedModule,
    MenuModule.forRoot(),
    ReportingStudioModule.forRoot(),
    appRouting,
    routing
  ],
  bootstrap: [AppComponent],
  entryComponents: [
    UnsupportedComponent
  ]
})
export class AppModule { }
