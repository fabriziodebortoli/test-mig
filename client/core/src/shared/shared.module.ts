import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from '@angular/material';
import { RouterModule } from '@angular/router';

import { MasonryModule } from 'angular2-masonry';

import { TbIconsModule } from '../icons/icons.module';

import { DialogModule } from '@progress/kendo-angular-dialog';
import { LayoutModule } from '@progress/kendo-angular-layout';

const KENDO_UI_MODULES = [
    DialogModule,
    LayoutModule
];

/**
 * Components
 */
import {
    DynamicCmpComponent
} from './components';
export * from './components';
const TB_COMPONENTS = [
    DynamicCmpComponent
];

/**
 * Containers - Contenitori di struttura della pagina derivati dalla versione desktop
 */
import {
    FrameComponent, FrameContentComponent, ViewComponent, ViewContainerComponent,
    DockpaneComponent, DockpaneContainerComponent,
    TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, LayoutContainerComponent,
    MessageDialogComponent,
    TbCardComponent, TbCardTitleComponent, TbCardSubtitleComponent, TbCardHeaderComponent, TbCardFooterComponent, TbCardContentComponent
} from './containers';
export * from './containers';

const TB_CONTAINERS = [
    FrameComponent, FrameContentComponent, ViewComponent, ViewContainerComponent,
    DockpaneComponent, DockpaneContainerComponent,
    TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, LayoutContainerComponent,
    MessageDialogComponent,
    TbCardComponent, TbCardTitleComponent, TbCardSubtitleComponent, TbCardHeaderComponent, TbCardFooterComponent, TbCardContentComponent
];

const TB_MODULES = [
    ReactiveFormsModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    RouterModule,
    TbIconsModule
    // MasonryModule
];

/**
 * Direttive per style o funzionalità applicate a componenti base
 */
// import { TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective } from './directives';
// import { ContextMenuDirective, LayoutTypeColumnDirective, LayoutTypeHboxDirective, LayoutTypeVboxDirective } from './directives';
// export * from './directives';

// const TB_DIRECTIVES = [
//     TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective,
//     ContextMenuDirective, LayoutTypeColumnDirective, LayoutTypeHboxDirective, LayoutTypeVboxDirective
// ];

@NgModule({
    imports: [TB_MODULES, KENDO_UI_MODULES],
    declarations: [TB_CONTAINERS, TB_COMPONENTS, /*TB_DIRECTIVES*/],
    exports: [TB_MODULES, TB_CONTAINERS, TB_COMPONENTS,  /*TB_DIRECTIVES*/]
})
export class TbSharedModule { }
