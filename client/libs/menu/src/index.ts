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

/**
 * TOPBAR
 */
import { TopbarComponent } from './components/topbar/topbar.component';
import { TopbarMenuComponent } from './components/topbar/topbar-menu/topbar-menu.component';
import { TopbarMenuTestComponent } from './components/topbar/topbar-menu/topbar-menu-test/topbar-menu-test.component';
import { TopbarMenuUserComponent } from './components/topbar/topbar-menu/topbar-menu-user/topbar-menu-user.component';
import { TopbarMenuAppComponent } from './components/topbar/topbar-menu/topbar-menu-app/topbar-menu-app.component';
import { TopbarMenuElementsComponent } from './components/topbar/topbar-menu/topbar-menu-element/topbar-menu-elements.component';
import { BPMIconComponent } from './components/topbar/bpm-icon/bpm-icon.component';

export { TopbarComponent } from './components/topbar/topbar.component';
export { TopbarMenuComponent } from './components/topbar/topbar-menu/topbar-menu.component';
export { TopbarMenuTestComponent } from './components/topbar/topbar-menu/topbar-menu-test/topbar-menu-test.component';
export { TopbarMenuUserComponent } from './components/topbar/topbar-menu/topbar-menu-user/topbar-menu-user.component';
export { TopbarMenuAppComponent } from './components/topbar/topbar-menu/topbar-menu-app/topbar-menu-app.component';
export { TopbarMenuElementsComponent } from './components/topbar/topbar-menu/topbar-menu-element/topbar-menu-elements.component';
export { BPMIconComponent } from './components/topbar/bpm-icon/bpm-icon.component';

const TB_TOPBAR_COMPONENTS = [
  TopbarComponent,
  TopbarMenuComponent,
  TopbarMenuTestComponent,
  TopbarMenuUserComponent,
  TopbarMenuAppComponent,
  TopbarMenuElementsComponent,
  BPMIconComponent
];

/**
 * MENU
 */
import { FavoritesComponent } from './components/menu/favorites/favorites.component';
import { SearchComponent } from './components/menu/search/search.component';
import { MenuComponent } from './components/menu/menu.component';
import { MenuContainerComponent } from './components/menu/menu-container/menu-container.component';
import { MenuContentComponent } from './components/menu/menu-content/menu-content.component';
import { MenuElementComponent } from './components/menu/menu-element/menu-element.component';
import { ItemCustomizationsDropdownComponent } from './components/menu/menu-element/item-customizations-dropdown/item-customizations-dropdown.component';
import { MenuStepperComponent } from './components/menu/menu-stepper/menu-stepper.component';
import { MenuTabberComponent } from './components/menu/menu-tabber/menu-tabber.component';
import { MenuTabComponent } from './components/menu/menu-tabber/menu-tab/menu-tab.component';
import { MostUsedComponent } from './components/menu/most-used/most-used.component';
import { HiddenTilesComponent } from './components/menu/hidden-tiles/hidden-tiles.component';
import { ApplicationDateComponent } from './components/application-date/application-date.component';
import { ReportSnapshotDropdownComponent } from './components/menu/menu-element/report-snapshot-dropdown/report-snapshot-dropdown.component';


export { FavoritesComponent } from './components/menu/favorites/favorites.component';
export { SearchComponent } from './components/menu/search/search.component';
export { MenuComponent } from './components/menu/menu.component';
export { MenuContainerComponent } from './components/menu/menu-container/menu-container.component';
export { MenuContentComponent } from './components/menu/menu-content/menu-content.component';
export { MenuElementComponent } from './components/menu/menu-element/menu-element.component';
export { ItemCustomizationsDropdownComponent } from './components/menu/menu-element/item-customizations-dropdown/item-customizations-dropdown.component';
export { MenuStepperComponent } from './components/menu/menu-stepper/menu-stepper.component';
export { MenuTabberComponent } from './components/menu/menu-tabber/menu-tabber.component';
export { MenuTabComponent } from './components/menu/menu-tabber/menu-tab/menu-tab.component';
export { MostUsedComponent } from './components/menu/most-used/most-used.component';
export { HiddenTilesComponent } from './components/menu/hidden-tiles/hidden-tiles.component';
export { ApplicationDateComponent } from './components/application-date/application-date.component';
export { ReportSnapshotDropdownComponent } from './components/menu/menu-element/report-snapshot-dropdown/report-snapshot-dropdown.component';

const TB_MENU_COMPONENTS = [
  FavoritesComponent,
  SearchComponent,
  MenuComponent,
  MenuContainerComponent,
  MenuContentComponent,
  MenuElementComponent,
  ItemCustomizationsDropdownComponent,
  MenuStepperComponent,
  MenuTabberComponent,
  MenuTabComponent,
  MostUsedComponent,
  HiddenTilesComponent,
  ApplicationDateComponent,
  ReportSnapshotDropdownComponent
];

/**
 * MENU Services
 */
import { ImageService } from './services/image.service';
import { MenuService } from './services/menu.service';
import { HttpMenuService } from './services/http-menu.service';
import { EasystudioService } from './services/easystudio.service';

export { ImageService } from './services/image.service';
export { MenuService } from './services/menu.service';
export { HttpMenuService } from './services/http-menu.service';
export { EasystudioService } from './services/easystudio.service';

export const TB_MENU_SERVICES = [
  ImageService,
  MenuService,
  HttpMenuService,
  EasystudioService
];

/**
 * EASYSTUDIO
 */
import { EasyStudioContextComponent } from './components/easystudio-context/easystudio-context.component';
import { CloneDocumentDialogComponent } from './components/clone-document-dialog/clone-document-dialog.component';

export { EasyStudioContextComponent } from './components/easystudio-context/easystudio-context.component';
export { CloneDocumentDialogComponent } from './components/clone-document-dialog/clone-document-dialog.component';

const TB_EASYSTUDIO_COMPONENTS = [
  EasyStudioContextComponent,
  CloneDocumentDialogComponent
];

/**
 * Modulo Settings
 */
import { TbSettingsModule } from './settings/settings.module';
export * from './settings/settings.module';

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
    TbSharedModule,
    TbSettingsModule
  ],
  declarations: [
    TB_MENU_COMPONENTS,
    TB_HOME_COMPONENTS,
    TB_TOPBAR_COMPONENTS,
    TB_EASYSTUDIO_COMPONENTS
  ],
  exports: [
    TB_MENU_COMPONENTS,
    TB_HOME_COMPONENTS,
    TB_TOPBAR_COMPONENTS,
    TB_EASYSTUDIO_COMPONENTS,
    TbSettingsModule
  ],
  providers: [TB_MENU_SERVICES],
  entryComponents: [CloneDocumentDialogComponent]
})
export class TbMenuModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: TbMenuModule,
      providers: [TB_MENU_SERVICES]
    };
  }
}
