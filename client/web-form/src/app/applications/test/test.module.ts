import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';

import { SharedModule } from './../../shared/shared.module';

import { LanguagesFactoryComponent, LanguagesComponent } from './languages/IDD_LANGUAGES.component';

@NgModule({
  imports: [
    SharedModule,
    RouterModule.forChild([
    {
        path: 'IDD_LANGUAGES_FRAME', component: LanguagesFactoryComponent,
    }])
  ],
  declarations: [LanguagesFactoryComponent, LanguagesComponent],
  exports: [LanguagesFactoryComponent],
  entryComponents: [LanguagesComponent]
})
export class TestModule { }
