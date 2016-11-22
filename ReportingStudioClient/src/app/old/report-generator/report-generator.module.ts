import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ReportGeneratorComponent } from './report-generator.component';
import { ReportObjectComponent } from './report-object-wrapper/report-object.component';

@NgModule({
  imports: [
    CommonModule
  ],
  declarations: [ReportGeneratorComponent, ReportObjectComponent],
  exports: [ReportGeneratorComponent]
})
export class ReportGeneratorModule { }
