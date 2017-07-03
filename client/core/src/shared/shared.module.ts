import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from '@angular/material';
import { RouterModule } from '@angular/router';

import { MasonryModule } from 'angular2-masonry';

import { TbIconsModule } from '../icons/icons.module';

import { DialogModule } from '@progress/kendo-angular-dialog';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { PopupModule } from '@progress/kendo-angular-popup';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';

const KENDO_UI_MODULES = [
    DialogModule,
    DateInputsModule,
    InputsModule,
    LayoutModule,
    PopupModule,
    ButtonsModule
];

/**
 * Components
 */
import {
    DynamicCmpComponent, ContextMenuComponent, DocumentComponent, PageNotFoundComponent, HeaderStripComponent,
    ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent,
    TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent, TopbarMenuElementsComponent,
    // OpenComponent, SaveComponent, Accordion
} from './components';
export * from './components';
const TB_COMPONENTS = [
    DynamicCmpComponent, ContextMenuComponent, DocumentComponent, PageNotFoundComponent, HeaderStripComponent,
    ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent,
    TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent, TopbarMenuElementsComponent,
    // OpenComponent, SaveComponent, Accordion
];

/**
 * Containers - Contenitori di struttura della pagina derivati dalla versione desktop
 */
import {
    FrameComponent, FrameContentComponent, ViewComponent, ViewContainerComponent,
    DockpaneComponent, DockpaneContainerComponent,
    TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, LayoutContainerComponent,
    MessageDialogComponent, TabberComponent, TabComponent,
    TbCardComponent, TbCardTitleComponent, TbCardSubtitleComponent, TbCardHeaderComponent, TbCardFooterComponent, TbCardContentComponent
} from './containers';
export * from './containers';

const TB_CONTAINERS = [
    FrameComponent, FrameContentComponent, ViewComponent, ViewContainerComponent,
    DockpaneComponent, DockpaneContainerComponent,
    TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, LayoutContainerComponent,
    MessageDialogComponent, TabberComponent, TabComponent,
    TbCardComponent, TbCardTitleComponent, TbCardSubtitleComponent, TbCardHeaderComponent, TbCardFooterComponent, TbCardContentComponent
];

/**
 */
import {
    ConnectionStatusComponent, TextComponent, UnknownComponent, LabelStaticComponent, CaptionComponent, NumericTextBoxComponent, MaskedTextBoxComponent,
    PhoneComponent, PasswordComponent, SectionTitleComponent, TextareaComponent, TimeInputComponent
} from './controls/';
export * from './controls';
const TB_CONTROLS = [
    ConnectionStatusComponent, TextComponent, UnknownComponent, LabelStaticComponent, CaptionComponent, NumericTextBoxComponent, MaskedTextBoxComponent,
    PhoneComponent, PasswordComponent, SectionTitleComponent, TextareaComponent, TimeInputComponent
];

const TB_MODULES = [
    ReactiveFormsModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    RouterModule,
    TbIconsModule,
    MasonryModule
];

/**
 * Direttive per style o funzionalità applicate a componenti base
 */
// import { TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective } from './directives';
import { ContextMenuDirective/*LayoutTypeColumnDirective, LayoutTypeHboxDirective, LayoutTypeVboxDirective */ } from './directives';
export * from './directives';

const TB_DIRECTIVES = [
    // TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective,
    // LayoutTypeColumnDirective, LayoutTypeHboxDirective, LayoutTypeVboxDirective,
    ContextMenuDirective
];

@NgModule({
    imports: [TB_MODULES, KENDO_UI_MODULES],
    declarations: [TB_CONTAINERS, TB_COMPONENTS, TB_CONTROLS, TB_DIRECTIVES],
    exports: [TB_MODULES, TB_CONTAINERS, TB_COMPONENTS, TB_CONTROLS, TB_DIRECTIVES]
})
export class TbSharedModule { }
