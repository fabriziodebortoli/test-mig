import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';

import { TbIconsModule } from '@taskbuilder/core';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    TbIconsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})


export class AppModule { }
