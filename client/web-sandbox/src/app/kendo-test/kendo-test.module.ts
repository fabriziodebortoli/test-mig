import { SharedModule } from './../shared/shared.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { routing } from './kendo-test.routing';

import { KendoPageComponent } from './kendo-page/kendo-page.component';

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    routing
  ],
  declarations: [KendoPageComponent]
})
export class KendoTestModule { }
