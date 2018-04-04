import { NgModule, ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { TbIconsModule } from '@taskbuilder/icons';
import { TbSharedModule } from '@taskbuilder/core';

import { ESPService } from './esp.service';
export { ESPService } from './esp.service';

import { ESPPageComponent, ESPPageFactoryComponent } from './esp-page/esp-page.component';
export { ESPPageComponent, ESPPageFactoryComponent } from './esp-page/esp-page.component';

import { ESPStandaloneComponent } from './esp-standalone/esp-standalone.component';
export { ESPStandaloneComponent } from './esp-standalone/esp-standalone.component';

@NgModule({
  imports: [
    CommonModule,
    TbSharedModule,
    TbIconsModule,
    RouterModule.forChild([
      { path: 'page', component: ESPPageFactoryComponent }
    ])
  ],
  declarations: [ESPPageComponent, ESPPageFactoryComponent, ESPStandaloneComponent],
  exports: [ESPPageComponent, ESPPageFactoryComponent, ESPStandaloneComponent],
  entryComponents: [ESPPageComponent, ESPPageFactoryComponent, ESPStandaloneComponent]
})
export class ESPModule {}
