import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from '@angular/material';
import { RouterModule } from '@angular/router';
import { HttpModule } from '@angular/http';

import { MasonryModule } from 'angular2-masonry';

import { TbIconsModule } from '@taskbuilder/icons';

import { DialogModule } from '@progress/kendo-angular-dialog';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { PopupModule } from '@progress/kendo-angular-popup';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { GridModule } from '@progress/kendo-angular-grid';
import { ChartsModule } from '@progress/kendo-angular-charts';
import { TreeModule } from 'angular-tree-component';



const KENDO_UI_MODULES = [
    GridModule,
    ChartsModule,
    DialogModule,
    DateInputsModule,
    DropDownsModule,
    InputsModule,
    LayoutModule,
    PopupModule,
    ButtonsModule
];

/**
 * Components
 */

import { ProxyRouteComponent } from './components/proxy-route/proxy-route.component';
import { DynamicCmpComponent } from './components/dynamic-cmp.component';
import { DynamicCmpComponentTree } from './components/dynamic-cmp.component';
import { ContextMenuComponent } from './components/context-menu/context-menu.component';
import { DocumentComponent } from './components/document.component';
import { PageNotFoundComponent } from './components/page-not-found.component';
import { HeaderStripComponent } from './components/header-strip/header-strip.component';
import { ToolbarTopComponent } from './components/toolbar/toolbar-top/toolbar-top.component';
import { ToolbarTopButtonComponent } from './components/toolbar/toolbar-top/toolbar-top-button/toolbar-top-button.component';
import { ToolbarSeparatorComponent } from './components/toolbar/toolbar-top/toolbar-separator.component';
import { ToolbarBottomComponent } from './components/toolbar/toolbar-bottom/toolbar-bottom.component';
import { ToolbarBottomButtonComponent } from './components/toolbar/toolbar-bottom/toolbar-bottom-button/toolbar-bottom-button.component';
import { TopbarComponent } from './components/topbar/topbar.component';
import { TopbarMenuComponent } from './components/topbar/topbar-menu/topbar-menu.component';
import { TopbarMenuTestComponent } from './components/topbar/topbar-menu/topbar-menu-test/topbar-menu-test.component';
import { TopbarMenuUserComponent } from './components/topbar/topbar-menu/topbar-menu-user/topbar-menu-user.component';
import { TopbarMenuAppComponent } from './components/topbar/topbar-menu/topbar-menu-app/topbar-menu-app.component';
import { TopbarMenuElementsComponent } from './components/topbar/topbar-menu/topbar-menu-element/topbar-menu-elements.component';
import { UnsupportedComponent } from './components/unsupported.component';
import { UnsupportedFactoryComponent } from './components/unsupported.component';
import { OpenComponent } from './components/explorer/open/open.component';
import { SaveComponent } from './components/explorer/save/save.component';
import { RadarComponent } from './components/radar/radar.component';
import { CultureSelectorComponent } from './components/culture-selector/culture-selector.component';
import { TbIconComponent } from './components/tb-icon/tb-icon.component';

// import { TbComponent } from './components/tb.component';
export { TbComponent } from './components/tb.component';
import { BOComponent } from './components/bo.component';
export { BOComponent } from './components/bo.component';

export { ProxyRouteComponent } from './components/proxy-route/proxy-route.component';
export { DynamicCmpComponent } from './components/dynamic-cmp.component';
export { DynamicCmpComponentTree } from './components/dynamic-cmp.component';
export { ContextMenuComponent } from './components/context-menu/context-menu.component';
export { DocumentComponent } from './components/document.component';
export { PageNotFoundComponent } from './components/page-not-found.component';
export { HeaderStripComponent } from './components/header-strip/header-strip.component';
export { ToolbarTopComponent } from './components/toolbar/toolbar-top/toolbar-top.component';
export { ToolbarTopButtonComponent } from './components/toolbar/toolbar-top/toolbar-top-button/toolbar-top-button.component';
export { ToolbarSeparatorComponent } from './components/toolbar/toolbar-top/toolbar-separator.component';
export { ToolbarBottomComponent } from './components/toolbar/toolbar-bottom/toolbar-bottom.component';
export { ToolbarBottomButtonComponent } from './components/toolbar/toolbar-bottom/toolbar-bottom-button/toolbar-bottom-button.component';
export { TopbarComponent } from './components/topbar/topbar.component';
export { TopbarMenuComponent } from './components/topbar/topbar-menu/topbar-menu.component';
export { TopbarMenuTestComponent } from './components/topbar/topbar-menu/topbar-menu-test/topbar-menu-test.component';
export { TopbarMenuUserComponent } from './components/topbar/topbar-menu/topbar-menu-user/topbar-menu-user.component';
export { TopbarMenuAppComponent } from './components/topbar/topbar-menu/topbar-menu-app/topbar-menu-app.component';
export { TopbarMenuElementsComponent } from './components/topbar/topbar-menu/topbar-menu-element/topbar-menu-elements.component';
export { UnsupportedComponent } from './components/unsupported.component';
export { UnsupportedFactoryComponent } from './components/unsupported.component';
export { OpenComponent } from './components/explorer/open/open.component';
export { SaveComponent } from './components/explorer/save/save.component';
export { RadarComponent } from './components/radar/radar.component';
export { CultureSelectorComponent } from './components/culture-selector/culture-selector.component';
export { TbIconComponent } from './components/tb-icon/tb-icon.component';

