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
  TbCoreModule,
  TbIconsModule
];

@NgModule({
  imports: [CommonModule, TB_MODULES]
})
export class TaskbuilderCoreModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: TaskbuilderCoreModule,
      providers: [TB_SERVICES]
    };
  }
}
