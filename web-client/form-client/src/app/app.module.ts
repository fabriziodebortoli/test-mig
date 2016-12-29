import { HomeComponent, HomeSidenavComponent } from './home';
import { UnsupportedFactoryComponent, UnsupportedComponent } from './unsupported.component';
import { SharedModule } from 'tb-shared';
import { MenuModule } from './menu/menu.module';
import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';
import { BrowserModule } from '@angular/platform-browser';
import { routing } from './app.routing';

import { MaterialModule } from '@angular/material';

import { CoreModule } from 'tb-core';
import { SidenavService } from './core/sidenav.service';

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
    MenuModule,
    routing
  ],
  bootstrap: [AppComponent],
  providers: [SidenavService],
  entryComponents: [UnsupportedComponent]
})
export class AppModule { }
