import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Modulo Core con tutti i principali servizi e componenti di TB
 */
import { TbCoreModule, TB_SERVICES } from './core/core.module';
export * from './core/core.module';
// export * from './core';

/**
 * Modulo Icon Font
 */
import { TbIconsModule } from './icons/icons.module';
export * from './icons/icons.module';

const TB_MODULES = [
  // TbCoreModule,
  TbIconsModule
];

export * from './shared/models';
export { SocketConnectionStatus } from './shared';

@NgModule({
  imports: [CommonModule, TB_MODULES],
  exports: [TB_MODULES]
})
export class TaskbuilderCoreModule { }
