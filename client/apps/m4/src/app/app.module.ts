import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';

import { TaskbuilderCoreModule } from '@taskbuilder/core';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    TaskbuilderCoreModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
