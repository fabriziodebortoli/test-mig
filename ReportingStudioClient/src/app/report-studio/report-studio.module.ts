import { WebSocketService } from './web-socket.service';
import { ReportStudioService } from './report-studio.service';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';
import { ReportStudioComponent } from './report-studio.component';
import { ToolbarComponent } from './toolbar/toolbar.component';
import { CanvasComponent } from './canvas/canvas.component';
import { ToolbarItemComponent } from './toolbar/toolbar-item/toolbar-item.component';

@NgModule({
  imports: [
    CommonModule,
    MaterialModule.forRoot(),
    RouterModule.forChild([
      { path: ':namespace', component: ReportStudioComponent }
    ])
  ],
  providers: [ReportStudioService, WebSocketService],
  declarations: [ReportStudioComponent, ToolbarComponent, CanvasComponent, ToolbarItemComponent]
})
export class ReportStudioModule { }
