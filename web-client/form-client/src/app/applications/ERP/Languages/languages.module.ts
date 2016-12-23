import { LanguagesFactoryComponent, LanguagesComponent } from './languages/languages.component';
import { routing } from './languages.routing';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'tb-shared';

@NgModule({
  imports: [
    SharedModule,
    CommonModule,
    routing
  ],
  declarations: [LanguagesFactoryComponent, LanguagesComponent],
  exports: [LanguagesFactoryComponent],
  entryComponents: [LanguagesComponent]
})
export class LanguagesModule { }
