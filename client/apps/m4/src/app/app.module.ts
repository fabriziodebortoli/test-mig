import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { TaskbuilderCoreModule, TbCoreModule } from '@taskbuilder/core';

import { AppComponent } from './app.component';
import { TestComponent } from './test/test.component';

import 'hammerjs';

@NgModule({
  declarations: [
    AppComponent,
    TestComponent
  ],
  imports: [
    BrowserModule,
    TaskbuilderCoreModule,
    TbCoreModule.forRoot()
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
