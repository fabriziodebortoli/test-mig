import { SharedModule } from 'tb-shared';
import { LanguagesFactoryComponent, LanguagesComponent } from './languages/languages.component';
import { routing } from './languages.routing';
import { NgModule } from '@angular/core';

@NgModule({
  imports: [
    SharedModule,
    routing
  ],
  declarations: [LanguagesFactoryComponent, LanguagesComponent],
  exports: [LanguagesFactoryComponent],
  entryComponents: [LanguagesComponent]
})
export class LanguagesModule { }
