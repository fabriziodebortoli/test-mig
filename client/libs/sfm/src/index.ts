import { NgModule, ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { TbIconsModule } from '@taskbuilder/icons';
import { TbSharedModule } from '@taskbuilder/core';

import { SFMService } from './sfm.service';
export { SFMService } from './sfm.service';

import { SFMPageComponent, SFMPageFactoryComponent } from './sfm-page/sfm-page.component';
export { SFMPageComponent, SFMPageFactoryComponent } from './sfm-page/sfm-page.component';

import { SFMStandaloneComponent } from './sfm-standalone/sfm-standalone.component';
export { SFMStandaloneComponent } from './sfm-standalone/sfm-standalone.component';

@NgModule({
  imports: [
    CommonModule,
    TbSharedModule,
    TbIconsModule,
    RouterModule.forChild([
      { path: 'page', component: SFMPageFactoryComponent }
    ])
  ],
  declarations: [SFMPageComponent, SFMPageFactoryComponent, SFMStandaloneComponent],
  exports: [SFMPageComponent, SFMPageFactoryComponent, SFMStandaloneComponent],
  entryComponents: [SFMPageComponent, SFMPageFactoryComponent, SFMStandaloneComponent]
})
export class SFMModule {}
