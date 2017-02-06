import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';

import { MaterialModule } from '@angular/material';

import { CoreModule } from './core/core.module';
import { SharedModule } from './shared/shared.module';
import { MenuModule } from './menu/menu.module';

import { SidenavService } from './core/sidenav.service';

import { routing } from './app.routing';
import { appRouting } from './applications/app.routing';

import { HomeComponent, HomeSidenavComponent } from './home';
import { UnsupportedFactoryComponent, UnsupportedComponent } from './unsupported.component';
import { AppComponent } from './app.component';

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
    HttpModule,
    MaterialModule.forRoot(),
    CoreModule.forRoot(),
    SharedModule,
    MenuModule.forRoot(),
    routing,
    appRouting
  ],
  bootstrap: [AppComponent],
  entryComponents: [
    UnsupportedComponent
  ]
})
export class AppModule { }
