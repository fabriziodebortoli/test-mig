import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpModule } from '@angular/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { TaskbuilderCoreModule } from '@taskbuilder/core';
import { TbIconsModule } from '@taskbuilder/icons';

@NgModule({
  imports: [
    CommonModule,
    BrowserAnimationsModule,
    HttpModule,
    TaskbuilderCoreModule,
    TbIconsModule
  ],
  exports: [
    TaskbuilderCoreModule,
    TbIconsModule
  ]
})
export class SharedModule { }
