import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from '@angular/material';
import { RouterModule } from '@angular/router';

import { MasonryModule } from 'angular2-masonry';

import { DialogModule } from '@progress/kendo-angular-dialog';

const KENDO_UI_MODULES = [
    DialogModule
];

/**
 * Containers - Contenitori di struttura della pagina derivati dalla versione desktop
 */
import {
    // FrameComponent, FrameContentComponent, ViewComponent, ViewContainerComponent,
    // DockpaneComponent, DockpaneContainerComponent,
    // TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, LayoutContainerComponent,
    MessageDialogComponent
} from './containers';

// export * from './containers/frame/frame.component';
// export * from './containers/frame/frame-content/frame-content.component';
// export * from './containers/view/view.component';
// export * from './containers/view/view-container/view-container.component';
// export * from './containers/dockpane/dockpane.component';
// export * from './containers/dockpane/dockpane-container/dockpane-container.component';
// export * from './containers/tiles/tile-manager/tile-manager.component';
// export * from './containers/tiles/tile-group/tile-group.component';
// export * from './containers/tiles/tile/tile.component';
// export * from './containers/tiles/tile-panel/tile-panel.component';
// export * from './containers/tiles/layout-container/layout-container.component';
export * from './containers/message-dialog/message-dialog.component';

const TB_CONTAINERS = [
    // FrameComponent, FrameContentComponent, ViewComponent, ViewContainerComponent,
    // DockpaneComponent, DockpaneContainerComponent,
    // TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, LayoutContainerComponent,
    MessageDialogComponent
];

const TB_MODULES = [
    ReactiveFormsModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    RouterModule,
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
    declarations: [TB_CONTAINERS, /*TB_DIRECTIVES*/],
    exports: [TB_MODULES, TB_CONTAINERS, /*TB_DIRECTIVES*/]
})
export class TbSharedModule { }
