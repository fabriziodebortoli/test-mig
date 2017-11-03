import { NgModule, ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { TbIconsModule } from '@taskbuilder/icons';
import { TbSharedModule } from '@taskbuilder/core';

import { BPMService } from './bpm.service';
export { BPMService } from './bpm.service';

import { BPMPageComponent, BPMPageFactoryComponent } from './bpm-page/bpm-page.component';
export { BPMPageComponent, BPMPageFactoryComponent } from './bpm-page/bpm-page.component';

@NgModule({
  imports: [
    CommonModule,
    TbSharedModule,
    TbIconsModule,
    RouterModule.forChild([
      { path: 'page', component: BPMPageFactoryComponent }
    ])
  ],
  declarations: [BPMPageComponent, BPMPageFactoryComponent],
  exports: [BPMPageComponent, BPMPageFactoryComponent],
  entryComponents: [BPMPageComponent, BPMPageFactoryComponent]
})
export class BPMModule { }
