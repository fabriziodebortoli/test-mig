import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';
import { RouterModule } from '@angular/router';

import { SharedModule } from '../shared/shared.module';

import { Logger } from '@taskbuilder/core';

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    MaterialModule,
  ],

  declarations:
  [
  ],
  exports:
  [
    RouterModule,
  ]
})
export class MenuModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: MenuModule
    };
  }

  constructor(private logger: Logger) {
    this.logger.debug('MenuModule instantiated - ' + Math.round(new Date().getTime() / 1000));
  }
}

