import { NgModule, ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { TbIconsModule } from '@taskbuilder/icons';
import { TbSharedModule } from '@taskbuilder/core';

import { ESService } from './es.service';
export { ESService } from './es.service';

import { ESPageComponent, ESPageFactoryComponent } from './es-page/es-page.component';
export { ESPageComponent, ESPageFactoryComponent } from './es-page/es-page.component';

import { ESStandaloneComponent } from './es-standalone/es-standalone.component';
export { ESStandaloneComponent } from './es-standalone/es-standalone.component';

import { ESComponent } from './es/es.component';
export { ESComponent } from './es/es.component';

@NgModule({
  imports: [
    CommonModule,
    TbSharedModule,
    TbIconsModule,
    RouterModule.forChild([
      { path: 'page', component: ESPageFactoryComponent }
    ])
  ],
  declarations: [ESPageComponent, ESPageFactoryComponent, ESStandaloneComponent, ESComponent],
  exports: [ESPageComponent, ESPageFactoryComponent, ESStandaloneComponent, ESComponent],
  entryComponents: [ESPageComponent, ESPageFactoryComponent, ESStandaloneComponent, ESComponent]
})
export class ESModule {}
