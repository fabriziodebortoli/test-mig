import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { TaskbuilderCoreModule, TbCoreModule } from '@taskbuilder/core';

import { AppComponent } from './app.component';
import { TestComponent } from './test/test.component';

import 'hammerjs';

import { routing } from './app.routing';

@NgModule({
  declarations: [
    AppComponent,
    TestComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    TaskbuilderCoreModule,
    TbCoreModule.forRoot(),
    routing
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
