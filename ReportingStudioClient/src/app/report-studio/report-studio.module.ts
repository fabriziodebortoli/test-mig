import { RouterModule } from '@angular/router';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';

import { WebSocketService } from './web-socket.service';

import { ToolbarComponent } from './toolbar/toolbar.component';
import { ToolbarItemComponent } from './toolbar/toolbar-item/toolbar-item.component';

import { ReportStudioService } from './report-studio.service';
import { ReportStudioComponent } from './report-studio.component';
import { ReportObjectWrapperComponent } from './report-object-wrapper/report-object-wrapper.component';
import { ReportObjectDirective } from './report-object-wrapper/report-object.directive';
import {
  ReportObjectRectangleComponent, ReportObjectImageComponent,
  ReportObjectFileComponent, ReportObjectTableComponent, ReportObjectTextComponent
} from './report-object-wrapper/report-objects';

@NgModule({
  imports: [
    CommonModule,
    MaterialModule.forRoot(),
    RouterModule.forChild([
      { path: ':namespace', component: ReportStudioComponent }
    ])
  ],
  providers: [ReportStudioService, WebSocketService],
  declarations: [ReportStudioComponent, ToolbarComponent, ToolbarItemComponent, ReportObjectWrapperComponent,
    ReportObjectRectangleComponent, ReportObjectImageComponent, ReportObjectTextComponent,
    ReportObjectFileComponent, ReportObjectTableComponent, ReportObjectDirective]
})
export class ReportStudioModule {

  static forRoot(): ModuleWithProviders {
    return { ngModule: ReportStudioModule };
  }
}
