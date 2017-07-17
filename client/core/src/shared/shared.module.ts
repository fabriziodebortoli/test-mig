import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from '@angular/material';
import { RouterModule } from '@angular/router';
import { HttpModule } from '@angular/http';

import { MasonryModule } from 'angular2-masonry';

import { TbIconsModule } from '../icons/icons.module';

import { DialogModule } from '@progress/kendo-angular-dialog';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { PopupModule } from '@progress/kendo-angular-popup';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { GridModule } from '@progress/kendo-angular-grid';
import { ChartsModule } from '@progress/kendo-angular-charts';

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
import {
    ProxyRouteComponent, DynamicCmpComponent, ContextMenuComponent, DocumentComponent, PageNotFoundComponent, HeaderStripComponent,
    ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent,
    TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent, TopbarMenuElementsComponent,
    UnsupportedComponent, UnsupportedFactoryComponent, OpenComponent, SaveComponent, Accordion, RadarComponent
} from './components';
export * from './components';
const TB_COMPONENTS = [
    ProxyRouteComponent, DynamicCmpComponent, ContextMenuComponent, DocumentComponent, PageNotFoundComponent, HeaderStripComponent,
    ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent,
    TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent, TopbarMenuElementsComponent,
    UnsupportedComponent, UnsupportedFactoryComponent, OpenComponent, SaveComponent, Accordion, RadarComponent
];

/**
 * Containers - Contenitori di struttura della pagina derivati dalla versione desktop
 */
import {
    FrameComponent, FrameContentComponent, ViewComponent, ViewContainerComponent,
    DockpaneComponent, DockpaneContainerComponent,
    TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, LayoutContainerComponent,
    MessageDialogComponent, DiagnosticDialogComponent, DiagnosticItemComponent, TabberComponent, TabComponent,
    TbCardComponent, TbCardTitleComponent, TbCardSubtitleComponent, TbCardHeaderComponent, TbCardFooterComponent, TbCardContentComponent
} from './containers';
export * from './containers';

const TB_CONTAINERS = [
    FrameComponent, FrameContentComponent, ViewComponent, ViewContainerComponent,
    DockpaneComponent, DockpaneContainerComponent,
    TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, LayoutContainerComponent,
    MessageDialogComponent, DiagnosticDialogComponent, DiagnosticItemComponent, TabberComponent, TabComponent,
    TbCardComponent, TbCardTitleComponent, TbCardSubtitleComponent, TbCardHeaderComponent, TbCardFooterComponent, TbCardContentComponent
];

/**
 */
import {
    ConnectionStatusComponent, TextComponent, UnknownComponent, LabelStaticComponent, CaptionComponent, NumericTextBoxComponent, MaskedTextBoxComponent,
    PhoneComponent, PasswordComponent, SectionTitleComponent, TextareaComponent, TimeInputComponent, DateInputComponent, CheckBoxComponent,
    RadioComponent, ColorPickerComponent, EmailComponent, ComboSimpleComponent, ComboComponent, ButtonComponent, EnumComboComponent,
    ImageComponent, LinkComponent, PlaceholderComponent, StateButtonComponent, FileComponent, BoolEditComponent, GridComponent,
    LinearGaugeComponent, HotlinkComponent, BodyEditComponent
} from './controls/';
export * from './controls';
const TB_CONTROLS = [
    ConnectionStatusComponent, TextComponent, UnknownComponent, LabelStaticComponent, CaptionComponent, NumericTextBoxComponent, MaskedTextBoxComponent,
    PhoneComponent, PasswordComponent, SectionTitleComponent, TextareaComponent, TimeInputComponent, DateInputComponent, CheckBoxComponent,
    RadioComponent, ColorPickerComponent, EmailComponent, ComboSimpleComponent, ComboComponent, ButtonComponent, EnumComboComponent,
    ImageComponent, LinkComponent, PlaceholderComponent, StateButtonComponent, FileComponent, BoolEditComponent, GridComponent,
    LinearGaugeComponent, HotlinkComponent, BodyEditComponent
];

const TB_MODULES = [
    ReactiveFormsModule,
    CommonModule,
    FormsModule,
    HttpModule,
    ReactiveFormsModule,
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
    imports: [TB_MODULES, KENDO_UI_MODULES],
    declarations: [TB_CONTAINERS, TB_COMPONENTS, TB_CONTROLS, TB_DIRECTIVES],
    exports: [TB_MODULES, TB_CONTAINERS, TB_COMPONENTS, TB_CONTROLS, TB_DIRECTIVES/*, KENDO_UI_MODULES*/],
    entryComponents: [UnsupportedComponent]
})
export class TbSharedModule { }
