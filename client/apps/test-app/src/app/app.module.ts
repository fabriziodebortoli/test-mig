import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { TbIconsModule } from '@taskbuilder/icons';
import { TbCoreModule, TaskbuilderCoreModule } from '@taskbuilder/core';

import { AppComponent } from './app.component';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    TaskbuilderCoreModule,
    TbCoreModule.forRoot(),
    TbIconsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
