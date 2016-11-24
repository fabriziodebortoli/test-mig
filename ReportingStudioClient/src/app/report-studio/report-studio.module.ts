import { WebSocketService } from './web-socket.service';
import { ReportStudioService } from './report-studio.service';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';
import { ReportStudioComponent } from './report-studio.component';
import { ToolbarComponent } from './toolbar/toolbar.component';
import { ToolbarItemComponent } from './toolbar/toolbar-item/toolbar-item.component';
import { ReportObjectWrapperComponent } from './report-object-wrapper/report-object-wrapper.component';
import { RectangleComponent } from './report-object-wrapper/report-objects/rectangle/rectangle.component';
import { ImageComponent } from './report-object-wrapper/report-objects/image/image.component';
import { TextComponent } from './report-object-wrapper/report-objects/text/text.component';
import { FileComponent } from './report-object-wrapper/report-objects/file/file.component';
import { TableComponent } from './report-object-wrapper/report-objects/table/table.component';

@NgModule({
    imports: [
        CommonModule,
        MaterialModule.forRoot(),
        RouterModule.forChild([
            { path: ':namespace', component: ReportStudioComponent }
        ])
    ],
    providers: [ReportStudioService, WebSocketService],
    declarations: [ReportStudioComponent, ToolbarComponent, ToolbarItemComponent, ReportObjectWrapperComponent, RectangleComponent, ImageComponent, TextComponent, FileComponent, TableComponent]
})
export class ReportStudioModule { }
