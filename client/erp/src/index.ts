import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ERP_SERVICES } from './core/core.module';

import { TbLibsModule } from '@taskbuilder/libs';

import { TbSharedModule } from '@taskbuilder/core';
export * from './core/core.module';

/**
 * Modulo Shared
 */
import { ERPSharedModule } from './shared/shared.module';
export * from './shared/shared.module';

const ERP_MODULES = [ERPSharedModule];


@NgModule({
  imports: [
    CommonModule,
    TbLibsModule,
    TbSharedModule,
    ERP_MODULES
  ],
  declarations: [
  ],
  exports: [
    ERP_MODULES
  ],
  providers: [ERP_SERVICES]
})
export class ERPModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: ERPModule,
      providers: [ERP_SERVICES]
    };
  }
}
