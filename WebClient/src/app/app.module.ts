import { FormsModule } from '@angular/forms';
import { HomeComponent } from './home.component';
import { ComponentService } from './core/services/component.service';
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
    CoreModule,
    routing
  ],
  bootstrap: [AppComponent],
  providers: [appRoutingProviders, ComponentService]
})
export class AppModule { }
