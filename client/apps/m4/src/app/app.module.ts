import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { TaskbuilderCoreModule } from '@taskbuilder/core';

import { AppComponent } from './app.component';
import { TestComponent } from './test/test.component';

@NgModule({
  declarations: [
    AppComponent,
    TestComponent
  ],
  imports: [
    BrowserModule,
    TaskbuilderCoreModule.forRoot()
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
