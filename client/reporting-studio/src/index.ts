import { UrlService } from '@taskbuilder/core';
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
import { AskdialogService } from './report-objects/askdialog/askdialog.service';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TbSharedModule, ComponentService, WebSocketService, HttpService, UtilsService, Logger } from '@taskbuilder/core';
import { RsTestComponent } from './rs-test.component';
import { ExcelModule } from '@progress/kendo-angular-grid'
import { RouterModule } from "@angular/router";
import { TbCoreModule } from "@taskbuilder/core";
export { ReportingStudioComponent, ReportingStudioFactoryComponent } from './reporting-studio.component';
export { ReportingStudioService } from './reporting-studio.service';
export { AskdialogService } from './report-objects/askdialog/askdialog.service';

export * from './rs-test.component';
export * from './models';
export * from './report-objects';

@NgModule({
  imports: [
    CommonModule,
    TbSharedModule,
    ExcelModule,
    RouterModule.forChild([
      { path: 'reportingstudio/:ns/:params', component: ReportingStudioFactoryComponent },
      { path: 'reportingstudio/', component: ReportingStudioFactoryComponent },
    ])
  ],
  declarations: [
    RsTestComponent,
    ReportLayoutComponent,
    AskdialogComponent,
    ReportingStudioFactoryComponent,
    ReportingStudioComponent,
    ReportFieldrectComponent,
    ReportTableComponent,
    ReportTextrectComponent,
    ReportImageComponent,
    ReportLinkComponent,
    ReportRectComponent,
    AskCheckComponent,
    AskDropdownlistComponent,
    AskGroupComponent,
    AskHotlinkComponent,
    AskRadioComponent,
    AskTextComponent

  ],
  exports: [
    RsTestComponent,
    ReportLayoutComponent,
    AskdialogComponent,
    ReportingStudioFactoryComponent,
    ReportingStudioComponent,
  ],
  entryComponents:
  [
    ReportingStudioComponent
  ],

  providers: [
    /* ComponentService,
     WebSocketService,
     HttpService,
     UtilsService,
     Logger,
     UrlService,
     */
    CookieService
  ]
})

export class ReportingStudioModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: ReportingStudioModule,


    };
  }
}
