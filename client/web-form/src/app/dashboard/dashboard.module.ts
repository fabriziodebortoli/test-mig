import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SharedModule } from '../shared/shared.module';
import { MenuModule } from '../menu/menu.module';

import { DashboardComponent } from './dashboard/dashboard.component';

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    MenuModule
  ],
  exports:[
    DashboardComponent
  ],
  declarations: [DashboardComponent]
})
export class DashboardModule { }
