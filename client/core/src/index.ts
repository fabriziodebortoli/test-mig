import { NgModule, ModuleWithProviders } from '@angular/core';

/**
 * Modulo Core con tutti i principali servizi e componenti di TB
 */
import { TbCoreModule } from './core/core.module';
export * from './core/core.module';

/**
 * Modulo Shared con tutti i moduli condivisi (Common, Form, Router, Material, Kendo)
 */
import { TbSharedModule } from './shared/shared.module';
export * from './shared/shared.module';

/**
 * Modulo Menu
 */
import { TbMenuModule } from './menu/menu.module';
export * from './menu/menu.module';

/**
 * Modulo Icon Font
 */
import { TbIconsModule } from './icons/icons.module';
export * from './icons/icons.module';

const TB_MODULES = [
  TbCoreModule,
  TbSharedModule,
  TbMenuModule,
  TbIconsModule
];

/**
 * Modulo principale della libreria ngTaskbuilder
 */
@NgModule({
  imports: [TB_MODULES],
  exports: [TB_MODULES],
})
export class TaskbuilderCoreModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: TaskbuilderCoreModule,
      // providers: [TB_SERVICES]
    };
  }
}
