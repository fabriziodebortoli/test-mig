import { SharedModule } from 'tb-shared';
import { MenuModule } from './menu/menu.module';
import { FormsModule } from '@angular/forms';
import { HomeComponent } from './home.component';
import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';
import { BrowserModule } from '@angular/platform-browser';
import { routing } from './app.routing';

import { MaterialModule } from '@angular/material';

import { CoreModule, SidenavService } from 'tb-core';

import { LibraryModule } from 'web-library';

import { AppComponent } from './app.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent
  ],
  imports: [
    FormsModule,
    BrowserModule,
    HttpModule,
    MaterialModule.forRoot(),
    CoreModule.forRoot(),
    LibraryModule.forRoot(),
    SharedModule,
    MenuModule,
    routing
  ],
  bootstrap: [AppComponent],
  providers: [SidenavService]
})
export class AppModule { }
