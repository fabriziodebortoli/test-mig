import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { TbIconsModule } from '@taskbuilder/icons';

import { AppComponent } from './app.component';

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
