import { DockpaneComponent } from './containers/dockpane/dockpane.component';

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MaterialModule } from '@angular/material';
import { RouterModule } from '@angular/router';

import { GridModule } from '@progress/kendo-angular-grid';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DialogModule } from '@progress/kendo-angular-dialog';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { PopupModule } from '@progress/kendo-angular-popup';


import { TopbarSearchComponent } from './topbar/topbar-search/topbar-search.component';
import { ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent } from './toolbar';
import { TabberComponent, TabComponent, TileManagerComponent, TileGroupComponent, TileComponent, ViewComponent, TilePanelComponent, FrameComponent } from './containers';
import { TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective } from './containers/tiles/tile/tile.size';
import { CaptionComponent, EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent, StateButtonComponent } from './controls/';
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
  TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent, TopbarSearchComponent,
  ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent,
  TabComponent, TabberComponent, ViewComponent, DockpaneComponent, FrameComponent,
  DynamicCmpComponent,
  EditComponent, CaptionComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent, GridComponent, DateInputComponent,
  StateButtonComponent,
  TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent,
  TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective,
  HeaderStripComponent,
  OpenComponent, SaveComponent,
  PlaceholderComponent, PasswordComponent, MaskedTextBoxComponent, NumericTextBoxComponent, ContextMenuComponent,
  ReportTextrectComponent, ReportFieldrectComponent
];

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterialModule.forRoot(),
    RouterModule,
    KENDO_UI_MODULES
  ],
  declarations: [TB_COMPONENTS],
  exports: [TB_COMPONENTS],
  entryComponents: [OpenComponent, SaveComponent]
})
export class SharedModule {

  constructor() {

  }

}
