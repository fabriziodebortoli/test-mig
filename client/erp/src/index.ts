import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Modulo Core con tutti i servizi di ERP
 */
import { ERPCoreModule, ERP_SERVICES } from './core/core.module';
export * from './core/core.module';

/**
 * Modulo Shared
 */
import { ERPSharedModule } from './shared/shared.module';
export * from './shared/shared.module';

const ERP_MODULES = [ERPSharedModule, ERPCoreModule];


@NgModule({
  imports: [
    CommonModule,
    ERP_MODULES
  ],
  declarations: [
  ],
  exports: [
    ERP_MODULES
  ]
})
export class ERPModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: ERPModule,
      providers: [ERP_SERVICES]
    };
  }
}
