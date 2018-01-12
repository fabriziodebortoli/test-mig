import { ReportGaugeComponent } from './report-objects/layout/gauge/gauge.component';
import { BarcodeComponent } from './report-objects/layout/barcode/barcode.component';
import { ReportChartPolarComponent } from './report-objects/layout/chart/chart-polar/chart-polar.component';
import { ReportChartPieComponent } from './report-objects/layout/chart/chart-pie/chart-pie.component';
import { ReportChartComponent } from './report-objects/layout/chart/chart.component';
import { ReportChartBarComponent } from './report-objects/layout/chart/chart-bar/chart-bar.component';
import { ReportChartRangeBarComponent } from './report-objects/layout/chart/chart-range-bar/chart-range-bar.component';
import { ReportChartBubbleComponent } from './report-objects/layout/chart/chart-bubble/chart-bubble.component';
import { ReportChartRadarComponent } from './report-objects/layout/chart/chart-radar/chart-radar.component';
import { AskTextComponent } from './report-objects/askdialog/ask-text/ask-text.component';
import { AskRadioComponent } from './report-objects/askdialog/ask-radio/ask-radio.component';
import { AskHotlinkComponent } from './report-objects/askdialog/ask-hotlink/ask-hotlink.component';
import { AskGroupComponent } from './report-objects/askdialog/ask-group/ask-group.component';
import { AskDropdownlistComponent } from './report-objects/askdialog/ask-dropdownlist/ask-dropdownlist.component';
import { AskCheckComponent } from './report-objects/askdialog/ask-check/ask-check.component';
import { ReportRectComponent } from './report-objects/layout/rect/rect.component';
import { ReportLinkComponent } from './report-objects/layout/link/link.component';
import { ReportImageComponent } from './report-objects/layout/image/image.component';
import { ReportTextrectComponent } from './report-objects/layout/textrect/textrect.component';
import { ReportTableComponent } from './report-objects/layout/table/table.component';
import { ReportFieldrectComponent } from './report-objects/layout/fieldrect/fieldrect.component';
import { ReportingStudioComponent, ReportingStudioFactoryComponent } from './reporting-studio.component';
import { AskdialogComponent } from './report-objects/askdialog/askdialog.component';
import { ReportLayoutComponent } from './report-objects/layout/layout.component';
import { ReportingStudioService } from './reporting-studio.service';
import { RsExportService } from './rs-export.service';
import { AskdialogService } from './report-objects/askdialog/askdialog.service';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TbSharedModule, ComponentService, WebSocketService, HttpService, UtilsService, Logger } from '@taskbuilder/core';
import { RsTestComponent } from './rs-test.component';
import { RouterModule } from "@angular/router";
import { TbCoreModule } from "@taskbuilder/core";
export { ReportingStudioComponent, ReportingStudioFactoryComponent } from './reporting-studio.component';
export { ReportingStudioService } from './reporting-studio.service';
export { RsExportService } from './rs-export.service';
export { AskdialogService } from './report-objects/askdialog/askdialog.service';
import { ExportdialogComponent } from './report-objects/exportdialog/exportdialog.component';
import { SnapshotdialogComponent } from './report-objects/snapshotdialog/snapshotdialog.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

export * from './rs-test.component';

export { barcode } from './models/barcode.model';
export { chart, series } from './models/chart.model';
export { PdfType, SvgType, PngType } from './models/export-type.model';
export { column } from './models/column.model';
export { link } from './models/link.model';
export { graphrect } from './models/graphrect.model';
export { fieldrect } from './models/fieldrect.model';
export { textrect } from './models/textrect.model';
export { table } from './models/table.model';
export { sqrrect } from './models/sqrrect.model';
export { baseobj } from './models/baseobj.model';
export { repeater } from './models/repeater.model';
export { TemplateItem } from './models/template-item.model';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    TbSharedModule,
    RouterModule.forChild([
      { path: 'reportingstudio/:ns/:params', component: ReportingStudioFactoryComponent },
      { path: 'reportingstudio/', component: ReportingStudioFactoryComponent },
    ])
  ],
  declarations: [
    RsTestComponent,
    ReportLayoutComponent,
    AskdialogComponent,
    ExportdialogComponent,
    SnapshotdialogComponent,
    ReportingStudioFactoryComponent,
    ReportingStudioComponent,
    ReportChartComponent,
    ReportChartPieComponent,
    ReportChartBarComponent,
    ReportChartRangeBarComponent,
    ReportChartBubbleComponent,
    ReportChartRadarComponent,
    ReportChartPolarComponent,
    ReportFieldrectComponent,
    ReportTableComponent,
    ReportTextrectComponent,
    ReportImageComponent,
    ReportLinkComponent,
    ReportRectComponent,
    ReportGaugeComponent,
    AskCheckComponent,
    AskDropdownlistComponent,
    AskGroupComponent,
    AskHotlinkComponent,
    AskRadioComponent,
    AskTextComponent,
    BarcodeComponent

  ],
  exports: [
    RsTestComponent,
    ReportLayoutComponent,
    AskdialogComponent,
    ExportdialogComponent,
    ReportingStudioFactoryComponent,
    ReportingStudioComponent,
  ],
  entryComponents:
  [
    ReportingStudioComponent
  ]
})

export class ReportingStudioModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: ReportingStudioModule
    };
  }
}
