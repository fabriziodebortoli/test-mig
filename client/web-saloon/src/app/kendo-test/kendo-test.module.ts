import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { routing } from './kendo-test.routing';

import { KendoPageComponent } from './kendo-page/kendo-page.component';

@NgModule({
  imports: [
    CommonModule,
    routing
  ],
  declarations: [KendoPageComponent]
})
export class KendoTestModule { }
