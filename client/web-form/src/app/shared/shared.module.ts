import {
  FrameContentComponent, ViewContainerComponent, DockpaneContainerComponent,
  DockpaneComponent, TabberComponent, TabComponent, TileManagerComponent,
  TileGroupComponent, TileComponent, ViewComponent, SelectorComponent,
  TilePanelComponent, LayoutContainerComponent, FrameComponent
} from './containers';

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from '@angular/material';
import { RouterModule } from '@angular/router';

import { GridModule } from '@progress/kendo-angular-grid';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DialogModule } from '@progress/kendo-angular-dialog';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { PopupModule } from '@progress/kendo-angular-popup';

import { ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent } from './toolbar';
import { TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective } from './containers/tiles/tile/tile.size';
import { LayoutTypeColumnDirective, LayoutTypeHboxDirective, LayoutTypeVboxDirective } from './containers/tiles/layout-styles';
import { CaptionComponent, EditComponent, ComboComponent, EnumComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent, StateButtonComponent, TextComponent } from './controls/';
import { DynamicCmpComponent } from './dynamic-cmp.component';
import { PageNotFoundComponent } from './page-not-found.component';
import { TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent } from './topbar/index';
import { HeaderStripComponent } from './header-strip/header-strip.component';
import { OpenComponent } from './explorer/open/open.component';
import { SaveComponent } from './explorer/save/save.component';
import { GridComponent } from './controls/grid/grid.component';
import { PlaceholderComponent } from './controls/placeholder/placeholder.component';
import { PasswordComponent } from './controls/password/password.component';
import { MaskedTextBoxComponent } from './controls/masked-text-box/masked-text-box.component';
import { NumericTextBoxComponent } from './controls/numeric-text-box/numeric-text-box.component';
import { DateInputComponent } from './controls/date-input/date-input.component';
import { ContextMenuComponent } from './controls/context-menu/context-menu.component';
import { ReportTextrectComponent } from './report-objects/textrect/textrect.component';
import { ReportFieldrectComponent } from './report-objects/fieldrect/fieldrect.component';
import { ReportTableComponent } from './report-objects/table/table.component';
import { LabelStaticComponent } from './controls/label-static/label-static.component';
import { ReportObjectImageComponent } from './report-objects/image/image.component';
import { ReportRectComponent } from './report-objects/rect/rect.component';
import { ContextMenuDirective } from './directives/context-menu.directive';

const KENDO_UI_MODULES = [
  GridModule,
  InputsModule,
  DateInputsModule,
  DialogModule,
  DropDownsModule,
  LayoutModule,
  PopupModule
];

const TB_COMPONENTS = [
  PageNotFoundComponent,
  TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent,
  FrameContentComponent, ViewContainerComponent, DockpaneContainerComponent, SelectorComponent,
  ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent,
  TabComponent, TabberComponent,
  ViewComponent, DockpaneComponent, FrameComponent, FrameContentComponent, ViewContainerComponent, DockpaneContainerComponent,
  DynamicCmpComponent,
  EditComponent, CaptionComponent, ComboComponent, EnumComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent, GridComponent, DateInputComponent,
  StateButtonComponent, TextComponent, LabelStaticComponent,
  TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, LayoutContainerComponent,
  HeaderStripComponent,
  OpenComponent, SaveComponent,
  PlaceholderComponent, PasswordComponent, MaskedTextBoxComponent, NumericTextBoxComponent, ContextMenuComponent,
  ReportTextrectComponent, ReportFieldrectComponent, ReportTableComponent, ReportObjectImageComponent, ReportRectComponent
];

const TB_DIRECTIVES = [
  TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective,
  LayoutTypeColumnDirective, LayoutTypeHboxDirective, LayoutTypeVboxDirective,
  ContextMenuDirective
];

@NgModule({
  imports: [
    ReactiveFormsModule, InputsModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule.forRoot(),
    RouterModule,
    KENDO_UI_MODULES
  ],
  declarations: [TB_COMPONENTS, TB_DIRECTIVES],
  exports: [TB_COMPONENTS, TB_DIRECTIVES],
  entryComponents: [OpenComponent, SaveComponent, ContextMenuComponent]
})
export class SharedModule {

  constructor() {

  }

}
