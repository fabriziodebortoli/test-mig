import { NgModule, ModuleWithProviders } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { TbSharedModule, Logger } from '@taskbuilder/core';
import { TbIconsModule } from '@taskbuilder/icons';

import { TbMenuModule as CoreMenu } from '@taskbuilder/core';

/** 
 * HOME 
 */
import { StandaloneDocumentComponent } from './components/home/standalone.document/standalone.document.component';
import { StandaloneReportComponent } from './components/home/standalone.report/standalone.report.component';
import { HomeSidenavLeftComponent } from './components/home/home-sidenav-left/home-sidenav-left.component';
import { HomeComponent } from './components/home/home.component';
// ---------------------------------------------------------------------------------------------------------------
export { StandaloneDocumentComponent } from './components/home/standalone.document/standalone.document.component';
export { StandaloneReportComponent } from './components/home/standalone.report/standalone.report.component';
export { HomeSidenavLeftComponent } from './components/home/home-sidenav-left/home-sidenav-left.component';
export { HomeComponent } from './components/home/home.component';

const TB_HOME_COMPONENTS = [
  HomeComponent, HomeSidenavLeftComponent,
  StandaloneDocumentComponent,
  StandaloneReportComponent
];

const NG_MODULES = [
  CommonModule,
  ReactiveFormsModule,
  FormsModule
];


@NgModule({
  imports: [
    NG_MODULES,
    CoreMenu,
    TbIconsModule,
    TbSharedModule
  ],
  declarations: [
    TB_HOME_COMPONENTS
  ],
  exports: [
    TB_HOME_COMPONENTS
  ],
  // providers: [MenuService]
})
export class TbMenuModule {
  // static forRoot(): ModuleWithProviders {
  //   return {
  //     ngModule: TbMenuModule,
  //     providers: [MenuService]
  //   };
  // }
}
