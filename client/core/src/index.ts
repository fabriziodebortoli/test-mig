import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Modulo Core con tutti i principali servizi e componenti di TB
 */
import { TbCoreModule, TB_SERVICES } from './core/core.module';
export * from './core/core.module';

/**
 * Modulo Shared
 */
import { TbSharedModule } from './shared/shared.module';
export * from './shared/shared.module';

/**
 * Modulo Menu
 */
import { TbMenuModule } from './menu/menu.module';
export * from './menu/menu.module';

const TB_MODULES = [
  TbSharedModule,
  TbMenuModule
];

@NgModule({
  imports: [CommonModule, TB_MODULES],
  exports: [TB_MODULES]
})
export class TaskbuilderCoreModule { }