const TB_COMPONENTS = [
    ProxyRouteComponent, DynamicCmpComponent, DynamicCmpComponentTree, ContextMenuComponent, DocumentComponent, PageNotFoundComponent, HeaderStripComponent,
    ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent,
    TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent, TopbarMenuElementsComponent,
    UnsupportedComponent, UnsupportedFactoryComponent, OpenComponent, SaveComponent, RadarComponent, CultureSelectorComponent, TbIconComponent
];

/**
 * Containers - Contenitori di struttura della pagina derivati dalla versione desktop
 */
import {
    FrameComponent, FrameContentComponent, ViewComponent, ViewContainerComponent,
    DockpaneComponent, DockpaneContainerComponent,
    TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, LayoutContainerComponent,
    MessageDialogComponent, DiagnosticDialogComponent, DiagnosticItemComponent, DynamicDialogComponent, TabberComponent, TabComponent,
    TbCardComponent, TbCardTitleComponent, TbCardSubtitleComponent, TbCardHeaderComponent, TbCardFooterComponent, TbCardContentComponent
} from './containers';
export * from './containers';

const TB_CONTAINERS = [
    FrameComponent, FrameContentComponent, ViewComponent, ViewContainerComponent,
    DockpaneComponent, DockpaneContainerComponent,
    TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, LayoutContainerComponent,
    MessageDialogComponent, DiagnosticDialogComponent, DiagnosticItemComponent, DynamicDialogComponent, TabberComponent, TabComponent,
    TbCardComponent, TbCardTitleComponent, TbCardSubtitleComponent, TbCardHeaderComponent, TbCardFooterComponent, TbCardContentComponent
];

/**
 */
import {
    ConnectionStatusComponent, TextComponent, UnknownComponent, LabelStaticComponent, CaptionComponent, NumericTextBoxComponent, MaskedTextBoxComponent,
    PhoneComponent, PasswordComponent, SectionTitleComponent, TextareaComponent, TimeInputComponent, DateInputComponent, CheckBoxComponent,
    RadioComponent, ColorPickerComponent, EmailComponent, ComboSimpleComponent, ComboComponent, ButtonComponent, EnumComboComponent,
    ImageComponent, LinkComponent, PlaceholderComponent, StateButtonComponent, FileComponent, BoolEditComponent, GridComponent,
    LinearGaugeComponent, HotlinkComponent, BodyEditComponent, BodyEditColumnComponent, TreeViewComponent
} from './controls/';
export * from './controls';
const TB_CONTROLS = [
    ConnectionStatusComponent, TextComponent, UnknownComponent, LabelStaticComponent, CaptionComponent, NumericTextBoxComponent, MaskedTextBoxComponent,
    PhoneComponent, PasswordComponent, SectionTitleComponent, TextareaComponent, TimeInputComponent, DateInputComponent, CheckBoxComponent,
    RadioComponent, ColorPickerComponent, EmailComponent, ComboSimpleComponent, ComboComponent, ButtonComponent, EnumComboComponent,
    ImageComponent, LinkComponent, PlaceholderComponent, StateButtonComponent, FileComponent, BoolEditComponent, GridComponent,
    LinearGaugeComponent, HotlinkComponent, BodyEditComponent, BodyEditColumnComponent, TreeViewComponent
];

const TB_MODULES = [
    ReactiveFormsModule,
    CommonModule,
    FormsModule,
    HttpModule,
    MaterialModule,
    RouterModule,
    TbIconsModule,
    MasonryModule
];

/**
 * Direttive per style o funzionalità applicate a componenti base
 */
import { TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective } from './directives';
import { ContextMenuDirective, LayoutTypeColumnDirective, LayoutTypeHboxDirective, LayoutTypeVboxDirective } from './directives';
export * from './directives';

const TB_DIRECTIVES = [
    TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective,
    LayoutTypeColumnDirective, LayoutTypeHboxDirective, LayoutTypeVboxDirective,
    ContextMenuDirective
];

@NgModule({
    imports: [TB_MODULES, KENDO_UI_MODULES, TreeModule],
    declarations: [TB_CONTAINERS, TB_COMPONENTS, TB_CONTROLS, TB_DIRECTIVES],
    exports: [TB_MODULES, TB_CONTAINERS, TB_COMPONENTS, TB_CONTROLS, TB_DIRECTIVES/*, KENDO_UI_MODULES*/],
    entryComponents: [UnsupportedComponent, RadarComponent]
})
export class TbSharedModule { }
