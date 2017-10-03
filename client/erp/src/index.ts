import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Modulo Core con tutti i servizi di ERP
 */
import { ERPCoreModule, ERP_SERVICES } from './core/core.module';
export * from './core/core.module';

import { ERPTestComponent } from './erp-test.component';
export { ERPTestComponent } from './erp-test.component';

@NgModule({
  imports: [
    CommonModule
  ],
  declarations: [
    ERPTestComponent
  ],
  exports: [
    ERPTestComponent
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
