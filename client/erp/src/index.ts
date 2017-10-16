import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ERP_SERVICES } from './core/core.module';

import { TbSharedModule } from '@taskbuilder/core';
export * from './core/core.module';

import { VatComponent } from './shared/controls/vat/vat.component';
export { VatComponent } from './shared/controls/vat/vat.component';

import { DialogModule } from '@progress/kendo-angular-dialog';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { PopupModule } from '@progress/kendo-angular-popup';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { GridModule, GridComponent } from '@progress/kendo-angular-grid';
import { ChartsModule } from '@progress/kendo-angular-charts';

const KENDO_UI_MODULES = [
    GridModule,
    ChartsModule,
    DialogModule,
    DateInputsModule,
    DropDownsModule,
    InputsModule,
    LayoutModule,
    PopupModule,
    ButtonsModule
];
/**
 * Modulo Shared
 */
import { ERPSharedModule } from './shared/shared.module';
export * from './shared/shared.module';

const ERP_MODULES = [ERPSharedModule, ERPCoreModule];


@NgModule({
  imports: [
    CommonModule,
    TbSharedModule,
    KENDO_UI_MODULES
    ERP_MODULES
  ],
  declarations: [
  ],
  exports: [
    VatComponent
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
