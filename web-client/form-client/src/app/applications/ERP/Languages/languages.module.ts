import { NgModule } from '@angular/core';

import { SharedModule } from './../../../shared/shared.module';

import { LanguagesFactoryComponent, LanguagesComponent } from './languages/IDD_LANGUAGES.component';
import { routing } from './languages.routing';

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
