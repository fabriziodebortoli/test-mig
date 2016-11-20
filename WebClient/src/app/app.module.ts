import { SharedModule } from './shared/shared.module';
import { MenuModule } from './menu/menu.module';
import { FormsModule } from '@angular/forms';
import { HomeComponent } from './home.component';
import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';
import { BrowserModule } from '@angular/platform-browser';
import { routing, appRoutingProviders } from './app.routing';

import { MaterialModule } from '@angular/material';

import { CoreModule } from './core/core.module';

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
    SharedModule,
    MenuModule,
    routing
  ],
  bootstrap: [AppComponent],
  providers: [appRoutingProviders]
})
export class AppModule { }
