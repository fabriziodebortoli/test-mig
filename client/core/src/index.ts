import { NgModule } from '@angular/core';

import { TbIconsModule } from './icons/icons.module';
export * from './icons/icons.module';

const TASKBUILDER_MODULES = [
  TbIconsModule
];

@NgModule({
  imports: [TASKBUILDER_MODULES],
  exports: [TASKBUILDER_MODULES]
})
export class TaskbuilderCoreModule { }
